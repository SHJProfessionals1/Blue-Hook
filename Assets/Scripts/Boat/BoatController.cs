using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class BoatController : MonoBehaviour
{
	public event Action OnMoveStarted;
	public event Action OnSteeringStarted;

	[SerializeField] Rigidbody rb;
	[SerializeField] BoatInput input;
	[SerializeField] BoatFuel fuel;
	[SerializeField] Collider waterCollider;
	[SerializeField] Player player;

	[Header("Speed")]
	[SerializeField] float maxSpeed = 12f;
	[SerializeField] float maxReverseSpeed = 4f;
	[SerializeField] float minThrottle = 0.6f;

	[Header("Steering")]
	[SerializeField] float turnSpeed = 60f;
	[SerializeField] float idleTurnMultiplier = 0.25f;
	[SerializeField] float turnSmooth = 4f;

	[SerializeField] float steerResponseLeft = 2.5f;
	[SerializeField] float steerResponseRight = 3.5f;
	[SerializeField] float maxRudderAngle = 0.8f;

	[Header("Bounds")]
	[SerializeField] float boundaryPadding = 1.5f;

	public Rigidbody Rigidbody => rb;
	public float MaxSpeed => maxSpeed;

	float throttle;
	float targetSteer;
	float currentSteer;
	float currentYawSpeed;

	bool wasSteering;
	bool wasMoving;
	bool isActive;
	Vector3 lastPosition;

	Bounds waterBounds;

	void Awake()
	{
		if (!rb) rb = GetComponent<Rigidbody>();

		rb.interpolation = RigidbodyInterpolation.Interpolate;
		rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
		rb.centerOfMass = new Vector3(0f, -0.5f, 0f);

		if (!waterCollider)
			waterCollider = GameObject.FindGameObjectWithTag("Water").GetComponent<Collider>();

		if (!player)
			player = FindFirstObjectByType<Player>();

		if (!fuel)
			fuel = GetComponent<BoatFuel>();
	}

	void Start()
	{
		waterBounds = waterCollider.bounds;
	}

	void OnEnable()
	{
		player.OnModeChanged += HandleModeChanged;
		HandleModeChanged(player.CurrentMode);
	}

	void OnDisable()
	{
		player.OnModeChanged -= HandleModeChanged;
	}

	void HandleModeChanged(PlayerMode mode)
	{
		isActive = mode == PlayerMode.OnBoat;

		if (isActive)
		{
			lastPosition = rb.position;
		}
		if (!isActive)
		{
			throttle = 0f;
			targetSteer = 0f;
			currentSteer = 0f;
			currentYawSpeed = 0f;

			wasSteering = false;
			wasMoving = false;

			rb.linearVelocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
		}
	}

	void Update()
	{
		if (!isActive)
			return;

		throttle = input.Throttle;

		bool wantsToMove = Mathf.Abs(throttle) > 0.01f;

		if (wantsToMove && !wasMoving)
			OnMoveStarted?.Invoke();

		wasMoving = wantsToMove;

		float steeringInput =
			Mathf.Clamp(input.Steering, -maxRudderAngle, maxRudderAngle);

		bool isSteeringNow = Mathf.Abs(steeringInput) > 0.01f;

		if (isSteeringNow && !wasSteering)
			OnSteeringStarted?.Invoke();

		wasSteering = isSteeringNow;
		targetSteer = steeringInput;
	}

	void FixedUpdate()
	{
		if (!isActive)
			return;

		if (fuel && !fuel.HasFuel)
		{
			rb.linearVelocity = Vector3.zero;

			rb.angularVelocity = Vector3.Lerp(
				rb.angularVelocity,
				Vector3.zero,
				5f * Time.fixedDeltaTime
			);

			return;
		}

		SmoothSteering();
		MoveInstant();
		RotateSmooth();
		ClampToWater();

		float distance = Vector3.Distance(rb.position, lastPosition);
		lastPosition = rb.position;

		float speed01 = rb.linearVelocity.magnitude / maxSpeed;
		float weightedDistance = distance * Mathf.Lerp(1f, 1.5f, speed01);

		if (distance > 0.001f)
			fuel?.ConsumeDistance(distance);
	}

	void SmoothSteering()
	{
		float response =
			targetSteer < currentSteer
				? steerResponseLeft
				: steerResponseRight;

		currentSteer = Mathf.MoveTowards(
			currentSteer,
			targetSteer,
			response * Time.fixedDeltaTime
		);
	}

	void MoveInstant()
	{
		if (Mathf.Abs(throttle) < 0.01f)
		{
			rb.linearVelocity = Vector3.zero;
			return;
		}

		bool reversing = throttle < 0f;
		float maxAllowedSpeed = reversing ? maxReverseSpeed : maxSpeed;

		float effectiveThrottle =
			Mathf.Sign(throttle) *
			Mathf.Max(Mathf.Abs(throttle), minThrottle);

		float targetSpeed = effectiveThrottle * maxAllowedSpeed;

		Vector3 lateral = Vector3.Project(rb.linearVelocity, transform.right);
		Vector3 vertical = Vector3.Project(rb.linearVelocity, transform.up);

		rb.linearVelocity =
			transform.forward * targetSpeed +
			lateral +
			vertical;
	}

	void RotateSmooth()
	{
		float speed = rb.linearVelocity.magnitude;
		float speed01 = Mathf.Clamp01(speed / maxSpeed);

		float turnFactor =
			speed > 0.1f
				? Mathf.Lerp(idleTurnMultiplier, 1f, speed01)
				: idleTurnMultiplier;

		float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
		float steeringDirection = forwardSpeed < -0.1f ? -1f : 1f;

		float targetYaw =
			currentSteer *
			steeringDirection *
			turnSpeed *
			turnFactor *
			Mathf.Deg2Rad;

		currentYawSpeed = Mathf.Lerp(
			currentYawSpeed,
			targetYaw,
			turnSmooth * Time.fixedDeltaTime
		);

		rb.angularVelocity = new Vector3(0f, currentYawSpeed, 0f);
	}

	void ClampToWater()
	{
		Vector3 pos = rb.position;

		pos.x = Mathf.Clamp(
			pos.x,
			waterBounds.min.x + boundaryPadding,
			waterBounds.max.x - boundaryPadding
		);

		pos.z = Mathf.Clamp(
			pos.z,
			waterBounds.min.z + boundaryPadding,
			waterBounds.max.z - boundaryPadding
		);

		rb.position = pos;
	}
}

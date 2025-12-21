using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public sealed class BoatCamera : MonoBehaviour
{
	[SerializeField] Player player;
	[SerializeField] BoatController boat;
	[SerializeField] Transform target;

	[SerializeField] Vector3 offset = new Vector3(0f, 4f, -6.75f);
	[SerializeField] float positionSmooth = 12f;

	[SerializeField] float pitch = 15f;
	[SerializeField] float yawSmooth = 3f;
	[SerializeField] float mouseSensitivity = 120f;

	[SerializeField] Vector2 yawRange = new Vector2(-45f, 45f);

	[SerializeField] float leanAmount = 8f;
	[SerializeField] float leanSmooth = 4f;

	float lookYaw;
	float currentYaw;
	float lean;

	bool cameraControlLocked;

	void Awake()
	{
		if (!player)
			player = GetComponentInParent<Player>();

		if (!boat)
			boat = FindFirstObjectByType<BoatController>();
	}

	void OnEnable()
	{
		player.OnEnterBoat += AlignToTarget;

		if (boat)
		{
			boat.OnMoveStarted += AlignToTarget;
			boat.OnSteeringStarted += AlignToTarget;
		}
	}

	void OnDisable()
	{
		player.OnEnterBoat -= AlignToTarget;

		if (boat)
		{
			boat.OnMoveStarted -= AlignToTarget;
			boat.OnSteeringStarted -= AlignToTarget;
		}
	}

	void Update()
	{
		UpdatePointerState();
		UpdateLookYaw();
		UpdateLean();
	}

	void LateUpdate()
	{
		if (!target)
			return;

		Vector3 desiredPos = target.TransformPoint(offset);
		transform.position = Vector3.Lerp(
			transform.position,
			desiredPos,
			positionSmooth * Time.deltaTime
		);

		float baseYaw = target.eulerAngles.y;
		float targetYaw = baseYaw + lookYaw;

		currentYaw = Mathf.LerpAngle(
			currentYaw,
			targetYaw,
			yawSmooth * Time.deltaTime
		);

		transform.rotation = Quaternion.Euler(
			pitch + lean,
			currentYaw,
			0f
		);
	}

	void UpdatePointerState()
	{
#if UNITY_EDITOR || UNITY_STANDALONE
		if (Input.GetMouseButtonDown(0))
			EvaluatePointerOwnership(Input.mousePosition);

		if (Input.GetMouseButtonUp(0))
			ReleasePointer();
#else
		if (Input.touchCount > 0)
		{
			Touch t = Input.GetTouch(0);

			if (t.phase == TouchPhase.Began)
				EvaluatePointerOwnership(t.position);

			if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
				ReleasePointer();
		}
#endif
	}

	void EvaluatePointerOwnership(Vector2 screenPos)
	{
		bool startedOnUI =
			EventSystem.current &&
			EventSystem.current.IsPointerOverGameObject();

		bool startedOnRightHalf =
			screenPos.x > Screen.width * 0.5f;

		cameraControlLocked = startedOnUI || !startedOnRightHalf;
	}

	void ReleasePointer()
	{
		cameraControlLocked = false;
	}

	void UpdateLookYaw()
	{
		if (cameraControlLocked)
			return;

		float mouseX = Input.GetAxis("Mouse X");
		lookYaw += mouseX * mouseSensitivity * Time.deltaTime;
		lookYaw = Mathf.Clamp(lookYaw, yawRange.x, yawRange.y);
	}

	void UpdateLean()
	{
		float steer = Input.GetAxis("Horizontal");

		lean = Mathf.Lerp(
			lean,
			-steer * leanAmount,
			leanSmooth * Time.deltaTime
		);
	}

	void AlignToTarget()
	{
		lookYaw = 0f;
	}
}

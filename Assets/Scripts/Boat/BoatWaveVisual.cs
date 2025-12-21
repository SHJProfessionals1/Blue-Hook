using UnityEngine;
using DG.Tweening;

public sealed class BoatWaveVisual : MonoBehaviour
{
	[Header("References")]
	[SerializeField] Transform cameraTransform;

	[Header("Bobbing")]
	[SerializeField] float bobHeight = 0.15f;
	[SerializeField] float bobDuration = 2.2f;

	[Header("Rotation - Pitch (X)")]
	[SerializeField] float pitchIntensity = 2.5f;
	[SerializeField] float pitchDuration = 3.0f;

	[Header("Rotation - Roll (Z)")]
	[SerializeField] float rollIntensity = 3.5f;
	[SerializeField] float rollDuration = 2.6f;

	[Header("Rotation - Yaw (Y)")]
	[SerializeField] float yawIntensity = 1.2f;
	[SerializeField] float yawDuration = 4.5f;

	[Header("Camera Influence")]
	[Range(0f, 1f)]
	[SerializeField] float cameraInfluence = 0.4f;

	[Header("Sea State")]
	[SerializeField] float intensityMultiplier = 1f;

	Vector3 initialLocalPos;
	Vector3 baseEuler;

	Tween bobTween;
	Tween pitchTween;
	Tween rollTween;
	Tween yawTween;

	float pitchValue;
	float rollValue;
	float yawValue;

	void Awake()
	{
		initialLocalPos = transform.localPosition;
		baseEuler = transform.localEulerAngles;

		if (!cameraTransform && Camera.main)
			cameraTransform = Camera.main.transform;
	}

	void OnEnable()
	{
		StartBobbing();
		StartPitch();
		StartRoll();
		StartYaw();
	}

	void OnDisable()
	{
		bobTween?.Kill();
		pitchTween?.Kill();
		rollTween?.Kill();
		yawTween?.Kill();

		transform.localPosition = initialLocalPos;
		transform.localEulerAngles = baseEuler;
	}
	void StartBobbing()
	{
		float phase = Random.value;

		bobTween = transform
			.DOLocalMoveY(
				initialLocalPos.y + bobHeight * intensityMultiplier,
				bobDuration
			)
			.SetEase(Ease.InOutSine)
			.SetLoops(-1, LoopType.Yoyo)
			.SetDelay(phase);
	}

	void StartPitch()
	{
		float phase = Random.value * 0.5f;

		pitchTween = DOTween
			.To(() => pitchValue, x => pitchValue = x,
				pitchIntensity * intensityMultiplier,
				pitchDuration)
			.SetEase(Ease.InOutSine)
			.SetLoops(-1, LoopType.Yoyo)
			.SetDelay(phase)
			.OnUpdate(ApplyRotation);
	}

	void StartRoll()
	{
		float phase = Random.value * 0.5f;

		rollTween = DOTween
			.To(() => rollValue, z => rollValue = z,
				rollIntensity * intensityMultiplier,
				rollDuration)
			.SetEase(Ease.InOutSine)
			.SetLoops(-1, LoopType.Yoyo)
			.SetDelay(phase)
			.OnUpdate(ApplyRotation);
	}

	void StartYaw()
	{
		float phase = Random.value;

		yawTween = DOTween
			.To(() => yawValue, y => yawValue = y,
				yawIntensity * intensityMultiplier,
				yawDuration)
			.SetEase(Ease.InOutSine)
			.SetLoops(-1, LoopType.Yoyo)
			.SetDelay(phase)
			.OnUpdate(ApplyRotation);
	}

	void ApplyRotation()
	{
		Vector3 camDir = cameraTransform ? cameraTransform.forward : Vector3.forward;
		camDir.y = 0f;
		camDir.Normalize();

		float camPitch = -camDir.z * cameraInfluence;
		float camRoll = camDir.x * cameraInfluence;

		Vector3 finalEuler = baseEuler + new Vector3(
			pitchValue * camPitch,
			yawValue,
			rollValue * camRoll
		);

		transform.localEulerAngles = finalEuler;
	}

	public void SetSeaIntensity(float value)
	{
		intensityMultiplier = Mathf.Max(0f, value);

		bobTween?.Kill();
		pitchTween?.Kill();
		rollTween?.Kill();
		yawTween?.Kill();

		pitchValue = rollValue = yawValue = 0f;

		StartBobbing();
		StartPitch();
		StartRoll();
		StartYaw();
	}
}

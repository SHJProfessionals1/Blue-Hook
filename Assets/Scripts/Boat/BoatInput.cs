using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

public sealed class BoatInput : MonoBehaviour, IBoatInput
{
	[SerializeField] Slider throttleSlider;
	[SerializeField] CurvedSteering steeringWheel;

	const float StopCenter = 22.5f;
	const float StopDuration = 0.25f;

	float sliderValue;
	UnityAction<float> sliderListener;

	Tween stopTween;

	void Awake()
	{
		if (!throttleSlider)
			return;

		throttleSlider.minValue = 0f;
		throttleSlider.maxValue = 100f;

		sliderListener = v => sliderValue = v;
		throttleSlider.onValueChanged.AddListener(sliderListener);

		SmoothStop();
	}

	public float Throttle =>
		throttleSlider
			? MapThrottle(sliderValue)
			: Input.GetAxis("Vertical");

	public float Steering =>
		steeringWheel
			? steeringWheel.Steering
			: Input.GetAxis("Horizontal");

	public void SmoothStop()
	{
		stopTween?.Kill();

		stopTween = DOTween.To(
			() => sliderValue,
			v =>
			{
				sliderValue = v;
				throttleSlider.SetValueWithoutNotify(v);
			},
			StopCenter,
			StopDuration
		).SetEase(Ease.OutCubic);
	}

	float MapThrottle(float value)
	{
		if (value <= 10f)
			return Mathf.Lerp(-1f, 0f, value / 10f);

		if (value <= 35f)
			return 0f;

		return Mathf.Lerp(0f, 1f, (value - 35f) / 65f);
	}
}

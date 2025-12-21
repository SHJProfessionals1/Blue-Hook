using UnityEngine;
using UnityEngine.UI;

public sealed class BoatInput : MonoBehaviour, IBoatInput
{
	[SerializeField] Slider throttleSlider;
	[SerializeField] CurvedSteering steeringWheel;

	float sliderValue;

	const float StopCenter = 22.5f;
	const float StopSnapRange = 15f;

	void Awake()
	{
		if (throttleSlider)
		{
			throttleSlider.minValue = 0f;
			throttleSlider.maxValue = 100f;
			throttleSlider.value = StopCenter;

			sliderValue = throttleSlider.value;
			SnapToStopIfClose();

			throttleSlider.onValueChanged.AddListener(v =>
			{
				sliderValue = v;
			});
		}
	}

	public float Throttle
	{
		get
		{
			if (throttleSlider)
				return MapThrottle(sliderValue);

#if UNITY_EDITOR || UNITY_STANDALONE
			return Input.GetAxis("Vertical");
#else
			return 0f;
#endif
		}
	}

	public float Steering
	{
		get
		{
			if (steeringWheel)
				return steeringWheel.Steering;

#if UNITY_EDITOR || UNITY_STANDALONE
			return Input.GetAxis("Horizontal");
#else
			return 0f;
#endif
		}
	}

	float MapThrottle(float value)
	{
		if (value <= 10f)
			return Mathf.Lerp(-1f, 0f, value / 10f);

		if (value <= 35f)
			return 0f;

		return Mathf.Lerp(0f, 1f, (value - 35f) / 65f);
	}

	void SnapToStopIfClose()
	{
		if (!throttleSlider)
			return;

		if (Mathf.Abs(throttleSlider.value - StopCenter) <= StopSnapRange)
		{
			throttleSlider.value = StopCenter;
			sliderValue = StopCenter;
		}
	}
}

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public sealed class BoatUI : MonoBehaviour
{
	[SerializeField] Rigidbody boatRb;
	[SerializeField] BoatFuel fuel;
	[SerializeField] BoatController boat;
	[SerializeField] TMP_Text speedText;
	[SerializeField] TMP_Text angleText;
	[SerializeField] Slider fuelSlider;

	void OnEnable()
	{
		fuel.OnFuelChanged += UpdateFuelUI;
		UpdateFuelUI(fuel.CurrentFuel, fuel.MaxFuel);
	}

	void OnDisable()
	{
		fuel.OnFuelChanged -= UpdateFuelUI;
	}

	void Update()
	{
		float speedKmh = boatRb.linearVelocity.magnitude * 3.6f;
		speedText.text = Mathf.RoundToInt(speedKmh) + " km/h";

		float angularY = boatRb.angularVelocity.y;
		float degreesPerSecond = MathF.Abs(angularY * Mathf.Rad2Deg);

		angleText.text = degreesPerSecond.ToString("0.0") + "°/s";
	}

	void UpdateFuelUI(float current, float max)
	{
		fuelSlider.value = current / max;
	}
}

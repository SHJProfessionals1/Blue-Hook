using System;
using UnityEngine;

public sealed class BoatFuel : MonoBehaviour
{
	public event Action<float, float> OnFuelChanged;

	[SerializeField] float maxFuel = 150f;
	[SerializeField] float fuelPerMeter = 0.05f;

	float currentFuel;

	public float CurrentFuel => currentFuel;
	public float MaxFuel => maxFuel;
	public bool HasFuel => currentFuel > 0f;

	void Awake()
	{
		currentFuel = maxFuel;
		OnFuelChanged?.Invoke(currentFuel, maxFuel);
	}

	public void ConsumeDistance(float distance)
	{
		if (currentFuel <= 0f || distance <= 0f)
			return;

		currentFuel = Mathf.Max(
			0f,
			currentFuel - distance * fuelPerMeter
		);

		OnFuelChanged?.Invoke(currentFuel, maxFuel);
	}

	public void Refuel(float amount)
	{
		currentFuel = Mathf.Min(maxFuel, currentFuel + amount);
		OnFuelChanged?.Invoke(currentFuel, maxFuel);
	}
}
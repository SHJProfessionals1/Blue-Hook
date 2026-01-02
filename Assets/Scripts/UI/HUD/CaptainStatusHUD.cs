using TMPro;
using UnityEngine;

public class CaptainStatusHUD : MonoBehaviour
{
	[SerializeField] Rigidbody boatRb;
	[SerializeField] BoatFuel fuel;
	[SerializeField] TMP_Text speedText;
	[SerializeField] TMP_Text fuelText;
	[SerializeField] TMP_Text coinsText;

	void OnEnable()
	{
		fuel.OnFuelChanged += UpdateFuelUI;
		EconomyManager.Instance.CoinsChanged += UpdateCoinsUI;
		UpdateFuelUI(fuel.CurrentFuel, fuel.MaxFuel);
		UpdateCoinsUI(EconomyManager.Instance.Coins);
	}

	void OnDisable()
	{
		fuel.OnFuelChanged -= UpdateFuelUI;
		EconomyManager.Instance.CoinsChanged -= UpdateCoinsUI;
	}

	void Update()
	{
		float speedKmh = boatRb.linearVelocity.magnitude * 3.6f;
		speedText.text = Mathf.RoundToInt(speedKmh) + " km/h";
	}
	void UpdateFuelUI(float current, float max)
	{
		int percent = Mathf.RoundToInt((current / max) * 100f);
		fuelText.text = percent + "%";
	}

	void UpdateCoinsUI(int coins)
	{
		coinsText.text = coins.ToString();
	}
}

using FishingGameTool.Fishing;
using FishingGameTool.Fishing.LootData;
using UnityEngine;
using UnityEngine.UI;

public class FisherUI : MonoBehaviour
{
	[SerializeField] private FishingSystem fishingSystem;
	[SerializeField] private Image powerFillImage;
	[SerializeField] private GameObject castButton;
	[SerializeField] private GameObject attractButton;
	[SerializeField] private GameObject returnToBoatButton;

	[SerializeField] private Image hookedImage;
	[SerializeField] private float fadeSpeed = 2f;
	[SerializeField] private float minAlpha = 0.15f;
	[SerializeField] private float maxAlpha = 1f;

	[SerializeField] private bool smoothFill = true;
	[SerializeField] private float smoothSpeed = 12f;

	private float currentFill = 1f;
	private bool wasCasting;

	private bool hasHookedLoot;
	private FishingLootData hookedLootData;
	private float fadeT;

	private void Reset()
	{
		powerFillImage = GetComponent<Image>();
	}

	private void Awake()
	{
		if (!fishingSystem)
			fishingSystem = FindAnyObjectByType<FishingSystem>();

		ForceHideHookedImage();
	}

	private void OnEnable()
	{
		if (!fishingSystem)
			fishingSystem = FindAnyObjectByType<FishingSystem>();

		if (!fishingSystem)
			return;

		fishingSystem.OnLootCaught += LootHooked;
		fishingSystem.OnLootLanded += LootLanded;
		fishingSystem.OnFishingStopped += FishingStopped;
	}

	private void OnDisable()
	{
		if (!fishingSystem)
			return;

		fishingSystem.OnLootCaught -= LootHooked;
		fishingSystem.OnLootLanded -= LootLanded;
		fishingSystem.OnFishingStopped -= FishingStopped;
	}

	private void FishingStopped()
	{
		hasHookedLoot = false;
		hookedLootData = null;
		ForceHideHookedImage();
	}

	private void LootHooked(FishingLootData data)
	{
		if (!fishingSystem)
			return;

		if (!fishingSystem.IsFishing)
		{
			hasHookedLoot = false;
			hookedLootData = null;
			ForceHideHookedImage();
			return;
		}

		if (data == null)
		{
			hasHookedLoot = false;
			hookedLootData = null;
			ForceHideHookedImage();
			return;
		}

		hasHookedLoot = true;
		hookedLootData = data;
		fadeT = 0f;

		if (hookedImage)
		{
			hookedImage.gameObject.SetActive(true);
			SetHookedAlpha(minAlpha);
		}
	}

	private void LootLanded(FishingLootData data)
	{
		hasHookedLoot = false;
		hookedLootData = null;
		ForceHideHookedImage();
	}

	private void Update()
	{
		if (!fishingSystem || !powerFillImage)
		{
			ForceHideHookedImage();
			return;
		}

		bool isCasting = fishingSystem._castInput;
		bool isFishing = fishingSystem.IsFishing;
		bool isPulling = fishingSystem._attractInput;

		if (castButton)
			castButton.SetActive(!(isFishing && !isCasting));

		if (attractButton)
			attractButton.SetActive(isFishing);

		if (returnToBoatButton)
			returnToBoatButton.SetActive(!isFishing);

		bool systemSaysCaught = fishingSystem._advanced != null && fishingSystem._advanced._caughtLoot;
		FishingLootData systemLootData = fishingSystem._advanced != null ? fishingSystem._advanced._caughtLootData : null;

		if (!isFishing || isPulling || !hasHookedLoot || !systemSaysCaught || systemLootData == null || hookedLootData == null || systemLootData != hookedLootData)
		{
			hasHookedLoot = false;
			hookedLootData = null;
			ForceHideHookedImage();
		}
		else
		{
			if (hookedImage)
			{
				if (!hookedImage.gameObject.activeSelf)
				{
					hookedImage.gameObject.SetActive(true);
					fadeT = 0f;
				}

				fadeT += Time.deltaTime * fadeSpeed;
				float a = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(fadeT) + 1f) * 0.5f);
				SetHookedAlpha(a);
			}
		}

		float targetFill;

		if (isCasting && !wasCasting)
		{
			float maxForce = Mathf.Max(0.0001f, fishingSystem._maxCastForce);
			targetFill = Mathf.Clamp01(fishingSystem._currentCastForce / maxForce);
			currentFill = targetFill;
		}
		else
		{
			if (!isCasting)
				targetFill = 1f;
			else
			{
				float maxForce = Mathf.Max(0.0001f, fishingSystem._maxCastForce);
				targetFill = Mathf.Clamp01(fishingSystem._currentCastForce / maxForce);
			}

			if (smoothFill)
				currentFill = Mathf.MoveTowards(currentFill, targetFill, smoothSpeed * Time.deltaTime);
			else
				currentFill = targetFill;
		}

		wasCasting = isCasting;
		powerFillImage.fillAmount = currentFill;
	}

	private void ForceHideHookedImage()
	{
		if (!hookedImage)
			return;

		SetHookedAlpha(0f);
		hookedImage.gameObject.SetActive(false);
	}

	private void SetHookedAlpha(float a)
	{
		if (!hookedImage)
			return;

		var c = hookedImage.color;
		c.a = a;
		hookedImage.color = c;
	}
}

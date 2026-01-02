using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName = "Store/Item")]
public sealed class StoreItemSO : ScriptableObject
{
	[SerializeField] private string itemId;
	public string ItemId => itemId;

	public string displayName;
	public AssetReferenceGameObject assetPrefab;
	public StoreCategoryId category;
	public Sprite icon;

	public CurrencyType currency = CurrencyType.Coins;
	[Min(0)] public int price;

#if UNITY_EDITOR
	private void OnValidate()
	{
		if (string.IsNullOrEmpty(itemId))
		{
			itemId = GenerateItemId();
			UnityEditor.EditorUtility.SetDirty(this);
		}

		if (!string.IsNullOrEmpty(itemId))
			DetectDuplicateIds();
	}

	private string GenerateItemId()
	{
		string cat = category.ToString().ToUpperInvariant();
		string name = string.IsNullOrWhiteSpace(displayName)
			? "ITEM"
			: displayName.Replace(" ", "_").ToUpperInvariant();

		string guid = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();
		return $"{cat}_{name}_{guid}";
	}

	private void DetectDuplicateIds()
	{
		var allItems = Resources.FindObjectsOfTypeAll<StoreItemSO>();
		for (int i = 0; i < allItems.Length; i++)
		{
			var other = allItems[i];
			if (other == null || other == this) continue;

			if (other.itemId == itemId)
			{
				Debug.LogError($"Duplicate StoreItemSO ID detected: {itemId}", this);
				return;
			}
		}
	}
#endif
}

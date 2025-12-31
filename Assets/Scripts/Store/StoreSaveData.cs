using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class StoreSaveData
{
	[SerializeField] private List<string> purchasedItemIds = new();
	[SerializeField] private List<EquippedEntry> equippedByCategory = new();

	private HashSet<string> purchasedLookup;
	private Dictionary<StoreCategoryId, string> equippedLookup;

	[Serializable]
	private struct EquippedEntry
	{
		public StoreCategoryId category;
		public string itemId;
	}

	public void Initialize()
	{
		purchasedLookup = new HashSet<string>(purchasedItemIds ?? new List<string>());

		equippedLookup = new Dictionary<StoreCategoryId, string>();
		if (equippedByCategory != null)
		{
			for (int i = 0; i < equippedByCategory.Count; i++)
			{
				var e = equippedByCategory[i];
				if (string.IsNullOrEmpty(e.itemId)) continue;
				equippedLookup[e.category] = e.itemId;
			}
		}
	}

	public bool IsPurchased(string itemId)
	{
		if (string.IsNullOrEmpty(itemId)) return false;
		if (purchasedLookup == null) Initialize();
		return purchasedLookup.Contains(itemId);
	}

	public bool Purchase(string itemId)
	{
		if (string.IsNullOrEmpty(itemId)) return false;
		if (purchasedLookup == null) Initialize();

		if (!purchasedLookup.Add(itemId))
			return false;

		purchasedItemIds.Add(itemId);
		SaveManager.MarkDirty();
		return true;
	}

	public string GetEquipped(StoreCategoryId category)
	{
		if (equippedLookup == null) Initialize();
		return equippedLookup.TryGetValue(category, out var id) ? id : null;
	}

	public void SetEquipped(StoreCategoryId category, string itemId)
	{
		if (equippedLookup == null) Initialize();

		if (!string.IsNullOrEmpty(itemId) && !IsPurchased(itemId))
			return;

		equippedLookup[category] = itemId;

		if (equippedByCategory == null)
			equippedByCategory = new List<EquippedEntry>();

		bool found = false;
		for (int i = 0; i < equippedByCategory.Count; i++)
		{
			if (equippedByCategory[i].category != category)
				continue;

			equippedByCategory[i] = new EquippedEntry { category = category, itemId = itemId };
			found = true;
			break;
		}

		if (!found)
			equippedByCategory.Add(new EquippedEntry { category = category, itemId = itemId });

		SaveManager.MarkDirty();
	}
}

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Store/Catalog")]
public sealed class StoreCatalogSO : ScriptableObject
{
	[Header("Categories")]
	[SerializeField] private List<StoreCategoryId> categories = new();
	public IReadOnlyList<StoreCategoryId> Categories => categories;

	[Header("Items")]
	[SerializeField] private List<StoreItemSO> items = new();
	public IReadOnlyList<StoreItemSO> Items => items;

	public List<StoreItemSO> GetItems(StoreCategoryId category)
	{
		var result = new List<StoreItemSO>(32);
		for (int i = 0; i < items.Count; i++)
		{
			var it = items[i];
			if (it != null && it.category == category)
				result.Add(it);
		}

		result.Sort((a, b) =>
		{
			string an = a != null ? a.displayName : "";
			string bn = b != null ? b.displayName : "";
			return string.CompareOrdinal(an, bn);
		});

		return result;
	}

	public bool ContainsCategory(StoreCategoryId category)
	{
		for (int i = 0; i < categories.Count; i++)
			if (categories[i].Equals(category))
				return true;

		return false;
	}
}

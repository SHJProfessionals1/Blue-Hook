public sealed class StoreService
{
	public StoreSaveData Save { get; }

	public StoreService(StoreSaveData saveData)
	{
		Save = saveData;
	}

	public bool IsPurchased(StoreItemSO item)
	{
		if (item == null || Save == null) return false;
		return Save.IsPurchased(item.ItemId);
	}

	public bool IsEquipped(StoreItemSO item)
	{
		if (item == null || Save == null) return false;
		var equippedId = Save.GetEquipped(item.category);
		return !string.IsNullOrEmpty(equippedId) && equippedId == item.ItemId;
	}

	public void Equip(StoreItemSO item)
	{
		if (item == null || Save == null) return;
		if (!IsPurchased(item)) return;
		Save.SetEquipped(item.category, item.ItemId);
	}

	public bool CanBuy(StoreItemSO item)
	{
		if (item == null || Save == null) return false;
		if (IsPurchased(item)) return false;

		var economy = EconomyManager.Instance;
		if (economy == null) return false;

		return economy.CanAfford(item.currency, item.price);
	}

	public bool Buy(StoreItemSO item)
	{
		if (!CanBuy(item))
			return false;

		var economy = EconomyManager.Instance;
		if (economy == null) return false;

		if (!economy.TrySpend(item.currency, item.price, $"buy:{item.ItemId}"))
			return false;

		return Save.Purchase(item.ItemId);
	}
}

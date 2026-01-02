using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public sealed class StoreEquipActionReceiver : MonoBehaviour
{
	[Serializable]
	public sealed class StoreItemEvent : UnityEvent<StoreItemSO> { }

	[Serializable]
	public sealed class EquipActionEntry
	{
		[SerializeField] private StoreItemSO item;
		public StoreItemSO Item => item;

		[SerializeField] private StoreItemEvent onEquipped;
		public StoreItemEvent OnEquipped => onEquipped;
	}

	[SerializeField] private List<EquipActionEntry> equipActions = new();

	private Dictionary<string, StoreItemEvent> map;

	private void Awake()
	{
		RebuildMap();
	}

	private void OnValidate()
	{
		RebuildMap();
	}

	private void RebuildMap()
	{
		map = new Dictionary<string, StoreItemEvent>(equipActions != null ? equipActions.Count : 0);

		if (equipActions == null)
			return;

		for (int i = 0; i < equipActions.Count; i++)
		{
			var e = equipActions[i];
			if (e == null || e.Item == null)
				continue;

			var id = e.Item.ItemId;
			if (string.IsNullOrEmpty(id))
				continue;

			if (map.ContainsKey(id))
				continue;

			map.Add(id, e.OnEquipped);
		}
	}

	public bool TryInvoke(StoreItemSO item)
	{
		if (item == null || string.IsNullOrEmpty(item.ItemId))
			return false;

		if (map == null)
			RebuildMap();

		if (map.TryGetValue(item.ItemId, out var ev) && ev != null)
		{
			ev.Invoke(item);
			return true;
		}

		return false;
	}

	public bool TryInvoke(string itemId)
	{
		if (string.IsNullOrEmpty(itemId))
			return false;

		if (map == null)
			RebuildMap();

		if (map.TryGetValue(itemId, out var ev) && ev != null)
		{
			ev.Invoke(null);
			return true;
		}

		return false;
	}

	public void InvokeDirect(StoreItemSO item)
	{
		if (item == null)
			return;

		TryInvoke(item);
	}
}

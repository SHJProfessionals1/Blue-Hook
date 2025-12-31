using System;
using UnityEngine;

[Serializable]
public sealed class SaveGame
{
	public const int CurrentVersion = 1;
	public int version = CurrentVersion;

	[Header("Core")]
	public ProgressionData progression = new();
	public WalletData wallet = new();

	[Header("Systems")]
	public StoreSaveData store = new();
	public SettingsData settings = new();

	public void Initialize()
	{
		store?.Initialize();
	}
}

[Serializable]
public sealed class ProgressionData
{
	[SerializeField] int level = 1;
	[SerializeField] int xp = 0;

	public int Level
	{
		get => level;
		set
		{
			int v = Mathf.Max(1, value);
			if (level == v) return;
			level = v;
			SaveManager.MarkDirty();
		}
	}

	public int Xp
	{
		get => xp;
		set
		{
			int v = Mathf.Max(0, value);
			if (xp == v) return;
			xp = v;
			SaveManager.MarkDirty();
		}
	}

	public void AddXp(int amount)
	{
		if (amount <= 0) return;
		Xp = xp + amount;
	}
}

[Serializable]
public sealed class WalletData
{
	[SerializeField] int coins = 0;
	[SerializeField] int pearls = 0;

	public int Coins
	{
		get => coins;
		set
		{
			int v = Mathf.Max(0, value);
			if (coins == v) return;
			coins = v;
			SaveManager.MarkDirty();
		}
	}

	public int Pearls
	{
		get => pearls;
		set
		{
			int v = Mathf.Max(0, value);
			if (pearls == v) return;
			pearls = v;
			SaveManager.MarkDirty();
		}
	}

	public void AddCoins(int amount)
	{
		if (amount <= 0) return;
		Coins = coins + amount;
	}

	public bool TrySpendCoins(int amount)
	{
		if (amount <= 0) return true;
		if (coins < amount) return false;
		Coins = coins - amount;
		return true;
	}

	public void AddPearls(int amount)
	{
		if (amount <= 0) return;
		Pearls = pearls + amount;
	}

	public bool TrySpendPearls(int amount)
	{
		if (amount <= 0) return true;
		if (pearls < amount) return false;
		Pearls = pearls - amount;
		return true;
	}
}

[Serializable]
public sealed class SettingsData
{
	[SerializeField] float musicVolume = 1f;
	[SerializeField] float sfxVolume = 1f;

	public float MusicVolume
	{
		get => musicVolume;
		set
		{
			float v = Mathf.Clamp01(value);
			if (Mathf.Approximately(musicVolume, v)) return;
			musicVolume = v;
			SaveManager.MarkDirty();
		}
	}

	public float SfxVolume
	{
		get => sfxVolume;
		set
		{
			float v = Mathf.Clamp01(value);
			if (Mathf.Approximately(sfxVolume, v)) return;
			sfxVolume = v;
			SaveManager.MarkDirty();
		}
	}
}

using FishingGameTool.Fishing;
using System;
using UnityEngine;

public class Player : MonoBehaviour
{
	[SerializeField] private FishingSystem fishingSystem;

	public PlayerMode CurrentMode { get; private set; }

	public event Action<PlayerMode> OnModeChanged;
	public event Action<PlayerMode> OnModeChangeRequested;
	public event Action OnEnterBoat;
	public event Action OnEnterFishing;

	private void Awake()
	{
		fishingSystem = GetComponentInChildren<FishingSystem>(true);
	}

	private void Start()
	{
		SetMode(PlayerMode.OnBoat);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.B))
			RequestBoatMode();
		else if (Input.GetKeyDown(KeyCode.F))
			RequestFishingMode();
	}

	public void RequestBoatMode()
	{
		if (fishingSystem.IsFishing)
			return;

		OnModeChangeRequested?.Invoke(PlayerMode.OnBoat);
	}

	public void RequestFishingMode()
	{
		OnModeChangeRequested?.Invoke(PlayerMode.Fishing);
	}

	public void SetMode(PlayerMode newMode)
	{
		if (CurrentMode == newMode)
			return;

		CurrentMode = newMode;
		OnModeChanged?.Invoke(CurrentMode);

		switch (CurrentMode)
		{
			case PlayerMode.OnBoat:
				OnEnterBoat?.Invoke();
				break;
			case PlayerMode.Fishing:
				OnEnterFishing?.Invoke();
				break;
		}
	}
}

public enum PlayerMode
{
	OnBoat,
	Fishing
}

using UnityEngine;

public sealed class BoatCharacterController : MonoBehaviour
{
	[SerializeField] private Player player;
	[SerializeField] private BoatSwapper boatSwapper;

	[SerializeField] private GameObject fishingCharacter;
	[SerializeField] private string characterOnBoatName = "CharacterOnBoat";

	private GameObject characterOnBoat;

	private void Awake()
	{
		if (player == null)
			player = FindFirstObjectByType<Player>();

		if (boatSwapper == null)
			boatSwapper = FindFirstObjectByType<BoatSwapper>();
	}

	private void OnEnable()
	{
		if (player != null)
			player.OnModeChanged += UpdateVisuals;

		if (boatSwapper != null)
		{
			boatSwapper.OnBoatUnloading += ClearBoatRefs;
			boatSwapper.OnBoatEquipped += OnBoatEquipped;
			OnBoatEquipped(boatSwapper);
		}

		if (player != null)
			UpdateVisuals(player.CurrentMode);
	}

	private void OnDisable()
	{
		if (player != null)
			player.OnModeChanged -= UpdateVisuals;

		if (boatSwapper != null)
		{
			boatSwapper.OnBoatUnloading -= ClearBoatRefs;
			boatSwapper.OnBoatEquipped -= OnBoatEquipped;
		}
	}

	private void OnBoatEquipped(BoatSwapper swapper)
	{
		var boat = swapper != null ? swapper.CurrentBoatInstance : null;
		characterOnBoat = FindDeep(boat, characterOnBoatName);

		if (player != null)
			UpdateVisuals(player.CurrentMode);
	}

	private void ClearBoatRefs()
	{
		characterOnBoat = null;
	}

	private void UpdateVisuals(PlayerMode mode)
	{
		if (fishingCharacter != null)
			fishingCharacter.SetActive(mode == PlayerMode.Fishing);

		if (characterOnBoat != null)
			characterOnBoat.SetActive(mode == PlayerMode.OnBoat);
	}

	private static GameObject FindDeep(GameObject root, string name)
	{
		if (root == null || string.IsNullOrEmpty(name))
			return null;

		var trs = root.GetComponentsInChildren<Transform>(true);
		for (int i = 0; i < trs.Length; i++)
			if (trs[i].name == name)
				return trs[i].gameObject;

		return null;
	}
}

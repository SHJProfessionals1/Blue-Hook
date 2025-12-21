using UnityEngine;

public class BoatCharacterController : MonoBehaviour
{
	[SerializeField] private Player player;

	[SerializeField] private GameObject characterFishing;
	[SerializeField] private GameObject characterOnBoat;

	private void Awake()
	{
		if(player == null)
			player = FindFirstObjectByType<Player>();
	}

	private void OnEnable()
	{
		player.OnModeChanged += UpdateVisuals;
		UpdateVisuals(player.CurrentMode);
	}

	private void OnDisable()
	{
		player.OnModeChanged -= UpdateVisuals;
	}

	private void UpdateVisuals(PlayerMode mode)
	{
		characterFishing.SetActive(mode == PlayerMode.Fishing);
		characterOnBoat.SetActive(mode == PlayerMode.OnBoat);
	}
}

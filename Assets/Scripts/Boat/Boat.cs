using UnityEngine;

public class Boat : MonoBehaviour
{
	[Header("Boat Visuals")]
	[SerializeField] private GameObject boatDriverVisual;
	[SerializeField] private GameObject fisherVisualOnBoat;

	private Player player;

	private void Awake()
	{
		if(player == null)
		player = FindFirstObjectByType<Player>();
	}

	private void OnEnable()
	{
		player.OnModeChanged += HandleModeChanged;
	}

	private void OnDisable()
	{
		player.OnModeChanged -= HandleModeChanged;
	}

	private void HandleModeChanged(PlayerMode mode)
	{
		switch (mode)
		{
			case PlayerMode.OnBoat:
				boatDriverVisual.SetActive(true);
				fisherVisualOnBoat.SetActive(false);
				break;

			case PlayerMode.Fishing:
				boatDriverVisual.SetActive(false);
				fisherVisualOnBoat.SetActive(true);
				break;
		}
	}
}

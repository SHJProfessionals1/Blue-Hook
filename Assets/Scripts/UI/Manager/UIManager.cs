using UnityEngine;

public sealed class UIManager : MonoBehaviour
{
	public static UIManager Instance { get; private set; }

	[SerializeField] Player player;

	[Header("UI Panels")]
	[SerializeField] GameObject boatUIPanel;
	[SerializeField] GameObject fishingUIPanel;

	void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
	}

	void OnEnable()
	{
		player.OnModeChanged += UpdateUI;
	}

	void OnDisable()
	{
		player.OnModeChanged -= UpdateUI;
	}

	void UpdateUI(PlayerMode mode)
	{
		boatUIPanel.SetActive(mode == PlayerMode.OnBoat);
		fishingUIPanel.SetActive(mode == PlayerMode.Fishing);
	}
}
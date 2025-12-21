using DG.Tweening;
using UnityEngine;

public class ScreenTransitionController : MonoBehaviour
{
	[SerializeField] private CanvasGroup fadePanel;
	[SerializeField] private float fadeDuration = 0.5f;

	private Player player;
	private bool isTransitioning;

	private void Awake()
	{
		if(!player)
		player = FindAnyObjectByType<Player>();
		fadePanel.alpha = 0f;
	}

	private void OnEnable()
	{
		player.OnModeChangeRequested += StartTransition;
	}

	private void OnDisable()
	{
		player.OnModeChangeRequested -= StartTransition;
	}

	private void StartTransition(PlayerMode targetMode)
	{
		if (isTransitioning)
			return;

		isTransitioning = true;

		fadePanel
			.DOFade(1f, fadeDuration)
			.SetUpdate(true)
			.OnComplete(() =>
			{
				player.SetMode(targetMode);

				fadePanel
					.DOFade(0f, fadeDuration)
					.SetUpdate(true)
					.OnComplete(() =>
					{
						isTransitioning = false;
					});
			});
	}
}

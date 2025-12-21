using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FishingSpot : MonoBehaviour
{
	[Header("UI Image")]
	public RectTransform fishingImage;
	public Image image;

	[Header("Animation")]
	public float duration = 0.35f;
	public Vector2 hiddenOffset = new Vector2(0, -80);

	[Header("Boat Layer")]
	public LayerMask boatLayer;

	private Vector2 shownPos;
	private Vector2 hiddenPos;

	private Tween moveTween;
	private Tween fadeTween;

	private void Start()
	{
		shownPos = fishingImage.anchoredPosition;
		hiddenPos = shownPos + hiddenOffset;

		fishingImage.anchoredPosition = hiddenPos;

		Color c = image.color;
		c.a = 0;
		image.color = c;

		fishingImage.gameObject.SetActive(false);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (IsBoat(other))
			Show();
	}

	private void OnTriggerExit(Collider other)
	{
		if (IsBoat(other))
			Hide();
	}

	private bool IsBoat(Collider other)
	{
		return ((1 << other.gameObject.layer) & boatLayer) != 0;
	}

	private void Show()
	{
		fishingImage.gameObject.SetActive(true);

		moveTween?.Kill();
		fadeTween?.Kill();

		moveTween = fishingImage
			.DOAnchorPos(shownPos, duration)
			.SetEase(Ease.OutBack);

		fadeTween = image
			.DOFade(1f, duration);
	}

	private void Hide()
	{
		moveTween?.Kill();
		fadeTween?.Kill();

		moveTween = fishingImage
			.DOAnchorPos(hiddenPos, duration)
			.SetEase(Ease.InBack);

		fadeTween = image
			.DOFade(0f, duration)
			.OnComplete(() =>
			{
				fishingImage.gameObject.SetActive(false);
			});
	}
}

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public sealed class StoreCategoryButton : MonoBehaviour
{
	[Header("Data")]
	[SerializeField] private StoreCategorySO data;

	[Header("UI")]
	[SerializeField] private Image iconImage;
	[SerializeField] private Button button;

	[Header("Slide X")]
	[SerializeField] private float selectedX = 0f;
	[SerializeField] private float unselectedX = 45f;
	[SerializeField] private float slideDuration = 0.2f;
	[SerializeField] private Ease slideEase = Ease.OutCubic;
	[SerializeField] private bool instantOnInit = true;

	private StoreUIController controller;
	private RectTransform rect;
	private Tween slideTween;

	public StoreCategoryId Id => data != null ? data.id : default;

	public void Init(StoreUIController controllerRef)
	{
		controller = controllerRef;

		if (button == null)
			button = GetComponent<Button>();

		if (iconImage == null)
			iconImage = GetComponentInChildren<Image>(true);

		if (iconImage == null)
			return;

		rect = iconImage.rectTransform;

		ApplyRectRules();

		if (button != null)
		{
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(OnPressed);
		}

		SetSelected(false, instantOnInit);
	}

	private void OnDisable()
	{
		slideTween?.Kill();
		slideTween = null;
	}

	private void OnPressed()
	{
		if (controller == null || data == null) return;
		controller.SelectCategory(data.id);
	}

	public void SetSelected(bool selected)
	{
		SetSelected(selected, false);
	}

	public void SetSelected(bool selected, bool instant)
	{
		if (data == null || iconImage == null || rect == null)
			return;

		ApplyRectRules();

		var sprite = selected ? data.iconSelected : data.iconNotSelected;
		if (sprite != null)
		{
			iconImage.sprite = sprite;
			iconImage.SetNativeSize();
		}

		float targetX = selected ? selectedX : unselectedX;

		slideTween?.Kill();
		slideTween = null;

		if (instant || slideDuration <= 0f)
		{
			Vector2 p = rect.anchoredPosition;
			p.x = targetX;
			rect.anchoredPosition = p;
			return;
		}

		slideTween = rect
			.DOAnchorPosX(targetX, slideDuration)
			.SetEase(slideEase)
			.SetUpdate(true);
	}

	private void ApplyRectRules()
	{
		rect.anchorMin = new Vector2(0.5f, 0.5f);
		rect.anchorMax = new Vector2(0.5f, 0.5f);
		rect.pivot = new Vector2(0.5f, 0.5f);
	}
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public sealed class StoreItemCard : MonoBehaviour
{
	[Header("UI")]
	[SerializeField] private Image icon;
	[SerializeField] private TMP_Text title;

	[Header("Button")]
	[SerializeField] private Button actionButton;

	[Header("Price Mode")]
	[SerializeField] private GameObject priceGroup;
	[SerializeField] private TMP_Text priceText;
	[SerializeField] private Image currencyIcon;

	[Header("Currency Sprites")]
	[SerializeField] private Sprite coinsSprite;
	[SerializeField] private Sprite pearlsSprite;

	[Header("Action Mode")]
	[SerializeField] private GameObject actionGroup;
	[SerializeField] private TMP_Text actionText;

	[Header("Show Animation")]
	[SerializeField] private CanvasGroup canvasGroup;
	[SerializeField] private float showDuration = 0.22f;
	[SerializeField] private float showSlideY = -18f;
	[SerializeField] private float showStartScale = 0.96f;
	[SerializeField] private Ease showEase = Ease.OutCubic;

	[Header("Hide Animation")]
	[SerializeField] private float hideDuration = 0.18f;
	[SerializeField] private float hideSlideY = -14f;
	[SerializeField] private float hideEndScale = 0.98f;
	[SerializeField] private Ease hideEase = Ease.InCubic;

	[Header("Buy Success Press")]
	[SerializeField, Range(0.8f, 1f)] private float pressScale = 0.97f;
	[SerializeField] private float pressDuration = 0.08f;
	[SerializeField, Range(1f, 1.2f)] private float popScale = 1.02f;
	[SerializeField] private float popDuration = 0.12f;
	[SerializeField] private float settleDuration = 0.10f;
	[SerializeField] private Ease pressEase = Ease.OutCubic;
	[SerializeField] private Ease popEase = Ease.OutBack;
	[SerializeField] private Ease settleEase = Ease.OutCubic;

	[Header("Buy Fail")]
	[SerializeField] private float buyFailShakeDuration = 0.25f;
	[SerializeField] private float buyFailShakeStrength = 14f;
	[SerializeField] private int buyFailShakeVibrato = 18;

	private StoreItemSO item;
	private StoreUIController controller;

	private RectTransform rect;
	private Tween showTween;
	private Tween hideTween;
	private Tween buyTween;

	public StoreItemSO Item => item;
	public bool Matches(StoreItemSO other) => item != null && other != null && item == other;
	public float HideDuration => hideDuration;

	public void Bind(StoreItemSO data, StoreUIController owner)
	{
		item = data;
		controller = owner;

		if (rect == null) rect = transform as RectTransform;

		if (canvasGroup == null)
			canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

		if (item == null || controller == null)
			return;

		if (icon != null) icon.sprite = item.icon;
		if (title != null) title.text = item.displayName;

		if (actionButton != null)
		{
			actionButton.onClick.RemoveAllListeners();
			actionButton.onClick.AddListener(OnPressed);
		}

		Refresh();
	}

	public void Refresh()
	{
		if (item == null || controller == null || controller.Store == null)
			return;

		var store = controller.Store;

		bool purchased = store.IsPurchased(item);
		bool equipped = purchased && store.IsEquipped(item);

		if (!purchased)
		{
			if (priceGroup != null) priceGroup.SetActive(true);
			if (actionGroup != null) actionGroup.SetActive(false);

			if (priceText != null)
				priceText.text = item.price.ToString();

			ApplyCurrencyIcon(item.currency);

			if (actionButton != null)
				actionButton.interactable = true;

			return;
		}

		if (priceGroup != null) priceGroup.SetActive(false);
		if (actionGroup != null) actionGroup.SetActive(true);

		if (equipped)
		{
			if (actionText != null) actionText.text = "EQUIPPED";
			if (actionButton != null) actionButton.interactable = false;
		}
		else
		{
			if (actionText != null) actionText.text = "EQUIP";
			if (actionButton != null) actionButton.interactable = true;
		}
	}

	private void ApplyCurrencyIcon(CurrencyType currency)
	{
		if (currencyIcon == null)
			return;

		switch (currency)
		{
			case CurrencyType.Coins:
				currencyIcon.sprite = coinsSprite;
				break;
			case CurrencyType.Pearls:
				currencyIcon.sprite = pearlsSprite;
				break;
		}

		currencyIcon.enabled = currencyIcon.sprite != null;
	}

	private void OnPressed()
	{
		if (item == null || controller == null)
			return;

		var store = controller.Store;

		if (!store.IsPurchased(item))
		{
			controller.TryBuy(item);
			return;
		}

		if (!store.IsEquipped(item))
			controller.Equip(item);
	}

	public void KillAnims()
	{
		showTween?.Kill();
		hideTween?.Kill();
		buyTween?.Kill();
		showTween = null;
		hideTween = null;
		buyTween = null;

		if (rect != null)
			rect.localScale = Vector3.one;
	}

	public void PlayShowAnim(float delay)
	{
		if (rect == null) rect = transform as RectTransform;

		if (canvasGroup == null)
			canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

		showTween?.Kill();
		showTween = null;

		Vector2 basePos = rect.anchoredPosition;

		canvasGroup.alpha = 0f;
		rect.localScale = Vector3.one * showStartScale;
		rect.anchoredPosition = basePos + new Vector2(0f, showSlideY);

		var seq = DOTween.Sequence()
			.SetUpdate(true)
			.SetDelay(delay);

		seq.Join(canvasGroup.DOFade(1f, showDuration));
		seq.Join(rect.DOAnchorPos(basePos, showDuration).SetEase(showEase));
		seq.Join(rect.DOScale(1f, showDuration).SetEase(showEase));

		showTween = seq;
	}

	public Tween PlayHideAnim(float delay)
	{
		if (rect == null) rect = transform as RectTransform;

		if (canvasGroup == null)
			canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

		hideTween?.Kill();
		hideTween = null;

		Vector2 basePos = rect.anchoredPosition;

		var seq = DOTween.Sequence()
			.SetUpdate(true)
			.SetDelay(delay);

		seq.Join(canvasGroup.DOFade(0f, hideDuration));
		seq.Join(rect.DOAnchorPos(basePos + new Vector2(0f, hideSlideY), hideDuration).SetEase(hideEase));
		seq.Join(rect.DOScale(hideEndScale, hideDuration).SetEase(hideEase));

		hideTween = seq;
		return seq;
	}

	public void PlayBuySuccess()
	{
		if (rect == null) rect = transform as RectTransform;

		buyTween?.Kill();
		buyTween = null;

		const float baseScale = 1f;

		var seq = DOTween.Sequence().SetUpdate(true);
		seq.Append(rect.DOScale(baseScale * pressScale, pressDuration).SetEase(pressEase));
		seq.Append(rect.DOScale(baseScale * popScale, popDuration).SetEase(popEase));
		seq.Append(rect.DOScale(baseScale, settleDuration).SetEase(settleEase));

		buyTween = seq;
	}

	public void PlayBuyFail()
	{
		if (rect == null) rect = transform as RectTransform;

		buyTween?.Kill();
		buyTween = null;

		var seq = DOTween.Sequence().SetUpdate(true);

		seq.Append(rect.DOShakeAnchorPos(
			buyFailShakeDuration,
			buyFailShakeStrength,
			buyFailShakeVibrato,
			90f,
			false,
			true
		));

		if (actionButton != null)
			seq.Join(actionButton.transform.DOPunchScale(Vector3.one * 0.06f, 0.18f, 10, 0.9f));

		buyTween = seq;
	}

	private void OnDisable()
	{
		KillAnims();
	}
}

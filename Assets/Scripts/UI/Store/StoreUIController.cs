using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public sealed class StoreUIController : MonoBehaviour
{
	[Header("Catalog")]
	[SerializeField] private StoreCatalogSO catalog;

	[Header("Store Panel Animation")]
	[SerializeField] private CanvasGroup storeGroup;
	[SerializeField] private RectTransform storeRoot;
	[SerializeField] private bool startHidden = false;
	[SerializeField] private bool disableOnHide = true;
	[SerializeField] private bool blockRaycastsWhenHidden = true;
	[SerializeField] private float showDuration = 0.22f;
	[SerializeField] private float hideDuration = 0.18f;
	[SerializeField] private float showSlideY = -24f;
	[SerializeField] private float hideSlideY = -18f;
	[SerializeField] private float showStartScale = 0.98f;
	[SerializeField] private float hideEndScale = 0.99f;
	[SerializeField] private Ease showEase = Ease.OutCubic;
	[SerializeField] private Ease hideEase = Ease.InCubic;

	[Header("StoreEquipActionReceiver")]
	[SerializeField] private StoreEquipActionReceiver equipActionReceiver;

	[Header("Categories")]
	[SerializeField] private List<StoreCategoryButton> categoryButtons;

	[Header("Grid")]
	[SerializeField] private Transform itemGridRoot;
	[SerializeField] private StoreItemCard itemCardPrefab;

	[Header("Currency UI")]
	[SerializeField] private TMP_Text coinsText;
	[SerializeField] private TMP_Text pearlsText;
	[SerializeField] private Image coinsIcon;
	[SerializeField] private Image pearlsIcon;

	[Header("Not Enough Feedback")]
	[SerializeField] private CanvasGroup feedbackGroup;
	[SerializeField] private TMP_Text feedbackText;
	[SerializeField] private RectTransform feedbackRect;
	[SerializeField] private float feedbackFadeIn = 0.08f;
	[SerializeField] private float feedbackHold = 0.55f;
	[SerializeField] private float feedbackFadeOut = 0.18f;
	[SerializeField] private float feedbackSlideY = 10f;
	[SerializeField] private float feedbackScaleIn = 0.98f;
	[SerializeField] private Ease feedbackEaseIn = Ease.OutCubic;
	[SerializeField] private Ease feedbackEaseOut = Ease.InCubic;

	[Header("Currency Animation")]
	[SerializeField] private float currencyPunchScale = 0.14f;
	[SerializeField] private float currencyPunchDuration = 0.20f;
	[SerializeField] private float currencyShakeDuration = 0.28f;
	[SerializeField] private float currencyShakeStrength = 12f;
	[SerializeField] private int currencyShakeVibrato = 18;

	[Header("Currency Icon Animation")]
	[SerializeField] private float iconPunchScale = 0.18f;
	[SerializeField] private float iconPunchDuration = 0.22f;
	[SerializeField] private float iconShakeDuration = 0.26f;
	[SerializeField] private float iconShakeStrength = 10f;
	[SerializeField] private int iconShakeVibrato = 16;
	[SerializeField] private float iconFlashMinAlpha = 0.35f;
	[SerializeField] private float iconFlashDuration = 0.12f;

	[Header("Transition")]
	[SerializeField] private bool animateOnRebuild = true;

	[Header("Hide Stagger")]
	[SerializeField] private float hideColStagger = 0.015f;
	[SerializeField] private float hideRowStagger = 0.03f;

	[Header("Show Stagger")]
	[SerializeField] private float showColStagger = 0.03f;
	[SerializeField] private float showRowStagger = 0.06f;

	private readonly List<StoreItemCard> pooledCards = new();

	private StoreService storeService;
	private StoreCategoryId currentCategory;
	private Coroutine transitionRoutine;

	public StoreService Store => storeService;

	private GridLayoutGroup grid;
	private RectTransform gridRect;

	private Tween feedbackTween;
	private Tween coinsTween;
	private Tween pearlsTween;
	private Tween coinsIconTween;
	private Tween pearlsIconTween;

	private RectTransform coinsTextRect;
	private RectTransform pearlsTextRect;
	private RectTransform coinsIconRect;
	private RectTransform pearlsIconRect;

	private Vector2 coinsTextBasePos;
	private Vector2 pearlsTextBasePos;
	private Vector2 coinsIconBasePos;
	private Vector2 pearlsIconBasePos;

	private Vector3 coinsTextBaseScale;
	private Vector3 pearlsTextBaseScale;
	private Vector3 coinsIconBaseScale;
	private Vector3 pearlsIconBaseScale;

	private float coinsIconBaseAlpha = 1f;
	private float pearlsIconBaseAlpha = 1f;

	private Tween storeTween;
	private Vector2 storeBasePos;
	private Vector3 storeBaseScale;
	private bool storeVisible = true;

	private void Awake()
	{
		grid = itemGridRoot != null ? itemGridRoot.GetComponent<GridLayoutGroup>() : null;
		gridRect = itemGridRoot as RectTransform;

		CacheCurrencyBases();
		CacheStoreBases();

		if (equipActionReceiver == null)
			equipActionReceiver = FindFirstObjectByType<StoreEquipActionReceiver>();

		if (feedbackGroup != null)
		{
			feedbackGroup.alpha = 0f;
			feedbackGroup.gameObject.SetActive(false);
		}

		if (categoryButtons != null)
			foreach (var b in categoryButtons)
				if (b != null) b.Init(this);

		var save = SaveManager.Instance != null ? SaveManager.Instance.Current : null;
		if (save != null && save.store != null)
		{
			save.store.Initialize();
			storeService = new StoreService(save.store);
		}
		else
		{
			var fallback = new StoreSaveData();
			fallback.Initialize();
			storeService = new StoreService(fallback);
		}

		TriggerEquippedActionsFromSave();

		currentCategory = GetFirstCategoryOrDefault();
		SelectCategory(currentCategory);

		if (startHidden) ApplyHiddenInstant();
		else ApplyShownInstant();
	}

	private StoreCategoryId GetFirstCategoryOrDefault()
	{
		if (categoryButtons != null)
		{
			for (int i = 0; i < categoryButtons.Count; i++)
			{
				var b = categoryButtons[i];
				if (b != null)
					return b.Id;
			}
		}

		if (catalog != null && catalog.Categories != null && catalog.Categories.Count > 0)
			return catalog.Categories[0];

		return default;
	}

	private void CacheStoreBases()
	{
		if (storeRoot == null)
			storeRoot = transform as RectTransform;

		if (storeGroup == null)
			storeGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

		if (storeRoot != null)
		{
			storeBasePos = storeRoot.anchoredPosition;
			storeBaseScale = storeRoot.localScale;
		}
	}

	private void ApplyHiddenInstant()
	{
		storeVisible = false;

		if (disableOnHide && storeGroup != null)
			storeGroup.gameObject.SetActive(false);

		if (storeGroup != null)
		{
			storeGroup.alpha = 0f;
			storeGroup.interactable = !blockRaycastsWhenHidden;
			storeGroup.blocksRaycasts = !blockRaycastsWhenHidden;
		}

		if (storeRoot != null)
		{
			storeRoot.anchoredPosition = storeBasePos + new Vector2(0f, hideSlideY);
			storeRoot.localScale = storeBaseScale * hideEndScale;
		}
	}

	private void ApplyShownInstant()
	{
		storeVisible = true;

		if (storeGroup != null)
		{
			storeGroup.gameObject.SetActive(true);
			storeGroup.alpha = 1f;
			storeGroup.interactable = true;
			storeGroup.blocksRaycasts = true;
		}

		if (storeRoot != null)
		{
			storeRoot.anchoredPosition = storeBasePos;
			storeRoot.localScale = storeBaseScale;
		}
	}

	public void ShowStore()
	{
		if (storeVisible)
			return;

		storeVisible = true;

		storeTween?.Kill();
		storeTween = null;

		if (storeGroup == null || storeRoot == null)
		{
			ApplyShownInstant();
			return;
		}

		storeGroup.gameObject.SetActive(true);
		storeGroup.interactable = false;
		storeGroup.blocksRaycasts = false;

		storeGroup.alpha = 0f;
		storeRoot.anchoredPosition = storeBasePos + new Vector2(0f, showSlideY);
		storeRoot.localScale = storeBaseScale * showStartScale;

		var seq = DOTween.Sequence().SetUpdate(true);
		seq.Join(storeGroup.DOFade(1f, showDuration));
		seq.Join(storeRoot.DOAnchorPos(storeBasePos, showDuration).SetEase(showEase));
		seq.Join(storeRoot.DOScale(storeBaseScale, showDuration).SetEase(showEase));

		seq.OnComplete(() =>
		{
			if (storeGroup != null)
			{
				storeGroup.interactable = true;
				storeGroup.blocksRaycasts = true;
			}
		});

		storeTween = seq;
	}

	public void HideStore()
	{
		if (!storeVisible)
			return;

		storeVisible = false;

		storeTween?.Kill();
		storeTween = null;

		if (storeGroup == null || storeRoot == null)
		{
			ApplyHiddenInstant();
			return;
		}

		storeGroup.interactable = false;
		storeGroup.blocksRaycasts = false;

		var seq = DOTween.Sequence().SetUpdate(true);
		seq.Join(storeGroup.DOFade(0f, hideDuration));
		seq.Join(storeRoot.DOAnchorPos(storeBasePos + new Vector2(0f, hideSlideY), hideDuration).SetEase(hideEase));
		seq.Join(storeRoot.DOScale(storeBaseScale * hideEndScale, hideDuration).SetEase(hideEase));

		seq.OnComplete(() =>
		{
			if (storeGroup == null) return;

			storeGroup.interactable = !blockRaycastsWhenHidden;
			storeGroup.blocksRaycasts = !blockRaycastsWhenHidden;

			if (disableOnHide)
				storeGroup.gameObject.SetActive(false);

			if (storeRoot != null)
			{
				storeRoot.anchoredPosition = storeBasePos;
				storeRoot.localScale = storeBaseScale;
			}
		});

		storeTween = seq;
	}

	public void ToggleStore()
	{
		if (storeVisible) HideStore();
		else ShowStore();
	}

	private void CacheCurrencyBases()
	{
		coinsTextRect = coinsText != null ? coinsText.transform as RectTransform : null;
		pearlsTextRect = pearlsText != null ? pearlsText.transform as RectTransform : null;

		coinsIconRect = coinsIcon != null ? coinsIcon.rectTransform : null;
		pearlsIconRect = pearlsIcon != null ? pearlsIcon.rectTransform : null;

		if (coinsIcon != null) coinsIconBaseAlpha = coinsIcon.color.a;
		if (pearlsIcon != null) pearlsIconBaseAlpha = pearlsIcon.color.a;

		if (coinsTextRect != null)
		{
			coinsTextBasePos = coinsTextRect.anchoredPosition;
			coinsTextBaseScale = coinsTextRect.localScale;
		}

		if (pearlsTextRect != null)
		{
			pearlsTextBasePos = pearlsTextRect.anchoredPosition;
			pearlsTextBaseScale = pearlsTextRect.localScale;
		}

		if (coinsIconRect != null)
		{
			coinsIconBasePos = coinsIconRect.anchoredPosition;
			coinsIconBaseScale = coinsIconRect.localScale;
		}

		if (pearlsIconRect != null)
		{
			pearlsIconBasePos = pearlsIconRect.anchoredPosition;
			pearlsIconBaseScale = pearlsIconRect.localScale;
		}
	}

	private void OnEnable()
	{
		if (EconomyManager.Instance == null)
			return;

		EconomyManager.Instance.CoinsChanged += OnCoinsChanged;
		EconomyManager.Instance.PearlsChanged += OnPearlsChanged;

		OnCoinsChanged(EconomyManager.Instance.Coins);
		OnPearlsChanged(EconomyManager.Instance.Pearls);
	}

	private void OnDisable()
	{
		if (EconomyManager.Instance != null)
		{
			EconomyManager.Instance.CoinsChanged -= OnCoinsChanged;
			EconomyManager.Instance.PearlsChanged -= OnPearlsChanged;
		}

		feedbackTween?.Kill();
		coinsTween?.Kill();
		pearlsTween?.Kill();
		coinsIconTween?.Kill();
		pearlsIconTween?.Kill();
		storeTween?.Kill();
	}

	private void OnCoinsChanged(int value)
	{
		if (coinsText != null)
			coinsText.text = value.ToString();
	}

	private void OnPearlsChanged(int value)
	{
		if (pearlsText != null)
			pearlsText.text = value.ToString();
	}

	public void SelectCategory(StoreCategoryId category)
	{
		currentCategory = category;

		if (categoryButtons != null)
			foreach (var b in categoryButtons)
				if (b != null) b.SetSelected(b.Id == category);

		var filtered = GetItemsForCategory(category);

		if (!animateOnRebuild)
		{
			ApplyItemsImmediate(filtered);
			return;
		}

		if (transitionRoutine != null)
			StopCoroutine(transitionRoutine);

		transitionRoutine = StartCoroutine(TransitionTo(filtered));
	}

	private List<StoreItemSO> GetItemsForCategory(StoreCategoryId category)
	{
		if (catalog != null)
			return catalog.GetItems(category);

		return new List<StoreItemSO>();
	}

	private IEnumerator TransitionTo(List<StoreItemSO> items)
	{
		yield return HideCurrent();

		ApplyItemsImmediate(items);

		Canvas.ForceUpdateCanvases();
		if (gridRect != null)
			LayoutRebuilder.ForceRebuildLayoutImmediate(gridRect);

		ShowCurrent();

		transitionRoutine = null;
	}

	private IEnumerator HideCurrent()
	{
		int activeCount = GetActiveCount();
		if (activeCount == 0)
			yield break;

		int cols = GetColumnCount();
		if (cols < 1) cols = 1;

		float maxDelay = 0f;
		float hideDur = pooledCards[0] != null ? pooledCards[0].HideDuration : 0.18f;

		for (int i = 0; i < activeCount; i++)
		{
			var card = pooledCards[i];
			if (card == null || !card.gameObject.activeSelf) continue;

			int col = i % cols;
			int row = i / cols;

			float delay = (col * hideColStagger) + (row * hideRowStagger);
			if (delay > maxDelay) maxDelay = delay;

			card.PlayHideAnim(delay);
		}

		yield return new WaitForSecondsRealtime(maxDelay + hideDur);
	}

	private void ShowCurrent()
	{
		int activeCount = GetActiveCount();
		if (activeCount == 0)
			return;

		int cols = GetColumnCount();
		if (cols < 1) cols = 1;

		for (int i = 0; i < activeCount; i++)
		{
			var card = pooledCards[i];
			if (card == null || !card.gameObject.activeSelf) continue;

			int col = i % cols;
			int row = i / cols;

			float delay = (col * showColStagger) + (row * showRowStagger);
			card.PlayShowAnim(delay);
		}
	}

	private void ApplyItemsImmediate(List<StoreItemSO> items)
	{
		if (itemGridRoot == null || itemCardPrefab == null)
			return;

		int needed = items != null ? items.Count : 0;

		while (pooledCards.Count < needed)
		{
			var card = Instantiate(itemCardPrefab, itemGridRoot);
			pooledCards.Add(card);
		}

		for (int i = 0; i < pooledCards.Count; i++)
		{
			var card = pooledCards[i];
			if (card == null) continue;

			bool active = i < needed;
			if (card.gameObject.activeSelf != active)
				card.gameObject.SetActive(active);

			if (!active)
				continue;

			card.Bind(items[i], this);
		}
	}

	private int GetActiveCount()
	{
		int count = 0;
		for (int i = 0; i < pooledCards.Count; i++)
		{
			var c = pooledCards[i];
			if (c != null && c.gameObject.activeSelf)
				count++;
			else
				break;
		}
		return count;
	}

	private int GetColumnCount()
	{
		if (grid == null || gridRect == null)
			return 1;

		if (grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
			return Mathf.Max(1, grid.constraintCount);

		float width = gridRect.rect.width - grid.padding.left - grid.padding.right;
		float cell = grid.cellSize.x;
		float spacing = grid.spacing.x;

		if (cell <= 0f)
			return 1;

		int cols = Mathf.FloorToInt((width + spacing) / (cell + spacing));
		return Mathf.Max(1, cols);
	}

	public void TryBuy(StoreItemSO item)
	{
		if (item == null || storeService == null)
			return;

		var card = FindCard(item);
		bool success = storeService.Buy(item);

		AnimateBuyCurrency(item.currency);

		if (success)
		{
			EquipInternal(item);

			if (card != null)
			{
				card.PlayBuySuccess();
				card.PlayEquipAnim();
			}

			RefreshAllCards();
		}
		else
		{
			if (card != null) card.PlayBuyFail();
			ShowFeedback(item.currency == CurrencyType.Coins ? "NOT ENOUGH COINS" : "NOT ENOUGH PEARLS");
		}
	}

	public void Equip(StoreItemSO item)
	{
		if (item == null || storeService == null)
			return;

		EquipInternal(item);

		var card = FindCard(item);
		if (card != null)
			card.PlayEquipAnim();

		RefreshAllCards();
	}

	private void EquipInternal(StoreItemSO item)
	{
		storeService.Equip(item);
		InvokeEquipAction(item);
	}

	private void InvokeEquipAction(StoreItemSO item)
	{
		if (equipActionReceiver == null)
			equipActionReceiver = FindFirstObjectByType<StoreEquipActionReceiver>();

		if (equipActionReceiver == null)
			return;

		equipActionReceiver.TryInvoke(item);
	}

	private void AnimateBuyCurrency(CurrencyType currency)
	{
		KillAndResetCurrencyAnims(currency);
		AnimateCurrency(currency);
		AnimateCurrencyIcon(currency);
	}

	private void KillAndResetCurrencyAnims(CurrencyType currency)
	{
		if (currency == CurrencyType.Coins)
		{
			coinsTween?.Kill();
			coinsIconTween?.Kill();
			coinsTween = null;
			coinsIconTween = null;

			if (coinsTextRect != null)
			{
				coinsTextRect.DOKill();
				coinsTextRect.anchoredPosition = coinsTextBasePos;
				coinsTextRect.localScale = coinsTextBaseScale;
			}

			if (coinsIconRect != null)
			{
				coinsIconRect.DOKill();
				coinsIconRect.anchoredPosition = coinsIconBasePos;
				coinsIconRect.localScale = coinsIconBaseScale;
			}

			if (coinsIcon != null)
				coinsIcon.color = new Color(coinsIcon.color.r, coinsIcon.color.g, coinsIcon.color.b, coinsIconBaseAlpha);

			return;
		}

		pearlsTween?.Kill();
		pearlsIconTween?.Kill();
		pearlsTween = null;
		pearlsIconTween = null;

		if (pearlsTextRect != null)
		{
			pearlsTextRect.DOKill();
			pearlsTextRect.anchoredPosition = pearlsTextBasePos;
			pearlsTextRect.localScale = pearlsTextBaseScale;
		}

		if (pearlsIconRect != null)
		{
			pearlsIconRect.DOKill();
			pearlsIconRect.anchoredPosition = pearlsIconBasePos;
			pearlsIconRect.localScale = pearlsIconBaseScale;
		}

		if (pearlsIcon != null)
			pearlsIcon.color = new Color(pearlsIcon.color.r, pearlsIcon.color.g, pearlsIcon.color.b, pearlsIconBaseAlpha);
	}

	private void AnimateCurrency(CurrencyType currency)
	{
		var rt = currency == CurrencyType.Coins ? coinsTextRect : pearlsTextRect;
		if (rt == null) return;

		var seq = DOTween.Sequence().SetUpdate(true);
		seq.Join(rt.DOPunchScale(Vector3.one * currencyPunchScale, currencyPunchDuration, 10, 0.9f));
		seq.Join(rt.DOShakeAnchorPos(currencyShakeDuration, currencyShakeStrength, currencyShakeVibrato, 90f, false, true));

		if (currency == CurrencyType.Coins) coinsTween = seq;
		else pearlsTween = seq;
	}

	private void AnimateCurrencyIcon(CurrencyType currency)
	{
		var img = currency == CurrencyType.Coins ? coinsIcon : pearlsIcon;
		var rt = currency == CurrencyType.Coins ? coinsIconRect : pearlsIconRect;

		if (img == null || rt == null) return;

		float baseAlpha = currency == CurrencyType.Coins ? coinsIconBaseAlpha : pearlsIconBaseAlpha;

		var seq = DOTween.Sequence().SetUpdate(true);

		seq.Join(rt.DOPunchScale(Vector3.one * iconPunchScale, iconPunchDuration, 10, 0.9f));
		seq.Join(rt.DOShakeAnchorPos(iconShakeDuration, iconShakeStrength, iconShakeVibrato, 90f, false, true));
		seq.Join(img.DOFade(Mathf.Clamp01(iconFlashMinAlpha), iconFlashDuration).SetLoops(2, LoopType.Yoyo));

		seq.OnComplete(() =>
		{
			if (img != null)
				img.color = new Color(img.color.r, img.color.g, img.color.b, baseAlpha);
		});

		if (currency == CurrencyType.Coins) coinsIconTween = seq;
		else pearlsIconTween = seq;
	}

	private void ShowFeedback(string msg)
	{
		if (feedbackGroup == null || feedbackText == null)
			return;

		feedbackTween?.Kill();

		feedbackText.text = msg;

		if (feedbackRect == null)
			feedbackRect = feedbackGroup.transform as RectTransform;

		Vector2 basePos = feedbackRect != null ? feedbackRect.anchoredPosition : Vector2.zero;
		Vector2 startPos = basePos + new Vector2(0f, -feedbackSlideY);

		feedbackGroup.gameObject.SetActive(true);
		feedbackGroup.alpha = 0f;

		if (feedbackRect != null)
		{
			feedbackRect.anchoredPosition = startPos;
			feedbackRect.localScale = Vector3.one * feedbackScaleIn;
		}

		var seq = DOTween.Sequence().SetUpdate(true);

		seq.Append(feedbackGroup.DOFade(1f, feedbackFadeIn));

		if (feedbackRect != null)
		{
			seq.Join(feedbackRect.DOAnchorPos(basePos, feedbackFadeIn).SetEase(feedbackEaseIn));
			seq.Join(feedbackRect.DOScale(1f, feedbackFadeIn).SetEase(feedbackEaseIn));
		}

		seq.AppendInterval(feedbackHold);

		seq.Append(feedbackGroup.DOFade(0f, feedbackFadeOut).SetEase(feedbackEaseOut));

		if (feedbackRect != null)
		{
			seq.Join(feedbackRect.DOAnchorPos(basePos + new Vector2(0f, feedbackSlideY), feedbackFadeOut).SetEase(feedbackEaseOut));
			seq.Join(feedbackRect.DOScale(0.99f, feedbackFadeOut).SetEase(feedbackEaseOut));
		}

		seq.OnComplete(() =>
		{
			if (feedbackGroup != null)
				feedbackGroup.gameObject.SetActive(false);

			if (feedbackRect != null)
			{
				feedbackRect.anchoredPosition = basePos;
				feedbackRect.localScale = Vector3.one;
			}
		});

		feedbackTween = seq;
	}

	private StoreItemCard FindCard(StoreItemSO item)
	{
		for (int i = 0; i < pooledCards.Count; i++)
		{
			var c = pooledCards[i];
			if (c != null && c.gameObject.activeInHierarchy && c.Matches(item))
				return c;
		}
		return null;
	}

	private void RefreshAllCards()
	{
		for (int i = 0; i < pooledCards.Count; i++)
		{
			var c = pooledCards[i];
			if (c != null && c.gameObject.activeInHierarchy)
				c.Refresh();
		}
	}

	private void TriggerEquippedActionsFromSave()
	{
		if (storeService == null || catalog == null)
			return;

		if (equipActionReceiver == null)
			equipActionReceiver = FindFirstObjectByType<StoreEquipActionReceiver>();

		if (equipActionReceiver == null)
			return;

		var cats = catalog.Categories;
		if (cats == null)
			return;

		for (int c = 0; c < cats.Count; c++)
		{
			var category = cats[c];
			var items = catalog.GetItems(category);
			if (items == null)
				continue;

			for (int i = 0; i < items.Count; i++)
			{
				var item = items[i];
				if (item == null)
					continue;

				if (storeService.IsEquipped(item))
				{
					equipActionReceiver.TryInvoke(item);
					break;
				}
			}
		}
	}
}

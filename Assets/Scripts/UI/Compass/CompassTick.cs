using UnityEngine;
using TMPro;
using UnityEngine.UI;

public sealed class CompassTick : MonoBehaviour
{
	public RectTransform Rect => rect;

	RectTransform rect;
	CanvasGroup canvasGroup;

	[SerializeField] Image line;
	[SerializeField] TMP_Text label;

	[Header("Tick Heights")]
	[SerializeField] float minorTickHeight = 16f;
	[SerializeField] float majorTickHeight = 28f;
	[SerializeField] float cardinalTickHeight = 40f;

	[Header("Label Font Sizes")]
	[SerializeField] float minorFontSize = 16f;
	[SerializeField] float majorFontSize = 16f;
	[SerializeField] float cardinalFontSize = 20f;

	void Awake()
	{
		rect = (RectTransform)transform;
		if (!TryGetComponent(out canvasGroup))
			canvasGroup = gameObject.AddComponent<CanvasGroup>();

		rect.anchorMin = new Vector2(0.5f, 0f);
		rect.anchorMax = new Vector2(0.5f, 0f);
		rect.pivot = new Vector2(0.5f, 0f);

		if (rect.rect.width <= 0f)
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 36f);

		label.gameObject.SetActive(true);
	}

	public void SetPosition(float x)
	{
		rect.anchoredPosition = new Vector2(x, 0f);
	}

	public void SetAngle(int angle)
	{
		angle = Mathf.RoundToInt(Mathf.Repeat(angle, 360f));

		bool isCardinal = angle % 90 == 0;
		bool isMajor = angle % 45 == 0;

		float height =
			isCardinal ? cardinalTickHeight :
			isMajor ? majorTickHeight :
			minorTickHeight;

		var rt = line.rectTransform;
		rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);

		float fontSize =
			isCardinal ? cardinalFontSize :
			isMajor ? majorFontSize :
			minorFontSize;

		label.fontSize = fontSize;

		if (isCardinal)
		{
			switch (angle)
			{
				case 0: label.text = "N"; break;
				case 90: label.text = "E"; break;
				case 180: label.text = "S"; break;
				case 270: label.text = "W"; break;
				default: label.text = angle.ToString(); break;
			}
		}
		else
		{
			label.text = angle.ToString();
		}
	}

	public void SetAlpha(float alpha)
	{
		canvasGroup.alpha = Mathf.Clamp01(alpha);
	}
}

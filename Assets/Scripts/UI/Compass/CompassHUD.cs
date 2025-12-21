using UnityEngine;
using TMPro;

public sealed class CompassHUD : MonoBehaviour
{
	[Header("References")]
	[SerializeField] Transform headingSource;
	[SerializeField] RectTransform scaleContainer;
	[SerializeField] RectTransform viewport;
	[SerializeField] CompassTick tickPrefab;
	[SerializeField] TMP_Text headingText;

	[Header("Scale Settings")]
	[SerializeField] int tickCount = 21;
	[SerializeField] int degreesPerTick = 15;

	[Header("Center Fade Zone")]
	[SerializeField] float fadeStart = 70f;
	[SerializeField] float fadeEnd = 30f;

	CompassTick[] ticks;
	float heading;
	float tickSpacing;

	void Awake()
	{
		if (!headingSource && Camera.main)
			headingSource = Camera.main.transform;

		ticks = new CompassTick[tickCount];

		ticks[0] = Instantiate(tickPrefab, scaleContainer);

		for (int i = 1; i < tickCount; i++)
			ticks[i] = Instantiate(tickPrefab, scaleContainer);
	}

	void Start()
	{
		Canvas.ForceUpdateCanvases();

		tickSpacing = ticks[0].Rect.rect.width;
		if (tickSpacing <= 0f)
			tickSpacing = 36f;

		if (headingSource)
			heading = headingSource.eulerAngles.y;
		else
			heading = 0f;

		UpdateTicks();
	}

	void Update()
	{
		if (!headingSource) return;

		heading = headingSource.eulerAngles.y;
		headingText.text = Mathf.RoundToInt(heading).ToString() + "°";

		UpdateTicks();
	}

	void UpdateTicks()
	{
		int centerIndex = tickCount / 2;
		float halfWidth = viewport.rect.width * 0.5f;

		float baseAngle = Mathf.Floor(heading / degreesPerTick) * degreesPerTick;
		float scroll = (heading - baseAngle) / degreesPerTick;

		for (int i = 0; i < tickCount; i++)
		{
			int offsetIndex = i - centerIndex;

			float tickAngle = Mathf.Repeat(baseAngle + offsetIndex * degreesPerTick, 360f);

			float x = (offsetIndex - scroll) * tickSpacing;
			bool visible = Mathf.Abs(x) <= halfWidth + tickSpacing * 0.5f;

			ticks[i].gameObject.SetActive(visible);
			if (!visible) continue;

			ticks[i].SetPosition(x);
			ticks[i].SetAngle((int)tickAngle);

			float absX = Mathf.Abs(x);
			float alpha = Mathf.InverseLerp(fadeEnd, fadeStart, absX);
			ticks[i].SetAlpha(alpha);
		}
	}
}

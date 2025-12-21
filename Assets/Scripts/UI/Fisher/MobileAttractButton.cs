using UnityEngine;
using UnityEngine.EventSystems;
using FishingGameTool.Fishing;

public class MobileAttractButton : MonoBehaviour,
	IPointerDownHandler,
	IPointerUpHandler,
	IPointerExitHandler
{
	[SerializeField] private FishingSystem fishingSystem;

	private bool isHeld;

	private void Awake()
	{
		if (!fishingSystem)
			fishingSystem = FindAnyObjectByType<FishingSystem>();
	}

	private void OnDisable()
	{
		isHeld = false;
		if (fishingSystem) fishingSystem.SetAttractInput(false);
	}

	private void Update()
	{
		if (!fishingSystem) return;

		if (!fishingSystem.IsFishing)
		{
			if (isHeld)
			{
				isHeld = false;
				fishingSystem.SetAttractInput(false);
			}
			return;
		}

		fishingSystem.SetAttractInput(isHeld);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (!fishingSystem) return;
		if (!fishingSystem.IsFishing) return;

		isHeld = true;
		fishingSystem.SetAttractInput(true);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (!fishingSystem) return;

		isHeld = false;
		fishingSystem.SetAttractInput(false);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!fishingSystem) return;

		isHeld = false;
		fishingSystem.SetAttractInput(false);
	}

	public void OnPointerCancel(PointerEventData eventData)
	{
		if (!fishingSystem) return;

		isHeld = false;
		fishingSystem.SetAttractInput(false);
	}
}

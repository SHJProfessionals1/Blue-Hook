using UnityEngine;
using UnityEngine.EventSystems;
using FishingGameTool.Fishing;

public class MobileCastButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	[SerializeField] private FishingSystem fishingSystem;

	private void Awake()
	{
		if (!fishingSystem)
			fishingSystem = FindAnyObjectByType<FishingSystem>();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (!fishingSystem) return;
		if (fishingSystem.IsFishing) return;

		fishingSystem.SetCastInput(true);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (!fishingSystem) return;

		fishingSystem.SetCastInput(false);
	}
}
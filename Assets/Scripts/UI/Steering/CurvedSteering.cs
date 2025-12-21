using UnityEngine;
using UnityEngine.EventSystems;

public sealed class CurvedSteering : MonoBehaviour,
	IDragHandler,
	IPointerDownHandler,
	IPointerUpHandler
{
	[SerializeField] RectTransform knob;

	[SerializeField] float width = 500f;
	[SerializeField] float curveStrength = 25f;
	[SerializeField] Vector2 pathOffset = new Vector2(0f, 25f);

	[SerializeField] float deadZone = 0.1f;
	[SerializeField] float returnSmoothTime = 0.15f;

	bool isDragging;
	float steering;
	float steeringVelocity;

	public float Steering => steering;

	void Start()
	{
		SetKnobFromSteering(0f);
	}

	void Update()
	{
		if (isDragging)
			return;

		if (Mathf.Abs(steering) < 0.001f)
		{
			steering = 0f;
			return;
		}

		steering = Mathf.SmoothDamp(
			steering,
			0f,
			ref steeringVelocity,
			returnSmoothTime
		);

		SetKnobFromSteering(steering);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (!RectTransformUtility.RectangleContainsScreenPoint(
			knob,
			eventData.position,
			eventData.pressEventCamera))
			return;

		isDragging = true;
		steeringVelocity = 0f;
		UpdateKnob(eventData);
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (!isDragging)
			return;

		UpdateKnob(eventData);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		isDragging = false;
	}

	void UpdateKnob(PointerEventData eventData)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			transform as RectTransform,
			eventData.position,
			eventData.pressEventCamera,
			out Vector2 localPos
		);

		float halfWidth = width * 0.5f;
		float x = Mathf.Clamp(localPos.x, -halfWidth, halfWidth);

		float t = Mathf.InverseLerp(-halfWidth, halfWidth, x);
		float rawSteer = Mathf.Lerp(-1f, 1f, t);

		if (Mathf.Abs(rawSteer) < deadZone)
			steering = 0f;
		else
			steering = Mathf.Sign(rawSteer) *
				Mathf.InverseLerp(deadZone, 1f, Mathf.Abs(rawSteer));

		SetKnobFromSteering(steering);
	}

	void SetKnobFromSteering(float value)
	{
		float x = Mathf.Lerp(
			-width * 0.5f,
			width * 0.5f,
			Mathf.InverseLerp(-1f, 1f, value)
		);

		float y = -Mathf.Pow(value, 2f) * curveStrength;

		knob.anchoredPosition = new Vector2(x, y) + pathOffset;
	}
}

using UnityEngine;
using DG.Tweening;

public sealed class MinimapController : MonoBehaviour
{
	[SerializeField] CanvasGroup circleGroup;
	[SerializeField] CanvasGroup squareGroup;
	[SerializeField] RectTransform border;

	[SerializeField] Camera minimapCamera;
	[SerializeField] BoatController boatController;

	[SerializeField] float circleIdleSize = 25f;
	[SerializeField] float circleMaxSpeedSize = 40f;
	[SerializeField] float squareSize = 60f;

	[SerializeField] float zoomSmooth = 6f;
	[SerializeField] float rotationSmooth = 10f;

	[SerializeField] float fadeDuration = 0.25f;
	[SerializeField] Ease fadeEase = Ease.OutQuad;

	bool showingSquare;
	float currentYaw;

	void Awake()
	{
		ShowCircleInstant();
	}

	void Update()
	{
		UpdateCameraFollow();
		UpdateZoom();
		UpdateRotation();
	}

	void UpdateCameraFollow()
	{
		if (!minimapCamera || !boatController)
			return;

		Vector3 pos = minimapCamera.transform.position;
		pos.x = boatController.transform.position.x;
		pos.z = boatController.transform.position.z;

		minimapCamera.transform.position = pos;
	}

	void UpdateRotation()
	{
		if (showingSquare || !boatController)
			return;

		float targetYaw = boatController.transform.eulerAngles.y;

		currentYaw = Mathf.LerpAngle(
			currentYaw,
			targetYaw,
			rotationSmooth * Time.deltaTime
		);

		border.localRotation = Quaternion.Euler(0f, 0f, currentYaw);
	}

	void UpdateZoom()
	{
		if (!minimapCamera || !boatController)
			return;

		float targetSize;

		if (showingSquare)
		{
			targetSize = squareSize;
		}
		else
		{
			float speed = boatController.Rigidbody.linearVelocity.magnitude;

			float speed01 = Mathf.Clamp01(
				speed / boatController.MaxSpeed
			);

			targetSize = Mathf.Lerp(
				circleIdleSize,
				circleMaxSpeedSize,
				speed01
			);
		}

		minimapCamera.orthographicSize = Mathf.Lerp(
			minimapCamera.orthographicSize,
			targetSize,
			zoomSmooth * Time.deltaTime
		);
	}

	public void ShowSquare()
	{
		if (showingSquare)
			return;

		showingSquare = true;

		circleGroup
			.DOFade(0f, fadeDuration)
			.SetEase(fadeEase)
			.OnComplete(() =>
			{
				circleGroup.interactable = false;
				circleGroup.blocksRaycasts = false;
			});

		squareGroup.gameObject.SetActive(true);
		squareGroup.alpha = 0f;
		squareGroup.interactable = false;
		squareGroup.blocksRaycasts = false;

		squareGroup
			.DOFade(1f, fadeDuration)
			.SetEase(fadeEase)
			.OnComplete(() =>
			{
				squareGroup.interactable = true;
				squareGroup.blocksRaycasts = true;
			});
	}

	public void ShowCircle()
	{
		if (!showingSquare)
			return;

		showingSquare = false;

		squareGroup
			.DOFade(0f, fadeDuration)
			.SetEase(fadeEase)
			.OnComplete(() =>
			{
				squareGroup.interactable = false;
				squareGroup.blocksRaycasts = false;
			});

		circleGroup.gameObject.SetActive(true);
		circleGroup.alpha = 0f;
		circleGroup.interactable = false;
		circleGroup.blocksRaycasts = false;

		circleGroup
			.DOFade(1f, fadeDuration)
			.SetEase(fadeEase)
			.OnComplete(() =>
			{
				circleGroup.interactable = true;
				circleGroup.blocksRaycasts = true;
			});
	}

	void ShowCircleInstant()
	{
		showingSquare = false;

		circleGroup.alpha = 1f;
		circleGroup.interactable = true;
		circleGroup.blocksRaycasts = true;

		squareGroup.alpha = 0f;
		squareGroup.interactable = false;
		squareGroup.blocksRaycasts = false;
	}
}

using UnityEngine;

public class StoreSpot : MonoBehaviour
{
	[SerializeField] StoreUIController storeUIController;
	[SerializeField] LayerMask boatLayer;

	private void OnTriggerEnter(Collider other)
	{
		if (!IsBoat(other))
			return;

		var input = other.GetComponentInParent<BoatInput>();
		if (input)
			input.SmoothStop();

		storeUIController.ShowStore();
	}

	private void OnTriggerExit(Collider other)
	{
		if (!IsBoat(other))
			return;

		storeUIController.HideStore();
	}

	bool IsBoat(Collider other)
	{
		return ((1 << other.gameObject.layer) & boatLayer) != 0;
	}
}

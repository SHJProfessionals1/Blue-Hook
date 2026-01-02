using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class BoatSwapper : MonoBehaviour
{
	[SerializeField] private Transform fishingCharacter;
	[SerializeField] private List<AssetReferenceGameObject> boats = new();
	[SerializeField] private Transform boatParent;

	public event Action OnBoatUnloading;
	public event Action<BoatSwapper> OnBoatEquipped;

	private AsyncOperationHandle<GameObject> currentBoatHandle;
	private GameObject currentBoatInstance;
	private bool isLoading;

	public GameObject CurrentBoatInstance => currentBoatInstance;

	private void Start()
	{
		if (boats.Count > 0) EquipBoat(boats[0]);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.K) && boats.Count > 0) EquipBoat(boats[0]);
		if (Input.GetKeyDown(KeyCode.L) && boats.Count > 1) EquipBoat(boats[1]);
	}

	public void EquipBoat(AssetReferenceGameObject boatPrefab)
	{
		if (isLoading || boatPrefab == null) return;
		StartCoroutine(SwapRoutine(boatPrefab));
	}

	public void EquipBoatFromStoreItem(StoreItemSO item)
	{
		if (item == null || item.assetPrefab == null)
			return;

		EquipBoat(item.assetPrefab);
	}

	private IEnumerator SwapRoutine(AssetReferenceGameObject newBoatPrefab)
	{
		isLoading = true;

		OnBoatUnloading?.Invoke();

		if (fishingCharacter != null)
		{
			fishingCharacter.SetParent(transform);
			fishingCharacter.gameObject.SetActive(false);
		}

		var oldHandle = currentBoatHandle;
		var oldInstance = currentBoatInstance;

		var op = newBoatPrefab.InstantiateAsync(boatParent);
		yield return op;

		if (op.Status != AsyncOperationStatus.Succeeded)
		{
			Debug.LogError("Boat load failed.");
			if (fishingCharacter != null) fishingCharacter.gameObject.SetActive(true);
			isLoading = false;
			yield break;
		}

		currentBoatHandle = op;
		currentBoatInstance = op.Result;

		DisableAllCameras(oldInstance);
		EnableFirstCamera(currentBoatInstance);

		var wave = boatParent ? boatParent.GetComponent<BoatWaveVisual>() : null;
		if (wave != null)
		{
			var cam = currentBoatInstance.GetComponentInChildren<Camera>(true);
			wave.SetCamera(cam ? cam.transform : (Camera.main ? Camera.main.transform : null));
		}

		if (fishingCharacter != null)
		{
			var mount = currentBoatInstance.GetComponent<BoatCharacterMount>();

			fishingCharacter.SetParent(currentBoatInstance.transform);

			if (mount != null)
			{
				fishingCharacter.localPosition = mount.localPosition;
				fishingCharacter.localRotation = Quaternion.Euler(mount.localEulerRotation);
			}
			else
			{
				fishingCharacter.localPosition = Vector3.zero;
				fishingCharacter.localRotation = Quaternion.identity;
			}

			fishingCharacter.gameObject.SetActive(true);
		}

		if (oldInstance != null)
			Addressables.ReleaseInstance(oldHandle);

		OnBoatEquipped?.Invoke(this);

		isLoading = false;
	}

	private static void DisableAllCameras(GameObject root)
	{
		if (root == null) return;

		var cams = root.GetComponentsInChildren<Camera>(true);
		for (int i = 0; i < cams.Length; i++)
			cams[i].enabled = false;

		var listeners = root.GetComponentsInChildren<AudioListener>(true);
		for (int i = 0; i < listeners.Length; i++)
			listeners[i].enabled = false;
	}

	private static void EnableFirstCamera(GameObject root)
	{
		if (root == null) return;

		var cam = root.GetComponentInChildren<Camera>(true);
		if (cam != null) cam.enabled = true;

		var listener = root.GetComponentInChildren<AudioListener>(true);
		if (listener != null) listener.enabled = true;
	}
}

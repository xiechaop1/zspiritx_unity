using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using HuaweiARUnityAdapter;

[RequireComponent(typeof(ARTrackedImageManager), typeof(WorldARController))]
public class ARTaggedImageManager : MonoBehaviour, IManager {

	ARTrackedImageManager TrackedImgManager;
	WorldARController HWWorlfController;
	Camera MainCamera;
	List<GameObject> lstTrackedImage = new List<GameObject>();

	public FieldEntityManager entityManager;
	public FieldStageManager stageManager;
	public void Init(UIEventManager eventManager, params IManager[] managers) {

	}
	public void RegisterManager(IManager manager) {
		if (manager is FieldEntityManager) {
			entityManager = manager as FieldEntityManager;
		} else if (manager is FieldStageManager) {
			stageManager = manager as FieldStageManager;
		}
	}
	void Awake() {
		TrackedImgManager = GetComponent<ARTrackedImageManager>();
		HWWorlfController = GetComponent<WorldARController>();
		MainCamera = GetComponent<Unity.XR.CoreUtils.XROrigin>().Camera;
	}

	void OnEnable() {
		TrackedImgManager.trackedImagesChanged += OnTrackedImagesChanged;
		HWWorlfController.trackedImagesChanged += OnTrackedImagesChanged;
	}

	void OnDisable() {
		TrackedImgManager.trackedImagesChanged -= OnTrackedImagesChanged;
		HWWorlfController.trackedImagesChanged -= OnTrackedImagesChanged;
	}
	void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs) {
		foreach (ARTrackedImage trackedImage in eventArgs.added) {
			lstTrackedImage.Add(trackedImage.gameObject);
			RefreshImageRecongition(trackedImage);
		}
		foreach (ARTrackedImage trackedImage in eventArgs.updated) {
			RefreshImageRecongition(trackedImage);
		}
		foreach (ARTrackedImage trackedImage in eventArgs.removed) {
			try {
				lstTrackedImage.Remove(trackedImage.gameObject);
			} catch (System.Exception) {
			}
		}
	}

	void RefreshImageRecongition(ARTrackedImage trackedImage) {
		stageManager.ImageFound(trackedImage.referenceImage.name);
		trackedImage.transform.localScale = Vector3.one;
		entityManager.PlaceImageTrackingEntity(trackedImage.referenceImage.name, trackedImage.gameObject);
	}

	void OnTrackedImagesChanged(HWTrackedImagesChangedEventArgs eventArgs) {
		foreach (HWTrackedImage trackedImage in eventArgs.added) {
			lstTrackedImage.Add(trackedImage.gameObject);
			RefreshImageRecongition(trackedImage);
		}
		foreach (HWTrackedImage trackedImage in eventArgs.updated) {
			RefreshImageRecongition(trackedImage);
		}
		foreach (HWTrackedImage trackedImage in eventArgs.removed) {
			try {
				lstTrackedImage.Remove(trackedImage.gameObject);
			} catch (System.Exception) {
			}
		}
	}

	void RefreshImageRecongition(HWTrackedImage trackedImage) {
		string name = trackedImage.referenceImage.AcquireName().Split('.')[0];
		stageManager.ImageFound(name);
		trackedImage.gameObject.transform.localScale = Vector3.one;
		entityManager.PlaceImageTrackingEntity(name, trackedImage.gameObject);
	}
}

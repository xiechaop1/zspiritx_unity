using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ARTaggedImageManager : MonoBehaviour, IManager {

	ARTrackedImageManager TrackedImgManager;

	//Dictionary<string, GameObject> bookImgName2Prefab = new Dictionary<string, GameObject>();
	public FieldEntityManager entityManager;
	public FieldStageManager stageManager;
	//public GameObject prefabPhone;
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
		//bookImgName2Prefab.Add("phone", prefabPhone);
	}

	void OnEnable() {
		TrackedImgManager.trackedImagesChanged += OnTrackedImagesChanged;
	}

	void OnDisable() {
		TrackedImgManager.trackedImagesChanged -= OnTrackedImagesChanged;
	}
	void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs) {
		//FieldEntityInfo info;
		foreach (ARTrackedImage trackedImage in eventArgs.added) {
			stageManager.ImageFound(trackedImage.referenceImage.name);

			// Give the initial image a reasonable default scale
			trackedImage.transform.localScale = Vector3.one;
			GameObject obj = entityManager.PlaceImageTrackingEntity(trackedImage.referenceImage.name);
			if (obj != null) {
				obj.transform.parent = trackedImage.transform;
				obj.transform.localPosition = Vector3.zero;
				obj.transform.localRotation = Quaternion.identity;
			}
			//GameObject prefab = GetPrefab(trackedImage);
			//if (prefab != null) {
			//	var obj = Instantiate(prefab, trackedImage.transform);
			//	if (obj.TryGetComponent(out info)) {
			//		info.actionManager = entityManager;
			//	}
			//}
			//UpdateInfo(trackedImage);
		}

		//foreach (ARTrackedImage trackedImage in eventArgs.updated) 
		//UpdateInfo(trackedImage);
	}
	//GameObject GetPrefab(ARTrackedImage trackedImage) {
	//	GameObject prefab = null;
	//	string name = trackedImage.referenceImage.name;
	//	bookImgName2Prefab.TryGetValue(name, out prefab);
	//	return prefab;
	//}
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(MeshRenderer))]
public class ARPlaneInfo : MonoBehaviour, IEventMessage {
	public Material matOnLoad;
	public Material matOnPlay;

	public Vector3 up {// => gameObject.transform.up;
		get {
			if (arEngineType == 1) {
				return gameObject.transform.up;
			} else if (arEngineType == 1) {
				return  hwPlane.planeNormal;
			}
			return Vector3.up;
		}
	}
	ARPlane arPlane;
	HuaweiARUnityAdapter.TrackedPlaneVisualizer hwPlane;
	//#if !UNITY_EDITOR
	//	public Vector3 center => arPlane.center;
	//	public Vector2 size => arPlane.size;
	//#else
	//	public Vector3 center => gameObject.transform.position;
	//	public Vector2 size = new Vector2(10f, 10f);
	//#endif

	//public float radiusAverage => (size.x + size.y) * 0.5f;
	//public float area => size.x * size.y;

	public bool isHorizontal => Vector3.Angle(Vector3.up, up) < 0.1f;

	public int arEngineType = 0;
	public Action<ARPlaneInfo> planeUpdated;

	//public void Awake() {

	//	if (!TryGetComponent(out arPlane)) {
	//		Debug.LogWarning("missing arPlane script");
	//	}

	//}
	public void Start() {
#if UNITY_EDITOR
		if (TryGetComponent(out arPlane)) {
			arPlane.boundaryChanged += OnPlaneUpdate;
			arEngineType = 1;
		} else if (TryGetComponent(out hwPlane)) {
			hwPlane.boundaryChanged += OnPlaneUpdate;
			arEngineType = 2;
		} else {
			Debug.LogWarning("missing arPlane script");
		}
#endif
		UIEventManager.CallEvent("FieldEntityManager", "RegisterARPlane", this);

		//FieldEntityManager.getInstance().RegisterARPlane(this);
	}
	public void OnDestroy() {
		UIEventManager.CallEvent("FieldEntityManager", "RemoveARPlane", this);
		//FieldEntityManager.getInstance().RemoveARPlane(this);
	}
#if UNITY_EDITOR
	public void OnEnable() {
		planeUpdated?.Invoke(this);
	}

	public void OnDisable() {
		planeUpdated?.Invoke(this);
	}
#endif

	public void SetPlaneVisibility(bool isVisible) {
		GetComponent<MeshRenderer>().material = isVisible ? matOnLoad : matOnPlay;
	}

	public void OnPlaneUpdate(ARPlaneBoundaryChangedEventArgs args) {
		OnPlaneUpdate();
	}
	public void OnPlaneUpdate() {
		planeUpdated?.Invoke(this);
		//SceneLoadManager.Log("ARPlane type:" + arEngineType + "\n" +"ARPlane Dir:"+AR);
		//entityManager.ARPlaneUpdated();
	}
}

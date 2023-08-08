using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(MeshRenderer))]
public class ARPlaneInfo : MonoBehaviour, IEventMessage {
	public Material matOnLoad;
	public Material matOnPlay;

	public Vector3 up => gameObject.transform.up;
#if !UNITY_EDITOR
	private ARPlane arPlane;
	public Vector3 center => arPlane.center;
	public Vector2 size => arPlane.size;
#else
	public Vector3 center => gameObject.transform.position;
	public Vector2 size = new Vector2(10f, 10f);
#endif
	public float radiusAverage => (size.x + size.y) * 0.5f;
	public float area => size.x * size.y;

	public bool isHorizontal => Vector3.Angle(Vector3.up, up) < 0.1f;

	public Action<ARPlaneInfo> planeUpdated;

	public void Awake() {
#if !UNITY_EDITOR
		if (!TryGetComponent(out arPlane)) {
			Debug.LogWarning("missing arPlane script");
		}
#endif
	}
	public void Start() {
#if !UNITY_EDITOR
		if (arPlane == null && !TryGetComponent(out arPlane)) {
			Debug.LogWarning("missing arPlane script");
		}else{
			arPlane.boundaryChanged += OnPlaneUpdate;
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
		planeUpdated?.Invoke(this);
		//entityManager.ARPlaneUpdated();
	}
}

using UnityEngine;
using System.Collections.Generic;
using HuaweiARUnitySDK;
using System.Collections;
using System;
using HuaweiARInternal;

namespace HuaweiARUnityAdapter {
	public class WorldARController : MonoBehaviour {
		[Tooltip("plane visualizer")]
		public GameObject planePrefabs;

		private List<ARPlane> newPlanes = new List<ARPlane>();

		private void Start() {
			DeviceChanged.OnDeviceChange += ARSession.SetDisplayGeometry;
		}

		public void Update() {
			_DrawPlane();
			//Touch touch;
			//if (ARFrame.GetTrackingState() != ARTrackable.TrackingState.TRACKING
			//    || Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
			//{

			//}
			//else
			//{
			//    _DrawARLogo(touch);

			//}
		}

		private void _DrawPlane() {
			newPlanes.Clear();
			ARFrame.GetTrackables(newPlanes, ARTrackableQueryFilter.NEW);
			for (int i = 0; i < newPlanes.Count; i++) {
				GameObject planeObject = Instantiate(planePrefabs, Vector3.zero, Quaternion.identity, transform);
				planeObject.GetComponent<TrackedPlaneVisualizer>().Initialize(newPlanes[i]);

			}
		}


	}
}

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
		//public GameObject arTagPrefabs;
		public Action<HWTrackedImagesChangedEventArgs> trackedImagesChanged;

		private List<ARPlane> newPlanes = new List<ARPlane>();

		private Dictionary<int, HWTrackedImage> m_AugmentedImages
			= new Dictionary<int, HWTrackedImage>();
		private List<ARAugmentedImage> m_TempAugmentedImages = new List<ARAugmentedImage>();

		private void Start() {
			DeviceChanged.OnDeviceChange += ARSession.SetDisplayGeometry;
		}

		public void Update() {
			if (ARFrame.GetTrackingState() != ARTrackable.TrackingState.TRACKING) {
				ARDebug.LogInfo("GetTrackingState no tracing return <<");
				return;
			}
			DrawPlane();
			UpdateImageTracking();
		}

		private void DrawPlane() {
			newPlanes.Clear();
			ARFrame.GetTrackables(newPlanes, ARTrackableQueryFilter.NEW);
			for (int i = 0; i < newPlanes.Count; i++) {
				GameObject planeObject = Instantiate(planePrefabs, Vector3.zero, Quaternion.identity, transform);
				planeObject.GetComponent<TrackedPlaneVisualizer>().Initialize(newPlanes[i]);

			}
		}

		private void UpdateImageTracking() {
			// Get updated augmented images for this frame.
			ARFrame.GetTrackables(m_TempAugmentedImages, ARTrackableQueryFilter.UPDATED);

			List<HWTrackedImage> added = new List<HWTrackedImage>();
			List<HWTrackedImage> update = new List<HWTrackedImage>();
			List<HWTrackedImage> removed = new List<HWTrackedImage>();

			foreach (ARAugmentedImage image in m_TempAugmentedImages) {
				DebugMenuManager.ShowLog(image.AcquireName().Split('.')[0]);
				HWTrackedImage ARTag = null;
				m_AugmentedImages.TryGetValue(image.GetDataBaseIndex(), out ARTag);

				if (image.GetTrackingState() == ARTrackable.TrackingState.TRACKING) {
					if (m_AugmentedImages.TryGetValue(image.GetDataBaseIndex(), out ARTag)) {
						var pos = image.GetCenterPose();
						ARTag.gameObject.transform.position = pos.position;
						ARTag.gameObject.transform.rotation = pos.rotation;
						update.Add(ARTag);
					} else {
						var pos = image.GetCenterPose();
						GameObject obj = new GameObject("imageTracking" + image.AcquireName());
						gameObject.transform.position = pos.position;
						gameObject.transform.rotation = pos.rotation;
						ARTag = new HWTrackedImage(obj, image);
						m_AugmentedImages.Add(image.GetDataBaseIndex(), ARTag);
						added.Add(ARTag);
					}
				} else if (image.GetTrackingState() == ARTrackable.TrackingState.STOPPED && m_AugmentedImages.TryGetValue(image.GetDataBaseIndex(), out ARTag)) {
					m_AugmentedImages.Remove(image.GetDataBaseIndex());
					removed.Add(ARTag);
				}
			}
			trackedImagesChanged?.Invoke(new HWTrackedImagesChangedEventArgs(added, update, removed));

		}
	}

	public class HWTrackedImage {
		public GameObject gameObject;
		public ARAugmentedImage referenceImage;
		public HWTrackedImage(GameObject gameObject, ARAugmentedImage referenceImage) {
			this.gameObject = gameObject;
			this.referenceImage = referenceImage;
		}
	}
	public struct HWTrackedImagesChangedEventArgs {
		public HWTrackedImagesChangedEventArgs(List<HWTrackedImage> added, List<HWTrackedImage> updated, List<HWTrackedImage> removed) {
			this.added = added;
			this.updated = updated;
			this.removed = removed;
		}

		public readonly List<HWTrackedImage> added { get; }
		public readonly List<HWTrackedImage> updated { get; }
		public readonly List<HWTrackedImage> removed { get; }
	}
}

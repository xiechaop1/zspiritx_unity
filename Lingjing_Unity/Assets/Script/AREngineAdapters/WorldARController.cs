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

		private Dictionary<ARPlane, GameObject> m_AugmentedPlanes = new Dictionary<ARPlane, GameObject>();
		private List<ARPlane> newPlanes = new List<ARPlane>();
		private Dictionary<int, HWTrackedImage> m_AugmentedImages = new Dictionary<int, HWTrackedImage>();
		private List<ARAugmentedImage> m_TempAugmentedImages = new List<ARAugmentedImage>();

		public static float CheckDelay = 0.06f;        // How long to wait until we check again.

		static Vector2Int resolution;                    // Current Resolution
		static ScreenOrientation orientation;        // Current Device Orientation
		static bool isAlive = true;                    // Keep this script running?



		private void Start() {
			//DeviceChanged.OnDeviceChange += ARSession.SetDisplayGeometry;
			StartCoroutine(CheckForChange());
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
				m_AugmentedPlanes.Add(newPlanes[i], planeObject);
			}
		}

		private void UpdateImageTracking() {
			// Get updated augmented images for this frame.
			ARFrame.GetTrackables(m_TempAugmentedImages, ARTrackableQueryFilter.UPDATED);

			List<HWTrackedImage> added = new List<HWTrackedImage>();
			List<HWTrackedImage> update = new List<HWTrackedImage>();
			List<HWTrackedImage> removed = new List<HWTrackedImage>();
			HWTrackedImage ARTag;
			foreach (ARAugmentedImage image in m_TempAugmentedImages) {
				//DebugMenuManager.ShowLog(image.AcquireName().Split('.')[0]);
				ARTag = null;
				int idxImage = image.GetDataBaseIndex();
				//m_AugmentedImages.TryGetValue(idxImage, out ARTag);

				if (image.GetTrackingState() == ARTrackable.TrackingState.TRACKING) {
					if (m_AugmentedImages.TryGetValue(idxImage, out ARTag)) {
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
						m_AugmentedImages.Add(idxImage, ARTag);
						added.Add(ARTag);
					}
				} else if (image.GetTrackingState() == ARTrackable.TrackingState.STOPPED && m_AugmentedImages.TryGetValue(idxImage, out ARTag)) {
					m_AugmentedImages.Remove(idxImage);
					removed.Add(ARTag);
				}
			}
			trackedImagesChanged?.Invoke(new HWTrackedImagesChangedEventArgs(added, update, removed));

		}

		IEnumerator CheckForChange() {
			resolution = new Vector2Int(Screen.width, Screen.height);
			orientation = Screen.orientation;

			while (isAlive) {

				// Check for a Resolution Change
				if (resolution.x != Screen.width || resolution.y != Screen.height) {
					resolution = new Vector2Int(Screen.width, Screen.height);
					ARSession.SetDisplayGeometry(resolution.x, resolution.y);
				}

				// Check for an Orientation Change
				if (orientation != Screen.orientation) {
					orientation = Screen.orientation;
					ARSession.SetDisplayGeometry(resolution.x, resolution.y);
				}
				yield return new WaitForSeconds(CheckDelay);
			}
		}

		void OnDestroy() {
			isAlive = false;
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

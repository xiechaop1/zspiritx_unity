namespace HuaweiARUnityAdapter {
	using System;
	using System.Collections.Generic;
	using HuaweiARUnitySDK;
	using UnityEngine;
	using HuaweiARInternal;

	public class TrackedPlaneVisualizer : MonoBehaviour {
		private static int s_planeCount = 0;

		private readonly Color[] k_planeColors = new Color[]
		{
			new Color(1.0f, 1.0f, 1.0f),
			new Color(0.5f,0.3f,0.9f),
			new Color(0.8f,0.4f,0.8f),
			new Color(0.5f,0.8f,0.4f),
			new Color(0.5f,0.9f,0.8f)
		};

		private ARPlane m_trackedPlane;

		public Action boundaryChanged;
		[HideInInspector]
		public Vector3 planeNormal { get; private set; }

		// Keep previous frame's mesh polygon to avoid mesh update every frame.
		private Pose m_previousFrameCenterPose = new Pose(Vector3.zero, Quaternion.identity);
		private List<Vector2> m_previousFrameMeshVertices = new List<Vector2>();
		private List<Vector3> m_meshVertices3D = new List<Vector3>();
		private List<Vector2> m_meshVertices2D = new List<Vector2>();

		private List<Color> m_meshColors = new List<Color>();

		private ARPlane.ARPlaneType planeType;

		public Mesh m_mesh { get; private set; }

		private MeshRenderer m_meshRenderer;

		public void Awake() {
			m_mesh = new Mesh();// GetComponent<MeshFilter>().mesh;
			m_meshRenderer = GetComponent<MeshRenderer>();
		}

		public void Update() {
			if (m_trackedPlane == null) {
				return;
			} else if (m_trackedPlane.GetSubsumedBy() != null) {
				m_meshRenderer.enabled = false;
				return;
			} else if (m_trackedPlane.GetTrackingState() == ARTrackable.TrackingState.STOPPED) {
				m_meshRenderer.enabled = false;
				//Destroy(gameObject);
				return;
			} else if (m_trackedPlane.GetTrackingState() == ARTrackable.TrackingState.PAUSED) {// whether to destory gameobject if not tracking
				m_meshRenderer.enabled = false;
				return;
			}

			m_meshRenderer.enabled = true;
			_UpdateMeshIfNeeded();
		}

		public void Initialize(ARPlane plane) {
			m_trackedPlane = plane;
			m_meshRenderer.material.SetColor("_GridColor", k_planeColors[s_planeCount++ % k_planeColors.Length]);
			planeType = m_trackedPlane.GetARPlaneType();
			Update();
		}

		private void _UpdateMeshIfNeeded() {
			m_trackedPlane.GetPlanePolygon(m_meshVertices2D);

			Pose centerPose = m_trackedPlane.GetCenterPose();
			if (m_previousFrameCenterPose.position == centerPose.position && m_previousFrameCenterPose.rotation == centerPose.rotation) {
				if (_AreVerticesListsEqual(m_previousFrameMeshVertices, m_meshVertices2D)) {
					return;
				}
			} else {
				switch (planeType) {
					case ARPlane.ARPlaneType.HORIZONTAL_UPWARD_FACING:
						if (Vector3.Angle(Vector3.up, centerPose.rotation * Vector3.up) > 0.1f) return;
						break;
					case ARPlane.ARPlaneType.HORIZONTAL_DOWNWARD_FACING:
						if (Vector3.Angle(Vector3.down, centerPose.rotation * Vector3.up) > 0.1f) return;
						break;
					case ARPlane.ARPlaneType.VERTICAL_FACING:
						Vector3 normal = centerPose.rotation * Vector3.up;
						if (Vector3.Angle(Vector3.up, normal) < 0.1f || Vector3.Angle(Vector3.down, normal) < 0.1f) return;
						break;
					case ARPlane.ARPlaneType.UNKNOWN_FACING:
					default:
						break;
				}
			}
			//if ((Vector3.Angle(Vector3.up, centerPose.rotation * Vector3.up) < 0.1f) != (m_trackedPlane.GetARPlaneType() == ARPlane.ARPlaneType.HORIZONTAL_UPWARD_FACING)) {
			//	return;
			//}

			m_previousFrameCenterPose = centerPose;

			m_previousFrameMeshVertices.Clear();
			m_previousFrameMeshVertices.AddRange(m_meshVertices2D);

			//Pose centerPose = m_trackedPlane.GetCenterPose();

			transform.position = centerPose.position;
			transform.rotation = centerPose.rotation;

			planeNormal = centerPose.rotation * Vector3.up;
			m_meshRenderer.material.SetVector("_PlaneNormal", planeNormal);

			m_trackedPlane.GetPlanePolygon(m_meshVertices3D);
			//for (int i = 0; i < m_meshVertices3D.Count; i++) {
			//	m_meshVertices3D[i] = centerPose.rotation * m_meshVertices3D[i] + centerPose.position;
			//}

			Triangulator tr = new Triangulator(m_meshVertices2D);

			m_mesh.Clear();
			m_mesh.SetVertices(m_meshVertices3D);
			m_mesh.SetIndices(tr.Triangulate(), MeshTopology.Triangles, 0);
			m_mesh.SetColors(m_meshColors);

			var meshFilter = GetComponent<MeshFilter>();
			if (meshFilter != null)
				meshFilter.sharedMesh = m_mesh;

			var meshCollider = GetComponent<MeshCollider>();
			if (meshCollider != null)
				meshCollider.sharedMesh = m_mesh;

			boundaryChanged?.Invoke();

		}

		private bool _AreVerticesListsEqual(List<Vector2> firstList, List<Vector2> secondList) {
			if (firstList.Count != secondList.Count) {
				return false;
			}

			for (int i = 0; i < firstList.Count; i++) {
				if (firstList[i] != secondList[i]) {
					return false;
				}
			}

			return true;
		}
	}
}

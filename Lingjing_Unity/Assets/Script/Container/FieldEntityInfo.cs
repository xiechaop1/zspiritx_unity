using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldEntityInfo : ItemInfo {
	public enum EntityToggleType {
		RamdomAround,
		ARTagTracking,
		ARTagAround,
		Location,
	}
	public EntityToggleType enumARType = EntityToggleType.RamdomAround;
	public string uuidImageTracking = "";
	public Vector3 offset = Vector3.zero;
	public bool isLookAt = false;
	public int enumSurfaceType = -1;
	public GameObject goReference;
	//public GameObject[] lstEntityCorner;
	public GameObject goFPSightMode;
	public float proximityDialog = 0f;
	public bool hasProximityDialog = false;
	public ParticleSystem animEmerge;
	public GameObject goVisual;

	int stageShowAnimation = -1;
	int stageHideAnimation = -1;
	float animationTimer = 0f;
	private void Update() {
		if (stageShowAnimation >= 0) {
			animationTimer += Time.deltaTime;
			switch (stageShowAnimation) {
				case 0:
					if (animationTimer > 0.5f) {
						ShowSelf();
						stageShowAnimation = -1;
					}
					break;
				default:
					break;
			}
		}
		if (stageHideAnimation >= 0) {
			animationTimer += Time.deltaTime;
			switch (stageHideAnimation) {
				case 0:
					if (animationTimer > 0.5f) {
						HideSelf();
						stageHideAnimation++;
					}
					break;
				case 1:
					if (animationTimer > 1f) {
						StoreSelf();
						stageHideAnimation = -1;
					}
					break;
				default:
					break;
			}
		}
	}

	public bool TryPlacing(Vector3 posTarget, Quaternion rotTarget, GameObject targetSurface, Transform rootWorld) {
		var posOld = transform.position;
		var rotOld = transform.rotation;
		transform.position = posTarget;
		if (isLookAt) {
			Vector3 LookDir = entityManager.goCamDir.transform.position - posTarget;
			LookDir.y = 0;
			transform.rotation = Quaternion.LookRotation(LookDir.normalized);
		} else {
			transform.rotation = rotTarget;
		}

		RaycastHit hit;
		Ray ray = new Ray(posTarget + (transform.up * 0.01f), Vector3.down);
		//Debug.Log("Try place at " + posTarget);
		if (Physics.Raycast(ray, out hit, 1f)) {
			//if (hit.collider.gameObject == targetSurface) 
			//Debug.Log("place at " + posTarget);
			gameObject.transform.parent = rootWorld;
			if (animEmerge) {
				ShowAnim();
			} else {
				ShowSelf();
			}
			return true;
			//}
		}
		transform.position = posOld;
		transform.rotation = rotOld;
		return false;
	}

	public void RemoveFromField() {
		if (animEmerge) {
			HideAnim();
		} else {
			HideSelf();
			StoreSelf();
		}
	}

	public bool TryGetScenePos(out Vector3 pos) {
		if (goReference != entityManager.goCamDir) {
			pos = goReference.transform.InverseTransformPoint(transform.position);
			return true;
		}
		GameObject go = entityManager.GetStageImgDir();
		if (go != null) {
			pos = go.transform.InverseTransformPoint(transform.position);
			return true;
		}
		pos = Vector3.zero;
		return false;
	}
	public bool TryGetUserPos(out Vector3 pos) {
		GameObject go = entityManager.goCamDir;
		pos = go.transform.InverseTransformPoint(transform.position);
		return true;
	}
	void ShowAnim() {
		stageShowAnimation = 0;
		stageHideAnimation = -1;
		animationTimer = 0f;
		animEmerge.GetComponent<ParticleSystem>().Play();
	}
	void ShowSelf() {
		goVisual.SetActive(true);
		if (proximityDialog > 0) {
			hasProximityDialog = true;
		}
	}
	void HideAnim() {
		stageShowAnimation = -1;
		stageHideAnimation = 0;
		animationTimer = 0f;
		animEmerge.GetComponent<ParticleSystem>().Play();
	}
	void HideSelf() {
		goVisual.SetActive(false);
	}
	void StoreSelf() {
		transform.parent = entityManager.stageManager.goRoot.transform;
		transform.localPosition = Vector3.zero;
	}


	public void OnDestroy() {
		UIEventManager.CallEvent("FieldEntityManager", "RemoveFieldEntitys", this);
	}

}

public abstract class ARUtilityListener : MonoBehaviour {
	public abstract void UseARUtility();
}

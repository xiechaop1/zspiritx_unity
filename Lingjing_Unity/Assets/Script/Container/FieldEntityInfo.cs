using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldEntityInfo : ItemInfo {
	public enum EntityToggleType {
		Ignore = 0,
		RamdomAroundCam = 1,
		ARTagAround = 11,
		ARTagPosition = 12,
		ARTagTracking = 13,
		GeoLocAround = 21,
		GeoLocPosition = 22,
		StageAround = 31,
		StagePosition = 32
	}
	public int entitySessionId = 0;
	public int entityItemId = 0;
	public string entityName = "0";
	public EntityToggleType enumARType = EntityToggleType.RamdomAroundCam;
	public string uuidImageTracking = "";
	public double latitude = 0d;
	public double longitude = 0d;
	public Vector3 offset = Vector3.zero;
	public bool isLookAt = false;
	public int enumSurfaceType = -1;
	public GameObject goReference;
	public Vector3 IdealPos => goReference.transform.position + goReference.transform.rotation * offset;
	//public GameObject[] lstEntityCorner;
	public GameObject goFPSightMode;
	public float proximityDialog = 0f;
	public bool hasProximityDialog = false;
	public ParticleSystem animEmerge;
	public GameObject goVisual;
	bool isShow = true;

	int stageShowAnimation = -1;
	int stageHideAnimation = -1;
	float animationTimer = 0f;
	private void Update() {
		animationTimer += Time.deltaTime;
		if (stageShowAnimation >= 0) {
			if (isShow) {
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
			//animationTimer += Time.deltaTime;

		} else if (stageHideAnimation >= 0) {
			//animationTimer += Time.deltaTime;
			switch (stageHideAnimation) {
				case 0:
					if (animationTimer > 0.5f) {
						HideSelf();
						stageHideAnimation++;
					}
					break;
				case 1:
					if (animationTimer > 1f) {
						if (!isShow) {
							StoreSelf();
						}
						stageHideAnimation = -1;
					}
					break;
				default:
					break;
			}
		} else if (animationTimer > 1f) {
			if (TryGetUserPos(out Vector3 pos)) {
				Debug.Log(pos.magnitude + " " + (entityManager.maxShowDistance * 1.5f) + " " + (pos.magnitude > entityManager.maxShowDistance * 1.5f));
				if (goVisual.activeInHierarchy) {
					if (pos.magnitude > entityManager.maxShowDistance * 1.5f) {
						HideFromField();
					}
				} else {
					if (pos.magnitude < entityManager.maxShowDistance) {
						PlaceOntoField();
					}
				}
			}

			animationTimer = 0f;
		}
	}

	public bool TryPlacing(Vector3 posTarget, Quaternion rotTarget, GameObject targetSurface, Transform rootWorld) {
		var posOld = transform.position;
		var rotOld = transform.rotation;
		transform.position = posTarget;
		transform.rotation = rotTarget;
		if (isLookAt) {
			Vector3 LookDir = entityManager.goCamDir.transform.position - posTarget;
			LookDir.y = 0;
			if (LookDir.sqrMagnitude > 0.01f) {
				transform.rotation = Quaternion.LookRotation(LookDir.normalized);
			}
		}

		RaycastHit hit;
		Ray ray = new Ray(posTarget + (transform.up * 0.01f), Vector3.down);
		//Debug.Log("Try place at " + posTarget);
		if (Physics.Raycast(ray, out hit, 1f)) {
			//if (hit.collider.gameObject == targetSurface) 
			//Debug.Log("place at " + posTarget);
			gameObject.transform.parent = rootWorld;

			isShow = true;
			//PlaceOntoField();
			//if (!goVisual.activeInHierarchy) {
			//	if (animEmerge) {
			//		ShowAnim();
			//	} else {
			//		ShowSelf();
			//	}
			//}
			return true;
			//}
		}
		transform.position = posOld;
		transform.rotation = rotOld;
		return false;
	}
	public void ForcePlacing(Transform rootWorld) {
		transform.position = goReference.transform.position + goReference.transform.rotation * offset;
		transform.rotation = goReference.transform.rotation;
		if (isLookAt) {
			Vector3 LookDir = entityManager.goCamDir.transform.position - transform.position;
			LookDir.y = 0;
			if (LookDir.sqrMagnitude > 0.01f) {
				transform.rotation = Quaternion.LookRotation(LookDir.normalized);
			}
		}
		gameObject.transform.parent = rootWorld;

		isShow = true;
		//PlaceOntoField();
		//if (!goVisual.activeInHierarchy) {
		//	if (animEmerge) {
		//		ShowAnim();
		//	} else {
		//		ShowSelf();
		//	}
		//}
	}

	public void RemoveFromField() {
		isShow = false;
		HideFromField();
		if (!animEmerge) {
			StoreSelf();
		}
	}

	public bool TryGetStagePos(out Vector3 pos) {
		if (goReference != entityManager.goCamDir) {
			pos = goReference.transform.InverseTransformPoint(transform.position);
			return true;
		}
		GameObject go = entityManager.GetStageDir();
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

	public void PlaceOntoField() {
		if (isShow && !goVisual.activeInHierarchy) {
			if (animEmerge) {
				ShowAnim();
			} else {
				ShowSelf();
			}
		}
	}
	public void HideFromField() {
		if (animEmerge) {
			HideAnim();
		} else {
			HideSelf();
		}
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
		transform.parent = entityManager.goStorage.transform;//.stageManager.goRoot.transform;
		transform.localPosition = Vector3.zero;
	}


	public void OnDestroy() {
		UIEventManager.CallEvent("FieldEntityManager", "RemoveFieldEntitys", this);
	}

}

public abstract class ARUtilityListener : MonoBehaviour {
	public abstract void UseARUtility();
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldEntityInfo : ItemInfo{
	public int enumSurfaceType = -1;
	public GameObject[] lstEntityCorner;
	public GameObject goFPSightMode;
	public bool TryPlacing(Vector3 posTarget, Quaternion rotTarget, GameObject targetSurface, Transform rootWorld) {
		var posOld = transform.position;
		var rotOld = transform.rotation;
		transform.position = posTarget;
		transform.rotation = rotTarget;
		RaycastHit hit;
		Ray ray = new Ray(posTarget + (transform.up * 0.01f), Vector3.down);
		//Debug.Log("Try place at " + posTarget);
		if (Physics.Raycast(ray, out hit, 1f)) {
			//if (hit.collider.gameObject == targetSurface) 
			//Debug.Log("place at " + posTarget);
			gameObject.transform.parent = rootWorld;
			return true;
			//}
		}
		transform.position = posOld;
		transform.rotation = rotOld;
		return false;
	}
	public void OnDestroy() {
		UIEventManager.CallEvent("FieldEntityManager", "RemoveFieldEntitys", this);
		//FieldEntityManager.getInstance().RemoveFieldEntitys(this);
	}

}

public abstract class ARUtilityListener : MonoBehaviour {
	public abstract void UseARUtility();
}

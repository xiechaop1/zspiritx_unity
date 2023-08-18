using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageDirMover : MonoBehaviour {
	GameObject goImageOrigin;
	public void Init(GameObject origin) {
		goImageOrigin = origin;
		transform.position = origin.transform.position;
		transform.rotation.SetLookRotation(GetForward());
	}

	Vector3 GetForward() {
		Vector3 fwd = goImageOrigin.transform.forward;
		if (fwd.y > -0.1f && fwd.y < 0.1f) {
			fwd = -goImageOrigin.transform.up;
		}
		fwd.y = 0;
		return fwd.normalized;
	}
	float timeFactor = 1f;
	// Update is called once per frame
	void Update() {
		if (timeFactor<10f) {
			timeFactor += Time.deltaTime;
		}
	}
}

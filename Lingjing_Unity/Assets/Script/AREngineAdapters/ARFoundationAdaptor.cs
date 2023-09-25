using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARFoundationAdaptor : MonoBehaviour {
	public ARSession arSession;
	public ARCameraManager cameraManager;
	public ARCameraBackground cameraBackground;
	public UnityEngine.InputSystem.XR.TrackedPoseDriver poseDriver;
	public void Init() {
		cameraManager.enabled = true;
		cameraBackground.enabled = true;
		poseDriver.enabled = true;
	}
	public void Enable() {
		arSession.enabled = true;
	}
	public void Disable() {
		arSession.enabled = false;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HuaweiARUnitySDK;
using HuaweiARUnityAdapter;

public class HWAREngineAdaptor : MonoBehaviour {
	public WorldARController worldARController;
	public SessionComponent sessionComponent;

	public BackGroundRenderer backGroundRenderer;

	public void Init() {
		worldARController.enabled = true;
		sessionComponent.enabled = true;
	}
	public void Enable() {
		backGroundRenderer.enabled = true;
	}
	public void Disable() {
		backGroundRenderer.enabled = false;
	}
}

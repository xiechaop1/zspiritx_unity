using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HuaweiARUnitySDK;
using HuaweiARUnityAdapter;

public class HWAREngineAdaptor : MonoBehaviour {
	public DeviceChanged deviceChanged;
	public WorldARController worldARController;
	public SessionComponent sessionComponent;
	public AugmentedImageExampleController imageController;

	public BackGroundRenderer backGroundRenderer;

	public void Init() {
		deviceChanged.enabled = true;
		worldARController.enabled = true;
		sessionComponent.enabled = true;
		imageController.enabled = true;
	}
	public void Enable() {
		backGroundRenderer.enabled = true;
	}
	public void Disable() {
		backGroundRenderer.enabled = false;
	}
}

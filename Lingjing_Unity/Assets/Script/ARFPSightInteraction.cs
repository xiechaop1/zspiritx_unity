using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ItemInfo))]
public class ARFPSightInteraction : ARUtilityListener {
	public override void UseARUtility() {
		UIEventManager.CallEvent("ARSightManager", "ToggleFPSight");
		//ARSightManager.getInstance().ToggleFPSight();
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARTagDebugListener : MonoBehaviour {
	public GameObject goDebug;
	// Start is called before the first frame update
	void Start() {
		UIEventManager.getInstance().SubscribeBroadcast("ARTagDebug", BroadcastHandler);
	}
	void BroadcastHandler(string msg) {
		if (goDebug==null) {
			return;
		}
		if (msg=="True") {
			goDebug.SetActive(true);
		}else{ 
			goDebug.SetActive(false);
		}
	}
}

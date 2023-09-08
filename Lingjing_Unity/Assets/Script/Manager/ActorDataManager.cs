using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Network;

public class ActorDataManager : MonoBehaviour, IManager {
	InputGPSManager gpsManager;
	WWWManager networkManager;
	public void Init(UIEventManager eventManager, params IManager[] managers) {
		foreach (var manager in managers) {
			RegisterManager(manager);
		}
	}
	public void RegisterManager(IManager manager) {
		if (manager is InputGPSManager) {
			gpsManager = manager as InputGPSManager;
		} else if (manager is WWWManager) {
			networkManager = manager as WWWManager;
		}
	}
}

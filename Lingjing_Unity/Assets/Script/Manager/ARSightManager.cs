using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FieldEntityManager))]
public class ARSightManager : MonoBehaviour, IManager {
	//public static ARSightManager getInstance() {
	//	if (instance != null) {
	//		return instance;
	//	} else {
	//		GameObject go = GameObject.Find("ARSightManager");
	//		if (go != null && go.TryGetComponent(out instance)) {
	//			return instance;
	//		}
	//	}
	//	Debug.LogError("MISSING ARSightManager ");
	//	return null;
	//}
	//private static ARSightManager instance;

	public GameObject fingerPrintMask;
	//public FieldEntityManager fieldEntityManager;
	private List<FieldEntityInfo> fieldFingerPrintEntities = new List<FieldEntityInfo>();
	private List<FieldEntityInfo> inventoryFingerPrintEntities = new List<FieldEntityInfo>();
	private bool isFPMode = false;
	public void Init(UIEventManager eventManager, params IManager[] managers) {//FieldEntityManager fieldEntityManager) {
		eventManager.RegisteredAction("ARSightManager", "ToggleFPSight", ToggleFPSight);
		FieldEntityManager fieldEntityManager = GetComponent<FieldEntityManager>();
		//foreach (var manager in managers) {
		//	if (manager is FieldEntityManager) {
		//		FieldEntityManager fieldEntityManager = manager as FieldEntityManager;
		fieldEntityManager.OnEntityPlaced += FieldAddEntity;
		fieldEntityManager.OnEntityRemoved += FieldRemoveEntity;
		//	}
		//}
	}

	public void FieldAddEntity(FieldEntityInfo fieldEntity) {
		if (fieldEntity.goFPSightMode != null && !fieldFingerPrintEntities.Contains(fieldEntity)) {
			fieldFingerPrintEntities.Add(fieldEntity);
		}
	}
	public void FieldRemoveEntity(FieldEntityInfo fieldEntity) {
		if (fieldFingerPrintEntities.Contains(fieldEntity)) {
			fieldFingerPrintEntities.Remove(fieldEntity);
		}
	}
	public void InventoryAddEntity(FieldEntityInfo fieldEntity) {
		if (fieldEntity.goFPSightMode != null && !inventoryFingerPrintEntities.Contains(fieldEntity)) {
			inventoryFingerPrintEntities.Add(fieldEntity);
		}
	}
	public void InventoryFieldRemoveEntity(FieldEntityInfo fieldEntity) {
		if (inventoryFingerPrintEntities.Contains(fieldEntity)) {
			inventoryFingerPrintEntities.Remove(fieldEntity);
		}
	}
	public void ToggleFPSight() {
		isFPMode = !isFPMode;
		GameObject go;
		for (int i = 0; i < fieldFingerPrintEntities.Count; i++) {
			go = fieldFingerPrintEntities[i].goFPSightMode;
			if (go != null) {
				go.SetActive(isFPMode);
			}
		}
		for (int i = 0; i < inventoryFingerPrintEntities.Count; i++) {
			go = inventoryFingerPrintEntities[i].goFPSightMode;
			if (go != null) {
				go.SetActive(isFPMode);
			}
		}
		fingerPrintMask.SetActive(isFPMode);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItemManager : MonoBehaviour, IManager {
	//public static InventoryItemManager getInstance() {
	//	if (instance != null) {
	//		return instance;
	//	} else {
	//		GameObject go = GameObject.Find("InventoryItemManager");
	//		if (go != null && go.TryGetComponent(out instance)) {
	//			return instance;
	//		}
	//	}
	//	Debug.LogError("MISSING InventoryItemManager ");
	//	return null;
	//}
	//private static InventoryItemManager instance;

	private InteractionView interactionView;
	private List<IEventMessage> lstItems = new List<IEventMessage>();
	public IEventMessage[] getItemList => lstItems.ToArray();

	public void Init(UIEventManager eventManager, params IManager[] managers) {
		interactionView = InteractionView.getInstance();
		//var eventManager = UIEventManager.getInstance();
		eventManager.RegisteredAction("InventoryItemManager", "AddInfo", AddInfo);
		eventManager.RegisteredAction("InventoryItemManager", "AddItem", AddItem);
		eventManager.RegisteredAction("InventoryItemManager", "RemoveInfo", RemoveInfo);
	}

	public void AddInfo(IEventMessage info) {
		FileInfo entityInfo = new FileInfo(info as ItemInfo);
		if (!lstItems.Contains(entityInfo)) {
			lstItems.Add(entityInfo);
			interactionView.UpdateInventory();
		}
	}
	public void AddItem(IEventMessage info) {
		ItemInfo entityInfo = info as ItemInfo;
		if (!lstItems.Contains(entityInfo)) {
			lstItems.Add(entityInfo);
			GameObject go = entityInfo.gameObject;
			go.transform.SetParent(transform);
			go.SetActive(false);
			interactionView.UpdateInventory();
		}
	}
	public void RemoveInfo(IEventMessage info) {
		ItemInfo entityInfo = info as ItemInfo;
		if (lstItems.Contains(entityInfo)) {
			lstItems.Remove(entityInfo);
			interactionView.UpdateInventory();
		}
	}
	public void ShowInventoryItem(InventoryItemIcon info) {
	}
}

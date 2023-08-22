using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItemManager : MonoBehaviour, IManager {
	public InteractionView interactionView;

	public InventoryView inventoryView;
	private List<IEventMessage> lstItems = new List<IEventMessage>();
	public IEventMessage[] getItemList => lstItems.ToArray();

	public void Init(UIEventManager eventManager, params IManager[] managers) {
		//interactionView = InteractionView.getInstance();
		//var eventManager = UIEventManager.getInstance();
		eventManager.RegisteredAction("InventoryItemManager", "AddInfo", AddInfo);
		eventManager.RegisteredAction("InventoryItemManager", "AddItem", AddItem);
		eventManager.RegisteredAction("InventoryItemManager", "RemoveInfo", RemoveInfo);
		eventManager.RegisteredAction("InventoryItemManager", "ShowInventoryItem", ShowInventoryItem);
	}

	public void AddInfo(IEventMessage info) {
		FileInfo entityInfo = new FileInfo(info as ItemInfo);
		if (!lstItems.Contains(entityInfo)) {
			lstItems.Add(entityInfo);
			inventoryView.UpdateInventory();
		}
	}
	public void AddItem(IEventMessage info) {
		ItemInfo entityInfo = info as ItemInfo;
		if (!lstItems.Contains(entityInfo)) {
			lstItems.Add(entityInfo);
			GameObject go = entityInfo.gameObject;
			go.transform.SetParent(transform);
			go.SetActive(false);
			inventoryView.UpdateInventory();
		}
	}
	public void RemoveInfo(IEventMessage info) {
		ItemInfo entityInfo = info as ItemInfo;
		if (lstItems.Contains(entityInfo)) {
			lstItems.Remove(entityInfo);
			inventoryView.UpdateInventory();
		}
	}
	public void ShowInventoryItem(IEventMessage info) {
		InventoryItemIcon itemInfo = info as InventoryItemIcon;
		if (itemInfo != null) {
			inventoryView.InteractWithEntity(itemInfo);
		}

	}
}

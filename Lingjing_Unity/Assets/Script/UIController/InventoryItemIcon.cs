using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemIcon : MonoBehaviour,IEventMessage {
	public ItemInfo entityInfo;
	public FileInfo fileInfo;
	public Image img;
	public Text hint;
	public void OpenFile() {
		UIEventManager.CallEvent("InventoryItemManager", "ShowInventoryItem", this);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryView : MonoBehaviour {
	public EntityActionManager actionManager;
	public InventoryItemManager inventoryManager;
	public GameObject goBackPack;
	public GameObject goInfoIcon;
	public GameObject goInventoryTray;

	public void Start() {
		UpdateInventory();
	}

	public void BackPackToggle() {
		bool isActive = goBackPack.activeInHierarchy;
		goBackPack.SetActive(!isActive);
	}
	public void UpdateInventory() {
		IEventMessage[] arrItems = inventoryManager.getItemList;
		ItemInfo itemInfo;
		FileInfo fileInfo;
		GameObject newGo;
		RectTransform trans;

		for (int i = goInventoryTray.transform.childCount - 1; i >= 0; i--) {
			Destroy(goInventoryTray.transform.GetChild(i).gameObject);
		}

		List<ItemInfo> lstItems = new List<ItemInfo>();
		List<FileInfo> lstFiles = new List<FileInfo>();
		for (int i = 0; i < arrItems.Length; i++) {
			if (arrItems[i] != null) {
				if (arrItems[i] is ItemInfo) {
					lstItems.Add(arrItems[i] as ItemInfo);
				} else if (arrItems[i] is FileInfo) {
					lstFiles.Add(arrItems[i] as FileInfo);
				}
			}
		}

		int cntItems = lstItems.Count;
		for (int i = 0; i < cntItems; i++) {
			itemInfo = lstItems[i];
			if (itemInfo != null) {
				newGo = Instantiate(goInfoIcon, goInventoryTray.transform);
				trans = newGo.GetComponent<RectTransform>();
				trans.anchoredPosition = new Vector2(200 * i + 100, 0);

				var iconInfo = newGo.GetComponent<InventoryItemIcon>();
				iconInfo.entityInfo = itemInfo;
				if (itemInfo.icon != null) {
					iconInfo.img.sprite = itemInfo.icon;
					if (string.IsNullOrWhiteSpace(itemInfo.iconHint)) {
						iconInfo.hint.text = itemInfo.iconHint;
					}
				}
			}
			newGo = null;
			trans = null;
		}
		for (int i = 0; i < lstFiles.Count; i++) {
			fileInfo = lstFiles[i];
			if (fileInfo != null) {
				newGo = Instantiate(goInfoIcon, goInventoryTray.transform);
				trans = newGo.GetComponent<RectTransform>();
				trans.anchoredPosition = new Vector2(200 * i + 200 * cntItems + 100, 0);

				var iconInfo = newGo.GetComponent<InventoryItemIcon>();
				iconInfo.fileInfo = fileInfo;
				if (fileInfo.icon != null) {
					iconInfo.img.sprite = fileInfo.icon;
					if (string.IsNullOrWhiteSpace(fileInfo.iconHint)) {
						iconInfo.hint.text = fileInfo.iconHint;
					}
				}
			}
			newGo = null;
			trans = null;
		}
		trans = goInventoryTray.GetComponent<RectTransform>();
		trans.sizeDelta = new Vector2(200 * (cntItems + lstFiles.Count) + 50, 000);

	}
	public void InteractWithEntity(InventoryItemIcon info) {
		if (info.entityInfo != null) {
			actionManager.InteractWithEntity(info.entityInfo);
		} else if (info.fileInfo != null) {
			actionManager.InteractWithFile(info.fileInfo);
		}
	}
}

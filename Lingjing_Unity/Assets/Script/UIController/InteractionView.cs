using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionView : MonoBehaviour {
	public static InteractionView getInstance() {
		if (instance != null) {
			return instance;
		} else {
			GameObject go = GameObject.Find("InteractionUIRoot");
			if (go != null && go.TryGetComponent(out instance)) {
				return instance;
			}
		}
		Debug.LogError("MISSING InteractionView ");
		return null;
	}
	private static InteractionView instance;

	public EntityActionManager actionManager;
	public InventoryItemManager inventoryManager;
	public GameObject goHintBox;
	public Text txtHint;
	public GameObject btnComfirm;
	public Text txtConfirmBtn;
	public Text txtExitBtn;
	//private Action<FieldEntityInfo> actHintbox;
	private ItemInfo entityInfo;
	public GameObject goBackPack;
	public GameObject goInfoIcon;
	public GameObject goInventoryTray;
	public void Start() {
		UpdateInventory();
	}
	public void ShowHint(string hint, string textComfirm = "确认") {
		goHintBox.SetActive(true);
		txtHint.text = hint;
		if (string.IsNullOrWhiteSpace(textComfirm)) {
			btnComfirm.SetActive(false);
		} else {
			btnComfirm.SetActive(true);
			txtConfirmBtn.text = textComfirm;
		}
	}
	public void ShowHint(ItemInfo info, string textComfirm, string textCancel = "取消") {
		goHintBox.SetActive(true);
		txtHint.text = info.strHintbox;
		entityInfo = info;
		if (string.IsNullOrWhiteSpace(textComfirm)) {
			btnComfirm.SetActive(false);
		} else {
			btnComfirm.SetActive(true);
			txtConfirmBtn.text = textComfirm;
		}
		//actHintbox = action;
	}
	public void UpdateHint(string newText) {
		txtHint.text = newText;
	}
	public void ConfirmHint() {
		if (entityInfo != null) {
			actionManager.ConfirmWithEntiry(entityInfo);
		}
		//actHintbox?.Invoke(entityInfo);
		//actHintbox = null;
		ExitHint();
	}
	public void ExitHint() {
		goHintBox.SetActive(false);
		actionManager.OnInteractionFinished(entityInfo);
		entityInfo = null;
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
		trans.sizeDelta = new Vector2(200 * (cntItems+lstFiles.Count)+50, 000);
		//var rect = trans.rect;
		//rect.width = 200 * lstItems.Length;
		//trans.set = rect;
	}
	public void InteractWithEntity(InventoryItemIcon info) {
		if (info.entityInfo != null) {
			actionManager.InteractWithEntity(info.entityInfo);
		} else if (info.fileInfo != null) {
			actionManager.InteractWithFile(info.fileInfo);
		}
	}
}

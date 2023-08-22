using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionView : MonoBehaviour {
	public EntityActionManager actionManager;
	public GameObject goHintBox;
	public Text txtHint;
	public GameObject btnComfirm;
	public Text txtConfirmBtn;
	public Text txtExitBtn;
	private ItemInfo entityInfo;

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

	public void ShowHint(ItemInfo info, string textCancel = "取消") {
		goHintBox.SetActive(true);
		txtHint.text = info.strHintbox;
		entityInfo = info;
		btnComfirm.SetActive(false);
	}
	public void ShowCollectableHint(ItemInfo info, string textCancel = "取消") {
		goHintBox.SetActive(true);
		txtHint.text = info.strHintbox;
		entityInfo = info;
		btnComfirm.SetActive(true);
		txtConfirmBtn.text = "记录信息";
	}
	public void ShowCollectableItem(ItemInfo info, string textCancel = "取消") {
		goHintBox.SetActive(true);
		txtHint.text = info.strHintbox;
		entityInfo = info;
		btnComfirm.SetActive(true);
		txtConfirmBtn.text = "收集道具";
	}


	Queue<string> queDialog = new Queue<string>();
	public void ShowDialog(ItemInfo info, string textCancel = "取消") {
		goHintBox.SetActive(true);
		string rawString = info.strHintbox;
		queDialog = new Queue<string>(rawString.Split('^'));
		entityInfo = info;
		AdvancedDialog();
	}
	public void AdvancedDialog() {
		if (queDialog.Count > 0) {
			txtHint.text = queDialog.Dequeue();
		}
		if (queDialog.Count > 0) {
			btnComfirm.SetActive(true);
			txtConfirmBtn.text = "继续";
		} else {
			btnComfirm.SetActive(false);
		}
	}

	public void UpdateHint(string newText) {
		txtHint.text = newText;
	}
	public void ConfirmHint() {
		if (entityInfo != null) {
			actionManager.ConfirmWithEntiry(entityInfo);
		}
	}
	public void ExitHint() {
		goHintBox.SetActive(false);
		actionManager.OnInteractionFinished(entityInfo);
		entityInfo = null;
	}
}

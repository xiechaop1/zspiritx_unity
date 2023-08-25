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
	public WebViewBehaviour webViewQuiz;

	public void ShowHint(string hint, string textComfirm = "ȷ��") {
		goHintBox.SetActive(true);
		txtHint.text = hint;
		if (string.IsNullOrWhiteSpace(textComfirm)) {
			btnComfirm.SetActive(false);
		} else {
			btnComfirm.SetActive(true);
			txtConfirmBtn.text = textComfirm;
		}
	}

	public void ShowHint(ItemInfo info, string textCancel = "ȡ��") {
		goHintBox.SetActive(true);
		txtHint.text = info.strHintbox;
		if (info is FieldEntityInfo) {
			Vector3 pos;
			if (((FieldEntityInfo)info).TryGetScenePos(out pos)) {
				txtHint.text += "\n �������:" + pos.ToString();
			}
		}
		entityInfo = info;
		btnComfirm.SetActive(false);
	}
	public void ShowCollectableHint(ItemInfo info, string textCancel = "ȡ��") {
		goHintBox.SetActive(true);
		txtHint.text = info.strHintbox;
		if (info is FieldEntityInfo) {
			Vector3 pos;
			if (((FieldEntityInfo)info).TryGetScenePos(out pos)) {
				txtHint.text += "\n �������:" + pos.ToString();
			}
		}
		entityInfo = info;
		btnComfirm.SetActive(true);
		txtConfirmBtn.text = "��¼��Ϣ";
	}
	public void ShowCollectableItem(ItemInfo info, string textCancel = "ȡ��") {
		goHintBox.SetActive(true);
		txtHint.text = info.strHintbox;
		if (info is FieldEntityInfo) {
			Vector3 pos;
			if (((FieldEntityInfo)info).TryGetScenePos(out pos)) {
				txtHint.text += "\n �������:" + pos.ToString();
			}
		}
		entityInfo = info;
		btnComfirm.SetActive(true);
		txtConfirmBtn.text = "�ռ�����";
	}

	public void ShowQuiz(ItemInfo info){
		webViewQuiz.StartWebView("quiz.html");
	}

	Queue<string> queDialog = new Queue<string>();
	public void ShowDialog(ItemInfo info, string textCancel = "ȡ��") {
		goHintBox.SetActive(true);
		string rawString = info.strHintbox;
		queDialog = new Queue<string>(rawString.Split('^'));
		entityInfo = info;
		AdvancedDialog();
		if (info is FieldEntityInfo) {
			Vector3 pos;
			if (((FieldEntityInfo)info).TryGetScenePos(out pos)) {
				txtHint.text += "\n �������:" + pos.ToString();
			}
		}
	}
	public void AdvancedDialog() {
		if (queDialog.Count > 0) {
			txtHint.text = queDialog.Dequeue();
		}
		if (queDialog.Count > 0) {
			btnComfirm.SetActive(true);
			txtConfirmBtn.text = "����";
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

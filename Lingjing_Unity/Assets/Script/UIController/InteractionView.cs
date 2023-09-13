using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionView : MonoBehaviour {
	public EntityActionManager actionManager;
	public AudioSource voiceLogPlayer;
	public GameObject goHintBox;
	public Text txtHint;
	public GameObject btnComfirm;
	public Text txtConfirmBtn;
	public Text txtExitBtn;
	private ItemInfo entityInfo;
	public WebViewBehaviour webViewQuiz;

	public void Awake() {
		webViewQuiz.OnCallback += OnQuizCallback;
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

	public void ShowHint(ItemInfo info, string textCancel = "取消") {
		goHintBox.SetActive(true);
		txtHint.text = info.strHintbox;
		if (info is FieldEntityInfo) {
			Vector3 pos;
			if (((FieldEntityInfo)info).TryGetStagePos(out pos)) {
				txtHint.text += "\n 相对场景坐标:" + pos.ToString();
			}
			if (((FieldEntityInfo)info).TryGetUserPos(out pos)) {
				txtHint.text += "\n 相对用户坐标:" + pos.ToString();
			}
		}
		entityInfo = info;
		btnComfirm.SetActive(false);
	}
	public void ShowCollectableHint(ItemInfo info, string textCancel = "取消") {
		goHintBox.SetActive(true);
		txtHint.text = info.strHintbox;
		if (info is FieldEntityInfo) {
			Vector3 pos;
			if (((FieldEntityInfo)info).TryGetStagePos(out pos)) {
				txtHint.text += "\n 相对场景坐标:" + pos.ToString();
			}
			if (((FieldEntityInfo)info).TryGetUserPos(out pos)) {
				txtHint.text += "\n 相对用户坐标:" + pos.ToString();
			}
		}
		entityInfo = info;
		btnComfirm.SetActive(true);
		txtConfirmBtn.text = "记录信息";
	}
	public void ShowCollectableItem(ItemInfo info, string textCancel = "取消") {
		goHintBox.SetActive(true);
		txtHint.text = info.strHintbox;
		if (info is FieldEntityInfo) {
			Vector3 pos;
			if (((FieldEntityInfo)info).TryGetStagePos(out pos)) {
				txtHint.text += "\n 相对场景坐标:" + pos.ToString();
			}
			if (((FieldEntityInfo)info).TryGetUserPos(out pos)) {
				txtHint.text += "\n 相对用户坐标:" + pos.ToString();
			}
		}
		entityInfo = info;
		btnComfirm.SetActive(true);
		txtConfirmBtn.text = "收集道具";
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

	public GameObject goNPCBox;
	public Text txtNPC;

	#region New NPC Interaction
	public GameObject[] btnSelects;
	public Text[] txtSelects;
	public Text txtNPCName;

	public void ShowNPCLog(ItemInfo info) {
		entityInfo = info;
		goNPCBox.SetActive(true);
		AdvancedLog(entityInfo.currDialog);
		if (entityInfo is FieldEntityInfo) {
			Vector3 pos;
			if (((FieldEntityInfo)info).TryGetStagePos(out pos)) {
				txtHint.text += "\n 相对场景坐标:" + pos.ToString();
			}
			if (((FieldEntityInfo)info).TryGetUserPos(out pos)) {
				txtHint.text += "\n 相对用户坐标:" + pos.ToString();
			}
		}
	}

	public void ExitNPCLog() {
		goNPCBox.SetActive(false);
		actionManager.OnInteractionFinished(entityInfo);
		entityInfo = null;
	}

	public void AdvancedLog(int selection) {
		DialogSentence nextDialog;
		if (selection == 0) {
			if (string.IsNullOrWhiteSpace(entityInfo.currDialog.sentence)) {
				return;
			}
			if (entityInfo.currDialog.userSelections.Length != 0) {
				return;
			}
			nextDialog = entityInfo.currDialog.nextSentence[0];
		} else if (selection <= entityInfo.currDialog.nextSentence.Length) {
			nextDialog = entityInfo.currDialog.nextSentence[selection - 1];
		} else {
			return;
		}
		AdvancedLog(nextDialog);
	}
	void AdvancedLog(DialogSentence sentence) {
		voiceLogPlayer.Pause();
		if (!string.IsNullOrWhiteSpace(sentence.sentence)) {
			entityInfo.currDialog = sentence;
			txtNPC.text = sentence.sentence;
			//if (!string.IsNullOrWhiteSpace(sentence.name)) 
				txtNPCName.text = sentence.name;
			
			for (int i = 0; i < btnSelects.Length; i++) {
				if (i < sentence.userSelections.Length) {
					btnSelects[i].SetActive(true);
					txtSelects[i].text = sentence.userSelections[i];
				} else {
					btnSelects[i].SetActive(false);
				}
			}

			if (sentence.sentenceClip != null) {
				voiceLogPlayer.clip = sentence.sentenceClip;
				voiceLogPlayer.Play();
			}
		} else if (!string.IsNullOrWhiteSpace(sentence.quizID)) {
			entityInfo.currDialog = sentence;
			webViewQuiz.StartWebView(Network.HttpUrlInfo.urlLingjingQuiz + sentence.quizID);
		} else if (!string.IsNullOrWhiteSpace(sentence.url)) {
			entityInfo.currDialog = sentence;
			webViewQuiz.StartWebView(sentence.url);
		} else {
			//Debug.Log("XXX");
			entityInfo.currDialog = sentence.nextSentence[0];
			ExitNPCLog();
		}

	}
	void OnQuizCallback(string msg) {
		string[] args = msg.Split('&');
		if (args[0] == "WebViewOff") {
			webViewQuiz.SetVisibility(false);
			try {
				if (args.Length > 1) {
					if (args[1] == "TrueAnswer") {
						AdvancedLog(1);
					} else if (args[1] == "FalseAnswer") {
						AdvancedLog(2);
					}
				} else {
					AdvancedLog(1);
				}
			} catch (Exception) {
				ExitQuiz();
			}
		}
	}
	#endregion

	public void ShowQuiz(ItemInfo info) {
		entityInfo = info;
		webViewQuiz.StartWebView("quiz.html");
	}
	public void ExitQuiz() {
		actionManager.OnInteractionFinished(entityInfo);
		entityInfo = null;
	}
}

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
		//webViewQuiz.OnWebClose += OnQuizClose;
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
			if (((FieldEntityInfo)info).TryGetScenePos(out pos)) {
				txtHint.text += "\n 相对坐标:" + pos.ToString();
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
			if (((FieldEntityInfo)info).TryGetScenePos(out pos)) {
				txtHint.text += "\n 相对坐标:" + pos.ToString();
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
			if (((FieldEntityInfo)info).TryGetScenePos(out pos)) {
				txtHint.text += "\n 相对坐标:" + pos.ToString();
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

	public void ShowNPCLog(ItemInfo info) {
		entityInfo = info;
		goNPCBox.SetActive(true);
		AdvancedLog(entityInfo.currDialog);
		if (entityInfo is FieldEntityInfo) {
			if (((FieldEntityInfo)entityInfo).TryGetScenePos(out Vector3 pos)) {
				txtNPC.text += "\n 相对坐标:" + pos.ToString();
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
	#region OLD NPC Interaction
	//public GameObject btnQuiz;
	//public GameObject btnDialog;

	//Queue<string> queDialog = new Queue<string>();

	//public void ShowNPCUI(ItemInfo info) {
	//	entityInfo = info;
	//	goNPCBox.SetActive(true);
	//	string npcIntro = "";
	//	string rawString = entityInfo.strHintbox;
	//	List<string> lstDialog;
	//	if (JSONReader.TryPraseString(rawString, "Intro", ref npcIntro)) {
	//		txtNPC.text = npcIntro;
	//		btnQuiz.SetActive(JSONReader.ContainsKey(rawString, "Quiz"));

	//		if (JSONReader.TryPraseArray(rawString, "Dialog", out lstDialog)) {
	//			queDialog = new Queue<string>(lstDialog);
	//			btnDialog.SetActive(queDialog.Count > 0);
	//		} else {
	//			btnDialog.SetActive(false);
	//		}
	//	} else if (JSONReader.TryPraseArray(rawString, "Dialog", out lstDialog)) {
	//		queDialog = new Queue<string>(lstDialog);
	//		btnDialog.SetActive(queDialog.Count > 0);
	//		AdvancedDialog();
	//	} else {
	//		txtNPC.text = rawString;
	//		btnQuiz.SetActive(false);
	//		btnDialog.SetActive(false);
	//	}
	//	if (entityInfo is FieldEntityInfo) {
	//		Vector3 pos;
	//		if (((FieldEntityInfo)entityInfo).TryGetScenePos(out pos)) {
	//			txtNPC.text += "\n 相对坐标:" + pos.ToString();
	//		}
	//	}
	//}
	//public void ShowQuiz() {
	//	btnQuiz.SetActive(false);
	//	btnDialog.SetActive(false);
	//	webViewQuiz.StartWebView("quiz.html");
	//}
	//public void ConfirmNPCUI() {
	//	//if (queDialog == null) {
	//	//	ShowDialog();
	//	//} else {
	//	AdvancedDialog();
	//	//}
	//}

	//public void ExitNPCUI() {
	//	//queDialog = null;
	//	goNPCBox.SetActive(false);
	//	actionManager.OnInteractionFinished(entityInfo);
	//	entityInfo = null;
	//}

	//public void ShowQuiz(ItemInfo info) {
	//	entityInfo = info;
	//	webViewQuiz.StartWebView("quiz.html");
	//	//ShowQuiz();
	//}
	//public void OnQuizClose() {
	//	ExitNPCUI();
	//}

	//public void ShowDialog() {
	//	btnQuiz.SetActive(false);
	//	List<string> lstDialog;
	//	if (JSONReader.TryPraseArray(entityInfo.strHintbox, "Dialog", out lstDialog)) {
	//		queDialog = new Queue<string>(lstDialog);
	//	} else {
	//		string rawString = entityInfo.strHintbox;
	//		queDialog = new Queue<string>(rawString.Split('^'));
	//	}

	//	AdvancedDialog();
	//}
	//public void AdvancedDialog() {
	//	btnQuiz.SetActive(false);
	//	if (queDialog.Count > 0) {
	//		txtNPC.text = queDialog.Dequeue();
	//	}
	//	if (queDialog.Count > 0) {
	//		btnDialog.SetActive(true);
	//		txtConfirmBtn.text = "继续";
	//	} else {
	//		btnDialog.SetActive(false);
	//	}
	//}
	#endregion
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Config;

public class InteractionView : MonoBehaviour {
	public SceneLoadManager sceneLoader;
	public FieldStageManager stageManager;
	public EntityActionManager actionManager;
	public InputGPSManager gpsManager;
	public AudioSource voiceLogPlayer;
	public GameObject goHomeIcon;
	public GameObject goHintBox;
	public Text txtHint;
	public GameObject goNoteBox;
	public Text txtNote;
	public GameObject btnComfirm;
	public Text txtConfirmBtn;
	public Text txtExitBtn;
	private ItemInfo entityInfo;
	public WebViewBehaviour webViewQuiz;
	public WebViewBehaviour webViewUtility;
	public WebViewBehaviour webViewMap;
	public MonoWebView webViewTask;

	public void Awake() {
		webViewQuiz.OnCallback += OnQuizCallback;
		webViewUtility.OnCallback += OnUtilityCallback;
	}

	float timer = 0f;
	public void Update() {
		if (isMap) {
			if (timer <= 0f) {
				webViewMap.Eval(string.Format("getLocation({0},{1});",
					gpsManager.camLatitude,
					gpsManager.camLongitude
					));
				timer = 3f;
			}
			timer -= Time.deltaTime;

		}
	}

	bool isBackpack = false;
	public void ToggleBackpack() {
		if (isInDialog || isMap)
			return;

		isBackpack = !isBackpack;
		if (isBackpack) {
			isMap = false;
			isTask = false;
			webViewUtility.StartWebView(Network.HttpUrlInfo.urlLingjingHtml +
				string.Format("baggageh5/all?user_id={0}&session_id={1}&story_id={2}",
					ConfigInfo.userId,
					ConfigInfo.sessionId,
					ConfigInfo.storyId));
		} else {
			webViewUtility.SetVisibility(false);
		}
	}
	bool isMap = false;
	public void ToggleMap() {
		//isMap = !isMap;
		//if (isMap) {
		//	webViewMap.StartWebView(Network.HttpUrlInfo.urlLingjingBackpack +
		//		string.Format("user_id={0}&session_id={1}&story_id={2}",
		//			ConfigInfo.userId,
		//			ConfigInfo.sessionId,
		//			ConfigInfo.storyId));
		//} else {
		//	webViewMap.SetVisibility(false);
		//}

		if (isInDialog || isBackpack)
			return;

		isMap = !isMap;
		if (isMap) {
			isBackpack = false;
			isTask = false;
			///maph5/get?user_id=1&session_id=1&story_id=1&story_stage_id=1&user_lng=116.1234&user_lat=39.1234&team_id=0
			webViewMap.StartWebView(Network.HttpUrlInfo.urlLingjingHtml +
				string.Format("maph5/get?user_id={0}&session_id={1}&story_id={2}&story_stage_id={3}&user_lat={4}&user_lng={5}&team_id=0",
					ConfigInfo.userId,
					ConfigInfo.sessionId,
					ConfigInfo.storyId,
					stageManager.currentStage.stroy_stage_id,
					gpsManager.camLatitude,
					gpsManager.camLongitude
					));
		} else {
			webViewMap.SetVisibility(false);
		}
	}
	bool isTask = false;
	public void ToggleTask() {
		//isTask = !isTask;
		//if (isTask) {
		//	webViewTask.StartWebView("https://h5.zspiritx.com.cn/home");
		//} else {
		//	webViewTask.SetVisibility(false);
		//}

		//if (isInDialog) 
		return;

		isTask = !isTask;
		if (isTask) {
			isBackpack = false;
			isMap = false;
			//webViewUtility.StartWebView(Network.HttpUrlInfo.urlLingjingBackpack +
			//	string.Format("user_id={0}&session_id={1}&story_id={2}",
			//		ConfigInfo.userId,
			//		ConfigInfo.sessionId,
			//		ConfigInfo.storyId));
		} else {
			webViewUtility.SetVisibility(false);
		}
	}

	void OnUtilityCallback(string msg) {
		try {
			JSONReader jsonMsg = new JSONReader(msg);
			int tmpInt = 0;
			if (jsonMsg.TryPraseInt("WebViewOff", ref tmpInt) && tmpInt == 1) {
				webViewUtility.SetVisibility(false);
				isBackpack = false;
				isMap = false;
				isTask = false;
			}
		} catch (Exception) {
			string[] args = msg.Split('&');
			if (args[0] == "WebViewOff") {
				webViewUtility.SetVisibility(false);
				isBackpack = false;
				isMap = false;
				isTask = false;
			}
		}
	}

	List<NotificationMessage> lstMsg = new List<NotificationMessage>();
	Queue<NotificationMessage> queMsg = new Queue<NotificationMessage>();

	public void AddNotice(NotificationMessage note) {
		foreach (var msg in lstMsg) {
			if (msg.id == note.id) {
				return;
			}
		}
		queMsg.Enqueue(note);
		lstMsg.Add(note);
		if (!goNoteBox.activeSelf) {
			goNoteBox.SetActive(true);
			ShowNextNotice();
			StartCoroutine(AsyncAutoNextNotice(queMsg.Count > 0 ? queMsg.Peek() : null));
		}
	}
	public void ShowNextNotice() {
		if (queMsg.Count > 0) {
			NotificationMessage note = queMsg.Dequeue();
			txtNote.text = note.msg;
		} else {
			goNoteBox.SetActive(false);
		}
	}

	IEnumerator AsyncAutoNextNotice(NotificationMessage nextNotice) {
		yield return new WaitForSeconds(5f);
		if (nextNotice == null || queMsg.Peek() == nextNotice) {
			ShowNextNotice();
		}
	}



	public void ShowHint(string hint, string textComfirm = "确认") {
		goHintBox.SetActive(true);
		goHomeIcon.SetActive(false);
		txtHint.text = hint;
		if (string.IsNullOrWhiteSpace(textComfirm)) {
			btnComfirm.SetActive(false);
		} else {
			btnComfirm.SetActive(true);
			txtConfirmBtn.text = textComfirm;
		}
	}

	public void ShowHint(ItemInfo info, string textCancel = "取消") {
		entityInfo = info;
		SetHintBoxActive(true);
		//goHintBox.SetActive(true);
		//goHomeIcon.SetActive(false);
		//info.SetInteractionState(true);
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
		btnComfirm.SetActive(false);
	}
	public void ShowCollectableHint(ItemInfo info, string textCancel = "取消") {
		entityInfo = info;
		SetHintBoxActive(true);
		//goHintBox.SetActive(true);
		//goHomeIcon.SetActive(false);
		//info.SetInteractionState(true);
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
		btnComfirm.SetActive(true);
		txtConfirmBtn.text = "记录信息";
	}
	public void ShowCollectableItem(ItemInfo info, string textCancel = "取消") {
		entityInfo = info;
		SetHintBoxActive(true);
		//goHintBox.SetActive(true);
		//goHomeIcon.SetActive(false);
		//info.SetInteractionState(true);
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
		SetHintBoxActive(false);
		//goHintBox.SetActive(false);
		//goHomeIcon.SetActive(true);
		//entityInfo.SetInteractionState(false);
		actionManager.OnInteractionFinished(entityInfo);
		entityInfo = null;
	}
	void SetHintBoxActive(bool isActive) {
		goHintBox.SetActive(isActive);
		goHomeIcon.SetActive(!isActive);
		if (entityInfo != null) {
			entityInfo.SetInteractionState(isActive);
		}
	}

	public void ExitScene() {
		if (goHintBox.activeInHierarchy) {
			ExitHint();
		}

		sceneLoader.ExitScene();
	}

	public GameObject goNPCBox;
	public Text txtNPC;

	#region New NPC Interaction
	public GameObject[] btnSelects;
	public Text[] txtSelects;
	public Text txtNPCName;

	bool isInDialog = false;
	public void ShowNPCLog(ItemInfo info) {
		entityInfo = info;
		goNPCBox.SetActive(true);
		goHomeIcon.SetActive(false);
		webViewUtility.SetVisibility(false);
		isInDialog = true;
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
		goHomeIcon.SetActive(true);
		isInDialog = false;
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
			//webViewQuiz.StartWebView(Network.HttpUrlInfo.urlLingjingQuiz + sentence.quizID + "&session_id=" + ConfigInfo.sessionId);
			webViewQuiz.StartWebView(Network.HttpUrlInfo.urlLingjingQuiz +
				string.Format("id={0}&user_id={1}&session_id={2}",
				   sentence.quizID,
				   ConfigInfo.userId,
				   ConfigInfo.sessionId));
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
		try {
			JSONReader jsonMsg = new JSONReader(msg);
			int tmpInt = 0;
			if (jsonMsg.TryPraseInt("WebViewOff", ref tmpInt) && tmpInt == 1) {
				webViewQuiz.SetVisibility(false);
			}
			if (jsonMsg.TryPraseInt("AnswerType", ref tmpInt)) {
				AdvancedLog(tmpInt);
			} else {
				AdvancedLog(1);
			}
		} catch (Exception) {
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

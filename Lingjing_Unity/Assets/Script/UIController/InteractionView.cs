using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Config;

public class InteractionView : MonoBehaviour {
	public ResourcesLibrary resourcesLib;
	public SceneLoadManager sceneLoader;
	public FieldStageManager stageManager;
	public FieldEntityManager entityManager;
	public EntityActionManager actionManager;
	public CombatManager combatManager;
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
	//private ItemInfo entityInfo;
	public MonoWebView webViewQuiz;
	public MonoWebView webViewUtility;
	public MonoWebView webViewMap;
	public MonoWebView webViewTask;

	public void Awake() {
		webViewQuiz.OnCallback += OnQuizCallback;
		webViewUtility.OnCallback += OnUtilityCallback;
		webViewMap.OnCallback += OnMapCallback;
		webViewTask.OnCallback += OnTaskCallback;
		entityManager.OnEntityPlaced += EntityPlacedAction;
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
		if (isInDialog || isMap || isTask)
			return;

		isBackpack = !isBackpack;
		if (isBackpack) {
			isMap = false;
			isTask = false;
			var (session_model_id, stroy_model_id, model_id, story_model_detail_id) = actionManager.GetEntityID();
			webViewUtility.StartWebView(Network.HttpUrlInfo.urlLingjingHtml +
				string.Format("baggageh5/all?user_id={0}&session_id={1}&story_id={2}&target_session_model_id={3}&target_story_model_id={4}&target_model_id={5}&target_story_model_detail_id={6}",
					ConfigInfo.userId,
					ConfigInfo.sessionId,
					ConfigInfo.storyId,
					session_model_id,
					stroy_model_id,
					model_id,
					story_model_detail_id));
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

		if (isInDialog || isBackpack || isTask)
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
					stageManager.currentStage.story_stage_id,
					gpsManager.camLatitude,
					gpsManager.camLongitude));
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

		if (isInDialog || isBackpack || isMap)
			return;

		isTask = !isTask;
		if (isTask) {
			isBackpack = false;
			isMap = false;
			//myh5/my?user_id=1&session_id=1&story_id=1
			//knowledgeh5/all?story_id=1&session_id=1&user_id=1 
			webViewTask.StartWebView(Network.HttpUrlInfo.urlLingjingHtml +
				string.Format("myh5/my?user_id={0}&session_id={1}&story_id={2}",
					ConfigInfo.userId,
					ConfigInfo.sessionId,
					ConfigInfo.storyId));
		} else {
			webViewTask.SetVisibility(false);
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
			string tmp = "";
			if (jsonMsg.TryPraseString("ExecAction", ref tmp) && !string.IsNullOrWhiteSpace(tmp) && tmp != "null") {
				actionManager.ExecuteAction(tmp);
				//SerializedEntityAction entityAction = new SerializedEntityAction(tmp);
				//DialogAction(entityAction);
			}
			if (jsonMsg.TryPraseString("GotoAction", ref tmp) && !string.IsNullOrWhiteSpace(tmp) && tmp != "null") {
				actionManager.GotoAction(tmp);
				//if (entityInfo != null) {
				//	try {
				//		entityInfo.currDialog = entityInfo.lstDialogs.First(x => x.localID == tmp);
				//		if (entityInfo.currDialog.isEmpty) {
				//			DialogAction(entityInfo.currDialog);
				//		}
				//		if (isTask || isBackpack || isMap) {
				//		} else {
				//			ShowNPCLog(entityInfo);
				//		}
				//	} catch (Exception) {

				//	}
				//}
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

	void OnMapCallback(string msg) {
		try {
			JSONReader jsonMsg = new JSONReader(msg);
			int tmpInt = 0;
			if (jsonMsg.TryPraseInt("WebViewOff", ref tmpInt) && tmpInt == 1) {
				webViewMap.SetVisibility(false);
				isBackpack = false;
				isMap = false;
				isTask = false;
			}
		} catch (Exception) {
			string[] args = msg.Split('&');
			if (args[0] == "WebViewOff") {
				webViewMap.SetVisibility(false);
				isBackpack = false;
				isMap = false;
				isTask = false;
			}
		}
	}

	void OnTaskCallback(string msg) {
		try {
			JSONReader jsonMsg = new JSONReader(msg);
			int tmpInt = 0;
			if (jsonMsg.TryPraseInt("WebViewOff", ref tmpInt) && tmpInt == 1) {
				webViewTask.SetVisibility(false);
				isBackpack = false;
				isMap = false;
				isTask = false;
			}
		} catch (Exception) {
			string[] args = msg.Split('&');
			if (args[0] == "WebViewOff") {
				webViewTask.SetVisibility(false);
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

	public void ShowTextHint(string hint, string textComfirm = "确认") {
		goHintBox.SetActive(true);
		//goHomeIcon.SetActive(false);
		txtHint.text = hint;
		if (string.IsNullOrWhiteSpace(textComfirm)) {
			btnComfirm.SetActive(false);
		} else {
			btnComfirm.SetActive(true);
			txtConfirmBtn.text = textComfirm;
		}
	}

	public void ShowHint(string hint) {//ItemInfo info, string textCancel = "取消") {
									   //entityInfo = info;
		SetHintBoxActive(true);
		//goHintBox.SetActive(true);
		//goHomeIcon.SetActive(false);
		//info.SetInteractionState(true);
		txtHint.text = hint;
		//if (info is FieldEntityInfo) {
		//	Vector3 pos;
		//	if (((FieldEntityInfo)info).TryGetStagePos(out pos)) {
		//		txtHint.text += "\n 相对场景坐标:" + pos.ToString();
		//	}
		//	if (((FieldEntityInfo)info).TryGetUserPos(out pos)) {
		//		txtHint.text += "\n 相对用户坐标:" + pos.ToString();
		//	}
		//}
		btnComfirm.SetActive(false);
	}
	public void ShowCollectableHint(string hint) {//ItemInfo info, string textCancel = "取消") {
												  //entityInfo = info;
		SetHintBoxActive(true);
		//goHintBox.SetActive(true);
		//goHomeIcon.SetActive(false);
		//info.SetInteractionState(true);
		txtHint.text = hint;
		//if (info is FieldEntityInfo) {
		//	Vector3 pos;
		//	if (((FieldEntityInfo)info).TryGetStagePos(out pos)) {
		//		txtHint.text += "\n 相对场景坐标:" + pos.ToString();
		//	}
		//	if (((FieldEntityInfo)info).TryGetUserPos(out pos)) {
		//		txtHint.text += "\n 相对用户坐标:" + pos.ToString();
		//	}
		//}
		btnComfirm.SetActive(true);
		txtConfirmBtn.text = "记录信息";
	}
	public void ShowCollectableItem(string hint) {//ItemInfo info, string textCancel = "取消") {
												  //entityInfo = info;
		SetHintBoxActive(true);
		//goHintBox.SetActive(true);
		//goHomeIcon.SetActive(false);
		//info.SetInteractionState(true);
		txtHint.text = hint;
		//if (info is FieldEntityInfo) {
		//	Vector3 pos;
		//	if (((FieldEntityInfo)info).TryGetStagePos(out pos)) {
		//		txtHint.text += "\n 相对场景坐标:" + pos.ToString();
		//	}
		//	if (((FieldEntityInfo)info).TryGetUserPos(out pos)) {
		//		txtHint.text += "\n 相对用户坐标:" + pos.ToString();
		//	}
		//}
		btnComfirm.SetActive(true);
		txtConfirmBtn.text = "收集道具";
	}

	public void UpdateHint(string newText) {
		txtHint.text = newText;
	}
	public void ConfirmHint() {
		//if (entityInfo != null) {
		actionManager.ConfirmWithEntiry();
		//}
	}
	public void ExitHint() {
		actionManager.ExitEntity();
		//if (entityInfo != null) {
		//SetHintBoxActive(false);
		//goHintBox.SetActive(false);
		//goHomeIcon.SetActive(true);
		//entityInfo.SetInteractionState(false);
		//actionManager.OnInteractionFinished();
		//ShowEntitySelection();
		//entityInfo = null;
		//}
	}
	public void HideHint() {
		SetHintBoxActive(false);
	}
	void SetHintBoxActive(bool isActive) {
		goHintBox.SetActive(isActive);
		//goHomeIcon.SetActive(!isActive);
		//if (entityInfo != null) {
		//actionManager.SetEntityInteractive(isActive);
		//}
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
	public bool ShowNPCLog() {//ItemInfo info) {
		if (isTask || isBackpack || isMap) {
			return false;
		}
		//if (string.IsNullOrEmpty(info.strHintbox) || info.currDialog == null) {
		//	return false;
		//}
		//entityInfo = info;
		SetNPCLogActive(true);
		actionManager.SetEntityInteractive(true);
		//goNPCBox.SetActive(true);
		//goHomeIcon.SetActive(false);
		//isInDialog = true;
		webViewUtility.SetVisibility(false);
		//if (entityInfo.currDialog.isEmpty) {
		//	entityInfo.currDialog = entityInfo.currDialog.nextAction[0];
		//}
		//AdvancedLog(entityInfo.currDialog, entityInfo as FieldEntityInfo);
		//if (entityInfo is FieldEntityInfo) {
		//	Vector3 pos;
		//	if (((FieldEntityInfo)info).TryGetStagePos(out pos)) {
		//		txtHint.text += "\n 相对场景坐标:" + pos.ToString();
		//	}
		//	if (((FieldEntityInfo)info).TryGetUserPos(out pos)) {
		//		txtHint.text += "\n 相对用户坐标:" + pos.ToString();
		//	}
		//}
		return true;
	}

	public void ExitNPCLog() {
		//if (entityInfo != null) {
		SetNPCLogActive(false);
		//goNPCBox.SetActive(false);
		//goHomeIcon.SetActive(true);
		//isInDialog = false;
		actionManager.OnInteractionFinished();
		//ShowEntitySelection();
		//entityInfo = null;
		//}
	}

	public void SetNPCLogActive(bool isActive) {
		goNPCBox.SetActive(isActive);
		//goHomeIcon.SetActive(!isActive);
		isInDialog = isActive;
		//if (entityInfo != null) {
		//actionManager.SetEntityInteractive(isActive);
		//}
	}
	public void AdvancedLog(int selection) { 
		actionManager.SelectedDialog(selection);
	}
	//	SerializedEntityAction nextDialog;
	//	if (selection == 0) {
	//		if (string.IsNullOrWhiteSpace(entityInfo.currDialog.sentence)) {
	//			return;
	//		}
	//		if (entityInfo.currDialog.userSelections.Length != 0) {
	//			return;
	//		}
	//		nextDialog = entityInfo.currDialog.nextAction[0];
	//	} else if (selection <= entityInfo.currDialog.nextAction.Length) {
	//		nextDialog = entityInfo.currDialog.nextAction[selection - 1];
	//	} else {
	//		return;
	//	}
	//	entityInfo.currDialog = nextDialog;
	//	AdvancedLog(nextDialog, entityInfo as FieldEntityInfo);
	//}
	public void AdvancedLog(SerializedEntityAction sentence) {
		voiceLogPlayer.Pause();
		if (!string.IsNullOrWhiteSpace(sentence.sentence)) {
			//entityInfo.currDialog = sentence;
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
			if (sentence.sentenceClip == null) {
				sentence.LinkClips(resourcesLib.audios);
			}
			if (sentence.sentenceClip != null) {
				voiceLogPlayer.clip = sentence.sentenceClip;
				voiceLogPlayer.Play();
			}
		} else if (!string.IsNullOrWhiteSpace(sentence.quizID)) {
			//entityInfo.currDialog = sentence;
			webViewQuiz.StartWebView(Network.HttpUrlInfo.urlLingjingQuiz +
				string.Format("id={0}&user_id={1}&session_id={2}",
				   sentence.quizID,
				   ConfigInfo.userId,
				   ConfigInfo.sessionId));
		} else if (!string.IsNullOrWhiteSpace(sentence.url)) {
			//entityInfo.currDialog = sentence;
			webViewQuiz.StartWebView(sentence.url);
		} else {
			//Debug.Log("XXX");
			//entityInfo.currDialog = sentence;//.nextAction[0];
			ExitNPCLog();
		}
		//if (entityInfo is FieldEntityInfo) {
		//	DialogAction(sentence, entityInfo as FieldEntityInfo);
		//} else {
		//DialogAction(sentence, entity);
		//}

	}
	//public void DialogAction(SerializedEntityAction entityAction, FieldEntityInfo entity = null) {
	//	if (!string.IsNullOrWhiteSpace(entityAction.combatInfo)) {
	//		Debug.Log(entityAction.combatInfo);
	//		combatManager.startCallback += OnCombatStartCallback;
	//		combatManager.PrepareBattleGround();
	//		combatManager.finishCallback += OnCombatFinishCallback;
	//	}
	//	stageManager.ShowEntities(entityAction.showModels);
	//	stageManager.HideEntities(entityAction.hideModels);
	//	stageManager.PickupEntities(entityAction.pickupModels);

	//	if (entity != null) {
	//		entity.ForcedMove(entityAction.displacement);
	//	}
	//}

	//void OnCombatStartCallback(string msg) {
	//	SetNPCLogActive(false);
	//}
	//void OnCombatFinishCallback(string msg) {
	//	SetNPCLogActive(true);
	//	JSONReader jsonMsg = new JSONReader(msg);
	//	int tmpInt = 0;
	//	if (jsonMsg.TryPraseInt("AnswerType", ref tmpInt)) {
	//		AdvancedLog(tmpInt);
	//	} else {
	//		AdvancedLog(1);
	//	}
	//	combatManager.startCallback -= OnCombatStartCallback;
	//	combatManager.finishCallback -= OnCombatFinishCallback;
	//}
	public void OnQuizCallback(string msg) {
		try {
			JSONReader jsonMsg = new JSONReader(msg);
			int tmpInt = 0;
			if (jsonMsg.TryPraseInt("WebViewOff", ref tmpInt) && tmpInt == 1) {
				webViewQuiz.SetVisibility(false);
				if (jsonMsg.TryPraseInt("AnswerType", ref tmpInt)) {
					actionManager.SelectedDialog(tmpInt);
					//AdvancedLog(tmpInt);
				} else {
					actionManager.SelectedDialog(1);
					//AdvancedLog(1);
				}
			}

		} catch (Exception) {
			//string[] args = msg.Split('&');
			//if (args[0] == "WebViewOff") {
			//	webViewQuiz.SetVisibility(false);
			//	try {
			//		if (args.Length > 1) {
			//			if (args[1] == "TrueAnswer") {
			//				AdvancedLog(1);
			//			} else if (args[1] == "FalseAnswer") {
			//				AdvancedLog(2);
			//			}
			//		} else {
			//			AdvancedLog(1);
			//		}
			//	} catch (Exception) {
			//		//ExitQuiz();
			//	}
			//}
		}
	}
	#endregion

	#region EntitySelectionHint
	public GameObject goEntitySelectionUI;
	public Text txtEntityHint;
	public void ShowEntitySelection(string entityName) {
		//if (actionManager.selectedEntityInfo != null) {
		goEntitySelectionUI.SetActive(true);
		txtEntityHint.text = entityName;
		//}
	}
	public void HideEntitySelection() {
		goEntitySelectionUI.SetActive(false);
		actionManager.DeselectEntity();
	}

	#endregion

	#region EntityPlacedHint
	public GameObject goEntityPlacedHint;
	Coroutine CoroutineHideEntityHint = null;
	public void EntityPlacedAction(FieldEntityInfo info) {
		//if (info.actionOnPlaced != null) {
		//	DialogAction(info.actionOnPlaced, info);
		//}
		goEntityPlacedHint.SetActive(true);
		if (CoroutineHideEntityHint != null) {
			StopCoroutine(CoroutineHideEntityHint);
		}
		CoroutineHideEntityHint = StartCoroutine(AsyncHideEntityHint());
	}
	IEnumerator AsyncHideEntityHint() {
		yield return new WaitForSeconds(5f);
		HideEntityHint();
	}
	public void HideEntityHint() {
		goEntityPlacedHint.SetActive(false);
		CoroutineHideEntityHint = null;
	}
	#endregion
	//public void ShowQuiz(ItemInfo info) {
	//	entityInfo = info;
	//	webViewQuiz.StartWebView("quiz.html");
	//}
	//public void ExitQuiz() {
	//	actionManager.OnInteractionFinished(entityInfo);
	//	entityInfo = null;
	//}
}

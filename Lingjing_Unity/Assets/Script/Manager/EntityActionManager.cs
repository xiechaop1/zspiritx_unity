using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Network;
using Config;

[RequireComponent(typeof(InventoryItemManager))]
public class EntityActionManager : MonoBehaviour, IManager {
	private FieldEntityManager entityManager;
	private CombatManager combatManager;
	private FieldStageManager stageManager;
	private WWWManager networkManager;
	public event Action<ItemInfo> eventEntitySelected;
	public InteractionView interactionView;
	private ActionMode actionMode = ActionMode.Idle;
	public ItemInfo selectedEntityInfo = null;
	public void Update() {
#if !UNITY_EDITOR
		if (Input.GetTouch(0).phase==TouchPhase.Began) {
#else
		if (Input.GetMouseButtonDown(0)) {
			//Debug.Log(actionMode);

#endif
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			switch (actionMode) {
				case ActionMode.World:
					if (Physics.Raycast(ray, out hit, 4.0f, 64)) {
						FieldEntityInfo entityInfo;
						if (hit.collider.gameObject.TryGetComponent(out entityInfo)) {
							InteractWithEntity(entityInfo);
						}
					}
					break;
				case ActionMode.Interactive:
					if (Physics.Raycast(ray, out hit, 4.0f, 128)) {
						ARInteractionInfo interactionInfo;
						if (hit.collider.gameObject.TryGetComponent(out interactionInfo)) {
							interactionInfo.SendInteraction();
						}
					}
					break;
				case ActionMode.Idle:
				default:
					break;
			}
		}
		if (actionMode == ActionMode.World && timer < 0) {
			FieldEntityInfo[] lstPlacedEntity = entityManager.arrPlacedEntity;
			Vector3 pos;
			foreach (FieldEntityInfo entityInfo in lstPlacedEntity) {
				if (!entityInfo || !entityInfo.hasProximityDialog) {
					continue;
				}
				if (entityInfo.TryGetUserPos(out pos) && pos.sqrMagnitude < entityInfo.proximityDialog * entityInfo.proximityDialog) {
					switch (entityInfo.enumActionType) {
						case EntityActionType.DialogActor:
							selectedEntityInfo = entityInfo;
							if (ShowNPCDialog()) {
								//if (interactionView.ShowNPCLog(entityInfo)) {
								actionMode = ActionMode.Idle;
								entityInfo.hasProximityDialog = false;
							}
							break;
						default:
							break;
					}
				}
			}
			timer = 1f;
		} else {
			timer -= Time.deltaTime;
		}
	}
	float timer = 1f;
	public void Init(UIEventManager eventManager, params IManager[] managers) {
		foreach (var manager in managers) {
			RegisterManager(manager);
		}
		entityManager.OnEntityPlaced += EntityPlacedAction;
	}
	public void RegisterManager(IManager manager) {
		if (manager is FieldEntityManager) {
			entityManager = manager as FieldEntityManager;
		} else if (manager is WWWManager) {
			networkManager = manager as WWWManager;
		} else if (manager is FieldStageManager) {
			stageManager = manager as FieldStageManager;
		} else if (manager is CombatManager) {
			combatManager = manager as CombatManager;
		}
	}

	public void Set2ARMode() {
		actionMode = ActionMode.World;
	}
	public void Set2IdleMode() {
		actionMode = ActionMode.Idle;
	}

	public string GetEntityName() {
		if (selectedEntityInfo != null) {
			return selectedEntityInfo.strName;
		}
		return "";
	}
	public (int, int, int, int) GetEntityID() {
		if (selectedEntityInfo != null && selectedEntityInfo is FieldEntityInfo) {
			FieldEntityInfo fieldEntity = selectedEntityInfo as FieldEntityInfo;
			return (fieldEntity.session_model_id, fieldEntity.stroy_model_id, fieldEntity.model_id, fieldEntity.story_model_detail_id);
		} else {
			return (0, 0, 0, 0);
		}
	}

	#region EntityActions
	public void EntityPlacedAction(FieldEntityInfo info) {
		if (info.actionOnPlaced != null) {
			ActionWithTarget(info.actionOnPlaced, info);
		}
	}
	public void ExecuteAction(string rawAction) {
		SerializedEntityAction entityAction = new SerializedEntityAction(rawAction);
		ActionWithoutTarget(entityAction);
	}
	public void GotoAction(string actionLocalID) {
		if (selectedEntityInfo != null) {
			try {
				selectedEntityInfo.currDialog = selectedEntityInfo.lstDialogs.First(x => x.localID == actionLocalID);
				if (selectedEntityInfo.currDialog.isEmpty) {
					CurrentDialogAction();
				}
				ShowNPCDialog();
			} catch (Exception) {

			}
		}
	}
	public void SelectedDialog(int selection) {
		SerializedEntityAction nextDialog;
		if (selection == 0) {
			if (string.IsNullOrWhiteSpace(selectedEntityInfo.currDialog.sentence)) {
				return;
			}
			if (selectedEntityInfo.currDialog.userSelections.Length != 0) {
				return;
			}
			nextDialog = selectedEntityInfo.currDialog.nextAction[0];
		} else if (selection <= selectedEntityInfo.currDialog.nextAction.Length) {
			nextDialog = selectedEntityInfo.currDialog.nextAction[selection - 1];
		} else {
			return;
		}
		selectedEntityInfo.currDialog = nextDialog;
		interactionView.AdvancedLog(selectedEntityInfo.currDialog);
		CurrentDialogAction();
	}
	bool ShowNPCDialog() {
		if (string.IsNullOrEmpty(selectedEntityInfo.strHintbox) || selectedEntityInfo.currDialog == null) {
			return false;
		}
		if (interactionView.ShowNPCLog()) {
			if (selectedEntityInfo.currDialog.isEmpty) {
				selectedEntityInfo.currDialog = selectedEntityInfo.currDialog.nextAction[0];
			}
			interactionView.AdvancedLog(selectedEntityInfo.currDialog);
			CurrentDialogAction();
			return true;
		}
		return false;
	}

	public void CurrentDialogAction() {
		ActionWithTarget(selectedEntityInfo.currDialog, selectedEntityInfo as FieldEntityInfo);
	}
	public void ActionWithTarget(SerializedEntityAction entityAction, FieldEntityInfo entity) {
		ActionWithoutTarget(entityAction);
		entity.ForcedMove(entityAction.displacement);
	}
	public void ActionWithoutTarget(SerializedEntityAction entityAction) {
		if (!string.IsNullOrWhiteSpace(entityAction.combatInfo)) {
			combatManager.startCallback += OnCombatStartCallback;
			combatManager.PrepareBattleGround();
			combatManager.finishCallback += OnCombatFinishCallback;
		}
		stageManager.ShowEntities(entityAction.showModels);
		stageManager.HideEntities(entityAction.hideModels);
		stageManager.PickupEntities(entityAction.pickupModels);
	}
	void OnCombatStartCallback(string msg) {
		interactionView.SetNPCLogActive(false);
		SetEntityInteractive(false);
	}
	void OnCombatFinishCallback(string msg) {
		interactionView.SetNPCLogActive(true);
		SetEntityInteractive(true);
		JSONReader jsonMsg = new JSONReader(msg);
		int tmpInt = 0;
		if (jsonMsg.TryPraseInt("AnswerType", ref tmpInt)) {
			SelectedDialog(tmpInt);
		} else {
			SelectedDialog(1);
		}
		combatManager.startCallback -= OnCombatStartCallback;
		combatManager.finishCallback -= OnCombatFinishCallback;
	}


	#endregion

	public void DeselectEntity() {
		selectedEntityInfo = null;
	}
	public void SetEntityInteractive(bool value) {
		selectedEntityInfo?.SetInteractionState(value);
		//selectedEntityInfo.SetInteractionMode(value);
	}
	public void InteractWithEntity(ItemInfo entityInfo) {
		interactionView.HideHint();
		SetEntityInteractive(false);
		//OnInteractionFinished();
		selectedEntityInfo = entityInfo;
		switch (selectedEntityInfo.enumActionType) {
			case EntityActionType.Debug:
				eventEntitySelected?.Invoke(selectedEntityInfo);
				break;
			case EntityActionType.ViewableInfo:
			case EntityActionType.CollectedInfo:
			case EntityActionType.CollectedItem:
				actionMode = ActionMode.Idle;
				interactionView.ShowHint(selectedEntityInfo.strHintbox);
				SetEntityInteractive(true);
				break;
			case EntityActionType.CollectableInfo:
				actionMode = ActionMode.Idle;
				interactionView.ShowCollectableHint(selectedEntityInfo.strHintbox);
				SetEntityInteractive(true);
				break;
			case EntityActionType.InteractiveItem:
				actionMode = ActionMode.Interactive;
				interactionView.ShowHint(selectedEntityInfo.strHintbox);
				SetEntityInteractive(true);
				//selectedEntityInfo.SetInteractionMode(true);
				break;
			case EntityActionType.CollectableItem:
				actionMode = ActionMode.Interactive;
				interactionView.ShowCollectableItem(selectedEntityInfo.strHintbox);
				SetEntityInteractive(true);
				//selectedEntityInfo.SetInteractionMode(true);
				break;
			case EntityActionType.UtilityItem:
				actionMode = ActionMode.Idle;
				selectedEntityInfo.GetComponent<ARUtilityListener>()?.UseARUtility();
				interactionView.ShowHint(selectedEntityInfo.strHintbox);//, "ȷ��");
				SetEntityInteractive(true);
				break;
			case EntityActionType.DialogActor:
				if (ShowNPCDialog()) {
					//if (interactionView.ShowNPCLog(selectedEntityInfo)) {
					actionMode = ActionMode.Idle;
				} else {
					actionMode = ActionMode.Idle;
					interactionView.ShowHint(selectedEntityInfo.strHintbox);
					SetEntityInteractive(true);
				}
				break;
			//case EntityActionType.Quiz:
			//actionMode = ActionMode.Idle;
			//interactionView.ShowQuiz(entityInfo);
			//break;
			//case EntityActionType.PlacableItem:
			default:
				selectedEntityInfo = null;
				actionMode = ActionMode.World;
				break;
		}
	}

	public void InteractWithFile(FileInfo fileInfo) {
		//interactionView.EndHint();
		//SetEntityInteractive(false);
		//OnInteractionFinished();
		//actionMode = ActionMode.Idle;
		//interactionView.ShowTextHint(fileInfo.strHintbox, "");
	}

	public void ConfirmWithEntiry() {
		switch (selectedEntityInfo.enumActionType) {

			case EntityActionType.CollectableInfo:
				selectedEntityInfo.enumActionType = selectedEntityInfo.enumItemType;
				UIEventManager.CallEvent("InventoryItemManager", "AddInfo", selectedEntityInfo);
				ExitEntity();
				break;
			case EntityActionType.CollectableItem:
				selectedEntityInfo.enumActionType = selectedEntityInfo.enumItemType;
				PickUpItem(selectedEntityInfo);
				ExitEntity();
				break;
			case EntityActionType.DialogActor:
				//interactionView.AdvancedDialog();
				break;
			case EntityActionType.Debug:
			case EntityActionType.ViewableInfo:
			case EntityActionType.CollectedInfo:
			case EntityActionType.InteractiveItem:
			default:
				ExitEntity();
				break;
		}
	}
	public void ExitEntity() {
		interactionView.HideHint();
		OnInteractionFinished();
		//SetEntityInteractive(false);
		//actionMode = ActionMode.World;
		//interactionView.ShowEntitySelection();
	}
	public void TryPickUpItem(string entityName) {

	}
	void PickUpItem(ItemInfo entityInfo) {
		StartCoroutine(AsyncPickUpItem(entityInfo));
		UIEventManager.CallEvent("FieldEntityManager", "RemoveFieldEntitys", entityInfo);
		UIEventManager.CallEvent("InventoryItemManager", "AddItem", entityInfo);
	}
	IEnumerator AsyncPickUpItem(ItemInfo entityInfo) {
		//is_test=1&session_id=6&user_id=1&story_id=1&story_model_id=19
		WWWData www = networkManager.GetHttpInfo(HttpUrlInfo.urlLingjingProcess,
			"pickup",
			string.Format("is_test=1&session_id={0}&user_id={1}&story_id={2}&story_model_id={3}",
				ConfigInfo.sessionId,
				ConfigInfo.userId,
				ConfigInfo.storyId,
				((FieldEntityInfo)entityInfo).stroy_model_id
				)
			);
		yield return www;
		if (www.isError) {
			LogManager.Warning(www.error);
			yield break;
		}
		if (string.IsNullOrEmpty(www.text)) {
			LogManager.Warning("FAILED to pickup item due to missing return info");
			yield break;
		}
		Debug.Log(www.text);
		yield break;
	}

	public void OnInteractionFinished() {
		SetEntityInteractive(false);
		//LogManager.Debug("Finished Interaction");
		//if (selectedEntityInfo != null) {
		//	switch (selectedEntityInfo.enumActionType) {
		//		case EntityActionType.CollectableInfo:
		//			break;
		//		case EntityActionType.InteractiveItem:
		//		case EntityActionType.CollectableItem:
		//			//selectedEntityInfo.SetInteractionMode(false);
		//			break;
		//		case EntityActionType.Debug:
		//		case EntityActionType.ViewableInfo:
		//		case EntityActionType.CollectedInfo:
		//		default:
		//			break;
		//	}
		//}
		actionMode = ActionMode.World;
		if (selectedEntityInfo != null) {
			interactionView.ShowEntitySelection(GetEntityName());
		}
	}

	public void UpdateHintText(string newText) {
		interactionView.UpdateHint(newText);
	}

	enum ActionMode {
		Idle = -1,
		World,
		Interactive,
	}
}
public enum EntityActionType {
	Debug = -1,
	ViewableInfo = 1,
	CollectableInfo = 2,
	CollectedInfo = 3,
	InteractiveItem = 11,
	CollectableItem = 12,
	CollectedItem = 13,
	UtilityItem = 14,
	PlacableItem = 15,
	DialogActor = 21,
	DialogEvent = 22,
	Quiz = 22,
}

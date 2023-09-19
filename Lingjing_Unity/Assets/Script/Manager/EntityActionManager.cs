using System;
using System.Collections;
using UnityEngine;
using Network;
using Config;

[RequireComponent(typeof(InventoryItemManager))]
public class EntityActionManager : MonoBehaviour, IManager {
	private FieldEntityManager entityManager;
	private WWWManager networkManager;
	public event Action<ItemInfo> eventEntityFound;
	public InteractionView interactionView;
	private ActionMode actionMode = ActionMode.Idle;
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
					if (Physics.Raycast(ray, out hit, 2.0f, 64)) {
						FieldEntityInfo entityInfo;
						if (hit.collider.gameObject.TryGetComponent(out entityInfo)) {
							InteractWithEntity(entityInfo);
						}
					}
					break;
				case ActionMode.Interactive:
					if (Physics.Raycast(ray, out hit, 2.0f, 128)) {
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
							actionMode = ActionMode.Idle;
							interactionView.ShowNPCLog(entityInfo);
							entityInfo.hasProximityDialog = false;
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
	}
	public void RegisterManager(IManager manager) {
		if (manager is FieldEntityManager) {
			entityManager = manager as FieldEntityManager;
		} else if (manager is WWWManager) {
			networkManager = manager as WWWManager;
		}
	}

	public void Set2ARMode() {
		actionMode = ActionMode.World;
	}
	public void Set2IdleMode() {
		actionMode = ActionMode.Idle;
	}

	public void InteractWithEntity(ItemInfo entityInfo) {
		interactionView.ExitHint();
		switch (entityInfo.enumActionType) {
			case EntityActionType.Debug:
				eventEntityFound?.Invoke(entityInfo);
				break;
			case EntityActionType.ViewableInfo:
				actionMode = ActionMode.Idle;
				interactionView.ShowHint(entityInfo);
				break;
			case EntityActionType.CollectableInfo:
				actionMode = ActionMode.Idle;
				interactionView.ShowCollectableHint(entityInfo);
				break;
			case EntityActionType.CollectedInfo:
				actionMode = ActionMode.Idle;
				interactionView.ShowHint(entityInfo);
				break;
			case EntityActionType.InteractiveItem:
				actionMode = ActionMode.Interactive;
				interactionView.ShowHint(entityInfo);
				entityInfo.SetInteractionMode(true);
				break;
			case EntityActionType.CollectableItem:
				actionMode = ActionMode.Interactive;
				interactionView.ShowCollectableItem(entityInfo);
				entityInfo.SetInteractionMode(true);
				break;
			case EntityActionType.CollectedItem:
				actionMode = ActionMode.Idle;
				interactionView.ShowCollectableHint(entityInfo);
				break;
			case EntityActionType.UtilityItem:
				actionMode = ActionMode.Idle;
				entityInfo.GetComponent<ARUtilityListener>()?.UseARUtility();
				interactionView.ShowHint(entityInfo, "х╥хо");
				break;
			case EntityActionType.DialogActor:
				actionMode = ActionMode.Idle;
				interactionView.ShowNPCLog(entityInfo);
				break;
			case EntityActionType.Quiz:
				actionMode = ActionMode.Idle;
				interactionView.ShowQuiz(entityInfo);
				break;
			case EntityActionType.PlacableItem:
			default:
				break;
		}
	}

	public void InteractWithFile(FileInfo fileInfo) {
		interactionView.ExitHint();
		actionMode = ActionMode.Idle;
		interactionView.ShowHint(fileInfo.strHintbox, "");
	}

	public void ConfirmWithEntiry(ItemInfo entityInfo) {
		switch (entityInfo.enumActionType) {

			case EntityActionType.CollectableInfo:
				entityInfo.enumActionType = entityInfo.enumItemType;
				UIEventManager.CallEvent("InventoryItemManager", "AddInfo", entityInfo);
				interactionView.ExitHint();
				break;
			case EntityActionType.CollectableItem:
				entityInfo.enumActionType = entityInfo.enumItemType;
				PickUpItem(entityInfo);
				interactionView.ExitHint();
				break;
			case EntityActionType.DialogActor:
				//interactionView.AdvancedDialog();
				break;
			case EntityActionType.Debug:
			case EntityActionType.ViewableInfo:
			case EntityActionType.CollectedInfo:
			case EntityActionType.InteractiveItem:
			default:
				interactionView.ExitHint();
				break;
		}
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
				((FieldEntityInfo)entityInfo).entityItemId
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

	public void OnInteractionFinished(ItemInfo entityInfo) {
		//LogManager.Debug("Finished Interaction");
		if (entityInfo != null) {
			switch (entityInfo.enumActionType) {
				case EntityActionType.CollectableInfo:
					break;
				case EntityActionType.InteractiveItem:
				case EntityActionType.CollectableItem:
					entityInfo.SetInteractionMode(false);
					break;
				case EntityActionType.Debug:
				case EntityActionType.ViewableInfo:
				case EntityActionType.CollectedInfo:
				default:
					break;
			}
		}
		actionMode = ActionMode.World;
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
	Quiz = 22,
}

using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InventoryItemManager))]
public class EntityActionManager : MonoBehaviour, IManager {
	//public static EntityActionManager getInstance() {
	//	if (instance != null) {
	//		return instance;
	//	} else {
	//		GameObject go = GameObject.Find("EntityActionManager");
	//		if (go != null && go.TryGetComponent(out instance)) {
	//			return instance;
	//		}
	//	}
	//	Debug.LogError("MISSING EntityActionManager ");
	//	return null;
	//}
	//private static EntityActionManager instance;

	//public InventoryItemManager itemManager;
	public event Action<ItemInfo> eventEntityFound;
	//private UIEventManager eventManager;
	private InteractionView interactionView;
	private ActionMode actionMode = ActionMode.World;
	public void Update() {
#if !UNITY_EDITOR
		if (Input.GetTouch(0).phase==TouchPhase.Began) {
#else
		if (Input.GetMouseButtonDown(0)) {
			Debug.Log(actionMode);

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
	}
	public void Init(UIEventManager eventManager, params IManager[] managers) {
		interactionView = InteractionView.getInstance();
		//GetComponent<FieldEntityManager>().RegisterManager(this);
		//this.eventManager = eventManager;
	}
	public void InteractWithEntity(ItemInfo entityInfo) {
		interactionView.ExitHint();
		switch (entityInfo.enumActionType) {
			case EntityActionType.Debug:
				eventEntityFound?.Invoke(entityInfo);
				break;
			case EntityActionType.ViewableInfo:
				actionMode = ActionMode.Idle;
				interactionView.ShowHint(entityInfo, "");
				break;
			case EntityActionType.CollectableInfo:
				actionMode = ActionMode.Idle;
				interactionView.ShowHint(entityInfo, "记录信息");
				break;
			case EntityActionType.CollectedInfo:
				actionMode = ActionMode.Idle;
				interactionView.ShowHint(entityInfo, "");
				break;
			case EntityActionType.InteractiveItem:
				actionMode = ActionMode.Interactive;
				interactionView.ShowHint(entityInfo, "");
				entityInfo.SetInteractionMode(true);
				break;
			case EntityActionType.CollectableItem:
				actionMode = ActionMode.Interactive;
				interactionView.ShowHint(entityInfo, "收集道具");
				entityInfo.SetInteractionMode(true);
				break;
			case EntityActionType.CollectedItem:
				actionMode = ActionMode.Idle;
				interactionView.ShowHint(entityInfo, "");
				break;
			case EntityActionType.UtilityItem:
				actionMode = ActionMode.Idle;
				entityInfo.GetComponent<ARUtilityListener>()?.UseARUtility();
				interactionView.ShowHint(entityInfo, "", "确认");
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
				//InventoryItemManager.getInstance().AddInfo(entityInfo);
				break;
			case EntityActionType.InteractiveItem:
				break;
			case EntityActionType.CollectableItem:
				entityInfo.enumActionType = entityInfo.enumItemType;

				UIEventManager.CallEvent("InventoryItemManager", "AddItem", entityInfo);
				UIEventManager.CallEvent("FieldEntityManager", "RemoveFieldEntitys", entityInfo);
				//InventoryItemManager.getInstance().AddItem(entityInfo);
				break;
			case EntityActionType.Debug:
			case EntityActionType.ViewableInfo:
			case EntityActionType.CollectedInfo:
			default:
				break;
		}
	}


	public void OnInteractionFinished(ItemInfo entityInfo) {
		if (entityInfo != null) {
			switch (entityInfo.enumActionType) {
				case EntityActionType.CollectableInfo:
					break;
				case EntityActionType.InteractiveItem:
					entityInfo.SetInteractionMode(false);
					break;
				case EntityActionType.CollectableItem:
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
}

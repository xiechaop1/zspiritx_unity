using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemInfo : MonoBehaviour, IEventMessage {
	public FieldEntityManager entityManager;
	public EntityActionType enumActionType = EntityActionType.Debug;
	public EntityActionType enumItemType = EntityActionType.Debug;
	public Sprite icon;
	public string iconHint = "";
	public string strName = "";
	[TextArea]
	public string strHintbox = "";
	public GameObject goInteractionMode;
	public InteractionAnimation interactionAnimation;
	public ARUtilityListener arUtiity;

	public DialogSentence currDialog;
	public DialogSentence[] lstDialogs;
	public void SetInteractionMode(bool value) {
		if (goInteractionMode != null) {
			goInteractionMode?.SetActive(value);
		}
	}
	public void SetInteractionState(bool value) {
		if (interactionAnimation != null) {
			if (value) {
				interactionAnimation.BeginInteraction();
			} else {
				interactionAnimation.EndInteraction();
			}
		}
	}
}

public class FileInfo : IEventMessage {
	public Sprite icon;
	public string iconHint = "";
	public string strName = "";
	public string strHintbox = "";
	public FileInfo(ItemInfo origin) {
		icon = origin.icon;
		iconHint = origin.iconHint;
		strName = origin.strName;
		strHintbox = origin.strHintbox;
	}
}
public abstract class InteractionAnimation : MonoBehaviour {
	public abstract void BeginInteraction();
	public abstract void EndInteraction();
}

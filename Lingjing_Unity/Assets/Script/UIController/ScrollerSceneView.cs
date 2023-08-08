using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollerSceneView : MonoBehaviour {
	public static ScrollerSceneView getInstance() {
		if (instance != null) {
			return instance;
		} else {
			GameObject go = GameObject.Find("ScrollerCanvas");
			if (go != null && go.TryGetComponent(out instance)) {
				return instance;
			}
		}
		Debug.LogError("MISSING ScrollerSceneView ");
		return null;
	}
	private static ScrollerSceneView instance;

	public InventoryItemManager inventoryManager;
	public EntityActionManager actionManager;
	public GameObject btnLeftFrame;
	public GameObject btnRightFrame;
	ScrollerFrameInfo infoCurrFrame;
	ScrollerFrameInfo infoLeftFrame;
	ScrollerFrameInfo infoRightFrame;


	public void LoadFrame(ScrollerFrameInfo frame) {
		frame.gameObject.SetActive(true);

		if (frame.leftFrame != null) {
			infoLeftFrame = frame.leftFrame as ScrollerFrameInfo;
			btnLeftFrame.SetActive(true);
		} else {
			btnLeftFrame.SetActive(false);
		}

		if (frame.rightFrame != null) {
			infoRightFrame = frame.rightFrame as ScrollerFrameInfo;
			btnRightFrame.SetActive(true);
		} else {
			btnRightFrame.SetActive(false);
		}
		infoCurrFrame = frame;
	}
	public void UnloadFrame() => UnloadFrame(infoCurrFrame);

	public void UnloadFrame(ScrollerFrameInfo frame) {
		if (frame != null) {
			frame.gameObject.SetActive(false);
		}
	}

	public void GotoLeftFrame() {
		UnloadFrame();
		LoadFrame(infoLeftFrame);

	}
	public void GotoRightFrame() {
		UnloadFrame();
		LoadFrame(infoRightFrame);
	}
}

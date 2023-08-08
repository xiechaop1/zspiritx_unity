using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollerElementInfo : ItemInfo {
	public void OnInteract() {
		ScrollerSceneView.getInstance().actionManager.InteractWithEntity(this);
	}
}

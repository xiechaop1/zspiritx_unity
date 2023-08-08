using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollerElementManager : MonoBehaviour, IManager {
	public EntityActionManager actionManager;
	public GameObject backgroundImageRoot;
	public GameObject uiRoot;
	public GameObject scrollerScenePrefab;
	public ScrollerSceneView sceneView;
	GameObject sceneRoot;
	public void Init(UIEventManager eventManager, params IManager[] managers) {
		foreach (var manager in managers) {
			RegisterManager(manager);
		}
	}
	public void RegisterManager(IManager manager) {
		if (manager is EntityActionManager) {
			actionManager = manager as EntityActionManager;
		}
	}
	public void PrepareScene() {
		sceneRoot = Instantiate(scrollerScenePrefab, backgroundImageRoot.transform);
		ScrollerSceneInfo info = sceneRoot.GetComponent<ScrollerSceneInfo>();
		sceneView.LoadFrame(info.StartingFrame);
		backgroundImageRoot.SetActive(true);
		uiRoot.SetActive(true);
	}
	public void StopScene() {
		backgroundImageRoot.SetActive(false);
		sceneView.UnloadFrame();
		uiRoot.SetActive(false);
		Destroy(sceneRoot);
		sceneRoot = null;
	}
}

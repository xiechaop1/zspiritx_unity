using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoadManager : MonoBehaviour, IManager {
	private enum SceneMode {
		None,
		Loading,
		Ready,
		Scroller,
		AR,
	}

	public GameObject Notch;
	public GameObject LoadingScreen;
	public WebViewBehaviour SplashWebView;
	public event Action<string> eventDebugInfo;

	public FieldStageInfo startingStage;

	//Utility
	private UIEventManager eventManager;
	public InventoryItemManager inventoryItemManager;
	private Network.WWWManager networkManager;

	//AR
	public UnityEngine.XR.ARFoundation.ARSession arSession;
	public FieldStageManager fieldStageManager;
	private FieldEntityManager fieldEntityManager;
	private EntityActionManager entityActionManager;
	private ARSightManager arSightManager;

	//Scroller
	public ScrollerElementManager scrollerElementManager;

	private SceneMode sceneMode = SceneMode.None;
	private bool isLoading = true;
	//private bool isForcedStop = false;
	public void Init(UIEventManager eventManager, params IManager[] managers) {

	}
	void Awake() {
		WorldInit();
		StartCoroutine(InitStartUp());
	}
	void WorldInit() {
		//Utility Init
		eventManager = UIEventManager.getInstance();
		eventManager.Init(eventManager);
		eventManager.SubscribeBroadcast("WebViewCall", WebviewCallbackSceneControl);

		networkManager = Network.WWWManager.getInstance();

		inventoryItemManager.Init(eventManager);
		entityActionManager = inventoryItemManager.GetComponent<EntityActionManager>();
		entityActionManager.Init(eventManager);

		//AR Init
		fieldEntityManager = fieldStageManager.GetComponent<FieldEntityManager>();
		fieldStageManager.Init(eventManager, fieldEntityManager);
		fieldEntityManager.Init(eventManager, entityActionManager,fieldStageManager);
		arSightManager = fieldEntityManager.GetComponent<ARSightManager>();
		arSightManager.Init(eventManager);

		//Scroller Init
		scrollerElementManager.Init(eventManager, entityActionManager);
		sceneMode = SceneMode.Loading;
	}

	IEnumerator InitStartUp() {
		yield return null;
#if UNITY_EDITOR
		float notch = 20;//Screen.safeArea.x;
#else
		float notch = Screen.safeArea.x;
#endif
		if (notch > 0.0f) {
			Notch.GetComponent<RectTransform>().anchorMin = new Vector2(0.0f, 1.0f - notch / Screen.width);
			//SplashWebView.GetComponent<RectTransform>().anchorMax = new Vector2(1.0f, 1.0f - notch / Screen.width);
		}
		yield return null;
		SplashWebView.StartWebViewHome();
		yield return null;
		sceneMode = SceneMode.Ready;
		isLoading = false;
		yield break;
	}

	void WebviewCallbackSceneControl(string msg) {
		if (msg == "StartARScene") {
			SplashWebView.SetVisibility(false);
			StartCoroutine(ARLoader());
		} else if (msg == "Start2DScene") {
			SplashWebView.SetVisibility(false);
			StartCoroutine(ScrollerLoader());
		}
	}
	IEnumerator ARLoader() {
		LoadingScreen.SetActive(true);
		yield return null;
		//if (isLoading) {
		//	yield return new WaitForSeconds(5);
		//}
		while (isLoading) {
			yield return null;
		}
		isLoading = true;
		sceneMode = SceneMode.AR;
		arSession.enabled = true;
		yield return null;
		fieldStageManager.PrepareBackstage(startingStage);
		yield return null;
		fieldEntityManager.PrepareScene();

		LoadingScreen.SetActive(false);
		eventDebugInfo?.Invoke("����ɨ�蹹����������ʹ���ֻ�����ɨ�������ǽ��");
		for (int i = 0; i < 20; i++) {
			if (fieldEntityManager.isSurfacesReady) {
				break;
			}
			yield return new WaitForSeconds(1);
		}
		eventDebugInfo?.Invoke("����ɨ�� " + (fieldEntityManager.isSurfacesReady ? "�ɹ�" : "ʧ��"));

		yield return null;
		for (int i = 0; i < 10; i++) {
			if (fieldEntityManager.isLoadFinish) {
				break;
			}
			fieldEntityManager.TryPlaceEntitys(10);
			yield return new WaitForSeconds(1);
		}
		eventDebugInfo?.Invoke("ģ�ͷ��� " + (fieldEntityManager.isLoadFinish ? "�ɹ�" : "ʧ��"));

		isLoading = false;
		yield break;
	}

	IEnumerator ScrollerLoader() {
		LoadingScreen.SetActive(true);
		yield return null;
		//if (isLoading) {
		//	yield return new WaitForSeconds(5);
		//}
		while (isLoading) {
			yield return null;
		}
		isLoading = true;
		sceneMode = SceneMode.Scroller;
		scrollerElementManager.PrepareScene();
		LoadingScreen.SetActive(false);

		isLoading = false;
		yield break;
	}

	public void Pause() {
		StopAllCoroutines();
		StartCoroutine(StopScenes());
	}
	public void ExitScene() {
		StopAllCoroutines();
		SplashWebView.StartWebView("splash.html");

		StartCoroutine(StopScenes());
	}
	IEnumerator StopScenes() {
		isLoading = true;
		switch (sceneMode) {
			case SceneMode.None:
				break;
			case SceneMode.Loading:
				break;
			case SceneMode.Scroller:
				scrollerElementManager.StopScene();
				break;
			case SceneMode.AR:
				fieldEntityManager.StopScene();
				yield return null;
				try {
					arSession.enabled = false;
					eventDebugInfo?.Invoke("AR�رճɹ�");
				} catch {
					eventDebugInfo?.Invoke("AR�ر�ʧ��");
				}
				break;
			default:
				break;
		}
		yield return null;
		yield return new WaitForSeconds(1f);
		sceneMode = SceneMode.Ready;
		isLoading = false;
		eventDebugInfo?.Invoke("�����رճɹ�");
		yield break;
	}
	//IEnumerator ARUnloader() {
	//	isLoading = true;
	//	fieldEntityManager.StopScene();
	//	isLoading = false;
	//	Debug.Log("LoadFinished");
	//	yield break;
	//}
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif
using UnityEngine.XR.ARFoundation;
using Network;
using Config;

public class SceneLoadManager : MonoBehaviour, IManager {
	public enum SceneMode {
		None,
		Loading,
		Ready,
		Scroller,
		AR,
	}
	enum ARSystem {
		ARFoundation,
		HWAREngine,
		ARFoundationNeedInstalled,
		HWARNeedInstalled,
		Unsupport
	}

	public event Action<string> webViewCallback;

	//public GameObject Notch;
	public GameObject LoadingScreen;
	public MonoWebView SplashWebView;
	//public event Action<string> eventDebugInfo;


	//Utility
	private UIEventManager eventManager;
	public InventoryItemManager inventoryItemManager;
	private WWWManager networkManager;
	private InputGPSManager gpsManager;
	public ActorDataManager dataManager;
	public DebugMenuManager debugManager;

	//AR
	private ARFoundationAdaptor arFoundationAdaptor;
	private HWAREngineAdaptor hwAREngineAdaptor;
	//public ARSession arSession;
	public FieldStageManager fieldStageManager;
	private FieldEntityManager fieldEntityManager;
	private EntityActionManager entityActionManager;
	private ARSightManager arSightManager;

	private ARSystem isARPossible;
	//public GameObject ARSceneScanHint;

	//Scroller
	public ScrollerElementManager scrollerElementManager;

	public SceneMode sceneMode = SceneMode.None;
	public bool isLoading = true;
	//private bool isForcedStop = false;
	public void Init(UIEventManager eventManager, params IManager[] managers) {

	}
	void Awake() {
		WorldInit();
		StartCoroutine(InitStartUp());
	}
	public void DebugLog(string log) {
		debugManager.ShowHint(log);
		//eventDebugInfo?.Invoke(log);
	}
	void WorldInit() {
#if UNITY_ANDROID
		Permission.RequestUserPermissions(new string[] {
				Permission.Camera,
				Permission.FineLocation
			});
#endif
		arFoundationAdaptor = GetComponent<ARFoundationAdaptor>();
		hwAREngineAdaptor = GetComponent<HWAREngineAdaptor>();

		//Utility Init

		eventManager = UIEventManager.getInstance();
		eventManager.Init(eventManager);
		//eventManager.SubscribeBroadcast("WebViewCall", WebviewCallbackSceneControl);

		networkManager = WWWManager.getInstance();

		gpsManager = InputGPSManager.getInstance();
		gpsManager.Init(eventManager, networkManager);

		dataManager.Init(eventManager, gpsManager, networkManager, fieldStageManager);

		inventoryItemManager.Init(eventManager);
		entityActionManager = inventoryItemManager.GetComponent<EntityActionManager>();

		fieldEntityManager = fieldStageManager.GetComponent<FieldEntityManager>();
		entityActionManager.Init(eventManager, fieldEntityManager, networkManager);

		//AR Init

		fieldStageManager.Init(eventManager, fieldEntityManager, networkManager);
		fieldEntityManager.Init(eventManager, entityActionManager, fieldStageManager, gpsManager, this);
		arSightManager = fieldEntityManager.GetComponent<ARSightManager>();
		arSightManager.Init(eventManager);

		//Scroller Init
		scrollerElementManager.Init(eventManager, entityActionManager);
		sceneMode = SceneMode.Loading;
	}

	IEnumerator InitStartUp() {
		yield return null;

		//goto HuaweiAR;

		//Unity ARFoundation
		if (ARSession.state == ARSessionState.None) {
			StartCoroutine(ARSession.CheckAvailability());
		}
		while (ARSession.state == ARSessionState.CheckingAvailability) {
			yield return null;
		}
		switch (ARSession.state) {
			case ARSessionState.Installing:
			case ARSessionState.Ready:
			case ARSessionState.SessionInitializing:
			case ARSessionState.SessionTracking:
				arFoundationAdaptor.Init();
				isARPossible = ARSystem.ARFoundation;
				goto ARSystemReady;
			case ARSessionState.NeedsInstall:
				DebugLog("ARCore Needs Install");
				isARPossible = ARSystem.ARFoundationNeedInstalled;
				goto ARSystemReady;
			case ARSessionState.Unsupported:
				break;
			case ARSessionState.CheckingAvailability:
			case ARSessionState.None:
			default:
				DebugLog("UnKnown Status");
				break;
				//isARPossible = ARSystem.Unsupport;
				//goto ARSystemReady;
		}


HuaweiAR:

//Fallback to HuaweiAREngine
		try {
			HuaweiARUnitySDK.ARAvailability isHWAR;
			int cnt = 0;
			for (; ; ) {
				isHWAR = HuaweiARUnitySDK.AREnginesApk.Instance.CheckAvailability();
				if (isHWAR != HuaweiARUnitySDK.ARAvailability.UNKNOWN_CHECKING) {
					if (isHWAR == HuaweiARUnitySDK.ARAvailability.UNKNOWN_TIMED_OUT) {
						if (cnt > 3) {
							break;
						}
						cnt++;
					} else {
						break;
					}
				}
			}
			switch (isHWAR) {
				case HuaweiARUnitySDK.ARAvailability.SUPPORTED_INSTALLED:
					hwAREngineAdaptor.Init();
					isARPossible = ARSystem.HWAREngine;
					goto ARSystemReady;
				case HuaweiARUnitySDK.ARAvailability.SUPPORTED_NOT_INSTALLED:
				case HuaweiARUnitySDK.ARAvailability.SUPPORTED_APK_TOO_OLD:
					DebugLog("AREngine Needs Install");
					isARPossible = ARSystem.HWARNeedInstalled;
					goto ARSystemReady;
				case HuaweiARUnitySDK.ARAvailability.UNSUPPORTED_DEVICE_NOT_CAPABLE:
				case HuaweiARUnitySDK.ARAvailability.UNSUPPORTED_EMUI_NOT_CAPABLE:
					break;
				case HuaweiARUnitySDK.ARAvailability.UNKNOWN_ERROR:
				case HuaweiARUnitySDK.ARAvailability.UNKNOWN_TIMED_OUT:
				case HuaweiARUnitySDK.ARAvailability.UNKNOWN_CHECKING:
				default:
					DebugLog("Unknown Status");
					break;
					//isARPossible = ARSystem.Unsupport;
					//goto ARSystemReady;
			}
		} catch (Exception) {

		}

		//FALLBACK to Other AR solutions.
		DebugLog("ARCore & AREngine Unsupported");
		isARPossible = ARSystem.Unsupport;


ARSystemReady:
		SplashWebView.OnCallback += WebviewCallbackSceneControl;
		//SplashWebView.OnWebClose += LoadAR;
		SplashWebView.StartWebView("https://h5.zspiritx.com.cn/home");
		//SplashWebView.StartWebView("splash.html");

		yield return null;
		gpsManager.BackStagePrepare();
		yield return null;
		sceneMode = SceneMode.Ready;
		isLoading = false;
		yield break;
	}

	public void DebugWebViewCallback(string msg) {
		WebviewCallbackSceneControl(msg);
	}
	void WebviewCallbackSceneControl(string msg) {
		try {
			JSONReader jsonMsg = new JSONReader(msg);
			int tmpInt = 0;
			//string tmpString;
			if (jsonMsg.TryPraseInt("WebViewOff", ref tmpInt) && tmpInt == 1) {
				SplashWebView.SetVisibility(false);
				StartCoroutine(ARLoader());
			}
			if (jsonMsg.TryPraseInt("DebugInfo", ref tmpInt)) {
				debugManager.isDebugMode = (tmpInt == 1);
			}
			if (jsonMsg.TryPraseInt("UserId", ref tmpInt)) {
				ConfigInfo.userId = tmpInt;
				//debugManager.isDebugMode = (tmpInt == 1);
			}
			if (jsonMsg.TryPraseInt("StoryId", ref tmpInt)) {
				ConfigInfo.storyId = tmpInt;
				//debugManager.isDebugMode = (tmpInt == 1);
			}
		} catch (Exception) {
			string[] args = msg.Split('&');
			if (args[0] == "WebViewOff") {
				SplashWebView.SetVisibility(false);
				StartCoroutine(ARLoader());
			} else if (args[0] == "Start2DScene") {
				SplashWebView.SetVisibility(false);
				StartCoroutine(ScrollerLoader());
			}
		}
	}
	void LoadAR() {
		StartCoroutine(ARLoader());
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
		switch (isARPossible) {
			case ARSystem.ARFoundation:
				arFoundationAdaptor.Enable();
				//arSession.enabled = true;
				break;
			case ARSystem.HWAREngine:
				hwAREngineAdaptor.Enable();
				break;
			case ARSystem.ARFoundationNeedInstalled:
				break;
			case ARSystem.HWARNeedInstalled:
				break;
			case ARSystem.Unsupport:
				break;
			default:
				break;
		}

		yield return AsyncStartSession();

		yield return fieldStageManager.AsyncPrepareStage();

		yield return null;
		//fieldStageManager.PrepareBackstage(startingStage);
		yield return null;
		fieldEntityManager.PrepareScene();

		yield return null;
		entityActionManager.Set2ARMode();

		LoadingScreen.SetActive(false);
		dataManager.SetUserInfoSharing(true);
		isLoading = false;
		debugManager.ShowHint("user_id: " + ConfigInfo.userId + "\nstory_id: " + ConfigInfo.storyId + "\nsession_id: " + ConfigInfo.sessionId);
		yield break;
	}

	IEnumerator AsyncStartSession() {
		WWWData www = networkManager.GetHttpInfo(HttpUrlInfo.urlLingjingProcess,
			"init",
			string.Format("is_test=1&user_id={0}&story_id={1}",
				ConfigInfo.userId,
				ConfigInfo.storyId
				)
			);
		yield return www;
		if (www.isError) {
			LogManager.Warning(www.error);
			yield break;
		}
		if (string.IsNullOrEmpty(www.text)) {
			LogManager.Warning("FAILED to create session due to missing return info");
			yield break;
		}
		//Debug.Log(www.text);
		if (!InitSessionInfo(www.text)) {
			LogManager.Warning("FAILED to create session due to missing session id");
			yield break;
		}
		yield return null;
		www = networkManager.GetHttpInfo(HttpUrlInfo.urlLingjingProcess,
			"join",
			string.Format("is_test=1&user_id={0}&story_id={1}&session_id={2}&role_id=1&team_id=0",
				ConfigInfo.userId,
				ConfigInfo.storyId,
				ConfigInfo.sessionId
				)
			);
		yield return www;
		if (www.isError) {
			LogManager.Warning(www.error);
			yield break;
		}
		if (string.IsNullOrEmpty(www.text)) {
			LogManager.Warning("FAILED to join session due to missing return info");
			yield break;
		}
		LogManager.Debug(www.text);
		yield return null;
		yield break;
	}
	bool InitSessionInfo(string wwwText) {
		string tmpStr = "";
		if (JSONReader.TryPraseString(wwwText, "data", ref tmpStr)) {
			int tmpInt = 0;
			if (JSONReader.TryPraseInt(tmpStr, "id", ref tmpInt)) {
				ConfigInfo.sessionId = tmpInt;
				return true;
			}
		}
		return false;
	}

	IEnumerator AsyncEndSession() {
		WWWData www = networkManager.GetHttpInfo(HttpUrlInfo.urlLingjingProcess,
			"finish",
			string.Format("is_test=1&user_id={0}&story_id={1}&session_id={2}&goal=retry",
				ConfigInfo.userId,
				ConfigInfo.storyId,
				ConfigInfo.sessionId
				)
			);
		yield return www;
		if (www.isError) {
			LogManager.Warning(www.error);
			yield break;
		}
		LogManager.Debug(www.text);
		yield return null;
		yield break;
	}

	IEnumerator ScrollerLoader() {
		LoadingScreen.SetActive(true);
		yield return null;
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
		SplashWebView.StartWebView("https://h5.zspiritx.com.cn/home");

		StartCoroutine(StopScenes());
	}
	IEnumerator StopScenes() {
		isLoading = true;
		entityActionManager.Set2IdleMode();
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
					switch (isARPossible) {
						case ARSystem.ARFoundation:
							arFoundationAdaptor.Disable();
							break;
						case ARSystem.HWAREngine:
							hwAREngineAdaptor.Disable();
							break;
						case ARSystem.Unsupport:
							break;
						default:
							break;
					}
					//arSession.enabled = false;
					//eventDebugInfo?.Invoke("AR关闭成功");
				} catch {
					//eventDebugInfo?.Invoke("AR关闭失败");
				}
				break;
			default:
				break;
		}
		yield return null;
		dataManager.SetUserInfoSharing(false);
		//yield return AsyncEndSession();
		yield return new WaitForSeconds(1f);
		sceneMode = SceneMode.Ready;
		isLoading = false;
		//eventDebugInfo?.Invoke("场景关闭成功");
		yield break;
	}

}

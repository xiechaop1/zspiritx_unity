using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

	public WebViewObject webViewObject;
	public event Action<string> webViewCallback;

	//public GameObject Notch;
	public GameObject LoadingScreen;
	public WebViewBehaviour SplashWebView;
	//public event Action<string> eventDebugInfo;


	//Utility
	private UIEventManager eventManager;
	public InventoryItemManager inventoryItemManager;
	private WWWManager networkManager;
	private InputGPSManager gpsManager;
	public ActorDataManager dataManager;
	public DebugMenuManager debugManager;

	//AR
	public ARSession arSession;
	public FieldStageManager fieldStageManager;
	private FieldEntityManager fieldEntityManager;
	private EntityActionManager entityActionManager;
	private ARSightManager arSightManager;

	private bool isARPossible = true;
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
		//Utility Init
		WebInit();

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
		entityActionManager.Init(eventManager, fieldEntityManager,networkManager);

		//AR Init

		fieldStageManager.Init(eventManager, fieldEntityManager, networkManager);
		fieldEntityManager.Init(eventManager, entityActionManager, fieldStageManager, gpsManager, this);
		arSightManager = fieldEntityManager.GetComponent<ARSightManager>();
		arSightManager.Init(eventManager);

		//Scroller Init
		scrollerElementManager.Init(eventManager, entityActionManager);
		sceneMode = SceneMode.Loading;
	}

	private void WebInit() {
		//isActive = true;
		webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
		webViewObject.Init(
			cb: (msg) => {
				LogManager.Debug(string.Format("CallFromJS[{0}]", msg));
				webViewCallback(msg);
				//UnityWebViewListener(msg);
				//status.text = msg;
			},
			err: (msg) => {
				LogManager.Debug(string.Format("CallOnError[{0}]", msg));
				//status.text = msg;
			},
			httpErr: (msg) => {
				LogManager.Debug(string.Format("CallOnHttpError[{0}]", msg));
				//status.text = msg;
			},
			started: (msg) => {
				LogManager.Debug(string.Format("CallOnStarted[{0}]", msg));
			},
			hooked: (msg) => {
				LogManager.Debug(string.Format("CallOnHooked[{0}]", msg));
			},
			cookies: (msg) => {
				LogManager.Debug(string.Format("CallOnCookies[{0}]", msg));
			},
			ld: (msg) => {
				LogManager.Debug(string.Format("CallOnLoaded[{0}]", msg));
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS
				// NOTE: the following js definition is required only for UIWebView; if
				// enabledWKWebView is true and runtime has WKWebView, Unity.call is defined
				// directly by the native plugin.
#if true
				var js = @"
                    if (!(window.webkit && window.webkit.messageHandlers)) {
                        window.Unity = {
                            call: function(msg) {
                                window.location = 'unity:' + msg;
                            }
                        };
                    }
                ";
#else
                // NOTE: depending on the situation, you might prefer this 'iframe' approach.
                // cf. https://github.com/gree/unity-webview/issues/189
                var js = @"
                    if (!(window.webkit && window.webkit.messageHandlers)) {
                        window.Unity = {
                            call: function(msg) {
                                var iframe = document.createElement('IFRAME');
                                iframe.setAttribute('src', 'unity:' + msg);
                                document.documentElement.appendChild(iframe);
                                iframe.parentNode.removeChild(iframe);
                                iframe = null;
                            }
                        };
                    }
                ";
#endif
#elif UNITY_WEBPLAYER || UNITY_WEBGL
                var js = @"
                    window.Unity = {
                        call:function(msg) {
                            parent.unityWebView.sendMessage('WebViewObject', msg);
                        }
                    };
                ";
#else
				var js = "";
#endif
				webViewObject.EvaluateJS(js + @"Unity.call('ua=' + navigator.userAgent)");
			}
			//transparent: false,
			//zoom: true,
			//ua: "custom user agent string",
			//radius: 0,  // rounded corner radius in pixel
			//// android
			//androidForceDarkMode: 0,  // 0: follow system setting, 1: force dark off, 2: force dark on
			//// ios
			//enableWKWebView: true,
			//wkContentMode: 0,  // 0: recommended, 1: mobile, 2: desktop
			//wkAllowsLinkPreview: true,
			//// editor
			//separated: false
			);
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        webViewObject.bitmapRefreshCycle = 1;
#endif
		// cf. https://github.com/gree/unity-webview/pull/512
		// Added alertDialogEnabled flag to enable/disable alert/confirm/prompt dialogs. by KojiNakamaru · Pull Request #512 · gree/unity-webview
		//webViewObject.SetAlertDialogEnabled(false);

		// cf. https://github.com/gree/unity-webview/pull/728
		//webViewObject.SetCameraAccess(true);
		//webViewObject.SetMicrophoneAccess(true);

		// cf. https://github.com/gree/unity-webview/pull/550
		// introduced SetURLPattern(..., hookPattern). by KojiNakamaru · Pull Request #550 · gree/unity-webview
		//webViewObject.SetURLPattern("", "^https://.*youtube.com", "^https://.*google.com");

		// cf. https://github.com/gree/unity-webview/pull/570
		// Add BASIC authentication feature (Android and iOS with WKWebView only) by takeh1k0 · Pull Request #570 · gree/unity-webview
		//webViewObject.SetBasicAuthInfo("id", "password");

		//webViewObject.SetScrollbarsVisibility(true);


		//webViewObject.SetMargins(5, 100, 5, Screen.height / 4);
		webViewObject.SetTextZoom(100);  // android only. cf. https://stackoverflow.com/questions/21647641/android-webview-set-font-size-system-default/47017410#47017410
		webViewObject.SetVisibility(false);

	}

	IEnumerator InitStartUp() {
		yield return null;

		if (ARSession.state == ARSessionState.None) {
			StartCoroutine(ARSession.CheckAvailability());
		}
		while (ARSession.state == ARSessionState.CheckingAvailability) {
			yield return null;
		}

		if (ARSession.state == ARSessionState.NeedsInstall) {
			DebugLog("ARCore Needs Install");
			isARPossible = false;
			//yield break;
		} else if (ARSession.state == ARSessionState.Unsupported) {
			DebugLog("ARCore Unsupported");
			isARPossible = false;
			//yield break;
		} else {
			isARPossible = true;
		}

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
		webViewCallback(msg);
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
		if (isARPossible) {

		} else {

		}
		arSession.enabled = true;
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
		Debug.Log(www.text);
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
					arSession.enabled = false;
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

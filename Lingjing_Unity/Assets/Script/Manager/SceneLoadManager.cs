using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	public GameObject Notch;
	public GameObject LoadingScreen;
	public WebViewBehaviour SplashWebView;
	public event Action<string> eventDebugInfo;

	public FieldStageInfo startingStage;

	//Utility
	private UIEventManager eventManager;
	public InventoryItemManager inventoryItemManager;
	private Network.WWWManager networkManager;
	private InputGPSManager gpsManager;

	//AR
	public UnityEngine.XR.ARFoundation.ARSession arSession;
	public FieldStageManager fieldStageManager;
	private FieldEntityManager fieldEntityManager;
	private EntityActionManager entityActionManager;
	private ARSightManager arSightManager;

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
		eventDebugInfo?.Invoke(log);
	}
	void WorldInit() {
		//Utility Init
		WebInit();

		eventManager = UIEventManager.getInstance();
		eventManager.Init(eventManager);
		eventManager.SubscribeBroadcast("WebViewCall", WebviewCallbackSceneControl);

		networkManager = Network.WWWManager.getInstance();

		gpsManager = InputGPSManager.getInstance();
		gpsManager.Init(eventManager);

		inventoryItemManager.Init(eventManager);
		entityActionManager = inventoryItemManager.GetComponent<EntityActionManager>();

		fieldEntityManager = fieldStageManager.GetComponent<FieldEntityManager>();
		entityActionManager.Init(eventManager, fieldEntityManager);

		//AR Init

		fieldStageManager.Init(eventManager, fieldEntityManager);
		fieldEntityManager.Init(eventManager, entityActionManager, fieldStageManager, this);
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
				Debug.Log(string.Format("CallFromJS[{0}]", msg));
				webViewCallback(msg);
				//UnityWebViewListener(msg);
				//status.text = msg;
			},
			err: (msg) => {
				Debug.Log(string.Format("CallOnError[{0}]", msg));
				//status.text = msg;
			},
			httpErr: (msg) => {
				Debug.Log(string.Format("CallOnHttpError[{0}]", msg));
				//status.text = msg;
			},
			started: (msg) => {
				Debug.Log(string.Format("CallOnStarted[{0}]", msg));
			},
			hooked: (msg) => {
				Debug.Log(string.Format("CallOnHooked[{0}]", msg));
			},
			cookies: (msg) => {
				Debug.Log(string.Format("CallOnCookies[{0}]", msg));
			},
			ld: (msg) => {
				Debug.Log(string.Format("CallOnLoaded[{0}]", msg));
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
		SplashWebView.OnWebClose += LoadAR;
		SplashWebView.StartWebView("splash.html");

		yield return null;
		gpsManager.BackStagePrepare();
		yield return null;
		sceneMode = SceneMode.Ready;
		isLoading = false;
		yield break;
	}

	public void DebugWebViewCallback(string msg){
		webViewCallback(msg);
	}
	void WebviewCallbackSceneControl(string msg) {
		if (msg == "XXX") {
			SplashWebView.SetVisibility(false);
			StartCoroutine(ARLoader());
		} else if (msg == "Start2DScene") {
			SplashWebView.SetVisibility(false);
			StartCoroutine(ScrollerLoader());
		}
	}
	void LoadAR(){ 
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
		arSession.enabled = true;
		yield return null;
		fieldStageManager.PrepareBackstage(startingStage);
		yield return null;
		fieldEntityManager.PrepareScene();

		LoadingScreen.SetActive(false);
		DebugLog("正在扫描构建环境，请使用手机缓慢扫描地面与墙壁");
		for (int i = 0; i < 20; i++) {
			if (fieldEntityManager.isSurfacesReady) {
				break;
			}
			yield return new WaitForSeconds(1);
		}
		DebugLog("环境扫描 " + (fieldEntityManager.isSurfacesReady ? "成功" : "失败"));

		isLoading = false;
		yield break;

		yield return null;
		for (int i = 0; i < 10; i++) {
			if (fieldEntityManager.isLoadFinish) {
				break;
			}
			fieldEntityManager.TryPlaceRamdomEntitys(10);
			yield return new WaitForSeconds(1);
		}
		DebugLog("模型放置 " + (fieldEntityManager.isLoadFinish ? "成功" : "失败"));

		isLoading = false;
		yield break;
	}

	//public void ForceReloadScene() {
	//	StartCoroutine(StageReload());
	//}
	//IEnumerator StageReload() {
	//	yield return null;
	//	while (isLoading) {
	//		yield return null;
	//	}
	//	if (sceneMode != SceneMode.AR) {
	//		yield break;
	//	}
	//	isLoading = true;
	//	for (int i = 0; i < 10; i++) {
	//		if (fieldEntityManager.isLoadFinish) {
	//			break;
	//		}
	//		fieldEntityManager.TryPlaceRamdomEntitys(10);
	//		yield return new WaitForSeconds(1);
	//	}
	//	DebugLog("模型放置 " + (fieldEntityManager.isLoadFinish ? "成功" : "失败"));

	//	isLoading = false;
	//	yield break;
	//}

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
					eventDebugInfo?.Invoke("AR关闭成功");
				} catch {
					eventDebugInfo?.Invoke("AR关闭失败");
				}
				break;
			default:
				break;
		}
		yield return null;
		yield return new WaitForSeconds(1f);
		sceneMode = SceneMode.Ready;
		isLoading = false;
		eventDebugInfo?.Invoke("场景关闭成功");
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

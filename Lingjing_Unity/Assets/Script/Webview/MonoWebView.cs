using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MonoWebView : MonoBehaviour {
	//public SceneLoadManager sceneLoadManager;
	//public string HomepageUrl;
	public Text status;
	WebViewObject webViewObject;
	public GameObject splashBackground;
	public event Action OnWebClose;
	public event Action<string> OnCallback;
	public bool isActive {
		private set;
		get;
	}

	private void Awake() {
		Init();
		isActive = false;
	}
	private void Start() {
		//StartWebViewHome();
	}
	private void Init() {
		//isActive = true;
		webViewObject = gameObject.AddComponent<WebViewObject>();
		webViewObject.Init(
			cb: (msg) => {
				LogManager.Debug(string.Format("CallFromJS[{0}]", msg));
				UnityWebViewListener(msg);
				status.text = msg;
			},
			err: (msg) => {
				LogManager.Debug(string.Format("CallOnError[{0}]", msg));
				status.text = msg;
			},
			httpErr: (msg) => {
				LogManager.Debug(string.Format("CallOnHttpError[{0}]", msg));
				status.text = msg;
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
		// Added alertDialogEnabled flag to enable/disable alert/confirm/prompt dialogs. by KojiNakamaru ， Pull Request #512 ， gree/unity-webview
		//webViewObject.SetAlertDialogEnabled(false);

		// cf. https://github.com/gree/unity-webview/pull/728
		//webViewObject.SetCameraAccess(true);
		//webViewObject.SetMicrophoneAccess(true);

		// cf. https://github.com/gree/unity-webview/pull/550
		// introduced SetURLPattern(..., hookPattern). by KojiNakamaru ， Pull Request #550 ， gree/unity-webview
		//webViewObject.SetURLPattern("", "^https://.*youtube.com", "^https://.*google.com");

		// cf. https://github.com/gree/unity-webview/pull/570
		// Add BASIC authentication feature (Android and iOS with WKWebView only) by takeh1k0 ， Pull Request #570 ， gree/unity-webview
		//webViewObject.SetBasicAuthInfo("id", "password");

		//webViewObject.SetScrollbarsVisibility(true);


		//webViewObject.SetMargins(5, 100, 5, Screen.height / 4);
		webViewObject.SetTextZoom(100);  // android only. cf. https://stackoverflow.com/questions/21647641/android-webview-set-font-size-system-default/47017410#47017410
		webViewObject.SetVisibility(false);

	}
	//public void StartWebViewHome() {
	//	StartWebView(HomepageUrl);
	//}
	IEnumerator LoadURL(string PageURL) {
		LogManager.Debug("TryLoadPage: " + PageURL);
#if !UNITY_WEBPLAYER && !UNITY_WEBGL
		if (PageURL.StartsWith("http")) {
			webViewObject.LoadURL(PageURL.Replace(" ", "%20"));
		} else {
			var exts = new string[]{
				".jpg",
				".js",
				".html"  // should be last
            };
			foreach (var ext in exts) {
				var url = PageURL.Replace(".html", ext);
				var src = System.IO.Path.Combine(Application.streamingAssetsPath, url);
				if (!System.IO.File.Exists(src)) {
					continue;
				}
				var dst = System.IO.Path.Combine(Application.temporaryCachePath, url);
				byte[] result = null;
				if (src.Contains("://")) {  // for Android
#if UNITY_2018_4_OR_NEWER
					// NOTE: a more complete code that utilizes UnityWebRequest can be found in https://github.com/gree/unity-webview/commit/2a07e82f760a8495aa3a77a23453f384869caba7#diff-4379160fa4c2a287f414c07eb10ee36d
					var unityWebRequest = UnityWebRequest.Get(src);
					yield return unityWebRequest.SendWebRequest();
					result = unityWebRequest.downloadHandler.data;
#else
                    var www = new WWW(src);
                    yield return www;
                    result = www.bytes;
#endif
				} else {
					result = System.IO.File.ReadAllBytes(src);
				}
				System.IO.File.WriteAllBytes(dst, result);
				if (ext == ".html") {
					webViewObject.LoadURL("file://" + dst.Replace(" ", "%20"));
					break;
				}
			}
		}
#else
        if (PageURL.StartsWith("http")) {
            webViewObject.LoadURL(PageURL.Replace(" ", "%20"));
        } else {
            webViewObject.LoadURL("StreamingAssets/" + PageURL.Replace(" ", "%20"));
        }
#endif
		yield break;
	}

	public RectOffset GetRectOffset() {
		RectOffset ret = new RectOffset();
		RectTransform rectTransf = gameObject.GetComponent<RectTransform>();
		if (rectTransf) {
			ret.left = (int)(rectTransf.position.x + rectTransf.rect.x * rectTransf.lossyScale.x);
			ret.top = (int)(Screen.height - rectTransf.position.y - (rectTransf.rect.y + rectTransf.rect.height) * rectTransf.lossyScale.y);
			ret.right = (int)(Screen.width - rectTransf.position.x - (rectTransf.rect.x + rectTransf.rect.width) * rectTransf.lossyScale.x);
			ret.bottom = (int)(rectTransf.position.y + rectTransf.rect.y * rectTransf.lossyScale.y);
		}
		//Debug.Log(rectTransf.position + "\n" + rectTransf.rect + "\n" + ret + "\n" + rectTransf.lossyScale);
		return ret;
	}
	public void Eval(string js) {
		webViewObject.EvaluateJS(js);
	}

	public void UnityWebViewListener(string msg) {
		if (status != null) {
			status.text = msg;
		}
		OnCallback?.Invoke(msg);
	}
	public void StartWebView(string pageUrl) {
		RectOffset ret = GetRectOffset();
		webViewObject.SetMargins(ret.left, ret.top, ret.right, ret.bottom);
		StartCoroutine(LoadURL(pageUrl));
		SetVisibility(true);

	}
	public void SetVisibility(bool state) {
		if (isActive == state) {
			return;
		}
		splashBackground.SetActive(state);
		webViewObject.SetVisibility(state);
		isActive = state;

		OnWebClose?.Invoke();

	}

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugMenuManager : MonoBehaviour {
	//public static DebugMenuManager getInstance() {
	//	if (instance != null) {
	//		return instance;
	//	} else {
	//		GameObject go = GameObject.Find("Canvas");
	//		if (go != null && go.TryGetComponent(out instance)) {
	//			return instance;
	//		}
	//	}
	//	Debug.LogError("MISSING DebugMenuManager ");
	//	return null;
	//}
	//private static DebugMenuManager instance;

	public GameObject goDebugView;
	public GameObject goHintBox;
	public GameObject Notch;
	public Text txtHint;
	public SceneLoadManager sceneLoadManager;
	public FieldStageManager fieldStageManager;
	public FieldEntityManager fieldEntityManager;
	public EntityActionManager entityActionManager;
	public ARSightManager arSightManager;
	public InteractionView interactionController;
	public InventoryItemManager inventoryItemManager;
	public WebViewBehaviour SplashWebView;
	public InputGPSManager gpsManager;
	//public UnityEngine.XR.ARFoundation.ARTrackedImageManager imageManager;
	//public UnityEngine.XR.ARSubsystems.XRReferenceImageLibrary altImageLib;

	public GameObject fakeImage;
	public string fakeImageName;
	public double lat = 39.852899870588018;//carpark 39.852899870588018; test 39.9347868240849;
	public double lon = 116.36157978764194;//carpark 116.36157978764194; test 116.28647419295855;

	public bool isDebugMode {
		get => debugMode;
		set {
			if (debugMode != value) {
				debugMode = value;
				goDebugView.SetActive(value);
			}
		}
	}

	private bool debugMode = true;
	private void Awake() {
		//WorldInit();
	}
	private void Start() {
		entityActionManager.eventEntityFound += ShowInfo;
		//sceneLoadManager.eventDebugInfo += ShowHint;
		//DebugInfo
		//StartCoroutine(Testloader());
	}

	public void ExitHint() {
		goHintBox.SetActive(false);
	}
	public void ShowHint(string hint) {
		goHintBox.SetActive(true);
		txtHint.text = hint;
	}

	public void ShowInfo(ItemInfo entityInfo) {
		ShowHint(entityInfo.strHintbox);
	}
	public void InitBtn() {
		sceneLoadManager.ExitScene();
		//SplashWebView.StartWebView("splash.html");
	}
	public void ShowCurrentGeoLoc() {
		ShowHint(gpsManager.camLatitude.ToString("F9") + ", " + gpsManager.camLongitude.ToString("F9") + "\n" +
			gpsManager.GetCurrentLatLonGCJ02().ToString("F9") + "\n" +
			gpsManager.GetCurrentLatLonBD09().ToString("F9") + "\n" +
			gpsManager.groundLatitude.ToString("F9") + ", " + gpsManager.groundLongitude.ToString("F9") + "\n");
	}
	//bool isDebugMode = false;
	public void DebugBtn() {
		//Config.ConfigInfo.test.testLatLon = new Vector2((float)lat,(float)lon);
		//carpark
		Config.ConfigInfo.test.testLatLon = new Vector2(39.852899870588018f, 116.36157978764194f);
		//test
		//Config.ConfigInfo.test.testLatLon = new Vector2(39.9347868240849f, 116.28647419295855f);

		//if (gpsManager.UpdateCamPos()) {
		//	gpsManager.UpdateGroundLatLonByCameraPos();
		//}


		//ShowHint(fieldEntityManager.goCamDir.transform.rotation.eulerAngles.y + "\n" + Input.compass.trueHeading + "\n" + (fieldEntityManager.goCamDir.transform.rotation.eulerAngles.y - Input.compass.trueHeading));

		//StartCoroutine(GPSTest());
#if UNITY_EDITOR
		//FakeImageFound();
#else
		//fieldStageManager.LocationUpdate(Config.ConfigInfo.test.testLatLon);
#endif
		//sceneLoadManager.Pause();

		//Debug.Log(Camera.main.WorldToScreenPoint(fakeImage.transform.position));

		//isDebugMode = !isDebugMode;
		//UIEventManager.BroadcastEvent("ARTagDebug", isDebugMode.ToString());
		//UIEventManager.BroadcastEvent("WebViewCall", "StartARScene");
		//UnityEngine.XR.ARSubsystems.XRReferenceImageLibrary tmpLib = imageManager.referenceLibrary as UnityEngine.XR.ARSubsystems.XRReferenceImageLibrary;
		//imageManager.referenceLibrary = altImageLib;
		//altImageLib = tmpLib;
	}
	public void PicnicBtn() {
		//SplashWebView.SetVisibility(false);
		//UIEventManager.BroadcastEvent("WebViewCall", "Start2DScene");
		//sceneLoadManager.DebugWebViewCallback("WebViewOff&FalseAnswer");
		sceneLoadManager.DebugWebViewCallback("{'WebViewOff':1, 'DebugInfo':0}");
	}
	public void PanicBtn() {
		//SplashWebView.SetVisibility(false);
		//UIEventManager.BroadcastEvent("WebViewCall", "StartARScene");
		sceneLoadManager.DebugWebViewCallback("WebViewOff&TrueAnswer");
	}
	public void TryPlaceEntity() {
		fieldEntityManager.TryPlaceRamdomEntities(10);
		ShowHint("模型放置 " + (fieldEntityManager.isLoadFinish ? "成功" : "失败"));
	}

	public void FakeImageFound() {
		fieldStageManager.ImageFound(fakeImageName);
		//trackedImage.transform.localScale = Vector3.one;
		fieldEntityManager.PlaceImageTrackingEntity(fakeImageName, fakeImage);
	}

}
public class LogManager {
	public static void Debug(string info) {
		UnityEngine.Debug.Log(info);
	}
	public static void Warning(string info) {
		UnityEngine.Debug.LogWarning(info);
	}
	public static void Error(string info) {
		UnityEngine.Debug.LogError(info);
	}

}
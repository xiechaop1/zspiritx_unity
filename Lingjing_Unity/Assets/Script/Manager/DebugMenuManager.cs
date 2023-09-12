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

	private void Awake() {
		//WorldInit();
	}
	private void Start() {
		entityActionManager.eventEntityFound += ShowInfo;
		sceneLoadManager.eventDebugInfo += ShowHint;
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
	//bool isDebugMode = false;
	public void DebugBtn() {
		//Config.ConfigInfo.test.testLatLon = new Vector2(39.9347868240849f, 116.28647419295855f);
		//if (gpsManager.UpdateCamPos()) {
		//	gpsManager.UpdateGroundLatLonByCameraPos();
		//}
		ShowHint(gpsManager.camLatitude.ToString("F9") + ", " + gpsManager.camLongitude.ToString("F9") + "\n" +
				gpsManager.GetCurrentLatLonGCJ02().ToString("F9") + "\n" +
				gpsManager.GetCurrentLatLonBD09().ToString("F9") + "\n" +
				gpsManager.groundLatitude.ToString("F9") + ", " + gpsManager.groundLongitude.ToString("F9") + "\n");

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
		sceneLoadManager.DebugWebViewCallback("WebViewOff&FalseAnswer");
	}
	public void PanicBtn() {
		//SplashWebView.SetVisibility(false);
		//UIEventManager.BroadcastEvent("WebViewCall", "StartARScene");
		sceneLoadManager.DebugWebViewCallback("WebViewOff&TrueAnswer");
	}
	public void TryPlaceEntity() {
		fieldEntityManager.TryPlaceRamdomEntities(10);
		ShowHint("ģ�ͷ��� " + (fieldEntityManager.isLoadFinish ? "�ɹ�" : "ʧ��"));
	}

	public void FakeImageFound() {
		fieldStageManager.ImageFound(fakeImageName);
		//trackedImage.transform.localScale = Vector3.one;
		fieldEntityManager.PlaceImageTrackingEntity(fakeImageName, fakeImage);
	}

	//IEnumerator GPSTest() {
	//	isDebugMode = !isDebugMode;
	//	LocationService locServ = Input.location;
	//	if (!isDebugMode) {
	//		locServ.Stop();
	//		yield break;
	//	}
	//	locServ.Start();
	//	LocationInfo locInfo;
	//	while (isDebugMode) {
	//		locInfo = locServ.lastData;
	//		ShowHint("lat: " + locInfo.latitude + "\nlon: " + locInfo.longitude + "\nalt: " + locInfo.altitude);
	//		yield return new WaitForSeconds(2f);
	//	}

	//}
	//void WorldInit() {
	//	UIEventManager eventManager = UIEventManager.getInstance();
	//	inventoryItemManager.Init(eventManager);

	//	fieldEntityManager.Init(eventManager);
	//	entityActionManager.Init(eventManager);
	//	arSightManager.Init(eventManager);

	//	//fieldEntityManager.actionManager = entityActionManager;
	//	//entityActionManager.interactionView = interactionController;
	//}
	//	IEnumerator Testloader() {

	//		entityActionManager.eventEntityFound += ShowInfo;

	//#if UNITY_EDITOR
	//		float notch = 20;//Screen.safeArea.x;
	//#else
	//		float notch = Screen.safeArea.x;
	//#endif
	//		if (notch > 0.0f) {
	//			Notch.GetComponent<RectTransform>().anchorMin = new Vector2(0.0f, 1.0f - notch / Screen.width);
	//			//SplashWebView.GetComponent<RectTransform>().anchorMax = new Vector2(1.0f, 1.0f - notch / Screen.width);
	//		}
	//		//yield return null;
	//		SplashWebView.StartWebViewHome();
	//		//SplashWebView.StartWebViewHome();
	//		while (SplashWebView.isActive) {
	//			yield return null;
	//		}
	//		//		for (int i = 0; SplashWebView.isActive;) {
	//		//			yield return null;
	//		//#if UNITY_EDITOR
	//		//			i++;
	//		//			if (i > 1200) {
	//		//				SplashWebView.UnityWebViewListener("StartARScene");
	//		//			}
	//		//#endif
	//		//		}

	//		fieldEntityManager.PrepareScene();
	//		ShowHint("����ɨ�蹹����������ʹ���ֻ�����ɨ�������ǽ��");
	//		for (int i = 0; i < 20; i++) {
	//			if (fieldEntityManager.isSurfacesReady) {
	//				break;
	//			}
	//			yield return new WaitForSeconds(1);
	//		}
	//		ShowHint("����ɨ�� " + (fieldEntityManager.isSurfacesReady ? "�ɹ�" : "ʧ��"));

	//		yield return null;
	//		for (int i = 0; i < 10; i++) {
	//			if (fieldEntityManager.isLoadFinish) {
	//				break;
	//			}
	//			fieldEntityManager.TryPlaceEntitys(10);
	//			yield return new WaitForSeconds(1);
	//		}
	//		//bool entityLoadingResult = fieldEntityManager.isLoadFinish;
	//		//Debug.Log(entityLoadingResult);
	//		ShowHint("ģ�ͷ��� " + (fieldEntityManager.isLoadFinish ? "�ɹ�" : "ʧ��"));
	//	}
}

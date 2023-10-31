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
	public GameObject goGeoLoc;
	public Text txtGeoLoc;
	public GameObject goObjLst;
	public Text txtObjLst;
	public SceneLoadManager sceneLoadManager;
	public FieldStageManager fieldStageManager;
	public FieldEntityManager fieldEntityManager;
	public EntityActionManager entityActionManager;
	public ARSightManager arSightManager;
	public InteractionView interactionController;
	public InventoryItemManager inventoryItemManager;
	//public WebViewBehaviour SplashWebView;
	public InputGPSManager gpsManager;
	public CombatManager combatManager;
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
		instance = this;
		//WorldInit();
	}
	private void Start() {
		entityActionManager.eventEntitySelected += ShowInfo;
		//sceneLoadManager.eventDebugInfo += ShowHint;
		//DebugInfo
		//StartCoroutine(Testloader());
	}
	private void Update() {
		if (goGeoLoc.activeInHierarchy) {
			txtGeoLoc.text = GetLatLon();
		}
		//if (goObjLst.activeInHierarchy) {
		//	txtObjLst.text = GetObjectList();
		//}
	}
	public void ExitHint() {
		goHintBox.SetActive(false);
	}
	public static void ShowLog(string log) {
		instance.ShowHint(log);
	}
	static DebugMenuManager instance;
	public void ShowHint(string hint) {
		goHintBox.SetActive(true);
		txtHint.text = hint;
	}

	public void ShowInfo(ItemInfo entityInfo) {
		ShowHint(entityInfo.strHintbox);
	}
	public void InitBtn() {
		FakeImageFound();
		//combatManager.PrepareBattleGround();
		//combatManager.finishCallback += OnCombatFinished;
		//sceneLoadManager.ExitScene();
		//SplashWebView.StartWebView("splash.html");
	}

	void OnCombatFinished(string info) {
		combatManager.finishCallback -= OnCombatFinished;
		ShowHint(info);
	}
	public void ShowCurrentGeoLoc() {
		goGeoLoc.SetActive(!goGeoLoc.activeSelf);
		//ShowHint(GetLatLon());
	}
	private string GetLatLon() {
		//gpsManager.GetCurrentLatLonWGS84(out double wgsLat, out double wgsLon);
		gpsManager.GetCurrentLatLonGCJ02(out double gcjLat, out double gcjLon);
		//gpsManager.GetCurrentLatLonBD09(out double bdLat, out double bdLon);
		Vector3 playerPos = fieldEntityManager.goCamDir.transform.position;
		string output =
			//wgsLat.ToString("F9") + ", " + wgsLon.ToString("F9") + "\n" +
			gcjLat.ToString("F9") + ", " + gcjLon.ToString("F9") + "\n" +
			//bdLat.ToString("F9") + ", " + bdLon.ToString("F9") + "\n" +
			//gpsManager.groundLatitude.ToString("F9") + ", " + gpsManager.groundLongitude.ToString("F9") + "\n";
			playerPos.x.ToString("F2") + ", " + playerPos.y.ToString("F2") + ", " + playerPos.z.ToString("F2");
		return output;
	}
	private string GetObjectList() {
		List<FieldEntityInfo> lstEntities = new List<FieldEntityInfo>();
		lstEntities.AddRange(fieldEntityManager.arrGeoLocEntity);
		lstEntities.AddRange(fieldEntityManager.arrTaggedEntity);
		lstEntities.AddRange(fieldEntityManager.arrFieldEntity);
		gpsManager.GetCurrentLatLon(out double newLat, out double newLng);
		string output = "";
		string tmp;
		foreach (FieldEntityInfo entity in lstEntities) {
			tmp = entity.entityUUID;
			if (entity.enumARType == FieldEntityInfo.EntityToggleType.GeoLocAround || entity.enumARType == FieldEntityInfo.EntityToggleType.GeoLocPosition) {
				tmp += ": " + InputGPSManager.FastGetDistance(entity.latitude, entity.longitude, newLat, newLng).ToString("F3");
			} else if (entity.enumARType == FieldEntityInfo.EntityToggleType.ARTagAround || entity.enumARType == FieldEntityInfo.EntityToggleType.ARTagPosition) {
				tmp += ": " + entity.uuidImageTracking;
			}
			//tmp = string.Format("{1}:{2}\n",entity.entityName,entity.geoLocDistance);
			output += tmp + "\n";
		}
		lstEntities.Clear();
		lstEntities.AddRange(fieldEntityManager.arrPlacedEntity);
		foreach (FieldEntityInfo entity in lstEntities) {
			tmp = entity.entityUUID;
			tmp += entity.transform.position.x + ", " + entity.transform.position.y + ", " + entity.transform.position.z;
			output += tmp + "\n";
		}
		return output;
	}
	//bool isDebugMode = false;
	public void DebugBtn() {
		//Config.ConfigInfo.test.testLatLon = new Vector2((float)lat,(float)lon);
		//carpark
		//Config.ConfigInfo.test.testLatLon = new Vector2(39.852899870588018f, 116.36157978764194f);
		//test
		//Config.ConfigInfo.test.testLatLon = new Vector2(39.9347868240849f, 116.28647419295855f);

		//if (gpsManager.UpdateCamPos()) {
		//	gpsManager.UpdateGroundLatLonByCameraPos();
		//}

		ShowHint(GetObjectList());

		//ShowHint(fieldEntityManager.goCamDir.transform.rotation.eulerAngles.y + "\n" + Input.compass.trueHeading + "\n" + (fieldEntityManager.goCamDir.transform.rotation.eulerAngles.y - Input.compass.trueHeading));

		//StartCoroutine(GPSTest());
		//FakeImageFound();
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
		sceneLoadManager.DebugWebViewCallback("{'WebViewOff':1, 'DebugInfo':1}");
	}
	public void PanicBtn() {
		//SplashWebView.SetVisibility(false);
		//UIEventManager.BroadcastEvent("WebViewCall", "StartARScene");
		//sceneLoadManager.DebugWebViewCallback("WebViewOff&TrueAnswer");
		interactionController.OnQuizCallback("{'WebViewOff':1, 'AnswerType':1}");
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
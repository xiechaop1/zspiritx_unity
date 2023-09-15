using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Network;
using Config;

public class ActorDataManager : MonoBehaviour, IManager {
	InputGPSManager gpsManager;
	WWWManager networkManager;
	FieldStageManager stageManager;
	bool isSharing = false;
	public void Init(UIEventManager eventManager, params IManager[] managers) {
		foreach (var manager in managers) {
			RegisterManager(manager);
		}
	}
	public void RegisterManager(IManager manager) {
		if (manager is InputGPSManager) {
			gpsManager = manager as InputGPSManager;
		} else if (manager is WWWManager) {
			networkManager = manager as WWWManager;
		} else if (manager is FieldStageManager) {
			stageManager = manager as FieldStageManager;
		}
	}

	public void SetUserInfoSharing(bool isShare) {
		isSharing = isShare;
	}
	float timer = 1f;
	void Update() {
		//GeoLocUpdate();
		if (isSharing) {
			if (timer < 0) {
				if (GeoLocUpdate()) {
					StartCoroutine(AsyncUpdatePlayerPos());
				}
				timer = 0.2f;
			} else {
				timer -= Time.deltaTime;
			}
		}
	}
	bool GeoLocUpdate() {
		if (gpsManager.UpdateCamPos()) {
			stageManager.LocationUpdate(gpsManager.GetCurrentLatLonWGS84());
			return true;
		}
		return false;
	}
	double lastLat = 0.0;
	double lastLon = 0.0;
	bool isLocUpdated() {
		if (lastLat == gpsManager.camLatitude && lastLon == gpsManager.camLongitude) {
			return false;
		} else {
			lastLat = gpsManager.camLatitude;
			lastLon = gpsManager.camLongitude;
			return true;
		}
	}
	IEnumerator AsyncUpdatePlayerPos() {
		var latlon = gpsManager.GetCurrentLatLonGCJ02();
		WWWData www = networkManager.GetHttpInfo(HttpUrlInfo.urlLingjingUser,
			"update_user_loc",
			string.Format("user_id={0}&lat={1}&lng={2}",
				ConfigInfo.userId,
				latlon.x,
				latlon.y
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
}

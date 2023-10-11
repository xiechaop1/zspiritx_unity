using System;
using UnityEngine;
using Config;
using System.Collections;

public class InputGPSManager : MonoBehaviour, IManager {
	#region SI
	private static InputGPSManager instance = null;
	public static InputGPSManager getInstance() {
		if (instance != null) {
			return instance;
		} else {
			GameObject go = GameObject.Find("InputGPSManager");
			if (go != null && go.TryGetComponent(out instance)) {
				DontDestroyOnLoad(go);
				return instance;
			}
		}
		Debug.LogError("MISSING InputGPSManager ");
		return null;
	}
	#endregion
#if UNITY_EDITOR
	private void Start() {
		if (ConfigInfo.test.testFlag) {
			//测试模式阻止GPS刷新
			//groundLatLon = ConfigInfo.test.testLatLon;
			camLatitude = ConfigInfo.test.testLatLon.x;
			camLongitude = ConfigInfo.test.testLatLon.y;
		}
	}
#endif

	public void Init(UIEventManager eventManager, params IManager[] managers) {
		locServ = Input.location;
		compass = Input.compass;
	}

	public void BackStagePrepare() {
		NativeGPSPlugin.StartLocation();
		locServ.Start();
		compass.enabled = true;
	}
	public void Pause() {
		locServ.Stop();
		compass.enabled = false;
	}
	Compass compass;
	LocationService locServ;


	#region Update Info
	public GameObject mainCamera;
	public double camLatitude = 0f;
	public double camLongitude = 0f;
	public double groundLatitude = 0f;
	public double groundLongitude = 0f;

	public void UpdateGroundCampass() {
		transform.rotation = GetNorth();
	}
	Quaternion GetNorth() {
		return Quaternion.Euler(0, mainCamera.transform.rotation.eulerAngles.y - compass.trueHeading, 0);
	}
	public bool UpdateCamPos() {
		GetCurrentLatLon(out double lat, out double lon);
		if (camLatitude == lat && camLongitude == lon) {
			return false;
		}
		camLatitude = lat;
		camLongitude = lon;
		return true;
	}
	Vector3 GeoLocCamPos() {
		return transform.InverseTransformPoint(mainCamera.transform.position);
	}
	public void UpdateGroundLatLonByCameraPos() {
		GetEndLatLng(camLatitude, camLongitude, -GeoLocCamPos(), out double lat, out double lon);
		groundLatitude = lat;
		groundLongitude = lon;
	}
	#endregion

	#region Get Basic GeoLoc
	public void GetCurrentLatLon(out double latitude, out double longitude) {
		GetCurrentLatLonGCJ02(out latitude, out longitude);
	}
	public Vector2 GetCurrentLatLon() {
		GetCurrentLatLon(out double dLat, out double dLon);
		return new Vector2((float)dLat, (float)dLon);
	}

	public void GetCurrentLatLonWGS84(out double latitude, out double longitude) {
#if UNITY_EDITOR
		latitude = ConfigInfo.test.testLatLon.x;
		longitude = ConfigInfo.test.testLatLon.y;
#else
		latitude = NativeGPSPlugin.GetLatitude();
		longitude = NativeGPSPlugin.GetLongitude();
#endif
	}
	public void GetCurrentLatLonGCJ02(out double latitude, out double longitude) {
		GetCurrentLatLonWGS84(out double dLat, out double dLon);
		WGS84ToGCJ02(dLat, dLon, out double lat, out double lon);
		latitude = lat;
		longitude = lon;
	}
	public void GetCurrentLatLonBD09(out double latitude, out double longitude) {
		GetCurrentLatLonWGS84(out double dLat, out double dLon);
		WGS84ToBD09(dLat, dLon, out double lat, out double lon);
		latitude = lat;
		longitude = lon;
	}
	#endregion

	#region Get Advanced GeoLoc
	public GameObject GetPosObject(double lat, double lon, float maxDistance = 20f) {
		UpdateGroundCampass();
		UpdateGroundLatLonByCameraPos();
		Vector3 objPos = FastGetDeltaDistance(groundLatitude, groundLongitude, lat, lon);
		if (FastGetDistance(camLatitude, camLongitude, lat, lon) > maxDistance) {
			return null;
		}
		//if (objPos.sqrMagnitude > maxDistance * maxDistance) {
		//	return null;
		//}
		GameObject ret = new GameObject("go_at_geoloc:" + lat + "," + lon);
		ret.transform.position = transform.TransformPoint(objPos);
		ret.transform.rotation = transform.rotation;
		return ret;
	}
	#endregion

	#region coordination conversion
	//地球半径，单位米
	private const double EARTH_RADIUS = 6378137;
	/// <summary>
	/// 圆周率 较 Math.PI 精度更大
	/// </summary>
	private const double PI = 3.1415926535897932384626;
	private const double X_PI = PI * 3000.0 / 180.0;

	/// <summary>
	/// 地理位置是否位于中国以外
	/// </summary>
	/// <param name="wgLat">WGS-84坐标纬度</param>
	/// <param name="wgLng">WGS-84坐标经度</param>
	/// <returns>
	///     true：国外
	///     false：国内
	/// </returns>
	public static bool OutOfChina(double wgLat, double wgLng) {
		if (wgLng < 72.004 || wgLng > 137.8347) {
			return true;
		}

		if (wgLat < 0.8293 || wgLat > 55.8271) {
			return true;
		}

		return false;
	}

	/// <summary>
	/// WGS-84坐标系转火星坐标系 (GCJ-02)
	/// 高德、腾讯使用此坐标系
	/// </summary>
	/// <param name="wgLat">WGS-84坐标纬度</param>
	/// <param name="wgLng">WGS-84坐标经度</param>
	/// <param name="mgLat">输出：GCJ-02坐标纬度</param>
	/// <param name="mgLng">输出：GCJ-02坐标经度</param>
	public static void WGS84ToGCJ02(double wgLat, double wgLng, out double mgLat, out double mgLng) {
		if (OutOfChina(wgLat, wgLng)) {
			mgLat = wgLat;
			mgLng = wgLng;
		} else {
			Delta(wgLat, wgLng, out double dLat, out double dLon);
			mgLat = wgLat + dLat;
			mgLng = wgLng + dLon;
		}
	}
	/// <summary>
	/// 火星坐标系 (GCJ-02)转百度坐标系 (BD-09)
	/// 百度使用此坐标系
	/// </summary>
	/// <param name="mgLat">GCJ-02坐标纬度</param>
	/// <param name="mgLon">GCJ-02坐标经度</param>
	/// <param name="bdLat">输出：百度坐标系纬度</param>
	/// <param name="bdLon">输出：百度坐标系经度</param>
	public static void GCJ02ToBD09(double mgLat, double mgLon, out double bdLat, out double bdLon) {
		double x = mgLon;
		double y = mgLat;
		double z = Math.Sqrt(x * x + y * y) + 0.00002 * Math.Sin(y * X_PI);
		double theta = Math.Atan2(y, x) + 0.000003 * Math.Cos(x * X_PI);
		bdLat = z * Math.Sin(theta) + 0.006;
		bdLon = z * Math.Cos(theta) + 0.0065;
	}

	/// <summary>
	/// WGS-84坐标系转百度坐标系 (BD-09)
	/// </summary>
	/// <param name="wgLat">WGS-84坐标纬度</param>
	/// <param name="wgLon">WGS-84坐标经度</param>
	/// <param name="bdLat">输出：百度坐标系纬度</param>
	/// <param name="bdLon">输出：百度坐标系经度</param>
	public static void WGS84ToBD09(double wgLat, double wgLon, out double bdLat, out double bdLon) {
		double mgLat;
		double mgLon;

		WGS84ToGCJ02(wgLat, wgLon, out mgLat, out mgLon);
		GCJ02ToBD09(mgLat, mgLon, out bdLat, out bdLon);
	}

	/// <summary>
	/// 火星坐标系 (GCJ-02)转WGS-84坐标系
	/// </summary>
	/// <param name="mgLat">GCJ-02坐标纬度</param>
	/// <param name="mgLng">GCJ-02坐标经度</param>
	/// <param name="wgLat">输出：WGS-84坐标纬度</param>
	/// <param name="wgLng">输出：WGS-84坐标经度</param>
	public static void GCJ02ToWGS84(double mgLat, double mgLng, out double wgLat, out double wgLng) {
		if (OutOfChina(mgLat, mgLng)) {
			wgLat = mgLat;
			wgLng = mgLng;
		} else {
			double dLat;
			double dLon;
			Delta(mgLat, mgLng, out dLat, out dLon);
			wgLat = mgLat - dLat;
			wgLng = mgLng - dLon;
		}
	}
	/// <summary>
	/// 百度坐标系 (BD-09)转火星坐标系 (GCJ-02)
	/// </summary>
	/// <param name="bdLat">百度坐标系纬度</param>
	/// <param name="bdLon">百度坐标系经度</param>
	/// <param name="mgLat">输出：GCJ-02坐标纬度</param>
	/// <param name="mgLon">输出：GCJ-02坐标经度</param>         
	public static void BD09ToGCJ02(double bdLat, double bdLon, out double mgLat, out double mgLon) {
		double x = bdLon - 0.0065;
		double y = bdLat - 0.006;
		double z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * X_PI);
		double theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * X_PI);
		mgLat = z * Math.Sin(theta);
		mgLon = z * Math.Cos(theta);
	}
	/// <summary>
	/// 百度坐标系 (BD-09)转WGS-84坐标系
	/// </summary>
	/// <param name="bdLat">百度坐标系纬度</param>
	/// <param name="bdLon">百度坐标系经度</param>
	/// <param name="wgLat">输出：WGS-84坐标纬度</param>
	/// <param name="wgLon">输出：WGS-84坐标经度</param>
	public static void BD09ToWGS84(double bdLat, double bdLon, out double wgLat, out double wgLon) {
		double mgLat;
		double mgLon;

		BD09ToGCJ02(bdLat, bdLon, out mgLat, out mgLon);
		GCJ02ToWGS84(mgLat, mgLon, out wgLat, out wgLon);
	}
	private static void Delta(double Lat, double Lon, out double dLat, out double dLon) {
		const double AXIS = 6378245.0;
		const double EE = 0.00669342162296594323;

		dLat = TransformLat(Lon - 105.0, Lat - 35.0);
		dLon = TransformLon(Lon - 105.0, Lat - 35.0);
		double radLat = Lat / 180.0 * PI;
		double magic = Math.Sin(radLat);
		magic = 1 - EE * magic * magic;
		double sqrtMagic = Math.Sqrt(magic);
		dLat = (dLat * 180.0) / ((AXIS * (1 - EE)) / (magic * sqrtMagic) * PI);
		dLon = (dLon * 180.0) / (AXIS / sqrtMagic * Math.Cos(radLat) * PI);
	}

	private static double TransformLat(double x, double y) {
		double ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y + 0.2 * Math.Sqrt(Math.Abs(x));
		ret += (20.0 * Math.Sin(6.0 * x * PI) + 20.0 * Math.Sin(2.0 * x * PI)) * 2.0 / 3.0;
		ret += (20.0 * Math.Sin(y * PI) + 40.0 * Math.Sin(y / 3.0 * PI)) * 2.0 / 3.0;
		ret += (160.0 * Math.Sin(y / 12.0 * PI) + 320 * Math.Sin(y * PI / 30.0)) * 2.0 / 3.0;
		return ret;
	}

	private static double TransformLon(double x, double y) {
		double ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1 * Math.Sqrt(Math.Abs(x));
		ret += (20.0 * Math.Sin(6.0 * x * PI) + 20.0 * Math.Sin(2.0 * x * PI)) * 2.0 / 3.0;
		ret += (20.0 * Math.Sin(x * PI) + 40.0 * Math.Sin(x / 3.0 * PI)) * 2.0 / 3.0;
		ret += (150.0 * Math.Sin(x / 12.0 * PI) + 300.0 * Math.Sin(x / 30.0 * PI)) * 2.0 / 3.0;
		return ret;
	}

	#endregion

	#region Lat-Lon to XYZ
	private static double rad(double d) {
		return d * PI / 180.0;
	}
	private static double deg(double r) {
		return r * 180 / PI;
	}
	public static double GetDistance(double lat1, double lng1, double lat2, double lng2) {
		double radLat1 = rad(lat1);
		double radLat2 = rad(lat2);
		double a = radLat1 - radLat2;
		double b = rad(lng1 - lng2);
		double s = 2 * Mathf.Asin(Mathf.Sqrt(Mathf.Pow(Mathf.Sin((float)a / 2), 2) +
			Mathf.Cos((float)radLat1) * Mathf.Cos((float)radLat2) * Mathf.Pow(Mathf.Sin((float)b / 2), 2)));
		s = s * EARTH_RADIUS;
		s = Mathf.Round((float)s * 10000) / 10000;
		return s;
	}

	//备注：z轴为北
	static double FastGetDeltaN(double lat1, double lat2, double lng) {
		double s = rad(lat2 - lat1);
		s *= EARTH_RADIUS;
		s = Mathf.Round((float)s * 10000) / 10000;
		return s;
	}

	//备注：x轴为东
	static double FastGetDeltaE(double lat, double lng1, double lng2) {
		double radLat = rad(lat);
		double b = rad(lng2 - lng1);
		double s = 2 * Mathf.Asin(Mathf.Cos((float)radLat) * Mathf.Sin((float)b / 2));
		s *= EARTH_RADIUS;
		s = Mathf.Round((float)s * 10000) / 10000;
		return s;
	}
	public static Vector3 FastGetDeltaDistance(double startLat, double startLng, double endLat, double endLng) {
		return new Vector3((float)FastGetDeltaE(startLat, startLng, endLng), 0, (float)FastGetDeltaN(startLat, endLat, endLng));
	}
	public static double FastGetDistance(double startLat, double startLng, double endLat, double endLng) {
		double x = FastGetDeltaE(startLat, startLng, endLng);
		double z = FastGetDeltaN(startLat, endLat, endLng);
		return Mathf.Sqrt((float)(x * x + z * z));
	}
	public static Vector3 FastGetDeltaDistance(Vector2 startLatLng, Vector2 endLatLng) {
		return new Vector3((float)FastGetDeltaE(startLatLng.x, startLatLng.y, endLatLng.y), 0, (float)FastGetDeltaN(startLatLng.x, endLatLng.x, endLatLng.y));
	}
	#endregion

	#region  XYZ to Lat-Lon
	static double FastGetNewLat(double lat, double lng, double zDistance) {
		double s = zDistance / EARTH_RADIUS;
		s += rad(lat);
		return deg(s);
	}
	static double FastGetNewLng(double lat, double lng, double xDistance) {
		double s = xDistance / EARTH_RADIUS;
		s = 2 * Mathf.Asin(Mathf.Sin((float)(s / 2)) / Mathf.Cos((float)rad(lat)));
		s += rad(lng);
		return deg(s);
	}

	public static void GetEndLatLng(double startLat, double startLng, Vector3 localPosition, out double endLat, out double endLng) {
		endLat = FastGetNewLat(startLat, startLng, localPosition.z);
		endLng = FastGetNewLng(startLat, startLng, localPosition.x);
	}
	public static Vector2 GetEndLatLng(double startLat, double startLng, Vector3 localPosition) {
		return new Vector2((float)FastGetNewLat(startLat, startLng, localPosition.z), (float)FastGetNewLng(startLat, startLng, localPosition.x));
	}
	#endregion
}

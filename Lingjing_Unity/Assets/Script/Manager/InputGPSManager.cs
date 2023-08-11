using UnityEngine;
using Config;
using System.Collections;

public class InputGPSManager : MonoBehaviour {
	#region SI
	private static InputGPSManager instance = null;
	public static InputGPSManager getInstance(){
		if (!instance) {
			GameObject go = new GameObject("InputGPSManager");  
			DontDestroyOnLoad(go);  
			instance = go.AddComponent<InputGPSManager>();
		}
		return instance;
	}
	#endregion
	private void Start() {
		if (ConfigInfo.test.testFlag) {
			//测试模式阻止GPS刷新
			m_vec2GroundLatLon = ConfigInfo.test.testLatLon;
			m_camLatitude = ConfigInfo.test.testLatLon.x;
			m_camLongitude = ConfigInfo.test.testLatLon.y;
		}
	}

	#region Update Info
	//logic process

	//相对位置的经纬度
	public Vector2	m_vec2GroundLatLon = Vector2.zero;
	public double m_camLatitude = 0f;
	public double m_camLongitude = 0f;
	public Vector3 m_camPos = Vector3.zero;
	public bool isCardShow = true;

	/// <summary>
	/// 接受Android 定位sdk数据
	/// </summary>
	//public void UpdateGPSPositionGD(string info){
	//	//process sdk callback info
	//	string[] strs = info.Split(',');
	//	if(strs.Length >= 2
	//		&& Mathf.Abs(float.Parse(strs[0])) > 0.001f
	//		&& Mathf.Abs(float.Parse(strs[1])) > 0.001f){
	//		m_camLatitude = double.Parse(strs[0]);
	//		m_camLongitude = double.Parse(strs[1]);
	//		if (ConfigInfo.test.testFlag) {
	//			//测试模式阻止GPS刷新
	//			m_camLatitude = ConfigInfo.test.testLatLon.x;
	//			m_camLongitude = ConfigInfo.test.testLatLon.y;
	//		}
	//		//导航时停止摄像机移动，节省性能
	//		//if (isCardShow) {
	//		//	//SLogManager.LogInfo("SInputGPSManager: Cam LatLon change to " + m_camLatitude + ", " + m_camLongitude);
	//		//	NotificationCenter.DefaultCenter ().PostNotification ("UpdateCameraPosFromLatLon");
	//		//}
	//	}
	//}

	///// <summary>
	///// 备注：相对坐标系原点变化时，刷新下场景建筑的显示
	///// </summary>
	///// <param name="newLatLon">New rela lat lon.</param>
	//public void UpdateGroundLatLon(Vector2 newLatLon) {
	//	//SLogManager.LogInfo("SInputGPSManager: Map LatLon change to " + newLatLon.x + ", " + newLatLon.y);
	//	m_vec2GroundLatLon = newLatLon;
	//	//NotificationCenter.DefaultCenter().PostNotification("UpdateBuildings");
	//}
	#endregion

	#region Lat-Lon to XYZ
	//latitude纬度, longitude经度 to distance
	private const double EARTH_RADIUS = 6378.137 * 1000;//地球半径
	private static double rad(double d) {
		return d * Mathf.PI / 180.0;
	}
	public static double GetDistance(double lat1, double lng1, double lat2, double lng2) {
		double radLat1 = rad(lat1);
		double radLat2 = rad(lat2);
		double a = radLat1 - radLat2;
		double b = rad(lng1) - rad(lng2);
		double s = 2 * Mathf.Asin(Mathf.Sqrt(Mathf.Pow(Mathf.Sin((float)a / 2), 2) +
			Mathf.Cos((float)radLat1) * Mathf.Cos((float)radLat2) * Mathf.Pow(Mathf.Sin((float)b / 2), 2)));
		s = s * EARTH_RADIUS;
		s = Mathf.Round((float)s * 10000) / 10000;
		return s;
	}

	//备注：z轴为北
	public double GetDeltaDistanceN(double lat1, double lng1, double lat2, double lng2){
		double dRet = 0.0f;
		dRet = GetDistance (lat1, lng1, lat2, lng1);
		if(lat2 < lat1){
			dRet *= -1;
		}
		return dRet;
	}

	//备注：x轴为东
	public double GetDeltaDistanceE(double lat1, double lng1, double lat2, double lng2){
		double dRet = 0.0f;
		dRet = GetDistance (lat1, lng1, lat1, lng2);
		if(lng2 < lng1){
			dRet *= -1;
		}
		return dRet;
	}

	//将经纬度转换为偏移量坐标
	public Vector3 GetDeltaDistance(Vector2 startLatLng, Vector2 endLatLng){
		return new Vector3 ((float)(GetDeltaDistanceE(startLatLng.x, startLatLng.y, endLatLng.x, endLatLng.y))
			, 0
			, (float)GetDeltaDistanceN(startLatLng.x, startLatLng.y, endLatLng.x, endLatLng.y));
	}

	//根据当前的经纬度计算出一个相对偏移量
	public Vector3 GetGroundDistance(Vector2 endLatLng){
		return GetDeltaDistance (m_vec2GroundLatLon, endLatLng);
	}
	public Vector3 GetCamDistance(Vector2 endLatLng) {
		return GetDeltaDistance(new Vector2((float)m_camLatitude, (float)m_camLongitude), endLatLng);
	}
	#endregion
}

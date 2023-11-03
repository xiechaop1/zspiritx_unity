using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config {
	public static class ConfigInfo {
		public static int userId = 3;   //µÇÂ¼ÕËºÅ
		public static int storyId = 3;  //¾ç±¾±àºÅ 
		public static int sessionId = 15;
		public const string versionId = "1.0.20231103" + (isDevelop ? "d" : "p");
		public const bool isDevelop = false;

		public static class Test {
			public static bool testFlag = !isDevelop;
			//public static Vector2 testLatLon = new Vector2(39.9928f, 116.3912f);
			public static Vector2 testLatLon = new Vector2(39.85441898f, 116.36652048f);
		}
	}
}
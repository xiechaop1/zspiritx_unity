using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config {
	public static class ConfigInfo {
		public static int userId = 1;	//µÇÂ¼ÕËºÅ
		public static int storyId = 1;  //¾ç±¾±àºÅ 
		public static int sessionId = 0;

		public static class test {
			public const bool testFlag = true;
			public static Vector2 testLatLon = new Vector2(39.9928f, 116.3912f);
		}
	}
}
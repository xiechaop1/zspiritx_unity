using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config {
	public static class ConfigInfo {
		public static int userId = 1;	//��¼�˺�
		public static int storyId = 1;  //�籾��� 
		public static int sessionId = 0;

		public static class test {
			public const bool testFlag = true;
			public static Vector2 testLatLon = new Vector2(39.9928f, 116.3912f);
		}
	}
}
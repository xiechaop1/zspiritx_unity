namespace Network {
	public static class HttpUrlInfo {
#if UNITY_ANDROID && !UNITY_EDITOR
		public static string filepath = "jar:file://" + UnityEngine.Application.dataPath + "!/assets/";
#elif UNITY_IOS && !UNITY_EDITOR
		public static string filepath = "file://" + UnityEngine.Application.dataPath + "/Raw/";
#else
		public static string filepath = "file://" + UnityEngine.Application.streamingAssetsPath + "/";
#endif



		public static string urlLingjingWebAddress = "https://api.zspiritx.com.cn/";
		//public static string urlLingjingQuiz = "https://h5.zspiritx.com.cn/qah5/qa_one?";
		//public static string urlLingjingBackpack = "https://h5.zspiritx.com.cn/baggageh5/all?";
		
		public static string urlLingjingHtml = "https://h5.zspiritx.com.cn/";

		public static string urlLingjingProcess = "https://api.zspiritx.com.cn/process/";
		public static string urlLingjingUser = "https://api.zspiritx.com.cn/user/";


	}
}

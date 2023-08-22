namespace Network
{
    public static class HttpUrlInfo
    {
#if UNITY_ANDROID && !UNITY_EDITOR
		public static string filepath = "jar:file://" + UnityEngine.Application.dataPath + "!/assets/";
#elif UNITY_IOS && !UNITY_EDITOR
		public static string filepath = "file://" + UnityEngine.Application.dataPath + "/Raw/";
#else
		public static string filepath = "file://" + UnityEngine.Application.streamingAssetsPath + "/";
#endif


		//public static string urlLingjingForGet = ;
		//public static string urlLingjingForPost = ;
		public static string urlLingjingWebAddress = "https://api.zspiritx.com.cn";

		//public static string urlYunForGet = "http://60.205.227.230/";
		//public static string urlYunWebAddress = "http://60.205.227.230/";

	}
}
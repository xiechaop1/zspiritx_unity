namespace Network {
	using UnityEngine;
	using UnityEngine.Networking;
	using System.IO;
	using System.Collections;
	using System.Collections.Generic;
	public class WWWData : CustomYieldInstruction {
		public override bool keepWaiting {
			get { return !isDone && !isError; }
		}
		public bool isError {
			get { return !string.IsNullOrEmpty(_error); }
		}
		public bool isDone {
			get { return downloadHandler != null; }
		}
		public byte[] data {
			get {
				if (isDone) {
					return downloadHandler.data;
				} else {
					return new byte[0];
				}
			}
		}
		public string text {
			get {
				if (isDone) {
					return downloadHandler.text;
				} else {
					return "";
				}
			}
		}
		public Texture2D texture {
			get {
				if (isDone) {
					return ((DownloadHandlerTexture)downloadHandler).texture;
				} else {
					return new Texture2D(0, 0);
				}
			}
		}
		public string error {
			get { return _error; }
		}

		internal DownloadHandler downloadHandler;
		internal string _error = "";

		public WWWData() {
			downloadHandler = null;
		}
	}


	public class WWWManager : MonoBehaviour,IManager {
		private static WWWManager instance = null;
		public static WWWManager getInstance() {
			if (instance != null) {
				return instance;
			} else {

				GameObject go = GameObject.Find("WWWManager");
				if (go != null && go.TryGetComponent(out instance)) {
					DontDestroyOnLoad(go);
					instance = go.AddComponent<WWWManager>();
					return instance;
				}
				//GameObject go = new GameObject("WWWManager");
				DontDestroyOnLoad(go);
				//m_Instance = go.AddComponent<WWWManager>();
				//return m_Instance;
			}
			Debug.LogError("MISSING WWWManager ");
			return null;
		}
		public void Init(UIEventManager eventManager, params IManager[] managers) {

		}

		#region GET-Info
		public WWWData GetHttpInfo(string url, string getMethod, string parameter, GameObject observer, string callback) {
			WWWData ret = new WWWData();
			if (string.IsNullOrEmpty(url)) {
				ret._error = "Error: url is null or empty!";
				goto End;
			}
			if (getMethod == null) {
				ret._error = "Error: methodName is null!";
				goto End;
			}
			if (string.IsNullOrEmpty(parameter)) {
				ret._error = "Error: parameter is null or empty!";
				goto End;
			}

			string requestUri = url + getMethod + "?" + parameter;
			//SLogManager.LogInfo(string.Format("NetworkSend: uri:{0} observer:{1} callback:{2}", requestUri, observer.name, callback));
			//发送网络请求
			UnityWebRequest www = UnityWebRequest.Get(requestUri);

			StartCoroutine(GetHttpInfoAsync(www, ret, observer, callback));

End:
			return ret;
		}
		public WWWData GetHttpInfo(string url, string parameter, GameObject observer, string callback) {
			return GetHttpInfo(url, "", parameter, observer, callback);
		}
		public WWWData GetHttpInfo(string url, string getMethod, string parameter) {
			return GetHttpInfo(url, getMethod, parameter, null, "");
		}
		public WWWData GetHttpInfo(string url, string parameter) {
			return GetHttpInfo(url, "", parameter, null, "");
		}

		IEnumerator GetHttpInfoAsync(UnityWebRequest www, WWWData ret, GameObject observer, string callback) {
			yield return www.SendWebRequest();
			switch (www.result) {
				case UnityWebRequest.Result.InProgress:
					break;
				case UnityWebRequest.Result.Success:
					string result = www.downloadHandler.text;
					if (observer != null && !string.IsNullOrEmpty(callback)) {
						observer.SendMessage(callback, result, SendMessageOptions.DontRequireReceiver);
					}
					ret.downloadHandler = www.downloadHandler;
					break;
				case UnityWebRequest.Result.ConnectionError:
				case UnityWebRequest.Result.ProtocolError:
				case UnityWebRequest.Result.DataProcessingError:
					ret._error = www.error;
					break;
				default:
					break;
			}
			//if (www.isNetworkError) {
			//	//SLogManager.LogError(www.error);
			//	ret._error = www.error;
			//} else {
			//	string result = www.downloadHandler.text;
			//	SLogManager.LogInfo(string.Format("NetworkRescieve: {0} char(s) string for observer: {1} callback:{2}", result.Length, observer.name, callback));
			//	if (observer != null && !string.IsNullOrEmpty(callback)) {
			//		observer.SendMessage(callback, result, SendMessageOptions.DontRequireReceiver);
			//	}
			//	ret.downloadHandler = www.downloadHandler;
			//}
		}
		#endregion

		#region GET-Texture
		public WWWData GetTexture(string url) {
			WWWData ret = new WWWData();
			if (string.IsNullOrEmpty(url)) {
				ret._error = "Error: url is null or empty!";
				goto End;
			}
			//SLogManager.LogInfo(string.Format("NetworkTextureGet: url:{0}", url));
			UnityWebRequest www = UnityWebRequestTexture.GetTexture(url, true);
			StartCoroutine(GetTextureAsync(www, ret));

End:
			return ret;
		}

		IEnumerator GetTextureAsync(UnityWebRequest www, WWWData ret) {
			yield return www.SendWebRequest();
			switch (www.result) {
				case UnityWebRequest.Result.InProgress:
					break;
				case UnityWebRequest.Result.Success:
					ret.downloadHandler = www.downloadHandler;
					break;
				case UnityWebRequest.Result.ConnectionError:
				case UnityWebRequest.Result.ProtocolError:
				case UnityWebRequest.Result.DataProcessingError:
					ret._error = www.error;
					break;
				default:
					break;
			}
			//if (www.isNetworkError) {
			//	ret._error = www.error;
			//} else {
			//	//SLogManager.LogInfo(string.Format("NetworkRescieve: {0}byte(s) texture", www.downloadHandler.data.Length));
			//	ret.downloadHandler = www.downloadHandler;
			//}
		}
		#endregion

		#region POST-Info
		public WWWData PostHttpMsg(string url, string postMethod, string postParameter) {
			WWWData ret = new WWWData();
			if (string.IsNullOrEmpty(url)) {
				ret._error = "Error: url is null or empty!";
				goto End;
			}
			if (postMethod == null) {
				ret._error = "Error: methodName is null!";
				goto End;
			}
			if (string.IsNullOrEmpty(postParameter)) {
				ret._error = "Error: parameter is null or empty!";
				goto End;
			}
			string requestUri = url + postMethod;
			UnityWebRequest www = UnityWebRequest.PostWwwForm(requestUri, postParameter);
			StartCoroutine(PostHttpMsgAsync(www, ret));
End:
			return ret;
		}
		IEnumerator PostHttpMsgAsync(UnityWebRequest www, WWWData ret) {
			yield return www.SendWebRequest();
			switch (www.result) {
				case UnityWebRequest.Result.InProgress:
					break;
				case UnityWebRequest.Result.Success:
					ret.downloadHandler = www.downloadHandler;
					break;
				case UnityWebRequest.Result.ConnectionError:
				case UnityWebRequest.Result.ProtocolError:
				case UnityWebRequest.Result.DataProcessingError:
					ret._error = www.error;
					break;
				default:
					break;
			}
			//if (www.isNetworkError) {
			//	ret._error = www.error;
			//} else {
			//	//SLogManager.LogInfo(string.Format("NetworkRescieve: {0} char(s) string after send request", www.downloadHandler.text.Length));
			//	ret.downloadHandler = www.downloadHandler;
			//}
		}
		#endregion

		#region POST-File
		public WWWData PostFile(string url, string postMethod, string fileName, byte[] data, string contentType) {
			WWWData ret = new WWWData();
			if (string.IsNullOrEmpty(url)) {
				ret._error = "Error: url is null or empty!";
				goto End;
			}
			if (postMethod == null) {
				ret._error = "Error: methodName is null!";
				goto End;
			}
			if (string.IsNullOrEmpty(fileName)) {
				ret._error = "Error: fileName is null or empty!";
				goto End;
			}

			string requestUri = url + postMethod;
			//SLogManager.LogInfo(string.Format("NetworkPostFile: uri:{0} dataType:{1} filesize:{2}", requestUri, contentType,data.Length));

			//DownloadHandler downloader = new DownloadHandlerBuffer();
			//UploadHandler uploader = new UploadHandlerRaw(data);
			//uploader.contentType = contentType;
			//UnityWebRequest www = new UnityWebRequest(requestUri, UnityWebRequest.kHttpVerbPUT, downloader, uploader);

			//List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
			//if (!string.IsNullOrEmpty(postParameter)) {
			//	formData.Add(new MultipartFormDataSection(postParameter));
			//}
			//formData.Add(new MultipartFormFileSection("Filedata",data,fileName,contentType));
			WWWForm formData = new WWWForm();
			formData.AddBinaryData("Filedata", data, fileName, contentType);
			UnityWebRequest www = UnityWebRequest.Post(requestUri, formData);

			StartCoroutine(PostFileAsync(www, ret));
End:
			return ret;
		}

		IEnumerator PostFileAsync(UnityWebRequest www, WWWData ret) {
			yield return www.SendWebRequest();
			switch (www.result) {
				case UnityWebRequest.Result.InProgress:
					break;
				case UnityWebRequest.Result.Success:
					ret.downloadHandler = www.downloadHandler;
					break;
				case UnityWebRequest.Result.ConnectionError:
				case UnityWebRequest.Result.ProtocolError:
				case UnityWebRequest.Result.DataProcessingError:
					ret._error = www.error;
					break;
				default:
					break;
			}
			//if (www.isNetworkError) {
			//	ret._error = www.error;
			//} else {
			//	//SLogManager.LogInfo(string.Format("NetworkRescieve: {0} char(s) string after upload", www.downloadHandler.text.Length));
			//	ret.downloadHandler = www.downloadHandler;
			//}
		}
		#endregion
	}

}

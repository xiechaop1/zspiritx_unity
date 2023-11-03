using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Network;

public class ResourcesLibrary : MonoBehaviour {
	public GameObject[] lstPrefabs;
	public AudioClip[] audios => lstAudio.ToArray();
	[SerializeField]
	List<AudioClip> lstAudio = new List<AudioClip>();
	public AudioClip[] voiceLogs;

	private void Awake() {
		StartCoroutine(LoadAudioFromUrl());
	}
	public bool canBackground => priorityList.Count == 0;
	public void AddPriorityAudioClip(string url) {
		priorityList.Enqueue(url);
	}
	public void AddAudioClip(string url) {
		updateList.Enqueue(url);
	}

	Queue<string> priorityList = new Queue<string>();
	Queue<string> updateList = new Queue<string>();
	private IEnumerator LoadAudioFromUrl() {
		yield return null;
		//initialize
		string url = "";
		string extention;
		AudioType audioType;
		AudioClip audioClip;
		WWWData www;

		for (; ; ) {
			url = "";
			while (string.IsNullOrEmpty(url)) {
				yield return null;
				if (priorityList.Count > 0) {
					url = priorityList.Dequeue();
				} else if (updateList.Count > 0) {
					url = updateList.Dequeue();
				}
			}

			////wait for item in queue
			//yield return updateList;

			//url = updateList.Dequeue();
			//if (string.IsNullOrEmpty(url)) {
			//	continue;
			//}
			try {
				extention = url.Split('.').Last();
				switch (extention) {
					case "aif":
					case "aiff":
						audioType = AudioType.AIFF;
						break;
					case "mp3":
						audioType = AudioType.MPEG;
						break;
					case "ogg":
						audioType = AudioType.OGGVORBIS;
						break;
					case "wav":
						audioType = AudioType.WAV;
						break;
					default:
						audioType = AudioType.WAV;
						break;
				}
			} catch (System.Exception) {
				LogManager.Warning("Try get file extension Failed");
				goto LoadFail;
			}

			if (extention == url) {
				LogManager.Warning("Missing File extension");
				goto LoadFail;
			}
			//Debug.Log(url);
			www = WWWManager.getInstance().GetAudioClip(url, audioType);
			yield return www;

			//skip error
			if (www.isError) {
				LogManager.Warning(www.error);
				goto LoadFail;
			}

			//SLogManager.LogInfo(string.Format("AudioPlayerManager: {0} audio download success", item._id));
			audioClip = www.audioClip;

			// check loaded
			if (audioClip.loadState == AudioDataLoadState.Unloaded) {
				audioClip.LoadAudioData();
			}
			yield return new WaitUntil(() => { return (audioClip.loadState == AudioDataLoadState.Loaded || audioClip.loadState == AudioDataLoadState.Failed); });
			if (audioClip.loadState == AudioDataLoadState.Failed) {
				goto LoadFail;
			}
			audioClip.name = url.Split('/').Last();
			lstAudio.Add(audioClip);
			continue;

LoadFail:
			;

		}
	}



}

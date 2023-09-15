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
	public void AddAudioClip(string url) {
		updateList.Enqueue(url);
	}
	CoroutineQueue<string> updateList = new CoroutineQueue<string>();
	private IEnumerator LoadAudioFromUrl() {
		yield return null;
		//initialize
		string url;
		string extention;
		AudioType audioType;
		AudioClip audioClip;
		WWWData www;

		for (; ; ) {
			//wait for item in queue
			yield return updateList;

			url = updateList.Dequeue();
			if (string.IsNullOrEmpty(url)) {
				continue;
			}
			try {
				extention = url.Split('.').Last();
				switch (extention) {
					case "wav":
					default:
						audioType = AudioType.WAV;
						break;
				}
			} catch (System.Exception) {
				LogManager.Warning("Try get file extension Failed");
				goto LoadFail;
			}

			if (extention==url) {
				LogManager.Warning("Missing File extension");
				goto LoadFail;
			}

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

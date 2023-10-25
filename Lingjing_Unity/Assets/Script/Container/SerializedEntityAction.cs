using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SerializedEntityAction {
	string localID;
	public string name = "";
	public string sentence = "";
	public string url = "";
	public string quizID = "";
	string clipID;
	public string clipURL = "";
	public AudioClip sentenceClip;

	public string[] userSelections;
	string[] nextID;
	public SerializedEntityAction[] nextAction;

	public string[] showModels;
	public string[] hideModels;
	public string[] pickupModels;
	public Vector3 displacement;

	public string combatInfo;
	public SerializedEntityAction(string json) {
		JSONReader obj;
		string tmpStr = "";
		double tmpf = 0.0;
		List<string> tmpLst;
		try {
			obj = new JSONReader(json);
		} catch {
			Debug.LogError("NONE-JSON-SENTENCE: " + json);
			return;
		}

		if (obj.TryPraseString("localID", ref tmpStr)) {
			localID = tmpStr;
		} else {
			Debug.LogError("NONE-ID-SENTENCE: " + json);
		}
		if (obj.TryPraseString("name", ref tmpStr)) {
			name = tmpStr;
		}
		if (obj.TryPraseString("sentence", ref tmpStr)) {
			sentence = tmpStr;
		}

		if (obj.TryPraseString("url", ref tmpStr)) {
			url = tmpStr;
		}
		if (obj.TryPraseString("quizID", ref tmpStr)) {
			if (int.TryParse(tmpStr, out int tmpInt) && tmpInt > 0) {
				quizID = tmpStr;
			}
		}
		if (obj.TryPraseString("sentenceClip", ref tmpStr)) {
			clipID = tmpStr;
		}
		if (obj.TryPraseString("sentenceClipURL", ref tmpStr) && !string.IsNullOrEmpty(tmpStr)) {
			clipURL = tmpStr;
			clipID = tmpStr.Split('/').Last();
		}

		if (obj.TryPraseArray("userSelections", out tmpLst)) {
			userSelections = tmpLst.ToArray();
		} else {
			userSelections = new string[0];
		}
		if (obj.TryPraseArray("nextID", out tmpLst)) {
			nextID = tmpLst.ToArray();
		} else {
			nextID = new string[0];
		}
		if (obj.TryPraseArray("showModels", out tmpLst)) {
			showModels = tmpLst.ToArray();
		} else {
			showModels = new string[0];
		}
		if (obj.TryPraseArray("hideModels", out tmpLst)) {
			hideModels = tmpLst.ToArray();
		} else {
			hideModels = new string[0];
		}
		if (obj.TryPraseArray("pickupModels", out tmpLst)) {
			pickupModels = tmpLst.ToArray();
		} else {
			pickupModels = new string[0];
		}
		Vector3 move = Vector3.zero;
		if (obj.TryPraseDouble("moveX", ref tmpf)) {
			move.x = (float)tmpf;
		}
		if (obj.TryPraseDouble("moveY", ref tmpf)) {
			move.y = (float)tmpf;
		}
		if (obj.TryPraseDouble("moveZ", ref tmpf)) {
			move.z = (float)tmpf;
		}
		displacement = move;
		if (obj.TryPraseString("combatInfo", ref tmpStr) && !string.IsNullOrEmpty(tmpStr)) {
			combatInfo = tmpStr;
		} else {
			combatInfo = "";
		}
	}
	public void LinkSentences(IEnumerable<SerializedEntityAction> lstSentences) {
		string Id;
		SerializedEntityAction output;
		nextAction = new SerializedEntityAction[nextID.Length];
		for (int i = 0; i < nextID.Length; i++) {
			output = null;
			Id = nextID[i];
			try {
				output = lstSentences.First(chat => chat.localID == Id);
				nextAction[i] = output;
			} catch (Exception) {

			}

			if (output == null) {
				Debug.LogError("MISSING-SENTENCE OR WRONG ID: " + nextID);
			}
		}
	}
	public void LinkClips(IEnumerable<AudioClip> lstClips) {
		try {
			//Debug.Log(clipID);
			//foreach (var clip in lstClips) {
			//	Debug.Log(clipID + "  " + clip.name + "  " + clip.name == clipID);
			//}
			sentenceClip = lstClips.First(clip => clip.name == clipID);
			//Debug.Log(sentenceClip.name);
		} catch (Exception) {

		}
	}
	public static SerializedEntityAction FindSentence(string id, IEnumerable<SerializedEntityAction> source) {
		return source.First(x => x.localID == id);
	}
}

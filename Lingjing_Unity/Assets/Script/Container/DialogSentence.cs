using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialogSentence {
	string localID;
	public string sentence = "";
	public string url = "";
	public string quizID = "";
	string clipID;
	public AudioClip sentenceClip;
	public string[] userSelections;
	string[] nextID;
	public DialogSentence[] nextSentence;
	public DialogSentence(string json) {
		JSONReader obj;
		string tmpStr = "";
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
		if (obj.TryPraseString("sentence", ref tmpStr)) {
			sentence = tmpStr;
		}

		if (obj.TryPraseString("url", ref tmpStr)) {
			url = tmpStr;
		}
		if (obj.TryPraseString("quizID", ref tmpStr)) {
			quizID = tmpStr;
		}
		if (obj.TryPraseString("sentenceClip", ref tmpStr)) {
			clipID = tmpStr;
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
	}
	public void LinkSentences(IEnumerable<DialogSentence> lstSentences) {
		string Id;
		DialogSentence output;
		nextSentence = new DialogSentence[nextID.Length];
		for (int i = 0; i < nextID.Length; i++) {
			Id = nextID[i];
			output = lstSentences.First(chat => chat.localID == Id);
			nextSentence[i] = output;
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
	public static DialogSentence FindSentence(string id, IEnumerable<DialogSentence> source) {
		return source.First(x => x.localID == id);
	}
}

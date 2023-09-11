using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldStageInfo : MonoBehaviour {
	public enum StageToggleType {
		ARTag,
		Location,
	}
	public StageToggleType stageToggleType;
	public string uuidARTag = "";
	public string uuidBGM = "";
	public float lat = 0f;
	public float lng = 0f;

	public string[] lstFieldEntityUUID;
	public string[] lstTaggedEntityUUID;
	public string[] lstGeolocEntityUUID;
	public AudioClip[] voiceLogs;

	public FieldStageInfo[] nextStages= new FieldStageInfo[0];
}
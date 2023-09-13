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
	public double lat = 0f;
	public double lng = 0f;
	public float proximity=10f;

	public string[] lstStageEntitiesUUID;
	public string[] lstStageEntityInfos;
	public AudioClip[] voiceLogs;

	public FieldStageInfo[] nextStages= new FieldStageInfo[0];
}
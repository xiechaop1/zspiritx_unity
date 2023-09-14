using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldStageInfo{// : MonoBehaviour {
	public FieldStageInfo(){ 
	}
	public enum StageToggleType {
		None = 0,
		ARTag = 1,
		Location = 2,
	}
	public string uuid = "";
	public StageToggleType stageToggleType = StageToggleType.None;
	public string uuidARTag = "";
	public string uuidBGM = "";
	public double lat = 0f;
	public double lng = 0f;
	public float proximity = 10f;

	public FieldEntityInfo[] lstStageEntities;
	//public string[] lstStageEntitiesUUID;
	//public string[] lstStageEntityInfos;
	//[TextArea]
	//public string stageEntityInfos;
	//public AudioClip[] voiceLogs;

	public FieldStageInfo[] nextStages = new FieldStageInfo[0];
}
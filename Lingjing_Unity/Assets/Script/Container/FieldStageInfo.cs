using System.Collections;
using System.Collections.Generic;


public class FieldStageInfo {
	public enum StageToggleType {
		None = 0,
		ARTag = 1,
		Location = 2,
	}
	public string uuid = "";
	public StageToggleType stageToggleType = StageToggleType.None;
	public string uuidARTag = "";
	public string uuidBGM = "";
	public double lat = 0.0;
	public double lng = 0.0;
	public float proximity = 10f;

	public FieldEntityInfo[] lstStageEntities = new FieldEntityInfo[0];

	public FieldStageInfo[] nextStages = new FieldStageInfo[0];
}
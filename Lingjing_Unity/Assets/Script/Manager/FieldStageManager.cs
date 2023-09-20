using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Network;
using Config;

public class FieldStageManager : MonoBehaviour, IManager {
	public ResourcesLibrary resourcesManager;
	public AudioSource backgroundMusicPlayer;

	public FieldStageInfo currentStage;
	private FieldEntityManager entityManager;
	private WWWManager networkManager;
	private List<FieldEntityInfo> entitiesManaged = new List<FieldEntityInfo>();

	public GameObject prefabAnimEmerge;

	public FieldEntityInfo[] lstFieldEntity = new FieldEntityInfo[0];
	public FieldEntityInfo[] lstTaggedEntity = new FieldEntityInfo[0];
	public FieldEntityInfo[] lstLocEntity = new FieldEntityInfo[0];

	public void Init(UIEventManager eventManager, params IManager[] managers) {
		foreach (var manager in managers) {
			RegisterManager(manager);
		}
	}
	public void RegisterManager(IManager manager) {
		if (manager is FieldEntityManager) {
			entityManager = manager as FieldEntityManager;
		} else if (manager is WWWManager) {
			networkManager = manager as WWWManager;
		}
	}
	public void ImageFound(string uuid) {
		FieldStageInfo[] nextStages = currentStage.nextStages;
		foreach (var stageInfo in nextStages) {
			if (stageInfo.stageToggleType == FieldStageInfo.StageToggleType.ARTag && stageInfo.uuidARTag == uuid) {
				StageAdvanced(stageInfo);
				break;
			}
		}
	}
	public void LocationUpdate(double newLat, double newLng) {
		FieldStageInfo[] nextStages = currentStage.nextStages;
		foreach (var stageInfo in nextStages) {
			if (stageInfo.stageToggleType == FieldStageInfo.StageToggleType.Location) {
				if ((stageInfo.lat == 0 && stageInfo.lng == 0) ||
					InputGPSManager.FastGetDistance(stageInfo.lat, stageInfo.lng, newLat, newLng) < stageInfo.proximity) {
					StageAdvanced(stageInfo);
					break;
				}
			}
		}
	}

	public void StageAdvanced(FieldStageInfo targetStage) {
		StartCoroutine(AdvanceStages(targetStage));
	}

	bool isStaging = false;
	private IEnumerator AdvanceStages(FieldStageInfo targetStage) {
		if (isStaging) {
			yield break;
		}
		isStaging = true;
		yield return null;
		entityManager.StopStage();
		yield return null;
		PrepareBackstage(targetStage);
		yield return null;
		entityManager.PrepareStage();
		yield return null;
		isStaging = false;
		if (targetStage.stageToggleType == FieldStageInfo.StageToggleType.ARTag) {
			entityManager.UpdateImageTracking(targetStage.uuidARTag);
		}
		yield break;
	}

	#region Backstage
	public void PrepareBackstage(FieldStageInfo targetStage) {
		currentStage = targetStage;
		LoadStageEntities(targetStage.lstStageEntities);
		if (!string.IsNullOrWhiteSpace(targetStage.uuidBGM)) {
			LoadMusic(targetStage.uuidBGM);
		}
	}
	public void CleanBackstage() {
		currentStage = null;
		lstFieldEntity = lstTaggedEntity = lstLocEntity = null;
		backgroundMusicPlayer.Pause();
	}

	void LoadStageEntities(FieldEntityInfo[] entities) {
		List<FieldEntityInfo> lstField = new List<FieldEntityInfo>();
		List<FieldEntityInfo> lstTagged = new List<FieldEntityInfo>();
		List<FieldEntityInfo> lstGeoLoc = new List<FieldEntityInfo>();

		FieldEntityInfo info;
		for (int i = 0; i < entities.Length; i++) {
			if (entities[i] != null && entitiesManaged.Contains(entities[i])) {
				info = entities[i];
			} else {
				continue;
			}
			switch (info.enumARType) {
				case FieldEntityInfo.EntityToggleType.ARTagTracking:
				case FieldEntityInfo.EntityToggleType.ARTagAround:
				case FieldEntityInfo.EntityToggleType.ARTagPosition:
					lstTagged.Add(info);
					break;
				case FieldEntityInfo.EntityToggleType.GeoLocAround:
				case FieldEntityInfo.EntityToggleType.GeoLocPosition:
					lstGeoLoc.Add(info);
					break;
				case FieldEntityInfo.EntityToggleType.StageAround:
				case FieldEntityInfo.EntityToggleType.StagePosition:
				case FieldEntityInfo.EntityToggleType.RamdomAroundCam:
				default:
					lstField.Add(info);
					break;
			}
		}
		lstFieldEntity = lstField.ToArray();
		lstTaggedEntity = lstTagged.ToArray();
		lstLocEntity = lstGeoLoc.ToArray();
	}
	void LoadMusic(string uuid) {
		AudioClip clip = null;
		foreach (var music in resourcesManager.audios) {
			if (music.name == uuid) {
				clip = music;
				break;
			}
		}
		if (clip != null) {
			backgroundMusicPlayer.clip = clip;
			backgroundMusicPlayer.Play();
		} else {
			backgroundMusicPlayer.Pause();
		}
	}
	#endregion
	#region Load Prepare
	public IEnumerator AsyncPrepareStage() {
		WWWData www = networkManager.GetHttpInfo(HttpUrlInfo.urlLingjingProcess,
		   "get_session_stages",
		   string.Format("is_test=1&user_id={0}&story_id={1}&session_id={2}",
			   ConfigInfo.userId,
			   ConfigInfo.storyId,
			   ConfigInfo.sessionId
			   )
		   );
		yield return www;
		if (www.isError) {
			LogManager.Warning(www.error);
			yield break;
		}
		if (string.IsNullOrEmpty(www.text)) {
			LogManager.Warning("FAILED to create stages due to missing return info");
			yield break;
		}
		string[] lstStages = null;
		List<string> lstTmp;
		if (JSONReader.TryPraseArray(www.text, "data", out lstTmp) && lstTmp.Count > 0) {
			lstStages = lstTmp.ToArray();
		} else {
			LogManager.Warning("FAILED to create stage due to missing return info");
		}

		string tmp = "";
		List<FieldStageInfo> tmpStages = new List<FieldStageInfo>();

		for (int i = 0; i < lstStages.Length; i++) {
			if (!JSONReader.TryPraseString(lstStages[i], "id", ref tmp)) {
				LogManager.Warning("FAILED to create stage due to missing stage id");
				goto LoopEnd;
			}

			www = networkManager.GetHttpInfo(HttpUrlInfo.urlLingjingProcess,
				"get_session_models_by_stage",
				string.Format("is_test=1&user_id={0}&story_id={1}&session_id={2}&session_stage_id={3}",
					ConfigInfo.userId,
					ConfigInfo.storyId,
					ConfigInfo.sessionId,
					tmp
					)
				);
			yield return www;
			if (www.isError) {
				LogManager.Warning(www.error);
				goto LoopEnd;
			}
			if (string.IsNullOrEmpty(www.text)) {
				LogManager.Warning("FAILED to create stage due to missing return info");
				goto LoopEnd;
			}
			if (JSONReader.TryPraseArray(www.text, "data", out lstTmp) && lstTmp.Count > 0 && !string.IsNullOrWhiteSpace(lstTmp[0])) {
				tmpStages.Add(PrepareStage(lstTmp[0]));
			} else {
				LogManager.Warning("FAILED to create stage due to missing return info");
			}
LoopEnd:
			yield return null;

		}
		foreach (var stage in tmpStages) {
			stage.nextStages = tmpStages.Where(x => x != stage).ToArray();
		}
		yield return null;
		PrepareBackstage(tmpStages[0]);
		yield break;
	}


	public FieldStageInfo PrepareStage(string rawInfo) {
		FieldStageInfo stage = new FieldStageInfo();
		string tmpStr = "";
		int tmpInt = 0;
		double tmpD = 0.0;
		JSONReader jsonInfo = new JSONReader(rawInfo);
		if (jsonInfo.TryPraseString("stage", ref tmpStr)) {
			JSONReader jsonStage = new JSONReader(tmpStr);
			if (jsonStage.TryPraseString("stage_u_id", ref tmpStr)) {
				stage.uuid = tmpStr;
			} else {
				stage.uuid = "";
			}
			if (jsonStage.TryPraseInt("scan_type", ref tmpInt)) {
				switch (tmpInt) {
					case 1:
						stage.stageToggleType = FieldStageInfo.StageToggleType.ARTag;
						break;
					case 2:
						stage.stageToggleType = FieldStageInfo.StageToggleType.Location;
						break;
					default:
						stage.stageToggleType = FieldStageInfo.StageToggleType.None;
						break;
				}
			} else {
				stage.stageToggleType = FieldStageInfo.StageToggleType.None;
			}

			if (jsonStage.TryPraseString("scan_image_id", ref tmpStr)) {
				stage.uuidARTag = tmpStr;
			} else {
				stage.uuidARTag = "";
			}
			if (jsonStage.TryPraseDouble("lat", ref tmpD)) {
				stage.lat = tmpD;
			} else {
				stage.lat = 0;
			}
			if (jsonStage.TryPraseDouble("lng", ref tmpD)) {
				stage.lng = tmpD;
			} else {
				stage.lng = 0;
			}
			if (jsonStage.TryPraseString("bgm", ref tmpStr) && !string.IsNullOrWhiteSpace(tmpStr)) {
				resourcesManager.AddAudioClip(tmpStr);
				stage.uuidBGM = tmpStr.Split('/').Last();
			} else {
				stage.uuidBGM = "BGM_01";
			}

			if (jsonStage.TryPraseDouble("misrange", ref tmpD)) {
				stage.proximity = (float)tmpD;
			} else {
				stage.proximity = 0;
			}
			if (jsonInfo.TryPraseString("session_stage", ref tmpStr) && JSONReader.TryPraseInt(tmpStr, "stroy_stage_id", ref tmpInt)) {
				stage.stroy_stage_id = tmpInt;
			} else {
				stage.stroy_stage_id = 0;
			}
			if (jsonInfo.TryPraseArray("session_models", out List<string> lstModels)) {
				stage.lstStageEntities = PrepareEntities(lstModels.ToArray());
			} else {
				stage.lstStageEntities = new FieldEntityInfo[0];
			}
		}
		return stage;
	}
	#endregion
	#region Stage Entities
	public FieldEntityInfo[] PrepareEntities(string[] entityInfos) {
		List<FieldEntityInfo> entities = new List<FieldEntityInfo>();
		FieldEntityInfo entityInfo;
		foreach (var rawInfo in entityInfos) {
			entityInfo = PrepareEntity(rawInfo);
			if (entityInfo != null) {
				entities.Add(entityInfo);
			}
		}
		return entities.ToArray();
	}
	FieldEntityInfo PrepareEntity(string rawInfo) {
		string prefabName = "";
		string tmp = "";
		if (JSONReader.TryPraseString(rawInfo, "model", ref tmp)) {
			if (JSONReader.TryPraseString(tmp, "model_u_id", ref prefabName)) {

			}
		}

		if (string.IsNullOrWhiteSpace(prefabName)) {
			return null;
		}
		JSONReader infoJson;
		if (JSONReader.TryPraseString(rawInfo, "story_model", ref tmp)) {
			infoJson = new JSONReader(tmp);
		} else {
			return null;
		}
		int tmpInt = 0;
		if (JSONReader.TryPraseString(rawInfo, "session_model", ref tmp) &&
				JSONReader.TryPraseInt(tmp, "session_id", ref tmpInt) &&
				tmpInt == ConfigInfo.sessionId) {
			goto BuildEntity;
		}
		return null;

BuildEntity:
		if (JSONReader.TryPraseInt(tmp, "id", ref tmpInt)) {
			foreach (var entityManaged in entitiesManaged) {
				if (entityManaged.entitySessionId == tmpInt) {
					return entityManaged;
				}
			}
		}
		GameObject prefab = null;
		FieldEntityInfo info;
		foreach (var entity in resourcesManager.lstPrefabs) {
			if (entity != null &&
					entity.TryGetComponent(out info) &&
					info.strName == prefabName) {
				prefab = entity;
				break;
			}
		}
		if (prefab == null) {
			return null;
		}
		GameObject obj = Instantiate(prefab, entityManager.goStorage.transform);
		info = obj.GetComponent<FieldEntityInfo>();

		info.entitySessionId = tmpInt;

		if (JSONReader.TryPraseInt(tmp, "story_model_id", ref tmpInt)) {
			info.entityItemId = tmpInt;
		}

		if (infoJson.TryPraseString("model_inst_u_id", ref tmp)) {
			info.entityName = tmp;
			obj.name = tmp;
		}

		double tmpD = 0.0;
		if (infoJson.TryPraseInt("scan_type", ref tmpInt)) {
			switch (tmpInt) {
				case 1:
					info.enumARType = FieldEntityInfo.EntityToggleType.RamdomAroundCam;
					break;
				case 11:
					info.enumARType = FieldEntityInfo.EntityToggleType.ARTagAround;
					break;
				case 12:
					info.enumARType = FieldEntityInfo.EntityToggleType.ARTagPosition;
					break;
				case 21:
					info.enumARType = FieldEntityInfo.EntityToggleType.GeoLocAround;
					break;
				case 22:
					info.enumARType = FieldEntityInfo.EntityToggleType.GeoLocPosition;
					break;
				default:
					info.enumARType = FieldEntityInfo.EntityToggleType.RamdomAroundCam;
					break;
			}
		} else {
			info.enumARType = FieldEntityInfo.EntityToggleType.RamdomAroundCam;
		}
		if (infoJson.TryPraseString("scan_image_id", ref tmp)) {
			info.uuidImageTracking = tmp;
		}
		if (infoJson.TryPraseDouble("lat", ref tmpD)) {
			info.latitude = tmpD;
		}
		if (infoJson.TryPraseDouble("lng", ref tmpD)) {
			info.longitude = tmpD;
		}
		Vector3 offset = new Vector3(0, 0, 0);
		if (infoJson.TryPraseDouble("show_x", ref tmpD)) {
			offset.x = (float)tmpD;
		}
		if (infoJson.TryPraseDouble("show_y", ref tmpD)) {
			offset.y = (float)tmpD;
		}
		if (infoJson.TryPraseDouble("show_z", ref tmpD)) {
			offset.z = (float)tmpD;
		}
		info.offset = offset;
		if (infoJson.TryPraseInt("direction", ref tmpInt)) {
			switch (tmpInt) {
				case 2:
					info.isLookAt = true;
					break;
				default:
					info.isLookAt = false;
					break;
			}
		} else {
			info.isLookAt = false;
		}
		if (infoJson.TryPraseDouble("act_misrange", ref tmpD)) {
			info.proximityDialog = (float)tmpD;
		} else {
			info.proximityDialog = 0f;
		}

		if (info.enumActionType == EntityActionType.DialogActor) {
			List<string> lstRawDialogs;
			DialogSentence sentence;
			if (JSONReader.TryPraseArray(info.strHintbox, "Dialog", out lstRawDialogs)) {
				List<DialogSentence> lstSentences = new List<DialogSentence>();
				foreach (string rawDialog in lstRawDialogs) {
					sentence = new DialogSentence(rawDialog);
					lstSentences.Add(sentence);
				}
				foreach (var item in lstSentences) {
					item.LinkSentences(lstSentences);
					item.LinkClips(resourcesManager.voiceLogs);
				}

				if (JSONReader.TryPraseString(info.strHintbox, "Intro", ref tmp)) {
					info.currDialog = DialogSentence.FindSentence(tmp, lstSentences);
				} else {
					info.currDialog = lstSentences[0];
				}
				info.lstDialogs = lstSentences.ToArray();
			}
		}
		if (prefabAnimEmerge != null) {
			GameObject goAnim = Instantiate(prefabAnimEmerge, obj.transform);
			info.animEmerge = goAnim.GetComponentInChildren<ParticleSystem>();
			info.goVisual.SetActive(false);
		}

		entitiesManaged.Add(info);
		return info;
	}

	public void OnRemoveFieldEntity(FieldEntityInfo info) {
		entitiesManaged.Remove(info);
	}
	#endregion
}

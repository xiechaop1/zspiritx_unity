using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldStageManager : MonoBehaviour, IManager {
	public ResourcesLibrary resourcesManager;
	public AudioSource backgroundMusicPlayer;

	public FieldStageInfo currentStage;
	private FieldEntityManager entityManager;
	private List<GameObject> goManaged = new List<GameObject>();

	public GameObject prefabAnimEmerge;

	public GameObject goRoot;
	public GameObject[] lstFieldEntity = new GameObject[0];
	public GameObject[] lstTaggedEntity = new GameObject[0];
	public GameObject[] lstLocEntity = new GameObject[0];

	public void Init(UIEventManager eventManager, params IManager[] managers) {
		foreach (var manager in managers) {
			RegisterManager(manager);
		}
	}
	public void RegisterManager(IManager manager) {
		if (manager is FieldEntityManager) {
			entityManager = manager as FieldEntityManager;
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
	public void LocationUpdate(Vector2 newLatLng) {
		FieldStageInfo[] nextStages = currentStage.nextStages;
		foreach (var stageInfo in nextStages) {
			if (stageInfo.stageToggleType == FieldStageInfo.StageToggleType.Location) {
				if ((stageInfo.lat == 0 && stageInfo.lng == 0) ||
					InputGPSManager.FastGetDistance(stageInfo.lat, stageInfo.lng, newLatLng.x, newLatLng.y) < stageInfo.proximity) {
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

	public void PrepareBackstage(FieldStageInfo targetStage) {
		currentStage = targetStage;
		PrepareEntities(targetStage.lstStageEntityInfos);
		(lstFieldEntity, lstTaggedEntity, lstLocEntity) = LoadStageEntities(targetStage.lstStageEntitiesUUID);
		if (!string.IsNullOrWhiteSpace(targetStage.uuidBGM)) {
			LoadMusic(targetStage.uuidBGM);
		}
	}

	void LoadMusic(string uuid) {
		AudioClip clip = null;
		foreach (var music in resourcesManager.lstMusics) {
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

	(GameObject[], GameObject[], GameObject[]) LoadStageEntities(string[] entityId) {
		List<GameObject> lstField = new List<GameObject>();
		List<GameObject> lstTagged = new List<GameObject>();
		List<GameObject> lstGeoLoc = new List<GameObject>();

		GameObject obj;
		FieldEntityInfo info;
		for (int i = 0; i < entityId.Length; i++) {
			if (string.IsNullOrWhiteSpace(entityId[i])) continue;
			obj = null;
			info = null;
			foreach (var go in goManaged) {
				if (go.TryGetComponent(out info) &&
						info.entityId == entityId[i]) {
					obj = go;
					break;
				}
			}
			if (info == null) { continue; }
			switch (info.enumARType) {
				case FieldEntityInfo.EntityToggleType.ARTagTracking:
				case FieldEntityInfo.EntityToggleType.ARTagAround:
				case FieldEntityInfo.EntityToggleType.ARTagPosition:
					lstTagged.Add(obj);
					break;
				case FieldEntityInfo.EntityToggleType.GeoLocAround:
				case FieldEntityInfo.EntityToggleType.GeoLocPosition:
					lstGeoLoc.Add(obj);
					break;
				case FieldEntityInfo.EntityToggleType.StageAround:
				case FieldEntityInfo.EntityToggleType.StagePosition:
				case FieldEntityInfo.EntityToggleType.RamdomAroundCam:
				default:
					lstField.Add(obj);
					break;
			}


		}

		return (lstField.ToArray(), lstTagged.ToArray(), lstGeoLoc.ToArray());
	}

	void PrepareEntities(string[] entityInfos) {
		foreach (var entityInfo in entityInfos) {
			PrepareEntity(entityInfo);
		}
	}
	(GameObject, FieldEntityInfo) PrepareEntity(string rawInfo) {
		string prefabName;
		string tmp = "";
		if (JSONReader.TryPraseString(rawInfo, "model_Id", ref tmp)) {
			prefabName = tmp;
		} else {
			return (null, null);
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
			return (null, null);
		}
		GameObject obj = Instantiate(prefab, goRoot.transform);
		info = obj.GetComponent<FieldEntityInfo>();

		if (JSONReader.TryPraseString(rawInfo, "field_id", ref tmp)) {
			info.entityId = tmp;
			obj.name = tmp;
		}
		int tmpInt = 0;
		double tmpD = 0.0;
		JSONReader infoJson = new JSONReader(rawInfo);
		if (infoJson.TryPraseInt("enumPlacing", ref tmpInt)) {
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
		if (infoJson.TryPraseString("image_Id", ref tmp)) {
			info.uuidImageTracking = tmp;
		}
		if (infoJson.TryPraseDouble("lat", ref tmpD)) {
			info.latitude = tmpD;
		}
		if (infoJson.TryPraseDouble("lon", ref tmpD)) {
			info.longitude = tmpD;
		}
		if (infoJson.TryPraseString("offset", ref tmp)) {
			Vector3 offset = new Vector3(0, 0, 0);
			if (JSONReader.TryPraseDouble(tmp, "x", ref tmpD)) {
				offset.x = (float)tmpD;
			}
			if (JSONReader.TryPraseDouble(tmp, "y", ref tmpD)) {
				offset.y = (float)tmpD;
			}
			if (JSONReader.TryPraseDouble(tmp, "z", ref tmpD)) {
				offset.z = (float)tmpD;
			}
			info.offset = offset;
		} else {
			info.offset = Vector3.zero;
		}
		if (infoJson.TryPraseInt("enumLookDir", ref tmpInt)) {
			switch (tmpInt) {
				case 1:
					info.isLookAt = true;
					break;
				default:
					info.isLookAt = false;
					break;
			}
		} else {
			info.isLookAt = false;
		}
		if (infoJson.TryPraseDouble("proximity", ref tmpD)) {
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
					item.LinkClips(currentStage.voiceLogs);
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

		goManaged.Add(obj);
		return (obj, info);
	}

	public void OnRemoveFieldEntity(GameObject obj) {
		goManaged.Remove(obj);
	}
}

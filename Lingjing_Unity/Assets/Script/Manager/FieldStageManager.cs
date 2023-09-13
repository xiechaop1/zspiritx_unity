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
				//Debug.Log(InputGPSManager.GetDistance(stageInfo.lat, stageInfo.lng, newLatLng.x, newLatLng.y));
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
	int entityCounter=0;

	(GameObject[], GameObject[], GameObject[]) LoadStageEntities(string[] entityPrefabs) {
		List<GameObject> lstField = new List<GameObject>();
		List<GameObject> lstTagged = new List<GameObject>();
		List<GameObject> lstGeoLoc = new List<GameObject>();

		GameObject obj;
		FieldEntityInfo info;
		for (int i = 0; i < entityPrefabs.Length; i++) {
			if (string.IsNullOrWhiteSpace(entityPrefabs[i])) continue;
			obj = null;
			info = null;
			foreach (var go in goManaged) {
				if (go.TryGetComponent(out info) &&
						info.strName == entityPrefabs[i]) {
					obj = go;
				}
			}
			if (obj == null) {
				(obj, info) = PrepareEntity(entityPrefabs[i]);
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
	(GameObject, FieldEntityInfo) PrepareEntity(string prefabName) {
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

		string tmp = "";
		int tmpInt = 0;
		double tmpD = 0.0;
		foreach (string rawInfo in currentStage.lstStageEntityInfos) {
			//Debug.Log(rawInfo);
			if (!JSONReader.TryPraseString(rawInfo, "model_Id", ref tmp)) { continue; }
			//Debug.Log(tmp + "==" + prefabName + "is " + (tmp == prefabName));
			if (tmp == prefabName) {
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
							break;
					}
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
				}
				if (infoJson.TryPraseDouble("proximity", ref tmpD)) {
					info.proximityDialog = (float)tmpD;
				}
				break;
			}
		}


		if (info.enumActionType == EntityActionType.DialogActor) {
			List<string> lstRawDialogs;
			DialogSentence sentence;
			//Debug.Log(info.strHintbox);
			if (JSONReader.TryPraseArray(info.strHintbox, "Dialog", out lstRawDialogs)) {
				List<DialogSentence> lstSentences = new List<DialogSentence>();
				foreach (string rawDialog in lstRawDialogs) {
					//Debug.Log(rawDialog);
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
				//if (JSONReader.TryPraseString(info.strHintbox, "Name", ref tmp)) {
				//	info.nameNPC = tmp;
				//}
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

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
					InputGPSManager.GetDistance(stageInfo.lat, stageInfo.lng, newLatLng.x, newLatLng.y) < stageInfo.proximity) {
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
		//lstFieldEntity = LoadStageEntities(targetStage.lstFieldEntityUUID);
		//lstTaggedEntity = LoadStageEntities(targetStage.lstTaggedEntityUUID);
		//lstLocEntity = LoadStageEntities(targetStage.lstGeolocEntityUUID);
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

	(GameObject[], GameObject[], GameObject[]) LoadStageEntities(string[] entityPrefabs) {
		List<GameObject> lstField = new List<GameObject>();
		List<GameObject> lstTagged = new List<GameObject>();
		List<GameObject> lstGeoLoc = new List<GameObject>();

		GameObject obj;
		FieldEntityInfo info;
		for (int i = 0; i < entityPrefabs.Length; i++) {
			if (!string.IsNullOrWhiteSpace(entityPrefabs[i])) {
				obj = null;
				info = null;
				foreach (var go in goManaged) {
					if (go.TryGetComponent(out info) &&
							info.strName == entityPrefabs[i]) {
						obj = go;
					}
				}
				if (obj == null) {
					foreach (var prefab in resourcesManager.lstPrefabs) {
						if (prefab != null &&
								prefab.TryGetComponent(out info) &&
								info.strName == entityPrefabs[i]) {
							obj = PrepareEntity(prefab);
							//lstField.Add(obj);
							break;

						}
					}
				}
				if (info != null) {
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
			}
		}

		return (lstField.ToArray(), lstTagged.ToArray(), lstGeoLoc.ToArray());
	}
	GameObject PrepareEntity(GameObject prefab) {
		GameObject obj = Instantiate(prefab, goRoot.transform);
		FieldEntityInfo info = obj.GetComponent<FieldEntityInfo>();
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
				string tmp = "";
				if (JSONReader.TryPraseString(info.strHintbox, "Intro", ref tmp)) {
					info.currDialog = DialogSentence.FindSentence(tmp, lstSentences);
				} else {
					info.currDialog = lstSentences[0];
				}
				info.lstDialogs = lstSentences.ToArray();
				if (JSONReader.TryPraseString(info.strHintbox, "Name", ref tmp)) {
					info.nameNPC = tmp;
				}
			}
		}
		if (prefabAnimEmerge != null) {
			GameObject goAnim = Instantiate(prefabAnimEmerge, obj.transform);
			info.animEmerge = goAnim.GetComponentInChildren<ParticleSystem>();
			info.goVisual.SetActive(false);
		}

		goManaged.Add(obj);
		return obj;
	}

	public void OnRemoveFieldEntity(GameObject obj) {
		goManaged.Remove(obj);
	}
}

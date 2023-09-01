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
					InputGPSManager.GetDistance(stageInfo.lat, stageInfo.lng, newLatLng.x, newLatLng.y) < 10.0f) {
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
		lstFieldEntity = LoadStageEntities(targetStage.lstFieldEntityUUID);
		lstTaggedEntity = LoadStageEntities(targetStage.lstTaggedEntityUUID);
		if (!string.IsNullOrWhiteSpace(targetStage.uuidBGM)) {
			LoadMusic(targetStage.uuidBGM);
		}
	}

	void LoadMusic(string uuid) {
		AudioClip clip = null;
		foreach (var music in resourcesManager.lstMusics) {
			//Debug.Log(music.name);
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

	GameObject[] LoadStageEntities(string[] entityPrefabs) {
		List<GameObject> lstOutput = new List<GameObject>();
		GameObject obj;
		FieldEntityInfo info;
		for (int i = 0; i < entityPrefabs.Length; i++) {
			if (!string.IsNullOrWhiteSpace(entityPrefabs[i])) {
				obj = goManaged.Find(x => x.GetComponent<FieldEntityInfo>().strName == entityPrefabs[i]);
				if (obj != null) {
					lstOutput.Add(obj);
				} else {
					foreach (var prefab in resourcesManager.lstPrefabs) {
						if (prefab != null && prefab.TryGetComponent(out info)) {
							if (info.strName == entityPrefabs[i]) {
								obj = PrepareEntity(prefab);
								lstOutput.Add(obj);
								break;
							}
						}
					}
				}
			}
		}

		return lstOutput.ToArray();
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
			}
		}
		if (prefabAnimEmerge!=null) {
			GameObject goAnim = Instantiate(prefabAnimEmerge, obj.transform);
			info.animEmerge = goAnim.GetComponentInChildren<ParticleSystem>();
		}

		goManaged.Add(obj);
		return obj;
	}

	public void OnRemoveFieldEntity(GameObject obj) {
		goManaged.Remove(obj);
	}
}

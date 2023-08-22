using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldStageManager : MonoBehaviour, IManager {
	public PrefabLoadList prefabs;
	public FieldStageInfo currentStage;
	private FieldEntityManager entityManager;
	private List<GameObject> goManaged = new List<GameObject>();
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
		lstFieldEntity = LoadStageEntities(targetStage.lstFieldEntityUUID);
		lstTaggedEntity = LoadStageEntities(targetStage.lstTaggedEntityUUID);
		currentStage = targetStage;
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
					foreach (var prefab in prefabs.lstPrefabs) {
						if (prefab != null && prefab.TryGetComponent(out info)) {
							if (info.strName == entityPrefabs[i]) {
								obj = Instantiate(prefab, goRoot.transform);
								lstOutput.Add(obj);
								goManaged.Add(obj);
								break;
							}
						}
					}
				}
			}
		}

		return lstOutput.ToArray();
	}
	public void OnRemoveFieldEntity(GameObject obj) {
		goManaged.Remove(obj);
	}
}

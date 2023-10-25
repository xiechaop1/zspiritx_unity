using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour, IManager {
	public CombatView combatView;
	public GameObject ARSceneScanHint;
	public GameObject combatRoot;
	public GameObject playerController;
	public List<GameObject> lstPrefabTeamPlayer = new List<GameObject>();
	public List<CombatActorInfo> lstTeamPlayer = new List<CombatActorInfo>();
	public List<CombatActorInfo> lstActiveTeamPlayer = new List<CombatActorInfo>();
	public List<GameObject> lstPrefabTeamMob = new List<GameObject>();
	public List<CombatActorInfo> lstTeamMob = new List<CombatActorInfo>();
	public List<CombatActorInfo> lstActiveTeamMob = new List<CombatActorInfo>();
	Queue<CombatActorInfo> queFieldEntity = new Queue<CombatActorInfo>();
	FieldEntityManager entityManager;
	Transform fieldCenter;
	public Action<string> finishCallback;
	public void Init(UIEventManager eventManager, params IManager[] managers) {
		foreach (var manager in managers) {
			RegisterManager(manager);
		}
		fieldCenter = entityManager.goCamDir.transform;
	}
	public void RegisterManager(IManager manager) {
		if (manager is FieldEntityManager) {
			entityManager = manager as FieldEntityManager;
		}
	}

	private float timeLastCheck = 1f;
	public void Update() {
		if (timeLastCheck < 0) {
			if (queFieldEntity.Count > 0) {
				TryPlaceRamdomEntities(10);
			}
			timeLastCheck = 2f;
		} else {
			timeLastCheck -= Time.deltaTime;
		}
	}
	public void PrepareBattleGround() {
		setEntityVisibility(true);
		entityManager.setEntityVisibility(false);
		GameObject obj;
		CombatActorInfo actor;
		foreach (var prefab in lstPrefabTeamPlayer) {
			if (prefab == playerController) {
				obj = playerController;
				actor = obj.GetComponent<CombatActorInfo>();
			} else {
				obj = Instantiate(prefab, entityManager.goStorage.transform);
				actor = obj.GetComponent<CombatActorInfo>();
				queFieldEntity.Enqueue(actor);
			}
			if (actor != null) {
				actor.combatManager = this;
				actor.OnHitpointZero += UpdateActorEnd;
				lstTeamPlayer.Add(actor);
				lstActiveTeamPlayer.Add(actor);
			}

		}
		foreach (var prefab in lstPrefabTeamMob) {
			obj = Instantiate(prefab, entityManager.goStorage.transform);
			actor = obj.GetComponent<CombatActorInfo>();
			queFieldEntity.Enqueue(actor);
			if (actor != null) {
				actor.combatManager = this;
				actor.OnHitpointZero += UpdateActorEnd;
				lstTeamMob.Add(actor);
				lstActiveTeamMob.Add(actor);
			}
		}
	}
	public void CombatPrepareDone() {
		combatView.SetActive(true);
		combatView.UpdateHint("战斗将在5秒后开始");
		StartCoroutine(AsyncBeginCombat());
	}
	IEnumerator AsyncBeginCombat() {
		yield return null;
		yield return new WaitForSeconds(1f);
		combatView.UpdateHint("战斗将在4秒后开始");
		yield return new WaitForSeconds(1f);
		combatView.UpdateHint("战斗将在3秒后开始");
		yield return new WaitForSeconds(1f);
		combatView.UpdateHint("战斗将在2秒后开始");
		yield return new WaitForSeconds(1f);
		combatView.UpdateHint("战斗即将开始");
		yield return new WaitForSeconds(1f);
		foreach (var actor in lstActiveTeamPlayer) {
			actor.Begin();
		}
		foreach (var actor in lstActiveTeamMob) {
			actor.Begin();
		}
	}
	public void EndBattleGround(int endType) {
		setEntityVisibility(false);
		combatView.SetActive(false);
		entityManager.setEntityVisibility(true);
		JSONWriter output = new JSONWriter();
		output.Add("AnswerType", endType);
		finishCallback?.Invoke(output.ToString());
	}
	public void setEntityVisibility(bool value) {
		combatRoot.SetActive(value);
	}
	public void TryPlaceRamdomEntities(int maxTries) {
		CombatActorInfo actor;
		for (int i = 0; i < maxTries; i++) {
			if (queFieldEntity.Count <= 0) {
				break;
			}

			actor = queFieldEntity.Dequeue();
			if (TryPlaceEntityAround(actor, 1f)) {
				//lstPlacedEntity.Add(actor);
			} else {
				queFieldEntity.Enqueue(actor);
			}
		}

		if (queFieldEntity.Count <= 0) {
			ARSceneScanHint.SetActive(false);
			CombatPrepareDone();
		} else if (!ARSceneScanHint.activeInHierarchy) {
			ARSceneScanHint.SetActive(true);
		}
	}
	public bool TryPlaceEntityAround(CombatActorInfo actor, float maxTolerance) {
		for (int i = 0; i < 50; i++) {
			Ray ray = new Ray(fieldCenter.position + fieldCenter.forward * 2f + new Vector3(UnityEngine.Random.Range(-maxTolerance, maxTolerance), 1f, UnityEngine.Random.Range(-maxTolerance, maxTolerance)), Vector3.down);
			if (Physics.Raycast(ray, out RaycastHit hit, 4f, 8) && TryPlacing(actor, hit)) {
				return true;
			}
		}
		return false;
	}

	public bool TryPlacing(CombatActorInfo actor, RaycastHit hit) {
		if (!hit.collider.gameObject.TryGetComponent(out ARPlaneInfo arPlane) || !arPlane.isHorizontal) {
			return false;
		}
		Vector3 posTarget = hit.point;
		var posOld = actor.transform.position;
		actor.transform.position = posTarget;
		actor.transform.localScale = Vector3.one;// * scale;

		Ray ray = new Ray(posTarget + (actor.transform.up * 0.01f), Vector3.down);
		if (Physics.Raycast(ray, out hit, 1f)) {
			//Debug.Log(actor.gameObject.name);
			actor.transform.parent = combatRoot.transform;
			Vector3 LookDir = entityManager.goCamDir.transform.position - posTarget;
			LookDir.y = 0;
			if (LookDir.sqrMagnitude > 0.01f) {
				actor.transform.rotation = Quaternion.LookRotation(LookDir.normalized);
			}

			return true;

		}
		actor.transform.position = posOld;
		return false;
	}

	public void UpdateActorEnd(CombatActorInfo actor) {
		if (lstActiveTeamPlayer.Remove(actor)) {
			if (lstActiveTeamPlayer.Count <= 0) {
				EndBattleGround(2);
			}
		} else if (lstActiveTeamMob.Remove(actor)) {
			if (lstActiveTeamMob.Count <= 0) {
				EndBattleGround(1);
			}
		}
	}
}

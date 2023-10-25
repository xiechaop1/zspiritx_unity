using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimBot : MonoBehaviour {
	public CombatActorInfo combatActor;
	[HideInInspector]
	public CombatActorInfo enemy;
	[HideInInspector]
	public GameObject muzzle;
	bool isActive;
	private void Awake() {
		combatActor.OnBegin += Init;
		combatActor.OnHalt += StopAI;
		muzzle = combatActor.launcher.muzzle;
	}
	private void Update() {
		if (!isActive) {
			return;
		}
		if (combatActor.launcher.currCooldown<=0) {
			Aim();
			combatActor.FireProjectile();
		}
	}
	void Init(CombatActorInfo actorInfo) {
		enemy = actorInfo.combatManager.lstActiveTeamPlayer[0];
		isActive = true;
	}
	void StopAI(CombatActorInfo actorInfo) {
		isActive = false;
	}
	void Aim(){
		muzzle.transform.LookAt(enemy.transform);
		muzzle.transform.Rotate(Random.Range(-5f, 5f), Random.Range(0f, 10f),0f);
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CombatActorInfo : MonoBehaviour {
	public CombatManager combatManager;
	public string uuid;
	public ProjectileLauncher launcher;
	public float maxHitpoint = 6f;
	[HideInInspector]
	public float currHitpoint = 0f;
	public Action<CombatActorInfo> OnInit;
	public Action<CombatActorInfo> OnBegin;
	public Action<CombatActorInfo> OnHitpointUpdated;
	public Action<CombatActorInfo> OnHitpointZero;
	public Action<CombatActorInfo> OnHalt;
	public void Init(){ 
		OnInit?.Invoke(this);
	}
	public void Begin() {
		currHitpoint = maxHitpoint;
		OnBegin?.Invoke(this);
	}
	public void FireProjectile() {
		launcher.FireProjectile(this);
	}
	public void OnHit(float hit) {
		currHitpoint -= hit;
		OnHitpointUpdated?.Invoke(this);
		if (currHitpoint <= 0) {
			OnHitpointZero?.Invoke(this);
			Halt();
		}
	}
	public void Halt(){
		OnHalt?.Invoke(this);
	}
}

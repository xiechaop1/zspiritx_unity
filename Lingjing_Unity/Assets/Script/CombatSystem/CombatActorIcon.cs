using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatActorIcon : MonoBehaviour {
	public CombatActorInfo combatActor;
	public Text healthInfo;
	private void Awake() {
		healthInfo.text = combatActor.uuid;
		combatActor.OnBegin += UpdateHitpoint;
		combatActor.OnHitpointUpdated += UpdateHitpoint;
	}
	void UpdateHitpoint(CombatActorInfo actor) {
		if (actor == combatActor) {
			healthInfo.text = "HP:" + combatActor.currHitpoint + "/" + combatActor.maxHitpoint;
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatView : MonoBehaviour {
	public CombatActorInfo combatActor;
	public InteractionView interactionView;
	public Text healthInfo;
	public Button btnFire;
	public Image cdVisual;
	bool isBattle = false;
	private void Awake() {
		btnFire.interactable = false;
		combatActor.OnBegin += InitView;
		combatActor.OnHitpointUpdated += UpdateHitpoint;
	}
	private void Update() {
		if (isBattle) {
			float cd = combatActor.launcher.currCooldown;
			if (cd > 0) {
				float maxCD = combatActor.launcher.initialCooldown;
				cdVisual.fillAmount = 1f - (cd / maxCD);
			} else {
				cdVisual.fillAmount = 1f;
				btnFire.interactable = true;
			}
		}
	}

	public void Fire() {
		combatActor.FireProjectile();
		btnFire.interactable = false;
		cdVisual.fillAmount = 0f;
	}
	public void UpdateHint(string msg) {
		healthInfo.text = msg;
	}
	void InitView(CombatActorInfo actor) {
		isBattle = true;
		btnFire.interactable = true;
		UpdateHitpoint(actor);
	}
	void UpdateHitpoint(CombatActorInfo actor) {
		if (actor == combatActor) {
			healthInfo.text = "HP:" + combatActor.currHitpoint + "/" + combatActor.maxHitpoint;
		}
	}
	public void SetActive(bool value) {
		gameObject.SetActive(value);
		interactionView.SetNPCLogActive(!value);
		if (value) {

		} else {
			btnFire.interactable = false;
		}
	}
}

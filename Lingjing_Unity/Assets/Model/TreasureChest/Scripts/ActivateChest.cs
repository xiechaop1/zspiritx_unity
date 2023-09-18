using UnityEngine;
using System.Collections;

public class ActivateChest : InteractionAnimation {

	public Transform lid, lidOpen, lidClose;    // Lid, Lid open rotation, Lid close rotation
	public float openSpeed = 5F;                // Opening speed
	public bool canClose;                       // Can the chest be closed

	[HideInInspector]
	public bool _open;                          // Is the chest opened

	void Update() {
		if (_open) {
			ChestClicked(lidOpen.rotation);
		} else {
			ChestClicked(lidClose.rotation);
		}
	}

	// Rotate the lid to the requested rotation
	void ChestClicked(Quaternion toRot) {
		if (lid.rotation != toRot) {
			lid.rotation = Quaternion.Lerp(lid.rotation, toRot, Time.deltaTime * openSpeed);
		}
	}

	public void ToggleChestOpen() {
		if (canClose) _open = !_open; else _open = true;
	}
	public override void BeginInteraction() {
		_open = true;
	}
	public override void EndInteraction(){ 
		if (canClose) _open = false;
	}
}

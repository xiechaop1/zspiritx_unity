using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARInteractionInfo : MonoBehaviour {
	public string info = "";
	public ARInteractListener listener;
	public void SendInteraction() {
		listener?.HasARInteract(info);
	}
}

public abstract class ARInteractListener:MonoBehaviour {
	public abstract void SetActionManager(EntityActionManager manager);
	public abstract void HasARInteract(string info);
}
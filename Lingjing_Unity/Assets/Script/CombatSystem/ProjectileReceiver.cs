using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Collider))]
public class ProjectileReceiver : MonoBehaviour {
	public float penetrationReduction = 1f;
	public float receiverFactor = 1f;
	public CombatActorInfo actor;
	public void OnHit(ProjectileObject projectile){
		actor.OnHit(projectile.currHit*receiverFactor);
	}
}

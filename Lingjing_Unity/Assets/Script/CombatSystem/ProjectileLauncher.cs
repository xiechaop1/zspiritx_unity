using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLauncher : MonoBehaviour {
	public GameObject muzzle;
	public GameObject[] lstPrefabs;
	int counter = 0;
	public float initialCooldown = 1f;
	[HideInInspector]
	public float currCooldown = 0f;
	private void Update() {
		if (currCooldown > 0) {
			currCooldown -= Time.deltaTime;
		}
	}
	public void FireProjectile(CombatActorInfo actor) {
		if (currCooldown <= 0) {
			GameObject prefabProjectile;
			counter++;
			if (counter >= lstPrefabs.Length) {
				counter = 0;
			}
			prefabProjectile = lstPrefabs[counter];
			if (prefabProjectile != null) {
				if (prefabProjectile.GetComponent<ProjectileObject>() != null) {
					GameObject projectile = Instantiate(prefabProjectile, muzzle.transform.position, muzzle.transform.rotation);
					ProjectileObject info = projectile.GetComponent<ProjectileObject>();
					info.Init(muzzle.transform.position, muzzle.transform.forward, actor);
				}
			}
			currCooldown = initialCooldown;
		}
	}
}

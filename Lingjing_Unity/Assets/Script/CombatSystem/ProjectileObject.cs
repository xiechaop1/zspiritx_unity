using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ProjectileObject : MonoBehaviour {
	public CombatActorInfo projector;
	public bool isFriendlyFire = false;
	public float baseHit = 1f;
	public float currHit = 1f;
	public float speed = 1f;
	public float gravity = 9.8f;
	public float initialPenetration = 1f;
	float currPentration = 1f;
	public float maxLifetime = 2f;
	float currLifetime = 0f;
	Vector3 velocity;
	public void Init(Vector3 pos, Vector3 dir, CombatActorInfo projector, float hitFactor = 1f) {
		this.projector = projector;
		gameObject.transform.position = pos;
		gameObject.transform.forward = dir;
		velocity = dir * speed;
		currHit = baseHit * hitFactor;
		currPentration = initialPenetration;
		currLifetime = 0f;
	}

	private void Update() {
		transform.position += velocity * Time.deltaTime;
		velocity += Vector3.down * gravity * Time.deltaTime;
		currLifetime += Time.deltaTime;
		if (currLifetime > maxLifetime) {
			Destroy(gameObject);
		}
	}
	private void OnTriggerEnter(Collider other) {
		if (other.TryGetComponent(out ProjectileReceiver receiver)) {
			if (receiver.actor != projector) {
				receiver.OnHit(this);
				currPentration -= receiver.penetrationReduction;
				if (currPentration <= 0f) {
					Destroy(gameObject);
				}
			}
		}
	}
}

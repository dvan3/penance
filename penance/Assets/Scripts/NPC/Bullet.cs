using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {
	private static float lifeTime = 0.01f;
	
	private Vector3 shootAt;
	private float accel;
	private float startTime;
	
	public void fire(Vector3 shootAt, float accel) {
		rigidbody.freezeRotation = true;
		rigidbody.useGravity = false;
		
		this.shootAt = shootAt;
		this.accel = accel;
		startTime = Time.time;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (Time.time - startTime >= lifeTime) {
			print ("destroy");
			Destroy(this);
		}

		rigidbody.AddExplosionForce(-accel, shootAt, 10.0f);
		
		if (transform.parent && transform.localPosition.y >= 0.001f) {
			transform.parent = null;
		}
	}
	
	void OnCollisionEnter() {
		print("collide");
	}
}

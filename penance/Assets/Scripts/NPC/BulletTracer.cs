using UnityEngine;
using System.Collections;

public class BulletTracer : MonoBehaviour {	
	public Object prefab;

	Transform dummyTracer;
	float shotForce;
	
	void Awake() {
		dummyTracer = transform.FindChild("agent_bones").FindChild("handcontrol_right").FindChild("gun").FindChild("dummy_tracer").transform;
		shotForce = 10.0f;
	}
	
	public void Shoot(Vector3 angle) {
		// Create instance
		GameObject bullet = (GameObject)Instantiate(prefab, dummyTracer.position, dummyTracer.rotation);
		bullet.transform.parent = dummyTracer.transform.parent;
		Vector3 shootAt = PlayerAttributes.Instance.transform.FindChild("PlayerModel").FindChild("female_bones").FindChild("centerbone").transform.position;		
		((Bullet)bullet.GetComponent(typeof(Bullet))).fire(shootAt, shotForce);

		

		
		
		// Accelerate towards player (+/- miss)
//		bullet.rigidbody.constantForce.force = gravity;
//		bullet.rigidbody.AddExplosionForce(shotForce, muzzle.position, shotRadius);
		
		// Fade to transparency
/*		float speed = bullet.rigidbody.velocity.magnitude;
		Material m = ((MeshRenderer)bullet.GetComponent(typeof(MeshRenderer))).material;
		Color c = m.color;
		if (speed < 1.0f) {
			c.a = speed;
			m.color = c;
		}*/
		
		// Destroy object
	}
}

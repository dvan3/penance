// TODO: (high priority) prevent penance from getting too close to solid objects
// TODO: (med priority) Figure out if we need to implement something special for Y-axis movement (need to test with camera angle + NPC)
// TODO: (low priority) implement penance spin (spin faster when linear velocity is greater)

using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
[RequireComponent (typeof (MeshCollider))]
public class PenanceMotion: MonoBehaviour {
	public static int NPCLayer = 10;
	public static int PenanceLayer = 9;
	public static int rangedDmg = 2;
	public static int meleeDmg = 1;
	
	public static PenanceMotion Instance;
	
	public string throwKey = "mouse 1";
	public string gripKey = "mouse 0";
	public string slashRight = "kris_slash1";
	public string slashLeft = "kris_slash2";
	
	public float flyAccel;
	public float throwAccel;
	public float maxAccel; // mostly makes sure the penance doesn't go crazy from really high forces
	public float recallAccel;
	public float regDrag;
	public float catchRadius;	
	public float maxDist; // how far the penance can go from the player
	public bool held; // should be true iff player state == 0
	public bool recall;
	public bool gripped;

	private GameObject playerObject;
	private Transform handTarget;
	
	void Awake () {
		// Alias references
		Instance = this;
		playerObject = transform.parent.gameObject;
		handTarget = playerObject.transform.FindChild("PlayerModel").FindChild("female_bones").FindChild("handcontrol_right").FindChild("penanced_hand").transform;
		
		// Default attributes
		flyAccel = 75.0f;
		throwAccel = 40.0f;
		maxAccel = 1000.0f;
		recallAccel = 2000.0f;
		regDrag = 1.0f;
		catchRadius = 0.3f;
		maxDist = 10.0f;
		held = true;
		recall = false;
		gripped = false;
		
		// Align penance with handTarget
		snapToHand();
		
		// Make sure the only physics affecting the penance comes from this script
	    rigidbody.freezeRotation = true;
	    rigidbody.useGravity = false;
	}
	
	// We'll use this function to keep up with user input, since it may occur between fixed updates
	void Update() {
		if (held) {
			if (Input.GetKeyDown(throwKey)) {
				throwIt ();
			} else if (Input.GetKey(gripKey)) { // using GetButton() allows us to check if it's still being held down from recall
				gripped = true;
			} else if (Input.GetKeyUp(gripKey)) {
				gripped = false;
			}
		} else {
			if (Input.GetKeyDown(gripKey)) {
				recall = true;
				rigidbody.drag = 10.0f; // this helps avoid circling the hand when trying to recall
			} else if (Input.GetKeyUp (gripKey)) {
				recall = false;
				rigidbody.drag = regDrag;
			}
		}
	}
	
	void OnCollisionEnter(Collision c) {
		print ("collide");
		
		if (c.gameObject.layer == NPCLayer) {
			EnemyAttributes a = (EnemyAttributes)c.gameObject.GetComponent(typeof(EnemyAttributes));
			a.Hit();
		}
	}
	
	private void slashAttack() {
		float attackX = Input.GetAxis("PenanceX");
		float attackZ = Input.GetAxis("PenanceZ");
		float swipeSpeed = Mathf.Sqrt(Mathf.Pow(attackX, 2) + Mathf.Pow(attackZ, 2));
		
		if (attackX > 0) {
			ThirdPersonAnimator.Instance.PlayAnimation(slashRight, swipeSpeed / 2);
		} else if (attackX < 0) {
			ThirdPersonAnimator.Instance.PlayAnimation(slashLeft, swipeSpeed / 2);
		}
	}
	
	void FixedUpdate() {
		if (gripped) {
			slashAttack();
			snapToHand();
		} else if (held) {
			// Do this every update to match penance position with animated hand
			snapToHand();
		} else if (recall) {
			float dist = Vector3.Distance(transform.position, handTarget.position);

			if (dist <= catchRadius) {
				catchIt();
			} else {
				recallIt(dist);
			}
		} else {
			moveIt();
		}
	}
	
	private void snapToHand() {
		transform.position = handTarget.position;
	}
	
	private void throwIt() {
		rigidbody.isKinematic = false;
		rigidbody.AddForce(Vector3.forward * throwAccel, ForceMode.Acceleration);
		transform.parent = null;
		held = false;
		gripped = false;
	}
	
	private void catchIt() {
		held = true;
		recall = false;
		transform.parent = playerObject.transform;
		rigidbody.drag = regDrag;
		rigidbody.isKinematic = true;
		snapToHand();
	}
	
	private void recallIt(float dist) {
		// Define a unit vector z_ that points towards my hand
		Vector3 z_ = Vector3.Normalize((handTarget.position - transform.position));

		// Slow down when we get close so we don't overshoot the hand (increase maxAccel otherwise to account for extra drag when recalling)
		float acl = Mathf.Pow(Vector3.Magnitude(rigidbody.velocity), 2) / dist; // instantaenous acceleration
		float accelCap = (dist > 2) ? 3.0f * maxAccel : Mathf.Clamp(maxAccel - acl, -maxAccel * 0.01f, maxAccel);
				
		// Apply a greater force as the penance approaches
		rigidbody.AddForce(Vector3.ClampMagnitude(z_ * recallAccel / dist, accelCap), ForceMode.Acceleration);
	}
	
	private void moveIt() {
		// Find mouse in world space
		Vector3 mouseToWorld = Vector3.zero;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit)) {
			mouseToWorld = hit.point;
		}
		
		// Define a unit vector target_ that points towards the mouse
		Vector3 target_ = Vector3.Normalize(mouseToWorld - transform.position);
				
		// Predict my position at next fixedUpdate()
		Vector3 a = Vector3.ClampMagnitude(target_ * flyAccel, maxAccel);
		float t = Time.fixedDeltaTime;
		Vector3 xi = transform.position;
		Vector3 v = rigidbody.velocity;
		Vector3 xf = xi + v * t + a * Mathf.Pow(t, 2) / 2;

		// Try to stay within maxDist of handTarget
		float distance = Vector3.Distance(handTarget.position, xf);
		float modifier = 1 / Mathf.Pow(distance, 0.25f); // keeps the actual maxDist closer to what we want it to be (helps account for momentum)
		if (distance > (maxDist * modifier)) {
			xf = target_ * maxDist * modifier + handTarget.position;
			
			float amag = a.magnitude;
			a = Vector3.ClampMagnitude((xf - xi - v * t) * 2 / Mathf.Pow (t, 2), amag);
		}
				
		// Accelerate towards mouse
		rigidbody.AddForce(a, ForceMode.Acceleration);
	}
}
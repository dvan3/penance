using UnityEngine;
using System.Collections;

using Pathfinding;
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent (typeof (BulletTracer))]
public class DemonTargeting : MonoBehaviour
{
	public static int DemonDmg = 12;
	
	/*AI Targeting Fields*/  
	public float distance;
	public Transform target;
	public float attackRange = 100f;
	public float enemyAttackRange = 1f;
	public float moveSpeed = 20f;
	public float damping = 6f;
	private bool isItAttacking = false;
	
	/*Patrolling AI Fields*/
	public Transform[] waypoint;
	public float patrolSpeed = 3f;
	public bool loop = true;
	public Transform player;
	public float dampingLook = 2f;
	public float pauseDuration = 1f;
	
	/*Private Patrolling AI Fields*/
	private float curTime;
	private int currentWaypoint = 0;
	private CharacterController character;
	
	/*Attacking Fields*/
	public GameObject attackTarget;
	public float attackTimer;
	public float coolDown;
	
	/*AI Pathfinding Fields*/
	private Seeker seeker;
    private CharacterController controller;
    public Path path;
    public float nextWaypointDistance = .1f;
    private int currentAIWaypoint = 0;
    public float repathRate = 1f;
    private float lastRepath = -9999;

	void Start ()
	{
		seeker = GetComponent<Seeker>();
		
		/*get the AI character*/
		character = GetComponent<CharacterController> ();
		
		//Makes the enemy target Player
		GameObject go = GameObject.Find ("PlayerModel") as GameObject;
		
		//find the transform of the player
		target = go.transform;
		
		attackTimer = 0;
		coolDown = 4.0f;
		attackTarget = GameObject.Find ("PlayerObj") as GameObject;
	}
	
	public void OnPathComplete (Path p) {
        p.Claim (this);
        if (!p.error) {
            if (path != null) path.Release (this);
            path = p;
            //Reset the waypoint counter
            currentAIWaypoint = 0;
        } else {
            p.Release (this);
            //Debug.Log ("Oh noes, the target was not reachable: "+p.errorLog);
        }
        
        //seeker.StartPath (transform.position,targetPosition, OnPathComplete);
    }
	
	void Update ()
	{
		//find the distance of the enemy to the player
		distance = Vector3.Distance (target.position, transform.position);
		
		/*player doesn't have enemy's attention*/
		if (AlterDemonAppearance.Instance.morphStage == 0)
		{
			/*Enemy has more waypoints to go to*/
			if (currentWaypoint < waypoint.Length)
					/*Patrol...duh*/
				patrol ();
			else {
				/*basically keep repeating the patrol*/
				if (loop)
					currentWaypoint = 0;
			}
			/*color code for "Does not have attention"*/
			renderer.material.color = Color.green; 
		}

		/*player has attention and is in range to be attacked*/
		if (AlterDemonAppearance.Instance.morphStage == 2 && distance < attackRange) 
		{
			/*rush the player*/
			rush ();
			//attack cooldown
			if (attackTimer > 0) {
				attackTimer -= Time.deltaTime;
			}
				
			if (attackTimer < 0) {
				attackTimer = 0;
			}
				
			if (attackTimer == 0) {
				Attack ();
				attackTimer = coolDown;
			}
		}
		
		/*Enemy is attacking*/
		if (isItAttacking) {
			/*color code for "Attacking Player"*/
			renderer.material.color = Color.red;
		}
		
	}
	 
	void rush ()
	{
		isItAttacking = true;
		renderer.material.color = Color.red;
		if (Time.time - lastRepath > repathRate && seeker.IsDone()) {
            lastRepath = Time.time+ Random.value*repathRate*0.5f;
            seeker.StartPath (transform.position, target.position, OnPathComplete);
        }
        
        if (path == null) {
            //We have no path to move after yet
            return;
        }
        
        if (currentAIWaypoint > path.vectorPath.Count) return; 
        if (currentAIWaypoint == path.vectorPath.Count) {
            //Debug.Log ("End Of Path Reached");
            currentAIWaypoint++;
            return;
        }
		
		var rotation = Quaternion.LookRotation (target.position - transform.position);
		transform.rotation = Quaternion.Slerp (transform.rotation, rotation, Time.deltaTime * damping);
		if (Vector3.Distance (target.position, transform.position) > enemyAttackRange) 
		{
			animation["demon_teleport"].speed = 2;
			animation.CrossFade("demon_teleport");
			Vector3 dir = (path.vectorPath[currentAIWaypoint]-transform.position).normalized;
        	dir *= moveSpeed;// * Time.deltaTime;
        	//transform.Translate (dir);
        	character.SimpleMove (dir);
		}
        
        //if (Vector3.Distance (transform.position,path.vectorPath[currentWaypoint]) < nextWaypointDistance) {
        if ( (transform.position-path.vectorPath[currentAIWaypoint]).sqrMagnitude < nextWaypointDistance*nextWaypointDistance) {
            currentAIWaypoint++;
            return;
        }
	}
	
	private void Attack ()
	{
		/*Get direction of enemy*/
		Vector3 dir = (target.transform.position - transform.position).normalized;
		float direction = Vector3.Dot (dir, transform.forward);
		
		/*if enemy is within attack range*/
		if (Vector3.Distance (target.position, transform.position) <= enemyAttackRange) {
			/*if enemy is facing player*/
			if (direction > 0) {
				PlayerAttributes eh = (PlayerAttributes)attackTarget.GetComponent ("PlayerAttributes");
				float random = Random.Range(1, 10);
				random *= .1f;
				if(random > .4)
				{
					Debug.Log("DEMON HIT!");
					/*Attack the player*/
					eh.AdjustCurrentHealth (-DemonTargeting.DemonDmg);
					PlayerAttributes.Instance.hit = true;
					animation.CrossFade("demon_phasewalk");
					if(PlayerAttributes.Instance.alive == false)
					{
						animation.Stop();	
					}
					eh.AdjustCurrentHealth (-DemonTargeting.DemonDmg);
					animation.CrossFade("Demon_phasewalk");
				}
				else
				{
					Debug.Log("You missed");
//					eh.AdjustCurrentHealth (0);
					animation.CrossFade("demon_phasewalk");
					if(PlayerAttributes.Instance.alive == false)
					{
						animation.Stop();	
					}
//					eh.AdjustCurrentHealth (0);
					animation.CrossFade("Demon_phasewalk");
				}
			}
		}
	}
	
	void patrol ()
	{
		Vector3 target = waypoint [currentWaypoint].position;
		//target.y = transform.position.y;
		Vector3 moveDirection = target - transform.position;
		
		/* If this number is 1 the character will jerk the 
		 * last bit to the waypoint and not be over it*/
		if (moveDirection.magnitude < 0.5f) {
			if (curTime == 0) {
				/*pause over the waypoint*/
				curTime = Time.time;
				animation.CrossFade("shade_idle");
			}
			/*reset pause time to 0 and move to next waypoint*/
			if ((Time.time - curTime) >= pauseDuration) {
				currentWaypoint++;
				curTime = 0;
			}
		} else {
			if (Time.time - lastRepath > repathRate && seeker.IsDone()) {
            lastRepath = Time.time+ Random.value*repathRate*0.5f;
            seeker.StartPath (transform.position, target, OnPathComplete);
	        }
	        
	        if (path == null) {
	            //We have no path to move after yet
	            return;
	        }
	        
	        if (currentAIWaypoint > path.vectorPath.Count) return; 
	        if (currentAIWaypoint == path.vectorPath.Count) {
	            //Debug.Log ("End Of Path Reached");
	            currentAIWaypoint++;
	            return;
	        }
			
			var rotation = Quaternion.LookRotation (target - transform.position);
			transform.rotation = Quaternion.Slerp (transform.rotation, rotation, Time.deltaTime * damping);
	
			Vector3 dir = (path.vectorPath[currentAIWaypoint]-transform.position).normalized;
	        dir *= patrolSpeed;// * Time.deltaTime;
	        character.SimpleMove (dir);
			animation.CrossFade("shade_movealong");
	        
	        //if (Vector3.Distance (transform.position,path.vectorPath[currentWaypoint]) < nextWaypointDistance) {
	        if ( (transform.position-path.vectorPath[currentAIWaypoint]).sqrMagnitude < nextWaypointDistance*nextWaypointDistance) {
	            currentAIWaypoint++;
	            return;
	        }
		}
	}
}

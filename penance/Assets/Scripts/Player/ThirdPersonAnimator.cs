using UnityEngine;
using System.Collections;

public class ThirdPersonAnimator : MonoBehaviour
{
	public enum Direction
	{
		Stationary, Forward, Backwards, Left, Right,
		LeftForward, RightForward, LeftBackwards, RightBackwards
	}
	
	public static ThirdPersonAnimator Instance;

	public Direction MoveDirection{ get; set; }
	
	void Awake()
	{
		Instance = this;	
	}
	
	void Update()
	{
		var deadZone = 0.1f;
		
		if(Input.GetAxis("PlayerZ") > deadZone || Input.GetAxis("PlayerZ") < -deadZone
			|| Input.GetAxis("PlayerX") > deadZone || Input.GetAxis("PlayerX") < -deadZone)
		{
			animation.CrossFade("kris_sprint");	
			if(PlayerAttributes.Instance.alive == false)
			{
				animation.Stop("kris_sprint");	
			}
		}
		else
		{
			/*PUT IDLE HERE!*/
			animation.CrossFade("kris_idle");
		}
	}
	
	public void PlayAnimation(string animationID, float speed) {
		animation[animationID].speed = speed;		
		animation.Play(animationID);
	}
	
	public void DetermineCurrentMoveDirection()
	{
		var forward = false;
		var backward = false;
		var left = false;
		var right = false;
		
		/*Moving forward*/
		if(ThirdPersonController.Instance.MoveVector.z > 0)
			forward = true;
		/*Moving backwards*/
		if(ThirdPersonController.Instance.MoveVector.z < 0)
			backward = true;
		/*Moving right*/
		if(ThirdPersonController.Instance.MoveVector.x > 0)
			right = true;
		/*Moving left*/
		if(ThirdPersonController.Instance.MoveVector.x < 0)
			left = true;
		
		/*Moving forward*/
		if(forward)
		{
			if(left)
				/*moving left forward*/
				MoveDirection = Direction.LeftForward;
			else if(right)
				/*moving right forward*/
				MoveDirection = Direction.RightForward;
			else
				/*moving forward*/
				MoveDirection = Direction.Forward;	
		}
		else if(backward)
		{
			if(left)
				/*moving left backwards*/
				MoveDirection = Direction.LeftBackwards;
			else if(right)
				/*moving right backwards*/
				MoveDirection = Direction.RightBackwards;
			else
				/*moving backwards*/
				MoveDirection = Direction.Backwards;	
		}
		else if(left)
		{
			/*Moving left*/
			MoveDirection = Direction.Left;
		}
		else if(right)
		{
			/*Moving right*/
			MoveDirection = Direction.Right;
		}
		else
		{
			/*not moving*/
			MoveDirection = Direction.Stationary;	
		}
	}
}

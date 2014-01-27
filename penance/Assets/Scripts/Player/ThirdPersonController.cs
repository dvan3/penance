/*Handles the player controls
 * 
 */

using UnityEngine;
using System.Collections;

[RequireComponent (typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
	public static ThirdPersonController Instance;
	
	/*Fields*/ 
	public float speedSmoothing = 10f;
	public float rotateSpeed = 1f;
	public float moveSpeed = 0f;
	public float jumpSpeed = 20f;
	public float gravity = 20f;
	public float terminalVelocity = 20f;
	public float wetpoopiness = 5f;
	
	
	/*Properties*/
	public Vector3 MoveVector = Vector3.zero;
	public float VerticalVelocity = 0f;
	public static CharacterController CharacterController;
	public CharacterController controller;
	
	void Awake()
	{
		CharacterController = GetComponent("CharacterController") as CharacterController;
		Instance = this;
		MoveVector = transform.TransformDirection(Vector3.forward);
		
		/*Set Main Camera to look at Player*/
		ThirdPersonCamera.UseExistingOrCreateNewMainCamera();
	}
	
	void GetLocomotionInput()
	{
		if(PlayerAttributes.Instance.alive == true)
		{
			Transform camera = Camera.mainCamera.transform;
			Vector3 forward = camera.TransformDirection(Vector3.forward);
			forward.y = 0;
			forward = forward.normalized;
			
			Vector3 right = new Vector3(forward.z, 0, -forward.x);
			
			float v = Input.GetAxisRaw("PlayerZ");
			float h = Input.GetAxisRaw("PlayerX");
			
			Vector3 targetDirection = h * right + v * forward;
		
			if(targetDirection != Vector3.zero)
			{
				MoveVector = targetDirection.normalized;
				MoveVector = Vector3.RotateTowards(MoveVector, targetDirection, rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);
				MoveVector = MoveVector.normalized;
			}
			
			float curSmooth = speedSmoothing * Time.deltaTime;
			
			float targetSpeed = Mathf.Min(targetDirection.magnitude, 1.0f) * 10f;
			
			moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, curSmooth);	
		}
	}
	
	void Update()
	{
		/*no main camera, don't do anything*/
		if(Camera.mainCamera == null)
			return;
		
		GetLocomotionInput();
		SnapAlignCharacterWithCamera();
		
		/*Multiply MoveVector by MoveSpeed*/
		Vector3 movement = MoveVector * moveSpeed + new Vector3(0, VerticalVelocity, 0);
		//movement *= Time.deltaTime;
		controller = GetComponent<CharacterController>();
		
		//ApplyGravity();
		
		controller.Move(movement * Time.deltaTime);
	
		//MoveVector.y = -0.05193463f;
		transform.rotation = Quaternion.LookRotation(MoveVector);

	}
	
	/*
	void ApplyGravity()
	{
		if(MoveVector.y > -terminalVelocity)
			MoveVector = new Vector3(MoveVector.x, MoveVector.y - gravity, MoveVector.z);
		
		if(ThirdPersonController.CharacterController.isGrounded && MoveVector.y < -1)
			MoveVector = new Vector3(MoveVector.x, -1, MoveVector.z);
	}*/
	
	//make our character snap to where the camera is looking
	void SnapAlignCharacterWithCamera()
	{
		//if our character is moving in x or z snap our character to camera
		if(MoveVector.x != 0 || MoveVector.z != 0)
		{
			transform.rotation = Quaternion.Euler(transform.eulerAngles.x,
												  Camera.mainCamera.transform.eulerAngles.y,
												  transform.eulerAngles.z);	
		}
	}
	
	Vector3 GetDirection()
	{
		return MoveVector;	
	}
	
	float GetSpeed()
	{
		return moveSpeed;	
	}
}

using UnityEngine;
using System.Collections;

public class ThirdPersonCamera : MonoBehaviour
{
	public static ThirdPersonCamera Instance;
	public Transform TargetLookAt;
	
	/*Fields*/
	public float distance = 20f;
	public float distanceMin = 3f;
	public float distanceMax = 10f;
	public float distanceSmooth = 0.05f;
	public float distanceResumeSmooth = 1f;
	public float x_MouseSensitivity = 2f;
	public float y_MouseSensitivity = 2f;
	public float mouseWheelSensitivity = 5f;
	public float x_Smooth = 0.05f;
	public float y_Smooth = 0.1f;
	public float y_MinLimit = -40f;
	public float y_MaxLimit = 80f;
	public float OcclusionDistanceStep = 0.5f;
	public int MaxOcclusionChecks = 10;
	private float mouseX = 0f;
	private float mouseY = 0f;
	private float velX = 0f;
	private float velY = 0f;
	private float velZ = 0f;
	private float velDistance = 0f;
	private float startDistance = 0f;
	private Vector3 position = Vector3.zero;
	private Vector3 desiredPosition = Vector3.zero;
	private float desiredDistance = 0f;
	private float distanceSmooth2 = 0f;
	private float preOccludedDistance = 0f;
	
	
	public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;
	public float minimumX = -360F;
    public float maximumX = 360F;
    public float minimumY = -60F;
    public float maximumY = 60F;
    float rotationY = 0F;
	
	/*Penance Camera Check shit*/
	GameObject tempCamera2;
	GameObject targetLookAt2;
	ThirdPersonCamera myCamera;
	
	void Awake ()
	{
		Instance = this;	
	}

	void Start ()
	{
		/*Validate our distance, insure that it is between our min and max*/
		distance = Mathf.Clamp (distance, distanceMin, distanceMax);
		
		//Screen.lockCursor = true;
		
		startDistance = distance;
		Reset ();
	}

	void LateUpdate ()
	{
		/*no target look at, do nothing get out*/
		if (TargetLookAt == null)
			return;	
		
		CameraCheck();
		
		HandlePlayerInput ();
		
		var count = 0;
		do {
			CalculateDesiredPosition ();
			count++;
		} while(CheckIfOccluded(count));
		
		UpdatePosition ();
	}
	
	void HandlePlayerInput ()
	{
		var deadZone = 0.01f;
		
		if (PenanceMotion.Instance.held)
		{
			mouseX += Input.GetAxis ("Mouse X") * x_MouseSensitivity;
			mouseY -= Input.GetAxis ("Mouse Y") * y_MouseSensitivity;
			/*limit the mouse Y*/
			mouseY = Helper.ClampAngle (mouseY, y_MinLimit, y_MaxLimit);
		
			/*Outside of deadzone*/
			if (Input.GetAxis ("Mouse ScrollWheel") < -deadZone || Input.GetAxis ("Mouse ScrollWheel") > deadZone)
			{
				desiredDistance = Mathf.Clamp (distance - Input.GetAxis ("Mouse ScrollWheel") * mouseWheelSensitivity,
													  distanceMin, distanceMax);
			
				preOccludedDistance = desiredDistance;
				distanceSmooth2 = distanceSmooth;
			}
		}
	}
	
	void CalculateDesiredPosition ()
	{
		/*Evaluate distance*/
		ResetDesiredDistance ();
		distance = Mathf.SmoothDamp (distance, desiredDistance, ref velDistance, distanceSmooth2);
		
		/*Calculate the desired position*/
		if(PenanceMotion.Instance.held)
		{
			desiredPosition = CalculatePosition (mouseY, mouseX, distance);
			if(PenanceMotion.Instance.gripped)
			{
				desiredPosition = CalculatePosition (30, 0, 5);
			}
		}
		else
		{
			desiredPosition = CalculatePosition (0, 0, 15);
		}

	}
	
	Vector3 CalculatePosition (float rotationX, float rotationY, float distance)
	{
		Vector3 direction = new Vector3 (0, 0, -distance);
		Quaternion rotation = Quaternion.Euler (rotationX, rotationY, 0);
		return TargetLookAt.position + (rotation * direction);
	}
	
	bool CheckIfOccluded (int count)
	{
		var isOccluded = false;
		
		var nearestDistance = CheckCameraPoints (TargetLookAt.position, desiredPosition);
		
		if (nearestDistance != -1) {
			if (count < MaxOcclusionChecks) {
				isOccluded = true;
				distance -= OcclusionDistanceStep;
				
				/*might want to change*/
				if (distance < 0.25f)
					distance = 0.25f;	
			} else
				distance = nearestDistance - Camera.mainCamera.nearClipPlane;
			
			desiredDistance = distance;
			distanceSmooth2 = distanceResumeSmooth;
		}
		return isOccluded;
	}
	
	float CheckCameraPoints (Vector3 from, Vector3 to)
	{
		var nearestDistance = -1f;
		
		RaycastHit hitInfo;
		
		Helper.ClipPlanePoints clipPlainPoints = Helper.ClipPlaneAtNear (to);
		
		/*Draw lines in the editor to make it easier to visualize*/
		
		Debug.DrawLine(from, to + transform.forward * -camera.nearClipPlane, Color.red);
		Debug.DrawLine(from, clipPlainPoints.UpperLeft);
		Debug.DrawLine(from, clipPlainPoints.LowerLeft);
		Debug.DrawLine(from, clipPlainPoints.UpperRight);
		Debug.DrawLine(from, clipPlainPoints.LowerRight);
		
		Debug.DrawLine(clipPlainPoints.UpperLeft, clipPlainPoints.UpperRight);
		Debug.DrawLine(clipPlainPoints.UpperRight, clipPlainPoints.LowerRight);
		Debug.DrawLine(clipPlainPoints.LowerRight, clipPlainPoints.LowerLeft);
		Debug.DrawLine(clipPlainPoints.LowerLeft, clipPlainPoints.UpperLeft);
		
		
		if (Physics.Linecast (from, clipPlainPoints.UpperLeft, out hitInfo) && hitInfo.collider.tag != "Player")
			nearestDistance = hitInfo.distance;
		
		if (Physics.Linecast (from, clipPlainPoints.LowerLeft, out hitInfo) && hitInfo.collider.tag != "Player")
		if (hitInfo.distance < nearestDistance || nearestDistance == -1)
			nearestDistance = hitInfo.distance;
		
		if (Physics.Linecast (from, clipPlainPoints.UpperRight, out hitInfo) && hitInfo.collider.tag != "Player")
		if (hitInfo.distance < nearestDistance || nearestDistance == -1)
			nearestDistance = hitInfo.distance;
		
		if (Physics.Linecast (from, clipPlainPoints.LowerRight, out hitInfo) && hitInfo.collider.tag != "Player")
		if (hitInfo.distance < nearestDistance || nearestDistance == -1)
			nearestDistance = hitInfo.distance;
		
		if (Physics.Linecast (from, to + transform.forward * -camera.nearClipPlane, out hitInfo) && hitInfo.collider.tag != "Player")
		if (hitInfo.distance < nearestDistance || nearestDistance == -1)
			nearestDistance = hitInfo.distance;
		
		return nearestDistance;
	}
	
	void ResetDesiredDistance ()
	{
		if (desiredDistance < preOccludedDistance) {
			var position = CalculatePosition (mouseY, mouseX, preOccludedDistance);
			
			var nearestDistance = CheckCameraPoints (TargetLookAt.position, position);
			
			if (nearestDistance == -1 || nearestDistance > preOccludedDistance) {
				desiredDistance = preOccludedDistance;
			}
		}
	}
	
	void UpdatePosition ()
	{
		/*get the x y z positions*/
		var posX = Mathf.SmoothDamp (position.x, desiredPosition.x, ref velX, x_Smooth);
		var posY = Mathf.SmoothDamp (position.y, desiredPosition.y, ref velY, y_Smooth);
		var posZ = Mathf.SmoothDamp (position.z, desiredPosition.z, ref velZ, x_Smooth);
		
		/*get the new vector position*/
		position = new Vector3 (posX, posY, posZ);
		
		/*transform based on the new position calculated*/
		transform.position = position;
		
		/*give the camera a target look at*/
		transform.LookAt (TargetLookAt);
	}
	
	public void Reset ()
	{
		/*set behind the character*/
		mouseX = 0;
		mouseY = 20;
		distance = startDistance;
		desiredDistance = distance;
		preOccludedDistance = distance;
	}
	
	/*Use the existing camera or make a new one*/
	public static void UseExistingOrCreateNewMainCamera ()
	{
		GameObject tempCamera;
		GameObject targetLookAt;
		ThirdPersonCamera myCamera;
		
		/*there is a camera, set it to temp*/
		if (Camera.mainCamera != null) {
			tempCamera = Camera.mainCamera.gameObject;
		} else { /*no camera, create one*/
			tempCamera = new GameObject ("Main Camera");
			
			/*Adding a Camera Compnent or temp cam won't be a Camera*/
			tempCamera.AddComponent ("Camera");
			
			/*this will set the temp camera to MainCamera*/
			tempCamera.tag = "MainCamera";
		}
		
		/*Adding the script ThirdPersonCamera to tempCamera*/
		tempCamera.AddComponent ("ThirdPersonCamera");
		
		/*Finding the camera and setting the Component*/
		myCamera = tempCamera.GetComponent ("ThirdPersonCamera") as ThirdPersonCamera;
		
		/*Assign Camera a target to look at*/
		targetLookAt = GameObject.Find ("CameraTarget") as GameObject;
		
		/*if no targetLookAt create one*/
		if (targetLookAt == null) {
			/*no look at object*/
			targetLookAt = new GameObject ("targetLookAt");
			
			/*position look at to world origin*/
			targetLookAt.transform.position = Vector3.zero;
		}
		
		/*Assign myCamera's target look at to the targetLookAt*/
		myCamera.TargetLookAt = targetLookAt.transform;
	}

	void CameraCheck ()
	{	
		/*there is a camera, set it to temp*/
		if (Camera.mainCamera != null)
		{
			tempCamera2 = Camera.mainCamera.gameObject;
		} else 
		{ /*no camera, create one*/
			tempCamera2 = new GameObject ("Main Camera");
			
			/*Adding a Camera Compnent or temp cam won't be a Camera*/
			tempCamera2.AddComponent ("Camera");
			
			/*this will set the temp camera to MainCamera*/
			tempCamera2.tag = "MainCamera";
		}
		
		/*Adding the script ThirdPersonCamera to tempCamera*/
		tempCamera2.AddComponent ("ThirdPersonCamera");
		
		/*Finding the camera and setting the Component*/
		myCamera = tempCamera2.GetComponent ("ThirdPersonCamera") as ThirdPersonCamera;
	
		if(PenanceMotion.Instance.held)
		{
		/*Assign Camera a target to look at*/
			targetLookAt2 = GameObject.Find ("CameraTarget") as GameObject;
			//Debug.Log("Targeting CameraTarget");
			if(PenanceMotion.Instance.gripped)
			{
				targetLookAt2 = GameObject.Find("SlashTarget") as GameObject;
				//Debug.Log("Targeting SlashTarget");
			}
		}
		else
		{
			targetLookAt2 = GameObject.Find ("PenanceTarget") as GameObject;
			//Debug.Log("Targeting PenanceTarget");
		}
		
		/*Assign myCamera's target look at to the targetLookAt*/
		myCamera.TargetLookAt = targetLookAt2.transform;
	}
}


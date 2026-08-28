//-----------------------------------------------------------------------------------------------------	
// Script controls active objects (those can be find)
//-----------------------------------------------------------------------------------------------------	
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ActiveObject : MonoBehaviour 
{

	public GameObject surroundingParticle; 						// Prefab for particle surrounding found objects movement
	public float finalScale = 0.5f; 							// Final scale of the object
	public float movementDamping = 4.0f;						// Smooth facing/movement value
	public float movementSpeed = 600.0f;						// Speed of object movement along the path
	public float arriveDistance = 70.0f;						// How far should object be to waypoint for its activation and choosing new
	public Vector2 pathModifier = new Vector2(0.5f, -0.3f);  	// Path parabolize modifier


	// Important internal variables - please don't change them blindly
	HintObject hintObject;
	bool chosen = false;
	bool activated = false;
	bool found = false;
	Vector3[] waypoints;
	int currentWaypoint = 0;
	float startScaleDistance;


	//=====================================================================================================
	// Initialize
	public void Init () 
	{
		// Destroy if it is not in a list of objects to find
		if (!chosen) 
		{
			Destroy(GetComponent<Collider2D>());
			return;
		}


		// Prepare path to Hint object
		waypoints = new Vector3[4];

		waypoints[0] = transform.position;
		waypoints[0].z = 0;

		waypoints[1].x = transform.position.x - (transform.position.x - hintObject.transform.position.x) * pathModifier.x;
		waypoints[1].y = hintObject.transform.position.y + hintObject.transform.position.y * pathModifier.y;

		waypoints[2] = hintObject.transform.position;
		waypoints[2].y *= (1 + pathModifier.y/2);
		waypoints[2].z = 0;

		waypoints[3] = hintObject.transform.position;
		waypoints[3].z += 1;

	}

	//-----------------------------------------------------------------------------------------------------	
	void LateUpdate () 
	{
		// Process flying to Hint object
		if(found)
		{
			if (currentWaypoint > 1) 
				transform.localScale = Vector3.Slerp(Vector3.one, Vector3.one * finalScale, 1 - Vector2.Distance(transform.position, hintObject.transform.position) / startScaleDistance);


			// Select next waypoint
			if(currentWaypoint < waypoints.Length)
				if (Vector2.Distance(transform.position, waypoints[currentWaypoint]) < arriveDistance) 
				{
					currentWaypoint++;
					if (currentWaypoint == 2) 
						startScaleDistance = Vector2.Distance(transform.position, hintObject.transform.position);
				}


			// Move to currentWaypoint
			if (currentWaypoint < waypoints.Length)
			{
				SmoothLookAt2D(transform, waypoints[currentWaypoint], movementDamping);
				transform.Translate(Vector3.right * movementSpeed * Time.deltaTime);
				transform.position = new Vector3 (transform.position.x, transform.position.y, waypoints[currentWaypoint].z);
			}
			else
				{
					hintObject.OneMoreCollected(gameObject);
					gameObject.SetActive(false);
					Destroy(this);
				}

		}


	}

	//-----------------------------------------------------------------------------------------------------	
	// React on click/touch
	void OnMouseUp () 
	{
		if (activated) 
		{
			found = true;
			activated = false;

			hintObject.ObjectFound (this);

			for (int i = 0; i < gameObject.transform.childCount; i++) 
				gameObject.transform.GetChild(i).gameObject.SetActive(false);
			
			if (surroundingParticle) 
				surroundingParticle.SetActive(true);
		}

	}

	//----------------------------------------------------------------------------------
	// Smoothly LookAt targetPosition in 2D
	void SmoothLookAt2D (Transform objectTransform, Vector2 targetPosition, float smoothingValue) 
	{
		Vector3 relative = objectTransform.InverseTransformPoint(targetPosition);
		float angle = Mathf.Atan2(relative.y, relative.x) * Mathf.Rad2Deg;

		objectTransform.Rotate (0, 0, Mathf.LerpAngle(0, angle, Time.deltaTime * smoothingValue) );
	}


	//-----------------------------------------------------------------------------------------------------	
	// Functions allows to Get/Set ActiveObject states
	//-----------------------------------------------------------------------------------------------------	
	public bool IsActivated ()
	{
		return activated;
	}

	//-----------------------------------------------------------------------------------------------------	
	public void Activate (bool state)
	{
		activated = state;
	}

	//-----------------------------------------------------------------------------------------------------	
	public bool IsChosen ()
	{
		return chosen;
	}

	//-----------------------------------------------------------------------------------------------------	
	public void Choose (HintObject hint)
	{
		hintObject = hint;
		chosen = true;
	}

	//-----------------------------------------------------------------------------------------------------	
	public bool IsFound ()
	{
		return found;
	}

	//-----------------------------------------------------------------------------------------------------	
	// Override objects Settings with given parameters
	public void OverrideObjectsSettings (GameObject newParticle, float newFinalScale, float newDamping, float newMovementSpeed, float newArriveDistance, Vector2 newPathModifier, GameObject particle) 
	{
		surroundingParticle = newParticle;
		finalScale = newFinalScale; 							
		movementDamping = newDamping;										
		movementSpeed = newMovementSpeed;						
		arriveDistance = newArriveDistance;	
		pathModifier = newPathModifier;	

		// Assign particle effect to this ActiveObject  
		if(particle) 
		{
			surroundingParticle = Instantiate(particle, transform.position, transform.rotation);
			surroundingParticle.transform.parent = transform;
			surroundingParticle.transform.localPosition = new Vector3 (0, 0, 1);
			surroundingParticle.SetActive(false);
		}      

	}

	//-----------------------------------------------------------------------------------------------------
	// Override objects Settings with given reference
	public void OverrideObjectsSettings (ActiveObject reference) 
	{
		if (reference)
			OverrideObjectsSettings (reference.surroundingParticle, reference.finalScale, reference.movementDamping, reference.movementSpeed, reference.arriveDistance, reference.pathModifier, reference.surroundingParticle);
	}

	//-----------------------------------------------------------------------------------------------------	
	// Set parameters for movement animation
	public void SetMovementParameters (float newFinalScale, float newDamping, float newMovementSpeed, float newArriveDistance) 
	{
		finalScale = newFinalScale; 							
		movementSpeed = newDamping;										
		movementSpeed = newMovementSpeed;						
		arriveDistance = newArriveDistance;		
	}

	//=====================================================================================================
}
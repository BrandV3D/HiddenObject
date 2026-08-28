//-----------------------------------------------------------------------------------------------------	
// Script manage activating hint indicator  movement and effects, that can find and point object 
//-----------------------------------------------------------------------------------------------------	
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Hint_FlyingIndicator : MonoBehaviour 
{
	enum ActivatingHintState {Disabled, Awake, Idle, Hide};


	public ParticleSystem appearFX;		// Effect to be enabled on enable
	public ParticleSystem idleFX;			// Effect to be enabled in idle movement
	public ParticleSystem hideFX;			// Effect to be enabled at hiding

	public float movementTime = 3;		// Time of object movement along the path
	public int movementRandomizer = 3000;	// Parameter to randomize movement


	// Important internal variables - please don't change them blindly
	Vector3 indicatorStartPosition;
	Vector3 velocity = Vector3.zero;
	GameObject objectToHint;
	ActivatingHintState state;
	int initialMovementRandomizer;


	//=====================================================================================================
	// Prepare
	void Start () 
	{
		indicatorStartPosition = transform.position;

		appearFX.gameObject.SetActive(false);
		idleFX.gameObject.SetActive(false);
		hideFX.gameObject.SetActive(false);

		initialMovementRandomizer = movementRandomizer;

		state = ActivatingHintState.Disabled;
	}

	//-----------------------------------------------------------------------------------------------------	
	// Process movement animation to object to hint
	void MoveTo (GameObject hintedObject) 
	{ 
		if (hintedObject  &&  hintedObject.activeSelf  &&  Vector2.Distance(transform.position, hintedObject.transform.position) > hintedObject.GetComponent<Renderer>().bounds.extents.x) 
		{
			if (movementRandomizer > 0)
				if (Vector2.Distance(transform.position, hintedObject.transform.position) < hintedObject.GetComponent<Renderer>().bounds.size.x * 2) 
					movementRandomizer = 0; 

			transform.position = Vector3.SmoothDamp(
													transform.position, 
													new Vector3(
																hintedObject.transform.position.x + Random.Range(-movementRandomizer, movementRandomizer), 
																hintedObject.transform.position.y + Random.Range(-movementRandomizer, movementRandomizer), 
																transform.position.z
																), 
													ref velocity, 
													movementTime
													);
		}
		else 
			Hide();

	}	

	//-----------------------------------------------------------------------------------------------------	
	// Process Hunt indication
	void Update () 
	{
		switch (state)
		{
			case ActivatingHintState.Disabled:
				break;


			case ActivatingHintState.Awake:
				if (!appearFX.isPlaying)  
					GoIdle();
				break;


			case ActivatingHintState.Idle:
				MoveTo (objectToHint);
				break;


			case ActivatingHintState.Hide:
				if (!hideFX.isPlaying)  
					GoDisabled();
				break;

		}

	}

	//-----------------------------------------------------------------------------------------------------	
	// Activate hint
	public void Activate (GameObject hintedObject) 
	{
		if (state != ActivatingHintState.Idle)
		{
			appearFX.gameObject.SetActive(true);
			objectToHint = hintedObject;
			state = ActivatingHintState.Awake;
		}
	}

	//-----------------------------------------------------------------------------------------------------	
	// Hide hint
	void Hide () 
	{ 
		idleFX.Clear();
		idleFX.gameObject.SetActive(false);
		hideFX.gameObject.SetActive(true);

		movementRandomizer = initialMovementRandomizer; 

		state = ActivatingHintState.Hide;
	}	

	//-----------------------------------------------------------------------------------------------------	
	// Go to Idle state
	void GoIdle () 
	{ 
		appearFX.Clear();
		appearFX.gameObject.SetActive(false);
		idleFX.gameObject.SetActive(true);

		state = ActivatingHintState.Idle;
	}

	//-----------------------------------------------------------------------------------------------------	
	// Go to Disabled state
	void GoDisabled () 
	{ 
		hideFX.Clear();
		hideFX.gameObject.SetActive(false);

		transform.position = indicatorStartPosition;

		state = ActivatingHintState.Disabled;
	}

	//-----------------------------------------------------------------------------------------------------	
}
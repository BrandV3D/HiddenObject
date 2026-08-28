//-----------------------------------------------------------------------------------------------------	
// Script manage hints: Flying - that can be activated to find and point object 
//						Timed - that appear to hint object from time to time
//-----------------------------------------------------------------------------------------------------	
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Hint : MonoBehaviour 
{
	public GameObject objectsHolder;				// Object that holds all active/hint objects as children
	public float replenishTime = 3;					// How offten flying hint can be used

	public GameObject timedIndicator;				// Object to visualise timed hint
	public Hint_FlyingIndicator flyingIndicator;	// Object to visualise flying hint

	public float timedHintDelay = 60;				// Delay till timed hint activation
	public float timedHintDuration = 1;				// Duration of timed hint visibility


	// Important internal variables - please don't change them blindly
	float replenishmentTime;
	float timedHintTime;


	//=====================================================================================================
	// Initialize
	void Start () 
	{
		if (!objectsHolder) 
			Debug.LogWarning("ATTENTION: ObjectsHolder for ActivatingHint isn't specified in " + gameObject.name + ". Please put there an gameObject that holds all active/hint objects as children." );


		if (flyingIndicator) 
			flyingIndicator.gameObject.SetActive(true);
		else  
			Debug.LogWarning("Indicator for ActivatingHint isn't specified in " + gameObject.name + ". Please put there an gameObject that will move towards object to hint." );


		if (timedIndicator) 
			timedIndicator.SetActive(false);
		else  
			Debug.LogWarning("timedIndicator for ActivatingHint isn't specified in " + gameObject.name + ". Please put there an gameObject that will move towards object to hint." );  


		timedHintTime = Time.time + timedHintDelay;
	}

	//-----------------------------------------------------------------------------------------------------	
	// Process timed hint if needed
	void Update () 
	{
		if (timedIndicator  &&  timedHintTime < Time.time)
		{
			GameObject objectToHighlight = GetObjectToHint(objectsHolder);

			if (objectToHighlight) 
			{
				timedIndicator.transform.position = new Vector3 (objectToHighlight.transform.position.x, objectToHighlight.transform.position.y, -1);
				timedIndicator.SetActive(true);
				timedHintTime = Time.time + timedHintDelay; 

				Invoke ("HideTimedIndicator", timedHintDuration);
			}
			else 
				timedHintTime = Mathf.Infinity;
		}

	}

	//-----------------------------------------------------------------------------------------------------	
	// HideTimedIndicator
	public void HideTimedIndicator()
	{
		timedIndicator.SetActive(false);
	}

	//-----------------------------------------------------------------------------------------------------	
	// Try to find object to hint
	public GameObject GetObjectToHint (GameObject holder)
	{
		if (!holder) return null;
		GameObject hintedObject = null;


		// Try to find any already activated ActiveObject
		ActiveObject[] hiddenObjects = holder.GetComponentsInChildren<ActiveObject>();	
		foreach (ActiveObject hiddenObject in hiddenObjects)
			if (hiddenObject.IsActivated())
			{
				hintedObject = hiddenObject.gameObject;
				break;
			}


		// if no ActiveObject been found - try to find any already activated HintObject  
		if(!hintedObject)
		{
			HintObject[] hintObjects = holder.GetComponentsInChildren<HintObject>();
			if (hintObjects.Length > 0)
				hintedObject = hintObjects[Random.Range(0, hintObjects.Length)].gameObject;

		}


		return hintedObject;

	}

	//-----------------------------------------------------------------------------------------------------	
	// Choose object to hint and  activate Flying Indicator to fly to it
	public void PointOut() 
	{
		if (flyingIndicator) 
			flyingIndicator.Activate (GetObjectToHint (objectsHolder));
		
		replenishmentTime = Time.time + replenishTime;
	}

	//-----------------------------------------------------------------------------------------------------	
	// Remaining time before hint will be ready to be activated again
	public float TimeTillReady()
	{
		return (replenishmentTime - Time.time);
	}

	//-----------------------------------------------------------------------------------------------------	
	// Is Hint ready to be activated (Replenished)
	public bool IsReady()
	{
		return (Time.time > replenishmentTime);
	}

	//-----------------------------------------------------------------------------------------------------	
}

//-----------------------------------------------------------------------------------------------------	
// Utility script - allows to change Animation speed on Start
//-----------------------------------------------------------------------------------------------------	
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Animation_ChangeSpeed : MonoBehaviour 
{
	public float newSpeed;

	//=====================================================================================================
	// Change Animation speed and remove itself
	void Start () 
	{
		foreach (AnimationState state in GetComponent<Animation>()) 
			state.speed = newSpeed;
		
		Destroy(this);
	}

	//-----------------------------------------------------------------------------------------------------
}
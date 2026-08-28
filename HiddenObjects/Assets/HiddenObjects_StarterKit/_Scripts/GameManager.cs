//-----------------------------------------------------------------------------------------------------	
// Script contains and processes Win/Lose conditions for the game.
// This component is required for some other components - please checkscripts and/or warning messages
//-----------------------------------------------------------------------------------------------------	
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TriggeredAction { None, Game_PreStart, Game_Lost, Game_Won, Game_Paused, HintObject_ListVisualized, HintObject_Collected, ActiveObject_Found, ActiveObject_Collected, Hint_Activated }

public class GameManager : MonoBehaviour 
{
	public int foundObjectCost = 100;			// How much one found object costs
	public int requiredScore;					// How many score should player has to Win
	public GameObject[] requiredObjects;		// List of mandatory objects, those player should find to Win
	public int allottedClicks;				// How many clicks player has to execute Win-conditions (before GameOver)
	public int allottedTime;					// How much time player has to execute Win-conditions (before GameOver)


	// Important internal variables - please don't change them blindly
	int score;
	int totalClicks;
	int objectsToFind;
	int totalScore;
	float endTime;
	Vector3 lastFoundObjectPosition;
	TriggeredAction triggeredAction;


	//=====================================================================================================
	// Initialize
	void Start () 
	{
		objectsToFind = requiredObjects.Length;
		triggeredAction = TriggeredAction.Game_PreStart;
	}

	//-----------------------------------------------------------------------------------------------------	
	// Check Win/Lose conditions and change gameState accordingly
	void LateUpdate ()
	{
		switch (triggeredAction)
		{
			case TriggeredAction.Game_PreStart:
				if (allottedTime > 0) 
					endTime = Time.time + allottedTime;
				triggeredAction = TriggeredAction.None;      	 
				break;

			case TriggeredAction.Game_Lost:
			case TriggeredAction.Game_Won:
				// Save new total score
				if (totalScore == 0)
				{
					if (PlayerPrefs.HasKey("TotalScore")) 
						totalScore = PlayerPrefs.GetInt("TotalScore");
					totalScore += score;
					PlayerPrefs.SetInt("TotalScore", totalScore);
				}
				break;

			case TriggeredAction.Game_Paused:
				break;

			case TriggeredAction.None:
			case TriggeredAction.HintObject_ListVisualized:
			case TriggeredAction.HintObject_Collected:
			case TriggeredAction.ActiveObject_Found:
			case TriggeredAction.ActiveObject_Collected:
			case TriggeredAction.Hint_Activated:
				triggeredAction = TriggeredAction.None;
				if (allottedClicks > 0  &&  Input.GetMouseButtonUp(0)) 
					totalClicks++;
			
				if ((allottedClicks > 0  && totalClicks > allottedClicks)  ||  (allottedTime > 0  &&  Time.time > endTime)) 
					triggeredAction = TriggeredAction.Game_Lost;
				else
					if (score >= requiredScore  &&  objectsToFind <= 0) 
						triggeredAction = TriggeredAction.Game_Won;
				break;
		}

	}

	//-----------------------------------------------------------------------------------------------------	
	// React when one more Active object found, but not collected yet
	public void ObjectFound (ActiveObject activeObject)
	{ 
		lastFoundObjectPosition = activeObject.gameObject.transform.position;
		score += foundObjectCost;

		// Check is found object from requiredObjects list or not 
		if (requiredObjects != null  &&  requiredObjects.Length > 0) 
			foreach (GameObject requiredObject in requiredObjects)   
				if (activeObject.gameObject == requiredObject) 
				{
					objectsToFind--;
					return;
				}

		triggeredAction = TriggeredAction.ActiveObject_Found;
	}

	//-----------------------------------------------------------------------------------------------------	
	// Get last triggeredAction and reset it to none;
	public TriggeredAction GetTriggeredAction ()
	{
		return triggeredAction;
	}

	//-----------------------------------------------------------------------------------------------------	
	// Set triggeredAction 
	public void SetTriggeredAction (TriggeredAction newAction)
	{
		triggeredAction = newAction;
	}

	//-----------------------------------------------------------------------------------------------------	
	// Returns lastFoundObject
	public Vector3 GetLastFoundObjectPosition ()
	{ 
		return lastFoundObjectPosition;
	}

	//-----------------------------------------------------------------------------------------------------	
	// Returns how many mandatory objects player should find to Win
	public int GetRemainingObjectsNum ()
	{
		return objectsToFind;
	}

	//-----------------------------------------------------------------------------------------------------	
	// Returns how much time player has before GameOver
	public float GetRemainingTime ()
	{
		if (allottedTime > 0) 
			return (endTime - Time.time);
		else 
			return -1;
	}

	//-----------------------------------------------------------------------------------------------------	
	// Returns how many clicks player has before GameOver
	public int GetRemainingClicks ()
	{
		return (allottedClicks - totalClicks);
	}

	//-----------------------------------------------------------------------------------------------------	
	// Returns how much score player needs to Win
	public int GetRemainingScore ()
	{
		return (requiredScore - score);
	}

	//-----------------------------------------------------------------------------------------------------	
	// Returns total collected score
	public int GetScore ()
	{
		return score;
	}

	//-----------------------------------------------------------------------------------------------------	
}
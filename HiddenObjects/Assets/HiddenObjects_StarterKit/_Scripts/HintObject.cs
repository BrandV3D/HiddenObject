//-----------------------------------------------------------------------------------------------------	
// Contains list of ActiveObjects to find and allows to visualize it
//-----------------------------------------------------------------------------------------------------	
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent (typeof(HintObject_Visualizer))]
public class HintObject : MonoBehaviour 
{
	public ActiveObject[] hiddenObjects;	// List of ActiveObjects (objects to find) assigned to this Hint holder
	public float disappearingSpeed = 5;   // Disappearing speed - when all objects have been found
    public bool alwaysActive = false;	// Hint will be activated and hiddenObjects-list will be shown OnStart


	// Important internal variables - please don't change them blindly
	bool firstActivation = false;	
	bool activated = false;
	int remainedObjectsNum;
	HintObject_Visualizer visualizer;
	GameManager gameManager;
	static HintObject oldHintObject;


	//=====================================================================================================
	// Prepare
	void Start () 
	{
		if (!visualizer) 
			visualizer = GetComponent<HintObject_Visualizer>();

		// Destroy redudant components if there are no hiddenObjects at all
		if (hiddenObjects.Length == 0)
		{
			Destroy(GetComponent<Collider2D>());
			Destroy(this);
			return;
		}
		else 
			remainedObjectsNum = hiddenObjects.Length;

		// Prepare visualizer  
		if (Camera.main.WorldToScreenPoint (transform.position).x < Screen.width/2) 
			visualizer.invertedX = true;
		
		visualizer.Create(this);

		// Connect to gameManager
		gameManager = FindObjectOfType<GameManager>();
		if (!gameManager) 
			Debug.LogError("System can't find TasksManager, please ensure that you have this component attached to any(and only one) object in scene!");


		// Activate if it should be alwaysActive
		if(alwaysActive) 
			OnMouseUp();
		
	}

	//-----------------------------------------------------------------------------------------------------	
	// React on click/touch
	void OnMouseUp () 
	{
		if (oldHintObject && oldHintObject != this)   oldHintObject.Hide();
		
		oldHintObject = this;

		if (!activated || alwaysActive)   
			Show(); 
		else 
			Hide();
	}

	//-----------------------------------------------------------------------------------------------------	
	// Show list
	void Show () 
	{
		// Activate/initialize ActiveObjects assigned to this Hint holder
		if(!firstActivation)
		{
			for (int i = 0; i < hiddenObjects.Length; i++) 
				hiddenObjects[i].Activate(true);
			
			firstActivation = true;
		}

		activated = true;
		visualizer.SetVisibility(activated);

		if (gameManager) 
			gameManager.SetTriggeredAction(TriggeredAction.HintObject_ListVisualized);
	}

	//-----------------------------------------------------------------------------------------------------	
	// Hide list
	void Hide () 
	{
		activated = false;
		visualizer.SetVisibility(activated);
	}

	//-----------------------------------------------------------------------------------------------------	
	// Update list if one more element found
	public void OneMoreCollected (GameObject _object) 
	{
		visualizer.DeleteFromList(_object);

		remainedObjectsNum --;
		if (remainedObjectsNum <= 0)  
		{ 
			Hide();
			StartCoroutine(ShrinkAndDestroy_Coroutine (transform.localScale.x/disappearingSpeed, disappearingSpeed)); 

			if (gameManager) 
				gameManager.SetTriggeredAction(TriggeredAction.HintObject_Collected);
		}

	}

	//-----------------------------------------------------------------------------------------------------	
	// ShrinkAndDestroy Coroutine 
	IEnumerator ShrinkAndDestroy_Coroutine (float targetScale, float speed) 
	{
		while (transform.localScale.x > targetScale * 1.1f)
		{
			transform.localScale = Vector3.Lerp(transform.localScale, new Vector3 (targetScale, targetScale, targetScale) , speed * Time.deltaTime);
			yield return null;
		}

		Destroy(gameObject);
	}

	//-----------------------------------------------------------------------------------------------------	
	// Initialize HiddenObjects Manually with activeObjectsSettingsActiveObjectsSettings
	public void InitializeHiddenObjects (ActiveObject activeObjectsSettings)
	{
		for (int i = 0; i < hiddenObjects.Length; i++)
		{
			hiddenObjects[i].Choose(this);
			hiddenObjects[i].OverrideObjectsSettings (activeObjectsSettings);
		}
	}

	//-----------------------------------------------------------------------------------------------------	
	// React when object  found, but not collected yet
	public void ObjectFound (ActiveObject activeObjects)
	{
		if (gameManager) 
			gameManager.ObjectFound(activeObjects);
	}

	//-----------------------------------------------------------------------------------------------------	
	// Initialize Visualizer
	public void InitVisualizer (HintObject_Visualizer _reference) 
	{
		visualizer = GetComponent<HintObject_Visualizer>();

        visualizer.silhouetteBased = _reference.silhouetteBased;
		visualizer.silhouetteScale = _reference.silhouetteScale;
		visualizer.centralElement = _reference.centralElement;

		visualizer.button_LeftPart = _reference.button_LeftPart;
		visualizer.button_MiddlePart = _reference.button_MiddlePart;
		visualizer.button_RightPart = _reference.button_RightPart;

		visualizer.fontSize = _reference.fontSize;
		visualizer.fontQuality = _reference.fontQuality;
		visualizer.fontColor = _reference.fontColor;
		visualizer.font = _reference.font;
        
		visualizer.centralElementInitialRotation = _reference.centralElementInitialRotation;
        visualizer.centralElementTargetRotation = _reference.centralElementTargetRotation;

        visualizer.movementPointsHolder = _reference.movementPointsHolder;
		visualizer.movementSpeed = _reference.movementSpeed;
		visualizer.movementDistance = _reference.movementDistance;

		visualizer.scaleModifier = _reference.scaleModifier;

        visualizer.invertedX = _reference.invertedX;
        visualizer.invertedY = _reference.invertedY;

    }
	//-----------------------------------------------------------------------------------------------------	
}
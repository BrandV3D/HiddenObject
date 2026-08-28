//-----------------------------------------------------------------------------------------------------	
// Prepare and initialize all objects (objects holds list of Hidden objects)
//-----------------------------------------------------------------------------------------------------	
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class ObjectsInitializer : MonoBehaviour 
{
	public int hintObjectsNum = 2;			// How many active HintObjects can be in scene
	public int objectsPerHint = 3;			// How many Active(hidden) objects should be assigned to each HintObject
	public int minDistance = 250; 			// Minimal distance between Hint object and ActiveObject assigned to it

	public ActiveObject overrideActiveObjectsSettings ;  			// Override ActiveObjects settings by this reference
	public HintObject_Visualizer overrideHintObjectsVisualizers;  	// Override HintObjects Visualizers by this reference

	public HintObject[] hintObjects;			// List of chosen hint objects


	//=====================================================================================================
	// Prepare everything
	void Awake () 
	{
		// Get all HintObjects (contain HintObject component) in children
		Component[] allHintObjects = gameObject.GetComponentsInChildren<HintObject>();
		// Get all ActiveObjects (contain ActiveObject component) in children
		Component[] hiddenObjects = GetComponentsInChildren <ActiveObject>();

		HintObject randomizedHint;
		ActiveObject randomizedObject;

		hintObjects = new HintObject[hintObjectsNum];

		// Choose objects and prepare lists of them
		if (allHintObjects.Length >= hintObjectsNum  && hiddenObjects.Length >= hintObjectsNum * objectsPerHint)
			for (int i = 0; i < hintObjectsNum; i++)
			{
				// Get random HintObject from allHintObjects list. Repeat this until it will be unique (have not been chosen before) 
				do
				{					
					randomizedHint = allHintObjects[Random.Range(0, allHintObjects.Length)] as HintObject;
				}
				while (hintObjects.Contains(randomizedHint));

				// Add found HintObject to hintObjects list
				hintObjects[i] = randomizedHint;
				hintObjects[i].hiddenObjects = new ActiveObject[objectsPerHint];

				// Init hintObjects Visualizer if needed
				if (overrideHintObjectsVisualizers) 
					hintObjects[i].InitVisualizer(overrideHintObjectsVisualizers); 
				


				// Find and assign ActiveObjects for this hintObjects    
				for (int j = 0; j < objectsPerHint; j++) 
				{
					// Get random ActiveObject from hiddenObjects list. Repeat this until it will be unique (have not been chosen before) 
					do
					{
						randomizedObject = hiddenObjects[Random.Range(0, hiddenObjects.Length)] as ActiveObject;                        
                    }
					while (randomizedObject.IsChosen()  ||  Vector2.Distance(hintObjects[i].transform.position, randomizedObject.transform.position) < minDistance);

                    // Setup and Assign found ActiveObjects for this HintObject
                    randomizedObject.Choose(hintObjects[i]);
					randomizedObject.OverrideObjectsSettings (overrideActiveObjectsSettings);
					hintObjects[i].hiddenObjects[j] = randomizedObject;
                }

            }		
		else
			Debug.LogError("Something wrong with quantity of objects: \n Hint objects:  found-" + allHintObjects.Length + ", requested-" + hintObjectsNum + ".    Hidden objects:  found-" + hiddenObjects.Length + ", requested-" + hintObjectsNum * objectsPerHint);


        // Init all ActiveObjects
        foreach (ActiveObject obj in hiddenObjects)  obj.Init ();


        Destroy(this); 
    }

	//-----------------------------------------------------------------------------------------------------	
}
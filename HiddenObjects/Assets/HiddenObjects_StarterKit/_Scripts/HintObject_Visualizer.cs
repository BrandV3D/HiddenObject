//-----------------------------------------------------------------------------------------------------	
// Script allows to create an process animated visualization of HintObject hiddenObjects list
// All elements are optional, so if you don't want to have some of parts - just don't assign related element
//-----------------------------------------------------------------------------------------------------	
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HintObject_Visualizer : MonoBehaviour 
{
	// Class to hold data about ListElement visuals
	public class ListElement
	{
		public string objectId;
		public GameObject hudObject;
		public string description;
		public Vector2 renderSize;
	}


	//[HideInInspector]
	public List<ListElement> listElements;				// Array of all visual listElements

	public bool silhouetteBased = false;				// Should hiddenObjects be visualized as silhouettes or by description
	public float silhouetteScale = 1;					// Custom scale for silhouettes

	public Sprite centralElement;						// Animated Central element of visualization
	public int centralElementInitialRotation = -90;		// Initial rotation of the Central element. At appearing Rotation will be animated from this to TargetRotation
	public int centralElementTargetRotation = 360;		// Target rotation of the Central element.  At appearing Rotation will be animated from InitialRotation to this

	public Sprite button_LeftPart;						// Left part of visual ListElement background
	public Sprite button_MiddlePart;						// Middle part of visual ListElement background
	public Sprite button_RightPart;						// Right part of visual ListElement background

	public float fontSize = 3;							// Caption(description) font size
	public Color fontColor = Color.white;				// Caption(description) color
	[Range (1, 10)]
	public float fontQuality = 1;						// Caption(description) font quality
	public Font font;									// Caption(description) font


	public Transform movementPointsHolder;				// Link to reference object that children positions will be used as template for listElements positioning
	public float movementSpeed  = 5f;					// How fast listElements will be positioning
	public float movementDistance  = 10f;				// How far from target position can listElements be

	public float scaleModifier = 100;					// Scale modifier for  whole visualization
	public bool invertedX = false;						// Should be X-position of listElements inverted related to positions in movementPointsHolder
	public bool invertedY = false;						// Should be Y-position of listElements inverted related to positions in movementPointsHolder


	// Important internal variables - please don't change them blindly
	Vector2[] movementPoints;
	GameObject central;


	//=====================================================================================================
	// Create whole visualization for HintObject hiddenObjects list
	public void Create (HintObject hintObject) 
	{
		// If there is no HintObject component attached to gameObject - destroy HintObject_Visualizer.
		if (hintObject == null  ||  hintObject.hiddenObjects.Length  == 0) 
		{
			Destroy(this);
			return;
		}

		// Create and initializeall listElements 
		if (listElements == null) 
			listElements = new List<ListElement>(hintObject.hiddenObjects.Length);
	
		for (int i = 0; i < hintObject.hiddenObjects.Length; i++) 
			listElements.Add (CreateListElement (hintObject.hiddenObjects [i].gameObject));
		
		listElements.Sort(Sorter);

        if (centralElementTargetRotation == 360) centralElementTargetRotation = 359;

       // Create and initialize Central element
       central = CreatePart (gameObject, centralElement);
		if (central)
		{
			central.SetActive(false);
			central.transform.eulerAngles = new Vector3(
														central.transform.rotation.eulerAngles.x,
														central.transform.rotation.eulerAngles.y,
														invertedX ? centralElementTargetRotation: centralElementInitialRotation
														);

			central.transform.localScale = Vector3.one * scaleModifier; 

			central.transform.position = new Vector3 (
														central.transform.position.x,
														central.transform.position.y,
														-0.1f
													);
		}


		//Prepare listElements animation
		movementPoints = new Vector2[listElements.Count];

		if (!movementPointsHolder || movementPointsHolder.childCount < listElements.Count)  
			Debug.LogWarning("ATTENTION: There is no movementPointsHolder (or not enough child points in it) for HintObject_Visualizer in " + gameObject.name + " object! \n    ->  System will generate default positions for all exceeded elements!" );

		Vector2 positionInverter = new Vector2 (1,1);   
		if (invertedX) positionInverter.x = -1;
		if (invertedY) positionInverter.y = -1;


		// Prepare movementPoints using movementPointsHolder children positions as templates
		for (int i = 0; i < listElements.Count; i++) 
		{
			if (movementPointsHolder && i < movementPointsHolder.childCount) 
			{
				movementPoints[i] = movementPointsHolder.GetChild(i).localPosition;
				movementPoints[i] = new Vector2 (movementPoints[i].x * positionInverter.x, movementPoints[i].y * positionInverter.y);
			}
			else  // Generate movementPoints if there is no reference in movementPointsHolder 
				movementPoints[i] = new Vector2 (
													(-listElements[i].renderSize.x*0.5f) * positionInverter.x  * scaleModifier,
													(-listElements[i].renderSize.y*1.25f) * i * positionInverter.y  * scaleModifier
												); 
			
			// Set listElements position to movementPoints[0]					 
			listElements[i].hudObject.transform.localPosition = movementPoints[0];
			listElements[i].hudObject.transform.position = new Vector3 (
																			listElements[i].hudObject.transform.position.x,
																			listElements[i].hudObject.transform.position.y,
																			-0.2f
																		);		
		}

	}

	//-----------------------------------------------------------------------------------------------------	
	// Set visibility of the list and  trigger appearance animation
	public void SetVisibility (bool visible) 
	{
		if (central)
		{
			int targetRotation = centralElementTargetRotation;
			if (invertedX) 
				targetRotation = centralElementTargetRotation/2;

			central.SetActive(visible);

			if (visible)
				StartCoroutine(Rotate_Coroutine (central, targetRotation));
			else
				central.transform.eulerAngles = new Vector3 (
																central.transform.rotation.eulerAngles.x,
																central.transform.rotation.eulerAngles.y,															
																invertedX ? centralElementTargetRotation: centralElementInitialRotation
															);
		}


		for (int i = 0; i < listElements.Count; i++)  
		{
			listElements[i].hudObject.SetActive(visible);

			if (visible) 
				StartCoroutine(MoveTo_Coroutine(listElements[i].hudObject, movementPoints[i]));
			
			else 
				listElements[i].hudObject.transform.localPosition = new Vector3 (
																					movementPoints[0].x,
																					movementPoints[0].y,
																					listElements[i].hudObject.transform.localPosition.z
																				);
		}

	}

	//-----------------------------------------------------------------------------------------------------	
	// Delete an object from the list
	public void DeleteFromList(GameObject _object)
	{
		for (int i = 0; i < listElements.Count; i++) 
			if (listElements[i].objectId == _object.name)
			{
				Destroy(listElements[i].hudObject);
				listElements.RemoveAt (i);
				break;
			}

		// Trigger animation to update list elements positions  
		for (int i = 0; i < listElements.Count; i++) 
			StartCoroutine(MoveTo_Coroutine(listElements[i].hudObject, movementPoints[i]));
	}

	//-----------------------------------------------------------------------------------------------------	
	// Create visual ListElement
	ListElement CreateListElement (GameObject hiddenObject)
	{
		ListElement listElement = new ListElement();
		listElement.objectId = hiddenObject.name;

		// Set caption/description, that can be  visualized
		listElement.description = hiddenObject.name;

		// Create main object      
		GameObject elementObject = new GameObject();
		elementObject.name = "(" + listElement.objectId + ")";
		elementObject.transform.parent = transform;
		elementObject.transform.localPosition = Vector3.zero;	

		GameObject caption = null;
		Vector2 silhouetteRenderSize = Vector2.zero;
		// Create Silhouette or Caption for  the  object
		if (silhouetteBased) 
			silhouetteRenderSize = CreateSilhouette(elementObject, hiddenObject).GetComponent<Renderer>().bounds.size;
		else
			caption = CreateCaption(elementObject, listElement.description, fontSize, fontQuality, font);

		// Create	 visual ListElement background from 3 parts	 
		GameObject leftPart = CreatePart(elementObject, button_LeftPart);
		GameObject middlePart = CreatePart(elementObject, button_MiddlePart);
		GameObject rightPart = CreatePart(elementObject, button_RightPart);


		// Place background parts properly and  calculate theit total render size      
		if (middlePart) 
		{
			if (caption != null)
			{ 
				middlePart.transform.localScale = new Vector3 (
																caption.GetComponent<Renderer>().bounds.size.x / middlePart.GetComponent<Renderer>().bounds.size.x,
																middlePart.transform.localScale.y,
																middlePart.transform.localScale.z
															);

				middlePart.transform.localPosition = new Vector3 (
																	middlePart.transform.localPosition.x,
																	middlePart.transform.localPosition.y,
																	-0.01f
																);
			}

				listElement.renderSize = new Vector2 (
														listElement.renderSize.x + middlePart.GetComponent<Renderer>().bounds.size.x,
														middlePart.GetComponent<Renderer>().bounds.size.y
													);
		}


		if (leftPart) 
		{
			leftPart.transform.localPosition = new Vector3 (
																-middlePart.GetComponent<Renderer>().bounds.extents.x,
																leftPart.transform.localPosition.y,
																leftPart.transform.localPosition.z
															);


			listElement.renderSize= new Vector2 (
													listElement.renderSize.x + leftPart.GetComponent<Renderer>().bounds.size.x,
													leftPart.GetComponent<Renderer>().bounds.size.y
												);
		}

		if (rightPart) 
		{
			rightPart.transform.localPosition = new Vector3 (
																middlePart.GetComponent<Renderer>().bounds.extents.x + rightPart.GetComponent<Renderer>().bounds.extents.x,
																rightPart.transform.localPosition.y,
																rightPart.transform.localPosition.z
															);

			listElement.renderSize = new Vector2 (
													listElement.renderSize.x + rightPart.GetComponent<Renderer>().bounds.size.x,
													rightPart.GetComponent<Renderer>().bounds.size.y
												);
		}

		// Update listElement.renderSize If there is no background or it render size < silhouetteRenderSize  
		listElement.renderSize = new Vector3 (
												(silhouetteRenderSize.x > listElement.renderSize.x) ? silhouetteRenderSize.x : listElement.renderSize.x,
												(silhouetteRenderSize.y > listElement.renderSize.y) ? silhouetteRenderSize.y : listElement.renderSize.y
											);




		// Assign elementObject as visualizer (listElement.hudObject)
		elementObject.transform.localScale = Vector3.one * scaleModifier;   

		elementObject.transform.position = new Vector3 (
															elementObject.transform.position.x,
															elementObject.transform.position.y,
															-1
														);
			
		listElement.hudObject = elementObject;
		listElement.hudObject.SetActive(false); 


		return listElement;

	}

	//-----------------------------------------------------------------------------------------------------	
	// Create Silhouette from hiddenObject render
	GameObject CreateSilhouette(GameObject parent, GameObject hiddenObject)
	{
		GameObject  newObject = new GameObject();

		newObject.name = hiddenObject.name;
		newObject.transform.parent = parent.transform;
		newObject.transform.localScale = Vector3.one * silhouetteScale;
		newObject.transform.localPosition = new Vector3 (0, 0, -0.1f);

		newObject.AddComponent<SpriteRenderer>();

		// Get texture coordinates from hiddenObject mesh.uv
		Vector2[] uvs   = hiddenObject.GetComponent<MeshFilter>().mesh.uv;
		Rect objectUVRect = new Rect (
											uvs[0].x * hiddenObject.GetComponent<Renderer>().material.mainTexture.width,
											uvs[0].y * hiddenObject.GetComponent<Renderer>().material.mainTexture.height,
											(uvs[2].x - uvs[0].x) * hiddenObject.GetComponent<Renderer>().material.mainTexture.width,
											(uvs[2].y  - uvs[0].y) * hiddenObject.GetComponent<Renderer>().material.mainTexture.height
										);

		//Create Silhouette		
		(newObject.GetComponent<Renderer>() as SpriteRenderer).sprite = Sprite.Create(hiddenObject.GetComponent<Renderer>().material.mainTexture as Texture2D, objectUVRect, new Vector2(0.5f, 0.5f), 100);
		(newObject.GetComponent<Renderer>() as SpriteRenderer).color = Color.black;
		(newObject.GetComponent<Renderer>() as SpriteRenderer).sprite.name =  "_Silhouette";


		return newObject;

	}

	//-----------------------------------------------------------------------------------------------------	
	// Create caption if font specified
	GameObject CreateCaption (GameObject parent, string caption, float fontSize, float fontQuality, Font font)
	{
		if(font) 
		{
			GameObject newObject = new GameObject();
			newObject.name = "Caption";
			newObject.transform.parent = parent.transform;
			newObject.transform.localScale = new Vector3 (fontSize/fontQuality, fontSize/fontQuality, 1);
			newObject.transform.localPosition = new Vector3 (0, 0, -0.1f);	

			newObject.AddComponent<TextMesh>();

			TextMesh  newObject_text = newObject.GetComponent<TextMesh>();
			newObject_text.text = caption;
			newObject_text.fontSize = (int)fontQuality;
			newObject_text.font = font;
			newObject_text.anchor = TextAnchor.MiddleCenter;
			newObject_text.GetComponent<Renderer>().material = font.material;
			newObject_text.color = fontColor;

			return newObject;
		} 
		else
			{ 
				Debug.LogWarning("ATTENTION: Font is missed in HintObject_Visualizer of " + gameObject.name + ". Caption will not be created!");
				return null;
			}

	}

	//-----------------------------------------------------------------------------------------------------	
	// Create visualization part if it sprite specified
	GameObject CreatePart (GameObject parent, Sprite sprite)
	{
		if(sprite)
		{
			GameObject newObject = new GameObject();
			newObject.name = sprite.name;
			newObject.transform.parent = parent.transform;
			newObject.transform.localScale = Vector3.one;
			newObject.transform.localPosition = Vector3.zero;	

			newObject.AddComponent<SpriteRenderer>();  

			Vector2 pivot = new Vector2(0.5f-sprite.bounds.center.x, 0.5f-sprite.bounds.center.y);
			(newObject.GetComponent<Renderer>() as SpriteRenderer).sprite = Sprite.Create(sprite.texture, sprite.textureRect, pivot, 100);
			(newObject.GetComponent<Renderer>() as SpriteRenderer).sprite.name =  sprite.name + "_sprite";

			return newObject;
		} 
		else
			{
				Debug.LogWarning("ATTENTION: One of Sprites is missed in HintObject_Visualizer of " + gameObject.name + ". Part of visualization will not be created!");
				return null;
			}

	}

	//-----------------------------------------------------------------------------------------------------	
	// Compare descriptions by names
	int Sorter(ListElement A, ListElement B)
	{
		if (A == null  ||  B == null) return 0;

		if (A.description.Length < B.description.Length) return 1;
		else
			if (A.description.Length > B.description.Length) return -1;
			else
				return 0;
	}

	//-----------------------------------------------------------------------------------------------------	
	// Animation(movement to intended positions) coroutine - used to animate list elements
	IEnumerator MoveTo_Coroutine (GameObject _object, Vector2 target)
	{ 
		while(_object != null  &&  _object.activeSelf  &&  Vector2.Distance(_object.transform.localPosition, target) > movementDistance)
			{ 
				Vector2 tmpPosition = Vector2.Lerp(_object.transform.localPosition, target, movementSpeed * Time.deltaTime);
			
				_object.transform.localPosition = new Vector3 (
																tmpPosition.x,
																tmpPosition.y,
																_object.transform.localPosition.z
															);

				yield return null;
			}

	}

	//-----------------------------------------------------------------------------------------------------
	// Animation(rotation to target angle) coroutine - used to animate central element	
	IEnumerator Rotate_Coroutine (GameObject _object, int target)
	{ 
		while(_object != null  &&  _object.activeSelf  &&  (int)_object.transform.rotation.eulerAngles.z != target)
		{

		_object.transform.eulerAngles = new Vector3(
													_object.transform.rotation.eulerAngles.x,
													_object.transform.rotation.eulerAngles.y,
													Mathf.Lerp (_object.transform.rotation.eulerAngles.z, target, movementSpeed * Time.deltaTime)
													);

			yield return null;
		}

	}

	//-----------------------------------------------------------------------------------------------------
}
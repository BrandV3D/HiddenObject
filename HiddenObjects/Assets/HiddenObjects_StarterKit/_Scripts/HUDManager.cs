//-----------------------------------------------------------------------------------------------------	
// Script processes and draw whole common GUI
//-----------------------------------------------------------------------------------------------------	
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



public class HUDManager : MonoBehaviour 
{

	public MenuWindow pauseMenu;			// Link to PauseMenu MenuWindow
	public Hint activatingHint;				// Link to activatingHint object

	public GUISkin guiSkin;					// GuiSkin to use
	public TextMesh pointsGainedFX;			// Link to 3D text with text and effect, those will be shown when one more object found
	public GameObject GameOverFX;			// Link to Object with Win effect
	public GameObject WinFX;				// Link to Object with GameOver effect
	public GameObject overlay;				// Link to Overlay object


	// Important internal variables - please don't change them blindly
	GameManager gameManager;
	Vector3 foundObjectPosition = Vector3.zero;
	Animation pointsGainedFX_anim;
	TriggeredAction triggeredAction;

	//=====================================================================================================
	// Initialize
	void Start () 
	{
		// Connect to gameManager
		gameManager = GameObject.FindObjectOfType<GameManager>();

		if (!gameManager) 
			Debug.LogError("System can't find TasksManager, please ensure that you have this component attached to any(and only one) object in scene!");

		if (pointsGainedFX) 
			pointsGainedFX.gameObject.SetActive(true);
		
		if (GameOverFX) 
			GameOverFX.SetActive(false);
		
		if (WinFX) 
			WinFX.SetActive(false);

		pointsGainedFX_anim = pointsGainedFX.GetComponent<Animation>();

	}

	//-----------------------------------------------------------------------------------------------------	
	// Check Win/Lose 
	void LateUpdate ()
	{

		if (!gameManager || !gameManager.enabled) 
			return;
		else
			triggeredAction = gameManager.GetTriggeredAction();

		// Show  overlay if needed
		if (overlay  &&  triggeredAction != TriggeredAction.Game_Lost  &&  triggeredAction != TriggeredAction.Game_Won  &&  triggeredAction != TriggeredAction.Game_Paused  &&  triggeredAction != TriggeredAction.Game_PreStart) 
			overlay.SetActive(false);
		else 
			overlay.SetActive(true);


		foundObjectPosition = gameManager.GetLastFoundObjectPosition();


		// Show FX for gained points if needed
		if (pointsGainedFX  &&  pointsGainedFX.transform.position != new Vector3(foundObjectPosition.x, foundObjectPosition.y, -1)  &&  foundObjectPosition != Vector3.zero)
		{
			pointsGainedFX.text = "+" + gameManager.foundObjectCost.ToString();
			pointsGainedFX.transform.position = new Vector3	(foundObjectPosition.x, foundObjectPosition.y, -1);

			if (pointsGainedFX_anim) 
				pointsGainedFX_anim.Play();	
		}  


		// Show FX for Win/Lose game according to gameManager gameState	
		if (GameOverFX  &&  WinFX)
			if (!GameOverFX.activeSelf  &&  !WinFX.activeSelf)
			{
				if (triggeredAction == TriggeredAction.Game_Won)  
					WinFX.SetActive(true);
				else
					if (triggeredAction == TriggeredAction.Game_Lost)  
						GameOverFX.SetActive(true);
			}


	}

	//-----------------------------------------------------------------------------------------------------	
	// Draw GUI
	void OnGUI() 
	{
		if (!gameManager || !gameManager.enabled) 
			return;
		else
			triggeredAction = gameManager.GetTriggeredAction();
		
		GUI.skin = guiSkin;


		if (triggeredAction != TriggeredAction.Game_Lost  &&  triggeredAction != TriggeredAction.Game_Won  &&  triggeredAction != TriggeredAction.Game_PreStart)
		{

			// Draw MENU button and show pauseMenu if it was pressed
			if (pauseMenu)
			{
				if (GUI.Button(new Rect(0,0,100,60),"MENU"))  
					pauseMenu.enabled = !pauseMenu.enabled;
				
				if (pauseMenu.enabled) 
					gameManager.SetTriggeredAction(TriggeredAction.Game_Paused);
				else  
					gameManager.SetTriggeredAction(TriggeredAction.None);
			}


			// Draw HINT button (or counter if it's disabled)
			if (activatingHint)
				if (!activatingHint.IsReady())  
					GUI.Box(new Rect(Screen.width-100,0,100,60),"" + Mathf.Floor(activatingHint.TimeTillReady()).ToString());
				else 
					// If button pressed - Enable activatingHint to help and PointOut a hidden object
					if (GUI.Button(new Rect(Screen.width-100,0,100,60),"HINT"))  
					{
						gameManager.SetTriggeredAction(TriggeredAction.Hint_Activated);
						activatingHint.PointOut();
					}


			// Draw current game  values
			GUI.Label (new Rect (0, 60, 250, 40), "SCORE :   " + gameManager.GetScore().ToString());

			if (gameManager.allottedTime > 0)  
				GUI.Label (new Rect (0, 100, 250, 40),  "Time left:   " + ((int)gameManager.GetRemainingTime()).ToString());
			
			if (gameManager.GetRemainingClicks() > 0 ) 
				GUI.Label (new Rect (0, 140, 250, 40),  "Clicks left: " + gameManager.GetRemainingClicks().ToString());

		}
		else
			if (triggeredAction == TriggeredAction.Game_Lost ||  triggeredAction == TriggeredAction.Game_Won)
			{ 
				if (GUI.Button(new Rect(Screen.width/2-160, Screen.height/2 + 50, 150, 50),"ONCE AGAIN"))  
					SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
				
				if (GUI.Button(new Rect(Screen.width/2+10, Screen.height/2 + 50, 150, 50),"MAIN MENU")) 
					SceneManager.LoadScene(1);
			}


	}

//-----------------------------------------------------------------------------------------------------	
}
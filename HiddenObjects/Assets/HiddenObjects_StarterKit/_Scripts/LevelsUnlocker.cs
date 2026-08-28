//----------------------------------------------------------------------------------
// This script unlocking levels if stored "TotalScore" is bigger than level cost
//----------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class LevelsUnlocker : MonoBehaviour 
{
	public MenuWindow levelsMenu;	// Link to levels window
	public int[] levelsCost;		// List of unlocking cost for each level
	public TextMesh scoreRenderer;  // Link to 3D text to render score


	// Important internal variables
	int totalScore;
	int nextLevelCost;


	//========================================================================================================
	// Process
	void Start () 
	{
		//PlayerPrefs.DeleteAll();

		if (!levelsMenu) 
			levelsMenu = GetComponent<MenuWindow>();
		
		if (!scoreRenderer)
			scoreRenderer = GetComponent<TextMesh>();


		// Get saved score
		if (PlayerPrefs.HasKey("TotalScore")) 
			totalScore = PlayerPrefs.GetInt("TotalScore"); 

		// Optional: Change  scoreRenderer text to visualize current Total score      
		if (scoreRenderer)  
			scoreRenderer.text = "Total score: " +  totalScore.ToString();


		if (levelsMenu)
		{
			// Unlock levels which cost is less than TotalScore
			for (int i = 0; i < levelsCost.Length; i++)
				if (totalScore >= levelsCost[i]) 
					levelsMenu.Elements[i].locked = false;
				else 
				{
					nextLevelCost = levelsCost[i];
					break;
				}

			// Update info-message, with  amount of points to earn to unlock next level  
			if (nextLevelCost > 0) 
				levelsMenu.Elements[levelsCost.Length].caption = "Earn additional " + (nextLevelCost-totalScore).ToString() + " points to unlock next level!";
			else 
				levelsMenu.Elements[levelsCost.Length].caption = "You've unlocked everything!";
		}    

	}

	//----------------------------------------------------------------------------------
}

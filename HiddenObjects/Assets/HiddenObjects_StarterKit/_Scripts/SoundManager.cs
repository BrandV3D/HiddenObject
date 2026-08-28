//-----------------------------------------------------------------------------------------------------	
// Script allows to assign and  automatically process all event-related(trigerred) sounds
// Based on triggeredAction value of gameManager
//-----------------------------------------------------------------------------------------------------	
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SoundManager : MonoBehaviour 
{

	[System.Serializable]
	public class GameSound
	{
		public string caption;
		public TriggeredAction triggeredAction;
		public AudioClip audioClip;
		public bool playOnce = false;
	}


	public AudioSource musicSource;		// Link to AudioSource handles music
	public AudioSource soundSource;		// Link to AudioSource handles sound effects
	public GameSound[] sounds;			// List of GameSounds to process


	// Important internal variables - please don't change them blindly
	GameManager gameManager;
	AudioClip clipToPlay;
	bool skip = false;
	bool playOnce = false;
	TriggeredAction triggeredAction;


	//=====================================================================================================
	// Initialize
	void Start () 
	{
		// Connect to gameManager
		gameManager = GameObject.FindObjectOfType<GameManager>();

		if (!gameManager) 
			Debug.LogError("System can't find TasksManager, please ensure that you have this component attached to any (and only one) object in scene!");

		if (!soundSource) 
			Debug.LogError("System can't find soundSource, please assign an AudioSource to the related SoundManager property of " + gameObject.name);

	}

	//-----------------------------------------------------------------------------------------------------	
	//  Process all event-related(trigerred) sounds
	void Update () 
	{
		if (!gameManager || !soundSource)
			return;
			
		// Check triggeredAction and switch to related AudioClip
		triggeredAction = gameManager.GetTriggeredAction();

		if (triggeredAction == TriggeredAction.None) 
			clipToPlay = null;
		else
			for (int i = 0; i < sounds.Length; i++)
				if (sounds[i].triggeredAction == triggeredAction  &&  sounds[i].audioClip != null)
				{
					clipToPlay = sounds[i].audioClip;
					playOnce = sounds[i].playOnce;
					break;
				}

		// Play sound  
		if (clipToPlay && !skip)  
		{
			soundSource.PlayOneShot(clipToPlay);
			// Prevent sounds loopping for long-term states
			skip = playOnce;
		}

	}

	//-----------------------------------------------------------------------------------------------------	
	// Mute/Unmute music
	public void muteMusic (bool mute) 
	{
		musicSource.mute = mute;
	}

	//-----------------------------------------------------------------------------------------------------	
	// Mute/Unmute all sounds process by this script
	public void muteSounds (bool mute) 
	{
		soundSource.mute = mute;
	}

	//-----------------------------------------------------------------------------------------------------	
}
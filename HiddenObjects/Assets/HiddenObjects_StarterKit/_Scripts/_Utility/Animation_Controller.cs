//-----------------------------------------------------------------------------------------------------	
// Utility script - allows to control different reactions for Animations
//-----------------------------------------------------------------------------------------------------	
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Animation_Controller : MonoBehaviour 
{
	// Types of Animation reactions
	public enum ReactionType {SwitchClips, PlayStop, Rewind}


	public AnimationClip alternativeClip;	// Alternative animation to swap 
	public ReactionType clickReaction;		// Reaction type
	public float delay = 0;					// Delay between bunches of cycles
	public int cycles = 0;					// How many times should be animation repeated before next delay
	public bool randomized = false;			// Randomize delay and cycles parameters
	public bool oneTimeActivation = false;	// Reaction will be activated only once
	public bool deleteIfStopped = false;	// Delete if animation isnt playing


	// Important internal variables - please don't change them blindly
	float startTime;
	float initialDelay;
	int initialCycles;
	AnimationClip initialClip;
	Animation anim;


	//=====================================================================================================
	// Initialize
	void Start () 
	{
		anim = GetComponent<Animation> ();
		initialDelay = delay;
		initialCycles = cycles;
		initialClip = anim.clip;

		if (randomized) 
		{
			delay = Random.Range(1, initialDelay);
			cycles = Random.Range(1, initialCycles);
		}

		startTime = Time.time + delay;
	}

	//-----------------------------------------------------------------------------------------------------	
	// Process cycles and delays if needed
	void Update () 
	{
		if (cycles <= 0)
		{
			if (deleteIfStopped && !anim.isPlaying) 
				Destroy(gameObject);
		}
		else
			if (startTime < Time.time)
			{
				for (int i = 0; i < cycles; i++)
					anim.PlayQueued(anim.clip.name);

				if (randomized) 
				{
					delay = Random.Range(1, initialDelay);
					cycles = Random.Range(1, initialCycles);
				}

				startTime = Time.time + anim.clip.length*cycles + delay;
			}

	}

	//-----------------------------------------------------------------------------------------------------	
	// React on click/touch according to clickReaction Type
	public void OnMouseUp () 
	{

		switch(clickReaction)
		{
			case ReactionType.SwitchClips:
				SwitchClip ();
				break;

			case ReactionType.PlayStop:
				if (anim.isPlaying) 
					anim.Stop(); 
				else 
					anim.Play();
				break;

			case ReactionType.Rewind:
				Rewind ();
				break;
		}

		if (oneTimeActivation) 
		{
			Destroy(GetComponent<Collider2D>());
			cycles = 0;
		}

	}

	//-----------------------------------------------------------------------------------------------------	
	// Rewind current animaion
	public void Rewind () 
	{
		anim.Rewind(anim.clip.name);
	}

	//-----------------------------------------------------------------------------------------------------	
	// Swap current animation between initialClip and alternativeClip
	public void SwitchClip () 
	{
		if (anim.IsPlaying(initialClip.name)) 
			anim.Play(alternativeClip.name);
		else 
			anim.Play(initialClip.name);		
	}

	//-----------------------------------------------------------------------------------------------------	
}
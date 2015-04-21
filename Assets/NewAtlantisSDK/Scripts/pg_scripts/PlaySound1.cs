﻿using UnityEngine;
using System.Collections;

	public class PlaySound1 : MonoBehaviour {
	
	public AudioClip mysound;
	Vector3 startpos;
	// Use this for initialization
	void Start () 
	{
		audio.Stop ();
		startpos = transform.position;
	}

	// Update is called once per frame
	void Update () 
	{
		if (!audio.isPlaying)
		{
			transform.position = startpos;
			rigidbody.useGravity = false;
			renderer.material.color = Color.white;

		}
	}

	void OnMouseDown ()
	{
		renderer.material.color = Color.red;
		audio.Play ();
		rigidbody.useGravity = true;
	}
	
	void OnMouseExit ()
	{
		//renderer.material.color = Color.white;
	}
	


	void OnMouseUp () 
	{
						//audio.Stop ();
						//transform.position = startpos;
						//rigidbody.useGravity = false;
						//renderer.material.color = Color.white;
	}

}

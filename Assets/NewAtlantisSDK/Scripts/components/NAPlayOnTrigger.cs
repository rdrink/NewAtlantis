using UnityEngine;
using System.Collections;

public class NAPlayOnTrigger : MonoBehaviour 
{
	public bool StopOnExit = false;
	void OnTriggerEnter(Collider collider) 
	{
		GetComponent<AudioSource>().Play();
	}

	void OnTriggerExit(Collider collider) 
	{
		if (StopOnExit)
			GetComponent<AudioSource>().Stop();
	}
}

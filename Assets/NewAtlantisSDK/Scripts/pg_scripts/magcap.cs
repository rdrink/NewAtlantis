using UnityEngine;
using System.Collections;

public class magcap : MonoBehaviour {
	public AudioClip mysound;
	Vector3 target;
	Vector3 startpos1;
	bool on = false;
	// Use this for initialization
	void Start () {
		target = new Vector3 (-0.0410f, -2.0f, -0.9652214f);
		audio.Stop ();
		startpos1 = transform.position;
	
	}
	void OnMouseEnter ()
	{
		renderer.material.color = Color.blue;
	}

	void OnMouseDown()
	{
		on = true;
		audio.Play ();
	}
	// Update is called once per frame
	void Update () {

		if (on)
		{
			Vector3 direction = target - transform.position;


			if (direction.magnitude > 00.0082)
			{
				direction.Normalize ();
				//transform.Translate (Vector3.forward * Time.deltaTime * 1);
				transform.Translate (direction * Time.deltaTime * 0.265f);
				//transform.LookAt (target);
			}
			else
			{
				on = false;
				audio.Stop ();
				transform.position = startpos1;
				renderer.material.color = Color.white;

			}
		}

			

	}
}

	
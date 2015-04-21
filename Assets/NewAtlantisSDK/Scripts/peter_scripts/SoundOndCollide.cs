using UnityEngine;
using System.Collections;

public class SoundOndCollide : MonoBehaviour {

	void OnCollisionEnter(Collision collision) {

			audio.Play();
		//Debug.Log ("rferge");
	}
}

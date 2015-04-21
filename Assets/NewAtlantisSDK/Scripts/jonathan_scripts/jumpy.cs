using UnityEngine;
using System.Collections;

public class jumpy : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	void OnMouseDown()
	{
		rigidbody.AddForce(new Vector3(0,300,0));
	}
}

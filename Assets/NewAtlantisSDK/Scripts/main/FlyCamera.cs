using UnityEngine;
using System.Collections;

public class FlyCamera : MonoBehaviour {

	Vector3 previousmouseposition;
	// Use this for initialization
	void Start () 
	{

	
	}
	
	// Update is called once per frame
	void Update () 
	{
		float speed = 4f;
		if (Input.GetKey (KeyCode.LeftShift)) {
						speed = 100f;
				}
		float dt = Time.deltaTime;
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			transform.position -= transform.right * speed * dt;
		}
		if (Input.GetKey(KeyCode.RightArrow))
		{
			transform.position += transform.right * speed * dt;
		}
		if (Input.GetKey(KeyCode.UpArrow))
		{
			transform.position += transform.forward * speed * dt;
		}
		if (Input.GetKey(KeyCode.DownArrow))
		{
			transform.position -= transform.forward * speed * dt;
		}

		/*if (Input.GetMouseButtonDown (0)) 
		{
			previousmouseposition = Input.mousePosition;

		}
		else if (Input.GetMouseButton(0))
		{
			Vector3 move = Input.mousePosition - previousmouseposition;
			previousmouseposition = Input.mousePosition;
			transform.Rotate(transform.up, move.x);
			transform.Rotate(transform.right, move.y*-1f);

		}
		*/
	
	}
}

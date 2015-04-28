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
		float speed = 10f;
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

		float joy_speed = 20f;
		float y = Input.GetAxis("Vertical");
		float x = Input.GetAxis("Horizontal");
		transform.position += transform.right * joy_speed * dt*x;
		transform.position += transform.forward * joy_speed * dt*y;


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



		float vx = Input.GetAxis("ViewX");
		float vy = Input.GetAxis("ViewY");

		Vector3 angles = transform.eulerAngles;
		angles.x += vy;
		angles.y += vx;
		transform.eulerAngles = angles;
	
	}
}

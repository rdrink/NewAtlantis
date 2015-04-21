using UnityEngine;
using System.Collections;

public class NAReflectionsFX : MonoBehaviour 
{

	private const int tap_count = 20;
	public float delaytime = 1f;
	float[] buffer = new float[100000];
	int readposition = 0;
	int writeposition = 0;
	
	public float feedback = 0.5f;
	public float[] reflections = new float[tap_count];
	//public int tap1 = 20000;
	// Use this for initialization
	void Start () 
	{
		for (int i=0; i<100000; ++i)
			buffer [i] = 0;
	}
	
	// Update is called once per frame
	void Update () 
	{

		//compute reflections 

		SphereDistribution d = new SphereDistribution ();
		d.CreateSpiral (tap_count);
		for (int i=0;i<tap_count;++i)
		{
			Ray ray = new Ray(gameObject.transform.position, d.positions[i]);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit))
			{
				//we hit something, we add a tap to the delay
				float distance = (hit.point-gameObject.transform.position).magnitude;
				float time = distance/340f;
				if (time < 1f)
				{
					reflections[i] = time;
				}
			}
			else
			{
				//nothing here
				reflections[i] = 0;
			}
		}

		//tap1 = (int)(delaytime * 44100f);
	}
	
	
	void OnAudioFilterRead(float[] data, int channels)
	{
		//write tap
		for (int i=0;i<data.Length;++i)
		{
			for (int t=0;t<tap_count;++t)
			{
				if (reflections[t] != 0)
				{
					int delay = (int)(reflections[t]*44100f);
					int writepos = writeposition+i+delay;
					float delay_gain = 1f;
					buffer[writepos%100000] = data[i] * delay_gain;
				}
			}

		}
		writeposition += data.Length;


		//read tap
		
		for (int i=0; i<data.Length; ++i) 
		{
			int readpos = readposition+i;
			data[i] += buffer[readpos%100000];
		}
		
		readposition += data.Length;
		
		
	}

}

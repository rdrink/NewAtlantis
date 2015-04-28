using UnityEngine;
using System.Collections;

public class HelpAmplifier : MonoBehaviour 
{

	public float Amplitude = 1f;
	public float Gain = 1f;
	public float target_gain = 1f;

	void Update()
	{
		float k = 0.99f;
		if (Amplitude > 0.01f)
		{
			target_gain = 1f/Amplitude;
		}

		Gain = Gain*k +  target_gain*(1-k); //Poor man compressor
	}


	void OnAudioFilterRead(float[] data, int channels)
	{
		//get current amplitude
		Amplitude = 0f;
		for (int i=0;i<data.Length;++i)
		{
			Amplitude += Mathf.Abs (data[i]);
		}
		Amplitude /= data.Length;
		
		for (int i=0;i<data.Length;++i)
		{
			data[i] *= Gain;
		}
		
	}

	void OnGUI()
	{
		GUI.Label (new Rect(0,Screen.height-30,200,30), "Compressor Gain=" + Gain);
	}
}

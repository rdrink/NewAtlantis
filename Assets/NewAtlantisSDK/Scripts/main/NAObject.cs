using UnityEngine;
//using UnityEditor;
using System.Collections;

public class NAObject 
{
	public string name = "";
	public Vector3 position = Vector3.zero;
	public Vector3 angles = Vector3.zero;
	public string file = "";
	public WWW www = null;
	public GameObject go = null;
	public string id = "";
	public NAObject(string _name, Vector3 _position, Vector3 _angles, string _file)
	{
		name = _name;
		position = _position;
		angles = _angles;
		file = _file;
	}

	public void Download()
	{
		www = new WWW ("http://tanant.info/newatlantis/objects/"+file);
	}
	public void DownloadLocal()
	{
		www = new WWW ("file://" + file);
	}

	public void Process()
	{
		if (www != null) 
		{
			if (www.isDone)
			{

				Object[] objs = www.assetBundle.LoadAll();
				Debug.Log ("Asset Bundle Objects count = " + objs.Length);
				foreach (Object o in objs)
				{
					Debug.Log ("Object " + o.name + " type:" + o.GetType());
					//if (o.GetType() == typeof(MonoScript))
					{
						//TextAsset script = o as TextAsset;
						//Debug.Log (script.text);
						//Debug.Log(script.text.Length);
					}

					/*
					if (o.GetType () == typeof(Camera))
					{
						Debug.LogError ("camera found");
					}
					*/
				}

				go = GameObject.Instantiate(www.assetBundle.mainAsset) as GameObject;
				go.transform.position = position;
				go.transform.eulerAngles = angles;
				www.assetBundle.Unload(false);
				www.Dispose();
				www = null;

				AudioSource[] sources = AudioSource.FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
				foreach (AudioSource s in sources)
				{
					NAReverbEffector eff = s.gameObject.GetComponent<NAReverbEffector>();
					if (eff == null)
						s.gameObject.AddComponent<NAReverbEffector>();
					AudioReverbFilter arf = s.gameObject.GetComponent<AudioReverbFilter>();
					if (arf == null)
						s.gameObject.AddComponent<AudioReverbFilter>();
					//NAReflectionsFX rfx = s.gameObject.GetComponent<NAReflectionsFX>();
					//if (rfx == null)
					//	s.gameObject.AddComponent<NAReflectionsFX>();
				}

				AudioListener al = go.GetComponent<AudioListener> ();
				if (al != null)
					al.enabled = false;

			}
		}
	}

	public string GetStatus()
	{
		if (www == null) 
		{
			return "inactive";
		}
		else
		{
			if (www.isDone)
			{
				return "done " + www.bytesDownloaded;
			}
			else
			{

				return "in progress " + www.bytesDownloaded;
			}
		}
	}
}

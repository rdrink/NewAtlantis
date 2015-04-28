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
	public int downloaded = 0;
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
		//www.threadPriority = ThreadPriority.Low;
	}
	public void DownloadLocal()
	{
		www = new WWW ("file://" + file);
	}

	public void Process()
	{
		//return;
		if (www != null) 
		{
			//return;
			//
			//return;
			if (www.isDone)
			{
				downloaded = www.bytesDownloaded;
				AssetBundle bundle = www.assetBundle;
				//Object[] objs = bundle.LoadAll();
				Object[] objs = bundle.LoadAllAssets();
				Debug.Log ("Asset Bundle Objects count = " + objs.Length);
				foreach (Object o in objs)
				{
					Debug.Log ("Object " + o.name + " type:" + o.GetType());
				}
				if (bundle.mainAsset == null)
				{
					go = new GameObject("object_root");
					go.transform.position = position;
					go.transform.eulerAngles = angles;
					foreach (Object o in objs)
					{
						if (o != null)
						{
							GameObject goChild = GameObject.Instantiate(o) as GameObject;
							if (goChild != null)
							{
								goChild.name = o.name;
								goChild.transform.parent = go.transform;
							}
						}
					}
					//Debug.LogError ("ERROR : " + www.url);
					//www.Dispose();
					//www = null;
					//return;
				}
				else
				{
					go = GameObject.Instantiate(www.assetBundle.mainAsset) as GameObject;
					go.name = www.assetBundle.mainAsset.name;
					go.transform.position = position;
					go.transform.eulerAngles = angles;
				}
				www.assetBundle.Unload(false);
				www.Dispose();
				www = null;

				AudioSource[] sources = AudioSource.FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
				//audio sources have to be augmented with specific NA behaviour
				Debug.Log ("Sources count = " + sources.Length);
				foreach (AudioSource s in sources)
				{
					NA.DecorateAudioSource(s);
					s.rolloffMode = AudioRolloffMode.Linear;
				}


				AudioListener al = go.GetComponent<AudioListener> ();
				if (al != null)
					al.enabled = false;


				/*
				NAReverbResonator[] resonators = NAReverbResonator.FindObjectsOfType(typeof(NAReverbResonator)) as NAReverbResonator[];
				
				Debug.Log ("NAReverbResonator count = " + resonators.Length);
				foreach (NAReverbResonator r in resonators)
				{
					NAReverbEffector eff = r.gameObject.GetComponent<NAReverbEffector>();
					if (eff == null)
						r.gameObject.AddComponent<NAReverbEffector>();
					AudioReverbFilter arf = r.gameObject.GetComponent<AudioReverbFilter>();
					if (arf == null)
                        r.gameObject.AddComponent<AudioReverbFilter>();
                }
                */
                
                
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

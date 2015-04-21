using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

public class App : MonoBehaviour 
{

	List<NAObject> listObjects = new List<NAObject>();
	string strSpace = "space5";
	string strFile = "file";
	NAObject currentSelection = null;
	NAObject currentLocal = null;

	WWW www = null;
	XmlDocument 		xml 		= null;
	XPathNavigator  	xpn			= null;
	XmlNamespaceManager xnm 		= null;

	Texture2D			texWhite = null;
	
	Camera mainCamera = null;
	Camera selectedCamera = null;
	bool bGUI = false;

	List<GameObject> cameras 	= new List<GameObject>();
	List<WWW> requests 			= new List<WWW>();

	GameObject goMainLight = null;
	// Use this for initialization
	void Start () 
	{
		GameObject go = GameObject.Find ("Cylinder");

		//listObjects.Add (new NAObject ("sphere", new Vector3 (5, 5, 5), Vector3.zero, "sphere.unity3d"));
		//listObjects.Add (new NAObject ("CubePD", new Vector3 (2, 2, 2), Vector3.zero, "CubePD.unity3d"));
		//listObjects.Add (new NAObject ("cubetest1", new Vector3 (0, 4, 0), Vector3.zero, "cubetest1.unity3d"));
		mainCamera = Camera.main;
		selectedCamera = mainCamera;
		cameras.Add (Camera.main.gameObject);
		texWhite = Resources.Load ("white") as Texture2D;
		goMainLight = GameObject.Find ("MainLightViewer");
	}


	void UnactivateCameras()
	{
		Camera[] cams = Camera.FindObjectsOfType (typeof(Camera)) as Camera[];
		foreach (Camera c in cams)
		{
			if (c != selectedCamera)
			{
				if (!cameras.Contains(c.gameObject))
				{
					cameras.Add (c.gameObject);
				}
				c.enabled = false;
				c.GetComponent<AudioListener>().enabled = false;
			}
		}
	}


	// Update is called once per frame
	void Update () 
	{
		foreach (NAObject o in listObjects) 
		{
			o.Process();
		}
		if (currentLocal != null)
			currentLocal.Process();

		if (www != null)
		{
			if (www.isDone)
			{
				Debug.Log (www.text);
				ParseXML(www.text);
				www.Dispose();
				www = null;
				DownloadAll();
			}
		}

		if (Input.GetKeyDown(KeyCode.Space))
		{
			RaycastHit hit;
			GameObject goPick = PickObject(Input.mousePosition, out hit);
			if (goPick != null && currentSelection != null)
			{
				currentSelection.go.transform.position = hit.point;
			}
		}
		if (Input.GetKeyDown(KeyCode.Return))
		{
			GameObject goProjectile = GameObject.CreatePrimitive(PrimitiveType.Cube);
			goProjectile.transform.position = mainCamera.transform.position;
			goProjectile.transform.localScale = Vector3.one*0.2f;
			goProjectile.AddComponent<Rigidbody>();
			goProjectile.rigidbody.AddForce(mainCamera.transform.forward*2000f);
			goProjectile.renderer.material.color = Color.blue;
		}
		UnactivateCameras ();

		foreach (WWW w in requests) 
		{
			if (w.isDone)
			{
				Debug.Log ("WWW is done : " + w.text);
				w.Dispose();
				requests.Remove(w);
			}
		}
	}


	void ParseXML(string str)
	{
		Debug.Log("parsing XML...");
		xml = new XmlDocument();
		xml.XmlResolver = null;
		try
		{
			xml.LoadXml(str);
		}
		catch (XmlException e)
		{
			//Console.WriteLine(e.Message);
		}
		xpn =  xml.CreateNavigator();
		XPathNodeIterator xpni = xpn.Select("/space");
		if (xpni == null)
		{
			Debug.Log("no space node");
			return;
		}
		xpni.MoveNext();
		if (xpni.Current == null)
			return;
		XPathNodeIterator xpnic = xpni.Current.SelectChildren(XPathNodeType.Element);
		if (xpnic == null)
			return;
		while(xpnic.MoveNext())
		{
			if(xpnic.Current.Name.Equals("object"))
			{
				Hashtable httags = null;


				string name = xpnic.Current.GetAttribute("name","");
				string filename = xpnic.Current.GetAttribute("filename","");
				string id = xpnic.Current.GetAttribute("id","");

				float x = float.Parse(xpnic.Current.GetAttribute("x",""));
				float y = float.Parse(xpnic.Current.GetAttribute("y",""));
				float z = float.Parse(xpnic.Current.GetAttribute("z",""));

				float ax = float.Parse(xpnic.Current.GetAttribute("ax",""));
				float ay = float.Parse(xpnic.Current.GetAttribute("ay",""));
				float az = float.Parse(xpnic.Current.GetAttribute("az",""));


				NAObject n = new NAObject (name, new Vector3 (x, y, z), new Vector3(ax, ay, az), filename);
				n.id = id;
				listObjects.Add(n);

				
				
				
			}
		}
	}



	void Connect(string space)
	{
		GetSpaceDescription(space);
	}



	void Disconnect()
	{
		foreach (NAObject o in listObjects) 
		{
			if (o.go != null)
			{	
				GameObject.Destroy(o.go);
				o.go = null;
			}
		}
		listObjects.Clear ();
		selectedCamera = mainCamera;
        cameras.Clear ();
        cameras.Add (mainCamera.gameObject);
        
    }
    
    void OnGUI()
    {

		GUI.color = new Color (0, 0, 0, 0.5f);
		GUI.DrawTexture (new Rect (0, 0, Screen.width, 30), texWhite);
		GUI.color = Color.white;
		GUI.Label(new Rect(0,0,400,30), "NewAtlantisNew Client - SAIC workshop");


		strSpace = GUI.TextField (new Rect (300, 0, 200, 30), strSpace);
		if (GUI.Button (new Rect(500,0, 100, 30), "Connect"))
		{
			Disconnect();
			Connect(strSpace);
			return;
		}
		if (GUI.Button (new Rect(600,0,100,30), "Disconnect"))
		{
			Disconnect();
			return;
        }

		if (GUI.Button (new Rect (700, 0, 100, 30), "pause")) 
		{
			Time.timeScale = 0f;
		}
		if (GUI.Button (new Rect (800, 0, 100, 30), "play")) 
		{
			Time.timeScale = 1f;
        }

        bGUI = GUI.Toggle (new Rect (0, 30, 100, 30), bGUI, "GUI");

		int camx = Screen.width - 100;
		int camy = 0;
		GUI.Label (new Rect(camx, camy, 100, 30), "Cameras : ");
		camy += 30;
		foreach (GameObject c in cameras)
		{
			if (selectedCamera == c.camera)
				GUI.color = Color.red;
			else
				GUI.color = Color.white;
			if (GUI.Button (new Rect(camx,camy,100,30), c.name))
			{
				selectedCamera.enabled = false;
				selectedCamera.GetComponent<AudioListener>().enabled = false;
				selectedCamera = c.camera;
				selectedCamera.enabled = true;
				selectedCamera.GetComponent<AudioListener>().enabled = true;
            }
           	camy +=30;
			GUI.color = Color.white;
        }

		//bottom toolbar 
		int bottomy = Screen.height - 30;
		GUI.color = new Color (0, 0, 0, 0.5f);
		GUI.DrawTexture (new Rect (0, bottomy, Screen.width, 30), texWhite);
		GUI.color = Color.white;
		if (GUI.Button (new Rect(0, bottomy, 100, 30), "light"))
		{
			goMainLight.SetActive(!goMainLight.activeSelf);

		}
    
		if (GUI.Button (new Rect(100, bottomy, 100, 30), "headlight"))
		{
			mainCamera.light.enabled = !mainCamera.light.enabled;
			
		}
		int lightx = 200;
		Light[] lights = Light.FindObjectsOfType (typeof(Light)) as Light[];
		foreach (Light l in lights)
		{
			if (l.name.Contains("Creature"))
				continue;
			if (l.name.Contains("Area"))
				continue;
			if (GUI.Button (new Rect(lightx, bottomy, 100, 30), l.name))
			{
				l.enabled = !l.enabled;
			}
			lightx +=100;
		}
        
        if (bGUI)
        {
            strFile = GUI.TextField (new Rect (400, 30, 200, 30), strFile);
			if (GUI.Button (new Rect(600,30, 100, 30), "load local"))
			{
				currentLocal = new NAObject ("local", new Vector3 (5, 5, 5), Vector3.zero, strFile);
				currentLocal.DownloadLocal();
			}
			/*if (GUI.Button (new Rect(0,60,100,30), "download all"))
			{
				DownloadAll();
			}
			*/

			if (GUI.Button (new Rect(300, 60, 50, 30), "x-"))
			{
				currentSelection.go.transform.position += new Vector3(-1,0,0);
			}
			if (GUI.Button (new Rect(350, 60, 50, 30), "x+"))
			{
				currentSelection.go.transform.position += new Vector3(1,0,0);
			}
			if (GUI.Button (new Rect(400, 60, 50, 30), "y-"))
			{
				currentSelection.go.transform.position += new Vector3(0,-1,0);
			}
			if (GUI.Button (new Rect(450, 60, 50, 30), "y+"))
			{
				currentSelection.go.transform.position += new Vector3(0,1,0);
			}
			if (GUI.Button (new Rect(500, 60, 50, 30), "z-"))
			{
				currentSelection.go.transform.position += new Vector3(0,0,-1);
			}
			if (GUI.Button (new Rect(550, 60, 50, 30), "z+"))
			{
				currentSelection.go.transform.position += new Vector3(0,0,1);
			}



			//état des downloads
			int y = 100;
			foreach (NAObject o in listObjects) 
			{
				if (o == currentSelection)
				{
					GUI.color = Color.red;
				}
				else
				{
					GUI.color = Color.white;
				}
				GUI.Label(new Rect(0,y,200,30), o.name);
				GUI.Label(new Rect(200,y,200,30), o.file);
				GUI.Label(new Rect(300,y,100,30), o.GetStatus());
				if (o.go != null)
				{
					if (GUI.Button (new Rect(400, y, 100, 30), "select"))
					{
						currentSelection = o;
					}
					if (GUI.Button (new Rect(500, y, 100, 30), "commit"))
					{
						o.position = o.go.transform.position;
						o.angles = o.go.transform.eulerAngles;
						SetObjectPosition(o.id, o.position.x, o.position.y, o.position.z);
					}
					if (GUI.Button (new Rect(600, y, 100, 30), "delete"))
					{
						o.position = o.go.transform.position;
						o.angles = o.go.transform.eulerAngles;
						SetObjectSpace(o.id, "trash");
						GameObject.Destroy(o.go);
						o.go = null;
                    }

					
				}

				//GUI.Label(new Rect(300,y,200,30), o.name);
				y+=30;
				GUI.color = Color.white;

			}

			int x = 0;

		}
	}

	void DownloadAll()
	{
		foreach (NAObject o in listObjects) 
		{
			o.Download();
		}
	}


	void GetSpaceDescription(string space)
	{
		string url = "http://www.tanant.info/newatlantis/getspace.php?password=qkvnhr7d3Y";
		url += "&space=" + space;
		www = new WWW (url);
	}

	void SetObjectPosition(string id, float x, float y, float z)
	{
		string url = "http://www.tanant.info/newatlantis/set.php?password=qkvnhr7d3Y&action=setposition";
		url += "&x=" + x;
		url += "&y=" + y;
		url += "&z=" + z;
		url += "&id=" + id;
		Debug.Log ("Request : " + url);
		WWW lwww = new WWW (url);
		requests.Add (lwww);
	}

	void SetObjectSpace(string id, string space)
	{
		string url = "http://www.tanant.info/newatlantis/set.php?password=qkvnhr7d3Y&action=setspace";
		url += "&space=" + space;
		url += "&id=" + id;
		Debug.Log ("Request : " + url);
		WWW lwww = new WWW (url);
        requests.Add (lwww);
    }

	public GameObject PickObject(Vector2 screenpos, out RaycastHit hit)
	{
		Vector3 v = screenpos;
		//RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(v);
		if (Physics.Raycast(ray, out hit))
		{
			return hit.collider.gameObject;
        }
        return null;
    }





}

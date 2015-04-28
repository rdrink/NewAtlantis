using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using NewAtlantis;

public class App : MonoBehaviour 
{

	List<NAObject> listObjects = new List<NAObject>();
	string strSpace = "main-level";
	string strFile = "file";
	NAObject currentSelection = null;
	NAObject currentLocal = null;
	public Object goPrefabCube; 
	public Object goPrefabAvatar; 
	string[] spaces = {"aix_workshop", "aix1", "aix2", "main-level", "space5", "jonathan"};
	WWW www = null;
	XmlDocument 		xml 		= null;
	XPathNavigator  	xpn			= null;
	XmlNamespaceManager xnm 		= null;
	Texture2D			texWhite 	= null;
	Camera 				mainCamera 	= null;
	Camera 				selectedCamera = null;
	bool	 			bGUI 		= false;

	List<GameObject> 	cameras 	= new List<GameObject>();
	List<WWW> 			requests 	= new List<WWW>();

	//"tools"
	private bool				bPushObjects = false;

	private bool				bStartPopup = true;

	private string 				strIP = "88.178.228.172";

	GameObject goMainLight = null;
	GameObject goAvatar = null;

	bool bChat 		= false;
	bool bNetwork 	= false;
	bool bCameras	= false;
	bool bLights	= false;
	bool bSpace		= true;
	bool bOptions	= false;
	bool bAbout		= true;

	//CHAT
	string strCurrentChatMessage = "";
	string strName = "noname";

	Rect mGuiWinRectChat 		= new Rect(Screen.width/2-200, Screen.height/2-200, 400, 400);
	Rect mGuiWinRectNetwork 	= new Rect(Screen.width/2-200, Screen.height/2-200, 400, 400);
	Rect mGuiWinRectCameras 	= new Rect(0, Screen.height/2-200, 200, 400);
	Rect mGuiWinRectLights 		= new Rect(400, Screen.height/2-200, 200, 400);
	Rect mGuiWinRectSpace 		= new Rect(150, Screen.height/2-200, 200, 400);
	Rect mGuiWinRectOptions 	= new Rect(600, Screen.height/2-200, 200, 400);
	Rect mGuiWinRectAbout 		= new Rect(800, Screen.height/2-200, 200, 400);
	
	//options


	// Use this for initialization
	void Start () 
	{


		//Input.GetJoystickNames(
		GameObject go = GameObject.Find ("Cylinder");

		//listObjects.Add (new NAObject ("sphere", new Vector3 (5, 5, 5), Vector3.zero, "sphere.unity3d"));
		//listObjects.Add (new NAObject ("CubePD", new Vector3 (2, 2, 2), Vector3.zero, "CubePD.unity3d"));
		//listObjects.Add (new NAObject ("cubetest1", new Vector3 (0, 4, 0), Vector3.zero, "cubetest1.unity3d"));
		mainCamera = Camera.main;
		selectedCamera = mainCamera;
		NA.listener = mainCamera.GetComponent<AudioListener>();
		cameras.Add (Camera.main.gameObject);
		texWhite = Resources.Load ("white") as Texture2D;
		goMainLight = GameObject.Find ("MainLightViewer");

		ChatManager.Log("system", "welcome to New Atlantis", 0);
		//Connect(strSpace);
	}


	void CreateNetworkAvatar()
	{
		if (Network.isServer || Network.isClient)
		{
			goAvatar = Network.Instantiate(goPrefabAvatar, Vector3.zero, Quaternion.identity, 0) as GameObject;
		}
		else
		{
			//no need

		}
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
		AudioListener[] listeners = AudioListener.FindObjectsOfType (typeof(AudioListener)) as AudioListener[];
		foreach (AudioListener al in listeners)
		{
			if (al.gameObject.GetComponent<Camera>() != selectedCamera)
			{
				al.enabled = false;
			}
		}

	}


	// Update is called once per frame
	void Update () 
	{
		if (goAvatar)
		{
			goAvatar.transform.position = transform.position;
			goAvatar.transform.eulerAngles = transform.eulerAngles;
		}
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
				DownloadAll(); //we download all the objects
			}
		}




		if (Input.GetKeyDown(KeyCode.P))
		{
			RaycastHit hit;
			GameObject goPick = PickObject(Input.mousePosition, out hit);
			if (goPick != null && currentSelection != null)
			{
				currentSelection.go.transform.position = hit.point;
			}
		}
		if (Input.GetKeyDown(KeyCode.S) || Input.GetButtonDown("Fire2"))
		{
				GameObject goProjectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				goProjectile.transform.position = selectedCamera.transform.position;
				goProjectile.transform.localScale = Vector3.one*1f;
				goProjectile.AddComponent<Rigidbody>();
				goProjectile.GetComponent<Rigidbody>().AddForce(selectedCamera.transform.forward*2000f);
				goProjectile.GetComponent<Renderer>().material.color = Color.blue;
				goProjectile.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
				goProjectile.GetComponent<Rigidbody>().mass = 0.1f;
				goProjectile.GetComponent<Rigidbody>().drag = 0f;
				goProjectile.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
				AudioSource src = goProjectile.AddComponent<AudioSource>();
				src.playOnAwake = false;
				//src.rolloffMode = AudioRolloffMode.Linear;
				src.clip = Resources.Load ("CLANG") as AudioClip;
				goProjectile.AddComponent<NAPlayOnCollide>();
				goProjectile.AddComponent<NAAudioSource>();
				NAPlayOnKeyPressed kp = goProjectile.AddComponent<NAPlayOnKeyPressed>();
				kp.key = KeyCode.A;
				NA.DecorateAudioSource(src);
		}

		if (Input.GetKeyDown(KeyCode.N))
		{
			CreateCube();

		}

		//if (Input.GetButtonDown("Fire1"))
		{
			
		}
		if (Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("Fire1"))
		{
			if (Network.isServer || Network.isClient)
			{
				GameObject goProjectile = Network.Instantiate(goPrefabCube, Vector3.zero, Quaternion.identity, 0) as GameObject;
				goProjectile.transform.position = selectedCamera.transform.position;
				goProjectile.transform.localScale = Vector3.one*1f;
				goProjectile.GetComponent<Rigidbody>().AddForce(selectedCamera.transform.forward*500f);
			}
			else
			{
				GameObject goProjectile = GameObject.CreatePrimitive(PrimitiveType.Cube);
				goProjectile.transform.position = selectedCamera.transform.position;
				goProjectile.transform.localScale = Vector3.one*1f;
				goProjectile.AddComponent<Rigidbody>();
				goProjectile.GetComponent<Rigidbody>().AddForce(selectedCamera.transform.forward*1000f);
				goProjectile.GetComponent<Renderer>().material.color = Color.blue;
				goProjectile.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
				//goProjectile.rigidbody.mass = 0.1f;
				//goProjectile.rigidbody.drag = 0.05f;
				AudioSource src = goProjectile.AddComponent<AudioSource>();
				src.playOnAwake = false;
				src.clip = Resources.Load ("CLANG") as AudioClip;
                goProjectile.AddComponent<NAPlayOnCollide>();
                goProjectile.AddComponent<NAAudioSource>();
                NAPlayOnKeyPressed kp = goProjectile.AddComponent<NAPlayOnKeyPressed>();
                kp.key = KeyCode.A;
				//src.rolloffMode = AudioRolloffMode.Linear;
                NA.DecorateAudioSource(src);
            }
        }
		if (Input.GetKeyDown(KeyCode.T) || Input.GetButtonDown("Fire3"))
		{
			GameObject goTrunk = GameObject.CreatePrimitive(PrimitiveType.Cube);
			goTrunk.transform.position = selectedCamera.transform.position;
			goTrunk.transform.localScale = new Vector3(1f,0.4f, 0.6f);
			goTrunk.AddComponent<Rigidbody>();
			goTrunk.GetComponent<Rigidbody>().AddForce(selectedCamera.transform.forward*1000f);
			goTrunk.GetComponent<Renderer>().material.color = Color.red;
			goTrunk.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
			goTrunk.GetComponent<Rigidbody>().mass = 2f;
			goTrunk.GetComponent<Rigidbody>().drag = 0f;
			AudioSource src = goTrunk.AddComponent<AudioSource>();
			src.playOnAwake = false;
			//src.rolloffMode = AudioRolloffMode.Linear;
			//src.clip = Resources.Load ("CLANG") as AudioClip;
			goTrunk.AddComponent<NAPlayOnCollide>();
			goTrunk.AddComponent<NAAudioRecorder>();

			NA.DecorateAudioSource(src);
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

		//force add
		if (Input.GetMouseButton(0) && bPushObjects)
		{
			RaycastHit hit;
			GameObject goPick = PickObject(Input.mousePosition, out hit);
			if (goPick != null)
			{
				Vector3 force = goPick.transform.position - selectedCamera.transform.position;
				force.Normalize();
				if (goPick.GetComponent<Rigidbody>() != null)
				{
					goPick.GetComponent<Rigidbody>().AddForce(force*500f);
				}
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

				if (filename.Contains ("2.9"))
					continue;
				NAObject n = new NAObject (name, new Vector3 (x, y, z), new Vector3(ax, ay, az), filename);
				n.id = id;

				listObjects.Add(n);

				
				
				
			}
		}
	}



	void Connect(string space)
	{

		Disconnect();
		GetSpaceDescription(space);
		bStartPopup = false;
	}



	void Disconnect()
	{
		bStartPopup = true;
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
		//GUI.Label(new Rect(0,0,400,30), "NewAtlantisNew Client - SAIC workshop");
		GUI.Label(new Rect(0,0,400,30), "New Atlantis Client v0.31");


		/*strSpace = GUI.TextField (new Rect (300, 0, 200, 30), strSpace);
		if (GUI.Button (new Rect(500,0, 100, 30), "Connect"))
		{
			Connect(strSpace);
			return;
		}
		if (GUI.Button (new Rect(600,0,100,30), "Disconnect"))
		{
			Disconnect();
			return;
        }

*/

		/*if (GUI.Button (new Rect (700, 0, 50, 30), "pause")) 
		{
			Time.timeScale = 0f;
		}
		if (GUI.Button (new Rect (750, 0, 50, 30), "play")) 
		{
			Time.timeScale = 1f;
        }*/

		/*
		GUI.Label (new Rect(0, 30, 200, 30), "ip=" + Network.player.ipAddress);
		strIP = GUI.TextField(new Rect(100, 30, 100, 30), strIP);
		if (GUI.Button (new Rect (200, 30, 100, 30), "start server")) 
		{
			Network.InitializeServer(32, 25002, false);
			CreateNetworkAvatar();
		}

		if (GUI.Button (new Rect (300, 30, 100, 30), "connect to server")) 
		{
			Network.Connect(strIP, 25002);
			//CreateNetworkAvatar();
		}

		*/






		if (bStartPopup)
		{
			float popupx = Screen.width/2-200;
			float popupy = Screen.height/2-200;
			GUI.Box (new Rect(popupx, popupy, 400, 400), "Welcome to the New Atlantis");
			popupy+=30;
			foreach (string space in spaces)
			{
				if (GUI.Button(new Rect(popupx, popupy, 400, 30), space))
				{
					strSpace = space;
					Connect(space);

				}
				popupy+=30;
			}
			popupy+=30;
			if (GUI.Button(new Rect(popupx, popupy, 400, 30), "close"))
			{
				bStartPopup = false;
			}



		}



		//toolbar


        bGUI = GUI.Toggle (new Rect (0, 60, 100, 30), bGUI, "GUI");

		/*
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
			string name = c.name;
			if (c.gameObject.transform.parent != null)
				name = c.gameObject.transform.parent.gameObject.name;
			if (GUI.Button (new Rect(camx,camy,100,30), name))
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
		*/
		//bottom toolbar 
		int bottomy = Screen.height - 30;
		GUI.color = new Color (0, 0, 0, 0.5f);
		GUI.DrawTexture (new Rect (0, bottomy, Screen.width, 30), texWhite);
		GUI.color = Color.white;
		//GUI.color = goMainLight.activeSelf ? Color.red : Color.white;
		/*if (GUI.Button (new Rect(0, bottomy, 100, 30), "light"))
		{
			goMainLight.SetActive(!goMainLight.activeSelf);
		}
		GUI.color = mainCamera.light.enabled ? Color.red : Color.white;
		if (GUI.Button (new Rect(100, bottomy, 100, 30), "headlight"))
		{
			mainCamera.light.enabled = !mainCamera.light.enabled;
		}
		*/

		//right toolbar
		/*GUI.color = new Color (0, 0, 0, 0.5f);
		GUI.DrawTexture (new Rect (Screen.width-100, 0, 100, Screen.height), texWhite);
		GUI.color = Color.white;
		int lightx = Screen.width-100;
		int lighty = Screen.height/2;
		GUI.Label(new Rect(lightx, lighty, 200, 30), "Lights");
		lighty += 30;
		Light[] lights = Light.FindObjectsOfType (typeof(Light)) as Light[];
		foreach (Light l in lights)
		{
			GUI.color = l.enabled ? Color.red : Color.white;
			if (l.name.Contains("Creature"))
				continue;
			if (l.name.Contains("Area"))
				continue;
			if (GUI.Button (new Rect(lightx, lighty, 100, 30), l.name))
			{
				l.enabled = !l.enabled;
			}
			//lightx +=100;
			lighty += 30;
		}
		*/













		GUI.color = bPushObjects?Color.red:Color.white;

		/*if (GUI.Button (new Rect(0, Screen.height-30, 100, 30), "push objects"))
		{
			bPushObjects = !bPushObjects;
		}

		if (GUI.Button (new Rect(100, Screen.height-30, 100, 30), "augment water"))
		{
			VerySpecialCase();
		}
		*/

		GUI.color = bChat?Color.red:Color.white;
		if (GUI.Button (new Rect(200, 0, 100, 30), "chat"))
		{
			bChat = !bChat;
        }

		GUI.color = bNetwork?Color.red:Color.white;
		if (GUI.Button (new Rect(300, 0, 100, 30), "network"))
		{
			bNetwork = !bNetwork;
        }

		GUI.color = bCameras?Color.red:Color.white;
		if (GUI.Button (new Rect(400, 0, 100, 30), "cameras"))
		{
			bCameras = !bCameras;
        }
		GUI.color = bLights?Color.red:Color.white;
		if (GUI.Button (new Rect(500, 0, 100, 30), "lights"))
		{
			bLights = !bLights;
        }

		GUI.color = bSpace?Color.red:Color.white;
		if (GUI.Button (new Rect(600, 0, 100, 30), "space"))
		{
			bSpace = !bSpace;
        }
		GUI.color = bOptions?Color.red:Color.white;
		if (GUI.Button (new Rect(700, 0, 100, 30), "options"))
		{
			bOptions = !bOptions;
        }
        GUI.color = bAbout?Color.red:Color.white;
		if (GUI.Button (new Rect(800, 0, 100, 30), "about"))
		{
			bAbout = !bAbout;
        }
        
        //to do : list of objects ?
        
        GUI.color = Color.white;

        if (bGUI)
        {
            strFile = GUI.TextField (new Rect (400, 30, 200, 30), strFile);
			/*if (GUI.Button (new Rect(600,30, 100, 30), "load local"))
			{
				currentLocal = new NAObject ("local", new Vector3 (5, 5, 5), Vector3.zero, strFile);
				currentLocal.DownloadLocal();
			}
			*/
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

			if (GUI.Button (new Rect(600, 60, 50, 30), "ry+"))
			{
				currentSelection.go.transform.eulerAngles += new Vector3(0,10,0);
			}
			if (GUI.Button (new Rect(650, 60, 50, 30), "ry-"))
			{
				currentSelection.go.transform.eulerAngles += new Vector3(0,-10,0);
			}




			//downloads state
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
				if (o.www != null)
					GUI.Label (new Rect(400, y, 100, 30), "downloading");
				else
					GUI.Label (new Rect(400, y, 100, 30), ""+o.downloaded/1000 + " KB");
				//GUI.Label(new Rect(400,y,100,30), o.GetStatus());
				if (o.go != null)
				{
					if (GUI.Button (new Rect(500, y, 50, 30), "select"))
					{
						if (currentSelection == o)
							currentSelection = null;
						else
							currentSelection = o;
					}
					if (GUI.Button (new Rect(550, y, 50, 30), "save"))
					{
						o.position = o.go.transform.position;
						o.angles = o.go.transform.eulerAngles;
						SetObjectPosition(o.id, o.position.x, o.position.y, o.position.z);
					}
					if (GUI.Button (new Rect(600, y, 50, 30), "delete"))
					{
						o.position = o.go.transform.position;
						o.angles = o.go.transform.eulerAngles;
						SetObjectSpace(o.id, "trash"); //move to trash
						GameObject.Destroy(o.go);
						o.go = null;
                    }
					/*if (GUI.Button (new Rect(700, y, 100, 30), "grab"))
                    {	
						o.go.transform.parent = selectedCamera.transform;
						o.go.transform.localPosition = new Vector3(0,0,1);
					}
					*/
				}

				//GUI.Label(new Rect(300,y,200,30), o.name);
				y+=30;
				GUI.color = Color.white;

			}

			int x = 0;

		}


		if (bChat)
			mGuiWinRectChat = GUI.Window(1, mGuiWinRectChat, WindowFunctionChat, "CHAT");

		if (bNetwork)
			mGuiWinRectNetwork = GUI.Window(2, mGuiWinRectNetwork, WindowFunctionNetwork, "NETWORK");

		if (bCameras)
			mGuiWinRectCameras = GUI.Window(3, mGuiWinRectCameras, WindowFunctionCameras, "CAMERAS");
		if (bLights)
			mGuiWinRectLights = GUI.Window(4, mGuiWinRectLights, WindowFunctionLights, "LIGHTS");
		if (bSpace)
			mGuiWinRectSpace = GUI.Window(5, mGuiWinRectSpace, WindowFunctionSpace, "SPACES");
		if (bOptions)
			mGuiWinRectOptions = GUI.Window(6, mGuiWinRectOptions, WindowFunctionOptions, "OPTIONS");
		if (bAbout)
			mGuiWinRectAbout = GUI.Window(7, mGuiWinRectAbout, WindowFunctionAbout, "ABOUT");
	}

	void DownloadAll()
	{
		foreach (NAObject o in listObjects) 
		{
			o.Download();
		}
	}


	//get the XML description of a given space name
	void GetSpaceDescription(string space)
	{
		//string url = "http://www.tanant.info/newatlantis/getspace.php?password=qkvnhr7d3Y";
		string url = Settings.URLWebServer + "getspace.php?password=qkvnhr7d3Y";
		url += "&space=" + space;
		www = new WWW (url);
	}

	//set an object position
	void SetObjectPosition(string id, float x, float y, float z)
	{
		//string url = "http://www.tanant.info/newatlantis/set.php?password=qkvnhr7d3Y&action=setposition";
		string url = Settings.URLWebServer + "set.php?password=qkvnhr7d3Y&action=setposition";
		url += "&x=" + x;
		url += "&y=" + y;
		url += "&z=" + z;
		url += "&id=" + id;
		Debug.Log ("Request : " + url);
		WWW lwww = new WWW (url);
		requests.Add (lwww);
	}

	//move an object to a given space name
	void SetObjectSpace(string id, string space)
	{
		//string url = "http://www.tanant.info/newatlantis/set.php?password=qkvnhr7d3Y&action=setspace";
		string url = Settings.URLWebServer + "set.php?password=qkvnhr7d3Y&action=setspace";
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


	public void VerySpecialCase()
	{
		GameObject go = GameObject.Find ("Daylight Water");
		if (go)
		{
			go.AddComponent<AudioSource>();
			go.GetComponent<AudioSource>().clip = Resources.Load ("splash") as AudioClip;
			MeshCollider collider = go.AddComponent<MeshCollider>();
			collider.isTrigger = true;
			go.AddComponent<NAPlayOnTrigger>();
		}
	}

	void OnConnectedToServer() 
	{
		Debug.Log("Connected to server");
		CreateNetworkAvatar();
	}

	void OnPlayerConnected(NetworkPlayer player) 
	{
		Debug.Log("Player connected from " + player.ipAddress + ":" + player.port);
		ChatManager.Log("system", "player connected", 0);
	}

	void CreateCube()
	{
		NetworkViewID viewID = Network.AllocateViewID();
		GetComponent<NetworkView>().RPC("SpawnBox", RPCMode.AllBuffered, viewID, transform.position);
	}

	void NetworkConnectToSpace(string _space)
	{
		GetComponent<NetworkView>().RPC("ConnectToSpace", RPCMode.AllBuffered, _space);
	}

	void NetworkChat(string _message)
	{
		GetComponent<NetworkView>().RPC("Chat", RPCMode.AllBuffered, strName, _message);
	}
	



	[RPC]
	void Chat(string _name, string _message) 
	{
		ChatManager.Log(_name, _message, 0);
	}

	[RPC]
	void SpawnBox(NetworkViewID viewID, Vector3 location) 
	{
		//Transform clone;
		GameObject clone;
		//clone = Instantiate(cubePrefab, location, Quaternion.identity) as Transform as Transform;
		clone = GameObject.CreatePrimitive(PrimitiveType.Cube);
		NetworkView nView = clone.AddComponent<NetworkView>();
		//NetworkView nView;
		//nView = clone.GetComponent<NetworkView>();
		nView.viewID = viewID;

		if (Network.isServer)
		{
			clone.AddComponent<Rigidbody>();
		}
	}

	[RPC]
	void ConnectToSpace(string _space) 
	{
		Connect(_space);
	}

	void WindowFunctionChat (int windowID)
	{
		GUI.color = Color.white;
		//GUI.color = new Color(0.7f,0.7f,1f);

		int maxcount = 12;
		int start = ChatManager.GetStart(maxcount);
		int end = ChatManager.GetEnd();
		for (int i=start;i<=end;++i)
		{
			GUILayout.BeginHorizontal();
			ChatEntry e = ChatManager.logs[i];
			GUILayout.Label ("[" + e.name + "] : " + e.str);
			GUILayout.EndHorizontal();
		}

		int diff = maxcount-(end-start);
		for (int i=0;i<diff;++i)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label ("");
			GUILayout.EndHorizontal();
		}

		GUILayout.BeginHorizontal();
		strName = GUILayout.TextArea(strName, GUILayout.Width(100));
		strCurrentChatMessage = GUILayout.TextArea(strCurrentChatMessage);
		if (GUILayout.Button("send", GUILayout.Width(100)))
		{
			NetworkChat(strCurrentChatMessage);
			strCurrentChatMessage = "";
		}

		GUILayout.EndHorizontal();


        
        GUI.DragWindow();
    }

	void WindowFunctionNetwork (int windowID)
	{
		GUI.color = Color.white;

		GUILayout.BeginHorizontal();
		GUILayout.Label ("This machine ip : " + Network.player.ipAddress + "(" + Network.player.externalIP + ")");// + " " + Network.player.externalIP);
		if (Network.isServer)
			GUILayout.Label ("[SERVER STARTED]");
		else if (Network.isClient)
			GUILayout.Label ("[CLIENT CONNECTED]");


		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();

		if (GUILayout.Button ("start server at " + Network.player.ipAddress)) 
		{
			MasterServer.RegisterHost("NewAtlantis", "New Atlantis test", "comment");
			Network.InitializeServer(32, 7890, true);
			CreateNetworkAvatar();
			NetworkConnectToSpace(strSpace);
        }
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		strIP = GUILayout.TextField(strIP);

		
		if (GUILayout.Button ("connect to " + strIP)) 
		{
            Network.Connect(strIP, 7890);
        }
		GUILayout.EndHorizontal();

		//GUI.color = new Color(0.7f,0.7f,1f);


		foreach (NetworkPlayer player in Network.connections)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label ("Player="+player.guid + " ip="+player.ipAddress + " port=" + player.port + " ping=" + Network.GetAveragePing(player) + "ms");
			GUILayout.EndHorizontal();
		}

        
       
        
        
        
        GUI.DragWindow();
    }
    
    
	void WindowFunctionCameras (int windowID)
	{
		GUI.color = Color.white;

		foreach (GameObject c in cameras)
		{
			if (selectedCamera == c.GetComponent<Camera>())
				GUI.color = Color.red;
			else
				GUI.color = Color.white;
			string name = c.name;
			if (c.gameObject.transform.parent != null)
				name = c.gameObject.transform.parent.gameObject.name;
			GUILayout.BeginHorizontal();
			if (GUILayout.Button (name))
			{
				selectedCamera.enabled = false;
				selectedCamera.GetComponent<AudioListener>().enabled = false;
				selectedCamera = c.GetComponent<Camera>();
				selectedCamera.enabled = true;
                selectedCamera.GetComponent<AudioListener>().enabled = true;
            }
			GUILayout.EndHorizontal();
            GUI.color = Color.white;
        }
        
      
        
        GUI.DragWindow();
    }

	void WindowFunctionLights (int windowID)
	{
		GUI.color = Color.white;
		
		Light[] lights = Light.FindObjectsOfType (typeof(Light)) as Light[];
		foreach (Light l in lights)
		{
			GUI.color = l.enabled ? Color.red : Color.white;
			if (l.name.Contains("Creature"))
				continue;
			if (l.name.Contains("Area"))
				continue;
			GUILayout.BeginHorizontal();
			if (GUILayout.Button (l.name))
            {
                l.enabled = !l.enabled;
            }
			GUILayout.EndHorizontal();
        }
        
        
        
        GUI.DragWindow();
    }

	void WindowFunctionSpace (int windowID)
	{
		GUI.color = Color.white;
		GUILayout.BeginHorizontal();
		strSpace = GUILayout.TextField (strSpace);
		if (GUILayout.Button ("Connect", GUILayout.Width(100)))
		{
			Connect(strSpace);
			return;
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if (GUILayout.Button ("Disconnect"))
        {
            Disconnect();
            return;
        }
		GUILayout.EndHorizontal();

		//HERE
		foreach (NAObject o in listObjects) 
		{
			/*if (o == currentSelection)
			{
				GUI.color = Color.red;
			}
			else
			{
				GUI.color = Color.white;
			}
			*/
			GUILayout.BeginHorizontal();
			GUILayout.Label(o.name, GUILayout.Width(100));
			//GUI.Label(new Rect(200,y,200,30), o.file);
			GUILayout.Label (""+o.downloaded/1000 + " KB");
			/*if (GUILayout.Button("download"))
			{
				o.Download();
			}*/
			GUILayout.EndHorizontal();
		}
            
        GUI.DragWindow();
    }

	void WindowFunctionAbout (int windowID)
	{
		GUI.color = Color.white;
		GUILayout.BeginHorizontal();
		GUILayout.TextArea("New Atlantis is a shared (multi-user) online virtual world dedicated to audio experimentation and practice. Unlike most online worlds where image is the primary concern, in New Atlantis sound comes first.");
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Mouse drag : look");
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Arrow keys : move");
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Space : clap");
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Return : Throw cube");
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("T : Create trunk");
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("S : Create sphere");
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("P : Put selection at mouse pos");
		GUILayout.EndHorizontal();
        GUI.DragWindow();
    }

	void WindowFunctionOptions (int windowID)
	{
		GUI.color = Color.white;

		GUI.color = NAAudioSource.bDisplayAudioSourceName ? Color.red : Color.white;
		GUILayout.BeginHorizontal();
		if (GUILayout.Button ("Display AudioSource names"))
		{
			NAAudioSource.bDisplayAudioSourceName = !NAAudioSource.bDisplayAudioSourceName;
		}
		GUILayout.EndHorizontal();

		GUI.color = NA.bAugmentAudioSources ? Color.red : Color.white;
		GUILayout.BeginHorizontal();
		if (GUILayout.Button ("Augment AudioSources"))
		{
			NA.bAugmentAudioSources = !NA.bAugmentAudioSources;
		}
		GUILayout.EndHorizontal();

		GUI.color = bPushObjects ? Color.red : Color.white;
		GUILayout.BeginHorizontal();
		if (GUILayout.Button ("push objects on click"))
		{
			bPushObjects = !bPushObjects;
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("master vol", GUILayout.Width(100));
		AudioListener.volume = GUILayout.HorizontalSlider(AudioListener.volume, 0, 5);
		GUILayout.EndHorizontal();

		GUI.DragWindow();
    }
    
    //
    
    
    
}

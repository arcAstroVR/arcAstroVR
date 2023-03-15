/*
 * Base Program : StelController
 * Part of the Stellarium-Unity bridge tools (c) 2017 by Georg Zotti and John Fillwalk.

 * Improvement program : aAV_StelController
 * Reorganize StelController for arcAstroVR. (c) 2021 by K.Iwashiro.
 */
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Defective.JSON;

[RequireComponent(requiredComponent: typeof(aAV_StreamingSkybox))]
public class aAV_StelController : MonoBehaviour {

	// These can be used for the planet field in the locations.
	public enum Planet { Sun, Mercury, Venus, Earth, Moon, Mars, Jupiter, Saturn, Uranus, Neptune, Pluto, Io, Europa, Ganymede, Callisto }
	public enum Bortle { Excellent=1, TrulyDark, Rural, RuralSuburbanTransition, Suburban, BrightSuburban, SuburbanUrbanTransition, City, InnerCity }

	[Tooltip("True if an instance of Stellarium is assumed running in the background, creating skyboxes or spouting the scene background.")]
	public bool connectToStellarium = true; 
	public int stelPort = 8090; // IP port of running Stellarium RemoteControl instance on localhost.
	[Tooltip("With Stellarium 0.18.1 and later, this should be the included 'skybox.ssc'")]
	public string skyboxScriptName = "skybox.ssc"; 
	[Tooltip("Location name to send to Stellarium")]
	public string locationname="Unity3D";
	[Tooltip("Country name to send to Stellarium")]
	public string country="UnityLand";
	[Tooltip("Location planet to send to Stellarium")]
	public aAV_StelController.Planet planet= aAV_StelController.Planet.Earth;
	[Tooltip("Location longitude to send to Stellarium [degrees, positive towards east]")]
	public float longitude=16.25f;
	[Tooltip("Location latitude to send to Stellarium [degrees, positive towards north]")]
	public float latitude=48.2f;
	[Tooltip("Location altitude (MAMSL) to send to Stellarium [m]")]
	public float altitude=280.0f;
	[Tooltip("Atmosphere pressure to send to Stellarium for refraction computation [deg. C]")]
	public float atmosphereTemperature = 10.0f;	 
	[Tooltip("Atmosphere pressure to send to Stellarium for refraction computation [mbar]")]
	public float atmospherePressure	= 1013.0f;   
	[Tooltip("Extinction factor (atmosphere haze) to send to Stellarium, [mag/airmass]")]
	public float atmosphereExtinctionFactor = 0.2f; 
	[Tooltip("Sky Quality (Bortle index) to send to Stellarium")]
	public Bortle lightPollutionBortleIndex = Bortle.TrulyDark;
	[Tooltip("Rotation angle to compensate for meridian convergence offset of grid-based coordinates. This is the azimuth of True North in terms of the local grid coordinate system.")]
	public float northAngle = 270.0f;

	[Tooltip("A GameObject usually in a Canvas whose Text component gets updated with new timestring if needed.")]
	public GameObject guiTimeText;

	[Tooltip("Enable Spout at startup? If false, we should disable some communication like view direction changes, FoV changes.")]

	private JSONObject json;			// state tracking object. 
	private int actionId = -1111;			// required for communication with Stellarium
	private int propertyId = -2222;		// required for communication with Stellarium
	public JSONObject jsonActions;
	private JSONObject jsonProperties;
	private JSONObject jsonTime;
	private JSONObject jsonObjInfo;
	private JSONObject jsonSunInfo;
	private JSONObject jsonMoonInfo;
	private JSONObject jsonVenusInfo;

	private bool flagQueryObjectInfo=false;
	private bool flagSetViewDirection=false;
	private GameObject stelBackground;
	private aAV_StreamingSkybox streamingSkybox;
	private aAV_CustomDateTime customDateTime;
	private aAV_GIS gis;
	private aAV_icon toggle;
	void Awake()
	{
		// GameObjectおよびコンポーネントの読み込み
		streamingSkybox = gameObject.GetComponent<aAV_StreamingSkybox>();
		gis = GameObject.Find("Main").GetComponent<aAV_GIS>();
		toggle = GameObject.Find("Main").transform.Find( "Menu/ToggleSwitch" ).gameObject.GetComponent<aAV_icon>();
		customDateTime = GetComponent<aAV_CustomDateTime>();
	}

	void Start()
	{ 
		StartCoroutine(InitializeStelJson());
	}

	void Update()
	{
		if (!jsonTime)
		{
			StartCoroutine(UpdateStelJson());
		}
	}

	public bool JsonIsValid()
	{
		return (json != null);
	}

	private IEnumerator InitializeStelJson()
	{
		if (connectToStellarium)
		{
			string url = "http://localhost:" + stelPort + "/api/main/status?propId=-2&actionId=-2";
			using (UnityWebRequest uwr = UnityWebRequest.Get(url))
			{
				uwr.chunkedTransfer = false;
				yield return uwr.SendWebRequest();
	
				if (uwr.isNetworkError)
				{
					Debug.LogWarning("StelController.InitializeStelJson() failed." + uwr.error + "; Continue without connection.");
					connectToStellarium = false;
					yield break;
				}
				else if (uwr.isHttpError)
				{
					Debug.LogWarning("StelController.InitializeStelJson(): Problem with answer from Stellarium: " + uwr.responseCode + "; Continue without connection.");
					connectToStellarium = false;
					yield break;
				}
	
				// Parse JSON answer
				json = new JSONObject(uwr.downloadHandler.text);
				jsonActions = json.GetField("actionChanges");
				jsonActions.GetField(ref actionId, "id");
				jsonProperties = json.GetField("propertyChanges");
				jsonProperties.GetField(ref propertyId, "id");
			}
			
			//Stellariumが地上表示"actionShow_Ground"ONの時は非表示にする
			var jsonChenges = jsonActions.GetField("changes");
			if (jsonChenges.GetField("actionShow_Ground").boolValue){
				StartCoroutine(DoAction("actionShow_Ground"));
			}

			//地点情報をStellariumに設定
			if(aAV_Public.basicInfo.type == "WG"){
				longitude = (float)aAV_Public.basicInfo.center_E;
				latitude = (float)aAV_Public.basicInfo.center_N;
			}else if(aAV_Public.basicInfo.type == "JP"){
				double[] EN = gis.JP2LonLat(aAV_Public.basicInfo.center_E, aAV_Public.basicInfo.center_N, aAV_Public.basicInfo.zone);
				longitude = (float)EN[0];
				latitude = (float)EN[1];
			}else if(aAV_Public.basicInfo.type == "UT"){
				double[] EN = gis.UTM2LonLat(aAV_Public.basicInfo.center_E, aAV_Public.basicInfo.center_N, aAV_Public.basicInfo.zone);
				longitude = (float)EN[0];
				latitude = (float)EN[1];
			}
			altitude = (int)aAV_Public.basicInfo.center_H;
			locationname = aAV_Public.basicInfo.location;
			country = aAV_Public.basicInfo.country;
			StartCoroutine(SetLocation(latitude, longitude, altitude, locationname, country, planet));
			
			//パブリック変数の設定
			int year = (int)Math.Abs(aAV_Public.basicInfo.year);
			if (aAV_Public.basicInfo.year<=0){	//AD表記をBC表記に変換
				year = (int)Math.Abs(aAV_Public.basicInfo.year -1);
			}
			int month = aAV_Public.basicInfo.month;
			int day = aAV_Public.basicInfo.day;
			int hour = aAV_Public.basicInfo.hour;
			int minute = aAV_Public.basicInfo.minute;
			int second = aAV_Public.basicInfo.second;
			double tz = 0;
			if(aAV_Public.basicInfo.timezone != null){
				if(!aAV_Public.basicInfo.timezone.Contains(":")){
					try {
						tz = double.Parse(aAV_Public.basicInfo.timezone);
					}catch{
						tz = 0;
					}
				}else{
					string[] timezone = aAV_Public.basicInfo.timezone.Split(':');
					tz = double.Parse(timezone[0])+Math.Sign(double.Parse(timezone[0]))*double.Parse(timezone[1])/60;
				}
			}
			Debug.Log("startTime="+year+'/'+month+'/'+day+' '+hour+':'+minute+':'+second);
			customDateTime.SetDateTime(year, month, day, hour, minute, second, (aAV_CustomDateTime.Era)(year<0?0:1));
			double startJD = customDateTime.ToJulianDay();
			StartCoroutine(SetJD(startJD -tz/24+0.5/100000));
			
			//icon設定
			toggle.initializeToggle();
		}
	}


	private IEnumerator UpdateStelJson()
	{
		if (connectToStellarium) // Esp time updates are not useful while in skybox mode!
		{
			string url = "http://localhost:" + stelPort + "/api/main/status?actionId=" + actionId + "&propId=" + propertyId;
			WWW www = new WWW(url);
			yield return www;
			JSONObject json2 = new JSONObject(www.text); // this should be a rather short JSON with only the changed actions/properties.
			if (json && json.count>0 && json2 && json2.count>0)
			{
				json.Merge(json2);
				jsonActions = json.GetField("actionChanges");
				if (jsonActions && jsonActions.HasField("id")) { jsonActions.GetField(ref actionId, "id"); }
				jsonProperties = json.GetField("propertyChanges");
				if (jsonProperties && jsonProperties.HasField("id")) { jsonProperties.GetField(ref propertyId, "id"); }
				jsonTime = json.GetField("time");
				//Debug.Log("Json now: " + json.Print(true));
			}
			else
			{
				Debug.LogWarning("StelController::UpdateStelJson(): json invalid. Setting to Deltas. Things may break from now!");
				json = json2;
				connectToStellarium = false;
				jsonTime = streamingSkybox.GetJsonTime();
				StartCoroutine(QueryObjectInfo("Sun"));
				StartCoroutine(QueryObjectInfo("Moon"));
				StartCoroutine(QueryObjectInfo("Venus"));
			}
		}
		else
		{
			jsonTime=streamingSkybox.GetJsonTime();
			StartCoroutine(QueryObjectInfo("Sun"));
			StartCoroutine(QueryObjectInfo("Moon"));
			StartCoroutine(QueryObjectInfo("Venus"));
		}
		UpdateTimeGUI();
	}

	private void UpdateTimeGUI()
	{
		if (guiTimeText)
		{
			UnityEngine.UI.Text uiText = guiTimeText.GetComponent<UnityEngine.UI.Text>();
			uiText.text = jsonTime["local"].stringValue;
		}
	}

	public IEnumerator UpdateSkyboxTiles()
	{
		if (connectToStellarium)
		{
			yield return new WaitForSeconds(0.5f);
			string url = "http://localhost:" + stelPort + "/api/scripts/run";
			Dictionary<string, string> payload = new Dictionary<string, string>
			{
				{ "id", skyboxScriptName }
			};
			using (UnityWebRequest uwr = UnityWebRequest.Post(url, payload))
			{
				uwr.chunkedTransfer = false;
				yield return uwr.SendWebRequest();
	
				if (uwr.isNetworkError)
				{
					Debug.LogWarning("StelController.UpdateSkyboxTiles() failed." + uwr.error);
					yield break;
				}
				else if (uwr.isHttpError)
				{
					Debug.LogWarning("StelController.UpdateSkyboxTiles(): Problem with WWW answer: " + uwr.responseCode + "; wait a bit before retrying.");
					yield return new WaitForSecondsRealtime(0.25f);
				}
				else
				{
					if (uwr.downloadHandler.text != "ok")
					{
						Debug.LogWarning("StelController.UpdateSkyboxTiles(): Cannot update skybox tiles via HTTP.");
					}
				}
			}
			yield return StartCoroutine(UpdateStelJson());
		}
	}

	public IEnumerator SetJD(double newJD)
	{
		if (connectToStellarium)
		{
			jsonTime = json.GetField("time");
			double jday = 0;
			if (jsonTime.GetField(ref jday, "jday"))
			{
				Debug.Log("Current JD:" + jday);
				string url = "http://localhost:" + stelPort + "/api/main/time";
				Dictionary<string, string> payload = new Dictionary<string, string>
				{
					{ "time", (newJD).ToString() },
					{ "timerate", "0" }
				};
				using (UnityWebRequest uwr = UnityWebRequest.Post(url, payload))
				{
					uwr.chunkedTransfer = false;
					yield return uwr.SendWebRequest();
	
					if (uwr.isNetworkError)
					{
						Debug.LogWarning("StelController.SetJD() failed." + uwr.error);
						yield break;
					}
					else if (uwr.isHttpError)
					{
						Debug.LogWarning("StelController.SetJD(): Problem with WWW answer: " + uwr.responseCode + "; wait a bit before retrying.");
						yield return new WaitForSecondsRealtime(0.25f);
					}
					else
					{
						Debug.Log(message: "StelController.SetJD() complete! --> Answer: " + uwr.responseCode + " " + uwr.downloadHandler.text);
						if (uwr.downloadHandler.text != "ok")
						{
							Debug.LogWarning("StelController.SetJD(): Cannot set JD via HTTP.");
						}
						else
						{
							yield return StartCoroutine(UpdateSkyboxTiles());
							yield return StartCoroutine(UpdateStelJson());
						}
					}
				}
			}
			else
			{
				Debug.LogWarning("StelController.SetJD(): Cannot read JD from JSON");
			}
		}
	}

	public IEnumerator SetTimerate(double newTimerate)
	{
		if (connectToStellarium)
		{
			jsonTime = json.GetField("time");
			double jday = 0;
			if (jsonTime.GetField(ref jday, "jday"))
			{
				//Debug.Log("Current JD:" + jday);
				string url = "http://localhost:" + stelPort + "/api/main/time";
				Dictionary<string, string> payload = new Dictionary<string, string>
				{
					{ "timerate", newTimerate.ToString() }
				};
				using (UnityWebRequest uwr = UnityWebRequest.Post(url, payload))
				{
					uwr.chunkedTransfer = false;
					yield return uwr.SendWebRequest();
	
					if (uwr.isNetworkError)
					{
						Debug.LogWarning("StelController.SetTimerate() failed." + uwr.error);
						yield break;
					}
					else if (uwr.isHttpError)
					{
						Debug.LogWarning("StelController.SetTimerate(): Problem with WWW answer: " + uwr.responseCode + "; wait a bit before retrying.");
						yield return new WaitForSecondsRealtime(0.25f);
					}
					else
					{
						//Debug.Log(message: "StelController.SetTimerate() complete! --> Answer: " + uwr.responseCode + " " + uwr.downloadHandler.text);
						if (uwr.downloadHandler.text != "ok")
						{
							Debug.LogWarning("StelController.SetTimerate(): Cannot set Timerate via HTTP.");
						}
						else
						{
							yield return StartCoroutine(UpdateStelJson());
							yield return StartCoroutine(UpdateSkyboxTiles());
						}
					}
				}
			}
			else
			{
				Debug.LogWarning("Cannot read JD from JSON. Time corrupt?");
			}
		}
	}


	public IEnumerator SetLocation(float latitude, float longitude, float altitude, string name, string country, Planet planet)
	{
		if (connectToStellarium)
		{
			string url = "http://localhost:" + stelPort + "/api/location/setlocationfields";
			Dictionary<string, string> payload = new Dictionary<string, string>
			{
				{ "latitude", (latitude).ToString() },
				{ "longitude", (longitude).ToString() },
				{ "altitude", (altitude).ToString() },
				{ "name", name },
				{ "country", country },
				{ "planet", planet.ToString() }
			};
			using (UnityWebRequest uwr = UnityWebRequest.Post(url, payload))
			{
				uwr.chunkedTransfer = false;
				yield return uwr.SendWebRequest();
	
				if (uwr.isNetworkError)
				{
					Debug.LogWarning("StelController.SetLocation(" + name + ") failed." + uwr.error);
					yield break;
				}
				else if (uwr.isHttpError)
				{
					Debug.LogWarning("StelController.SetLocation(" + name + "): Problem with WWW answer: " + uwr.responseCode + "; wait a bit before retrying.");
					yield return new WaitForSecondsRealtime(0.25f);
				}
				else
				{
					//Debug.Log("Location Form upload complete! Sent set: name=" + name + " etc. --> Received: " + uwr.downloadHandler.text);
					if (uwr.downloadHandler.text.StartsWith("ok"))
					{
						yield return StartCoroutine(UpdateStelJson());
					}
					else
					{
						Debug.LogWarning("StelController.SetLocation(" + name + "): Cannot set location via HTTP:" + uwr.downloadHandler.text);
					}
				}
			}
		}
	}

	public IEnumerator DoAction(string actionName, bool updateJson = true)
	{
		if (connectToStellarium)
		{

			string url = "http://localhost:" + stelPort + "/api/stelaction/do";
			Dictionary<string, string> payload = new Dictionary<string, string>
			{
				{ "id", actionName }
			};
			using (UnityWebRequest www = UnityWebRequest.Post(url, payload))
			{
				www.chunkedTransfer = false;
				yield return www.SendWebRequest();

				if (www.isNetworkError || www.isHttpError)
				{
					Debug.LogWarning("StelController.DoAction(" + actionName + ") failed." + www.error);
				}
				else
				{
					Debug.Log(message: "DoAction() complete! Sent set: id=" + actionName + " --> Received: " + www.downloadHandler.text);
					if ((www.downloadHandler.text == "true") || (www.downloadHandler.text == "false") || (www.downloadHandler.text == "ok"))
					{
						yield return StartCoroutine(UpdateSkyboxTiles());
						toggle.initializeToggle();
						if (updateJson)
							yield return StartCoroutine(UpdateStelJson());
					}
					else
					{
						Debug.LogWarning(message: "Could not trigger action " + actionName + " via HTTP: " + www.downloadHandler.text);
					}
				}
			}
		}
	}

	public string GetActionValue(string actionName)
	{
		UpdateStelJson();
		if (jsonActions)
		{
			JSONObject jsonActChanges = jsonActions.GetField("changes");
			try
			{
				return jsonActChanges[actionName].ToString(); 
			}
			catch (System.Exception)
			{
				return "null";
			}
		}
		else
			return "null";
	}


	public string GetPropertyValue(string propertyName)
	{
		if (jsonProperties)
		{
			JSONObject jsonPropChanges = jsonProperties.GetField("changes");
			try
			{
				return jsonPropChanges[propertyName].ToString(); 
			}
			catch (System.Exception)
			{
				return "null";
			}
		}
		else
			return "null";
	}

	public IEnumerator SetProperty(string propertyName, string newValue, bool updateJson=true)
	{
		if (connectToStellarium)
		{
			string url = "http://localhost:" + stelPort + "/api/stelproperty/set";
			Dictionary<string, string> payload = new Dictionary<string, string>
			{
				{ "id", propertyName },
				{ "value", newValue }
			};
			using (UnityWebRequest uwr = UnityWebRequest.Post(url, payload))
			{
				uwr.chunkedTransfer = false;
				yield return uwr.SendWebRequest();
	
				if (uwr.isNetworkError)
				{
					Debug.LogWarning("StelController.SetProperty(" + propertyName + ") failed." + uwr.error);
					yield break;
				}
				else if (uwr.isHttpError)
				{
					Debug.LogWarning("StelController.SetProperty(" + propertyName + "): Problem with WWW answer: " + uwr.responseCode + "; wait a bit before retrying.");
					yield return new WaitForSecondsRealtime(0.25f);
				}
				else
				{
					if (uwr.downloadHandler.text == "ok")
					{
						if (updateJson)
							yield return StartCoroutine(UpdateStelJson());
					}
				}
			}
		}
	}

	private IEnumerator QueryObjectInfo(string objectName)
	{
		if (!streamingSkybox) Debug.LogError("no streamingSkybox defined???");
		if (objectName=="Sun") jsonSunInfo = streamingSkybox.GetSunInfo();
		if (objectName == "Moon") jsonMoonInfo = streamingSkybox.GetMoonInfo();
		if (objectName == "Venus") jsonVenusInfo = streamingSkybox.GetVenusInfo();
		yield break;
	}

	public JSONObject GetLastObjectInfo()
	{
		return jsonObjInfo;
	}

	public IEnumerator UpdateLightObjInfo()
	{
		yield return QueryObjectInfo("Sun");
		if (jsonSunInfo == null)
			Debug.LogError("Sun not retrieved");
		yield return QueryObjectInfo("Moon");
		if (jsonMoonInfo == null)
			Debug.LogError("Moon not retrieved");
		yield return QueryObjectInfo("Venus");
		if (jsonVenusInfo == null)
			Debug.LogError("Venus not retrieved");
	}

	public JSONObject GetLightObjInfo()
	{
		double alt = 0.0f;

		if (streamingSkybox.isActiveAndEnabled)
		{
			return streamingSkybox.GetLightObject();
		}
		else
		{
			Debug.LogWarning("StreamingSkybox not active/enabled. Scene setup wrong!");
			return null;
		}
	}

	public string GetTimeString()
	{
		StartCoroutine(UpdateStelJson());
		Debug.Log("jsonTime="+jsonTime);
		if (jsonTime && jsonTime["local"])
		{
			return jsonTime["local"].stringValue;
		}
		else
		{
			Debug.LogWarning("StelController::GetTimeString() error");
			return "StelController::GetTimeString() error";
		}
	}

	public float GetSolarLongitude()
	{
		if (!jsonSunInfo)
		{
			Debug.LogWarning("StelController::GetSolarLongitude(): no jsonSunInfo!");
			return 0.0f;
		}
		else
		{
			return (float) jsonSunInfo["elong"].doubleValue;
		}
	}

	public float GetSolarAltitude()
	{
		if (!jsonSunInfo)
		{
			Debug.LogWarning("StelController::GetSolarAltitude(): no jsonSunInfo!");
			return 30.0f;
		}
		else
		{
			return (float) jsonSunInfo["altitude"].doubleValue;
		}
	}
}


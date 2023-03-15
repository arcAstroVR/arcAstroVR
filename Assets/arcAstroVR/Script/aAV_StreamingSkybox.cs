/* Interfacing Unity3D with Stellarium. Joint project (started 2015-12) of Georg Zotti (LBI ArchPro) and John Fillwalk (IDIA Lab).
// Authors of this script: 
// Neil Zehr (IDIA Lab)
// David Rodriguez (IDIA Lab)
// Georg Zotti (LBI ArchPro)
// aAV_StreamingSkybox: Reorganize StreamingSkybox for arcAstroVR. (c) 2021 by K.Iwashiro.
 */
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Defective.JSON;

[RequireComponent(requiredComponent: typeof(aAV_StelController))]
public class aAV_StreamingSkybox : MonoBehaviour {

	[Tooltip("Subdirectory name in StreamingAssets folder")]
	private string pathBase;
	private string dataFile="unityData.txt"; 
	private string skyName = "";

	private Dictionary<string, Texture2D> sides = new Dictionary<string, Texture2D>();

	private bool isCreated;
	private bool isLoaded;
	private DateTime lastModified;
	private GameObject progressBar;
	private Text screenMode;
	private float loading;
	private int load0,load1, load2, load3, load4, load5, load6;
	private double JD;

	private JSONObject jsonTime;	  // a small JSON format-ident to StelController*s JSONtime that represents the time data from the unityData.txt;
	private JSONObject jsonLightInfo; // a small JSON that contains data about the current luminaire from the unityData.txt. 
	private JSONObject jsonSunInfo;   // a small JSON that contains data about Sun   from the unityData.txt. 
	private JSONObject jsonMoonInfo;  // a small JSON that contains data about Moon  from the unityData.txt. 
	private JSONObject jsonVenusInfo; // a small JSON that contains data about Venus from the unityData.txt. 
	private aAV_UI dateUI;
	private aAV_StelController controller;
	private aAV_CustomDateTime customDateTime;

	FileSystemWatcher watcher1;
	FileSystemWatcher watcher2;

	private void Awake()
	{
		pathBase = "~/.stellarium/";
		if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
		{
			Regex reg = new Regex(@"(^.*?\/Application Support\/).*");
			Match match = reg.Match(Application.persistentDataPath);
			pathBase = match.Groups[1] + "Stellarium/";
		}
		else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
		{
			Regex reg = new Regex(@"(^.*?\/AppData\/).*");
			Match match = reg.Match(Application.persistentDataPath);
			pathBase = match.Groups[1] + "Roaming/Stellarium/";
		}

		// prepare Material dictionary
		sides.Add("Unity1-north.png", null);
		sides.Add("Unity2-east.png", null);
		sides.Add("Unity3-south.png", null);
		sides.Add("Unity4-west.png", null);
		sides.Add("Unity5-top.png", null);
		sides.Add("Unity6-bottom.png", null);
		controller = gameObject.GetComponent<aAV_StelController>();
		dateUI = GameObject.Find("Main").transform.Find("Menu/DateTimeSetting").gameObject.GetComponent<aAV_UI>();
		watcher1 = new FileSystemWatcher();
		watcher2 = new FileSystemWatcher();
		isCreated = false;
		isLoaded = false;
		progressBar=GameObject.Find("Main").transform.Find("Menu/ProgressBar").gameObject;
		screenMode=GameObject.Find("Main").transform.Find("Menu/TopBar/screen").gameObject.GetComponent<Text>();
		load0 = load1 = load2 = load3 = load4 = load5 = load6 = 0;
		
	}

	private void Start()
	{
		ParseDataFile();
		skyName = "f1";
		FileCopy();
	}

	private void Update()
	{
		if (isLoaded) {
			if(!progressBar.activeSelf){
				loading = 0;
				progressBar.SetActive(true);
			}
			progressBar.transform.Find("progress").gameObject.GetComponent<Text>().text = ((int)loading).ToString()+"%";
			progressBar.GetComponent<Image>().fillAmount = loading/100f;
		}
		if (isCreated) {
			isCreated = false;
			isLoaded = false;
			Debug.Log(dataFile+" is Changed.");
			FileCopy();
			StartCoroutine(DoGetImages(SkydataPath));
			ParseDataFile();
			progressBar.SetActive(false);
			load0 = load1 = load2 = load3 = load4 = load5 = load6 = 0;
			
			//擬似リアルタイム回転リセット
			dateUI.GetComponent<aAV_UI>().tempRotation = 0;
		}
	}

	private void OnEnable()
	{
		//unityData.txtの監視
		watcher1.Path = pathBase;
		watcher1.NotifyFilter = NotifyFilters.LastWrite;
		watcher1.Filter = "*"+dataFile;
		watcher1.Changed += OnLiveDirectoryChanged;
		watcher1.Created += OnLiveDirectoryChanged;
		watcher1.IncludeSubdirectories = false;
		watcher1.EnableRaisingEvents = true;
		Debug.Log("Watch File = "+pathBase+dataFile);
		
		//Unity*.pngの監視
		watcher2.Path = pathBase;
		watcher2.NotifyFilter = NotifyFilters.LastWrite;
		watcher2.Filter = "Unity*png";
		watcher2.Changed += OnLiveDirectoryChanged;
		watcher2.Created += OnLiveDirectoryChanged;
		watcher2.IncludeSubdirectories = false;
		watcher2.EnableRaisingEvents = true;

	}

	private void OnDisable()
	{
		watcher1.Changed -= OnLiveDirectoryChanged;
		watcher1.Created -= OnLiveDirectoryChanged;
		watcher1.EnableRaisingEvents = false;
		watcher2.Changed -= OnLiveDirectoryChanged;
		watcher2.Created -= OnLiveDirectoryChanged;
		watcher2.EnableRaisingEvents = false;
	}

	public void OnLiveDirectoryChanged(object source, FileSystemEventArgs e)
	{
		isLoaded = true;
		if(e.FullPath.Contains(dataFile)){
			load0 = 1;
		}else if(e.FullPath.Contains("Unity1-north.png")){
			load1 = 1;
		}else if(e.FullPath.Contains("Unity2-east.png")){
			load2 = 1;
		}else if(e.FullPath.Contains("Unity3-south.png")){
			load3 = 1;
		}else if(e.FullPath.Contains("Unity4-west.png")){
			load4 = 1;
		}else if(e.FullPath.Contains("Unity5-top.png")){
			load5 = 1;
		}else if(e.FullPath.Contains("Unity6-bottom.png")){
			load6 = 1;
		}
		loading = (load0+load1+load2+load3+load4+load5+load6)*100/7;
		if (loading == 100){
			isCreated = true;
		}
		Debug.Log("Skybox Loaded="+e.FullPath+"："+ ((int)loading).ToString()+"%");
	}

	private Material CreateSkyboxMaterial(Dictionary<string, Texture2D> sides)
	{
		Shader skyboxShader = Shader.Find("Skybox/6 Sided - Arbitrary Rotation");
		if (!skyboxShader) Debug.LogError("Shader not found!");
		Material mat = new Material(skyboxShader);
		mat.SetTexture("_FrontTex", sides["Unity4-west.png"]);
		mat.SetTexture("_BackTex", sides["Unity2-east.png"]);
		mat.SetTexture("_LeftTex", sides["Unity1-north.png"]);
		mat.SetTexture("_RightTex", sides["Unity3-south.png"]);
		mat.SetTexture("_UpTex", sides["Unity5-top.png"]);
		mat.SetTexture("_DownTex", sides["Unity6-bottom.png"]);
		float rot = controller.northAngle + 180;
		mat.SetFloat("_Direction", Mathf.Repeat(rot, 360));
		return mat;
	}

	public JSONObject GetSunInfo()
	{
		if (!jsonSunInfo) Debug.LogError("StreamingSkybox: sunInfo not initialized!");
		return jsonSunInfo;
	}
	public JSONObject GetMoonInfo()
	{
		if (!jsonMoonInfo) Debug.LogError("StreamingSkybox: moonInfo not initialized!");
		return jsonMoonInfo;
	}
	public JSONObject GetVenusInfo()
	{
		if (!jsonVenusInfo) Debug.LogError("StreamingSkybox: venusInfo not initialized!");
		return jsonVenusInfo;
	}

	public string SkydataPath
	{
		get
		{
			return System.IO.Path.Combine(pathBase , skyName );
		}
	}

	public string SkyName
	{
		get
		{
			return skyName;
		}

		set
		{
			//スクリーン切り替え：skyName=F1〜F12
			skyName = value;
			if(!Directory.Exists(SkydataPath)){
				FileCopy();
			}
			StartCoroutine(DoGetImages(SkydataPath));
			ParseDataFile();
			//指定スクリーンJD値をDateUIに反映
			dateUI.JDsetup(JD);
			screenMode.text = "Screen : "+ skyName.ToUpper();
		}
	}

	private void SetSkybox(Material material) {
		RenderSettings.skybox = material;
		DynamicGI.UpdateEnvironment();
	}

	// 6面画像ファイルの更新
	public IEnumerator DoGetImages(string directory) {
		Debug.Log("start DoGetImages："+directory);
		List<string> filenames = new List<string>(sides.Keys);
		Texture2D texture = Texture2D.whiteTexture;
		//Debug.Log("Filenames dictionary has " + filenames.Count + " entries");
		foreach (string filename in filenames) {
			string filePath = System.IO.Path.Combine(directory, filename);
			//Debug.Log("Trying to get image " + filePath);
			
			if (filePath.Contains("://")) {
				UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(filePath);
				uwr.chunkedTransfer = false;
				yield return uwr.SendWebRequest();

				if (uwr.isNetworkError || uwr.isHttpError)
				{
					Debug.LogError(message: "Texture file download problem: " + uwr.error + " for file: " + uwr.url);
				}
				else
				{
					// Get downloaded asset bundle
					texture = DownloadHandlerTexture.GetContent(uwr);
					Debug.Log("HTTP: " + uwr.responseCode  + " --> Retrieved Texture " + uwr.url + " with " + uwr.downloadedBytes.ToString() + "Bytes");
					if (texture == null)
						texture = Texture2D.blackTexture;
				}
			}
			else
			{
				byte[] data= File.ReadAllBytes(filePath);
				texture = new Texture2D(512, 512, TextureFormat.BGRA32, true, false);
				if (!texture.LoadImage(data))
				{
					Debug.LogWarning(message: "CANNOT RETRIEVE TEXTURE FROM FILE" + filePath);
				}
				
			}
			if (texture.width != texture.height) // Likely in "live Skybox mode"
			{
				texture = CropTexture(texture);
			}
			texture.wrapMode = TextureWrapMode.Clamp;
			sides[filename] = texture;
		}
		SetSkybox(CreateSkyboxMaterial(sides));
	}

	public static Texture2D CropTexture(Texture2D originalTexture) {
		Rect cropRect=new Rect((originalTexture.width*.5f)-(originalTexture.height * .5f),0f,originalTexture.height,originalTexture.height);
		if(cropRect.height <= 0 || cropRect.width <= 0) return null; 
		
		Texture2D newTexture = new Texture2D((int)cropRect.width, (int)cropRect.height, TextureFormat.RGBA32, false);
		Color[] pixels = originalTexture.GetPixels((int)cropRect.x, (int)cropRect.y, (int)cropRect.width, (int)cropRect.height, 0);
		newTexture.SetPixels(pixels);

		newTexture.Apply();
		newTexture.wrapMode = TextureWrapMode.Clamp;
		return newTexture;
	}

	public JSONObject GetJsonTime()
	{
		if (!jsonTime) ParseDataFile();
		return jsonTime;
	}

	public JSONObject GetLightObject()
	{
		if (!jsonLightInfo)
		{
			Debug.LogWarning("StreamingSkybox::GetLightObject: Light object not defined!");
		}
		//Debug.Log("StreamingSkybox::GetLightObject: " + jsonLightInfo.ToString());
		return jsonLightInfo;
	}

	//unity.txtの読み込み
	private void ParseDataFile()
	{
		Dictionary<string, string> fileInfo = new Dictionary<string, string>();
		fileInfo.Clear();

		string filePath = System.IO.Path.Combine(SkydataPath, dataFile);
		//Debug.Log("Reading data from local file " + filePath);
		try
		{
			using (StreamReader sr = new StreamReader(filePath))
			{
				String line;
				while ((line = sr.ReadLine()) != null)
				{
					//Debug.Log(line);
					String[] keyValPair = line.Split(new Char[] { ':' }, 2);
					//Debug.Log(keyValPair);
					if (keyValPair.Length == 2)
						fileInfo.Add(keyValPair[0], keyValPair[1]);
				}
			}
		}
		catch (Exception e)
		{
			// Let the user know what went wrong.
			Debug.LogWarning("The file could not be read:" + e.Message);
		}

		jsonLightInfo = new JSONObject();
		jsonSunInfo = new JSONObject();
		jsonMoonInfo = new JSONObject();
		jsonVenusInfo = new JSONObject();
		try
		{
			jsonSunInfo.AddField("name", "Sun");
			jsonSunInfo.AddField("altitude", float.Parse(fileInfo["Sun Altitude"]));
			jsonSunInfo.AddField("azimuth",  float.Parse(fileInfo["Sun Azimuth"]));
			jsonSunInfo.AddField("vmag",	 float.Parse(fileInfo["Sun Magnitude"]));
			jsonSunInfo.AddField("vmage",	float.Parse(fileInfo["Sun Magnitude (after extinction)"]));
			jsonSunInfo.AddField("elong",	float.Parse(fileInfo["Sun Longitude"]));
			if (fileInfo.ContainsKey("Sun Size"))
				jsonSunInfo.AddField("diameter", float.Parse(fileInfo["Sun Size"]));
			else
				jsonSunInfo.AddField("diameter", 0.5f);
			jsonMoonInfo.AddField("name", "Moon");
			jsonMoonInfo.AddField("altitude", float.Parse(fileInfo["Moon Altitude"]));
			jsonMoonInfo.AddField("azimuth",  float.Parse(fileInfo["Moon Azimuth"]));
			jsonMoonInfo.AddField("illumination", float.Parse(fileInfo["Moon illumination"]));
			jsonMoonInfo.AddField("vmag",	 float.Parse(fileInfo["Moon Magnitude"]));
			jsonMoonInfo.AddField("vmage",	float.Parse(fileInfo["Moon Magnitude (after extinction)"]));
			//jsonMoonInfo.AddField("elong",  float.Parse(fileInfo["Moon Longitude"]));
			if (fileInfo.ContainsKey("Moon Size"))
				jsonMoonInfo.AddField("diameter", float.Parse(fileInfo["Moon Size"]));
			else
				jsonMoonInfo.AddField("diameter", 0.5f);
			jsonVenusInfo.AddField("name", "Venus");
			jsonVenusInfo.AddField("altitude", float.Parse(fileInfo["Venus Altitude"]));
			jsonVenusInfo.AddField("azimuth",  float.Parse(fileInfo["Venus Azimuth"]));
			jsonVenusInfo.AddField("vmag",	 float.Parse(fileInfo["Venus Magnitude"]));
			jsonVenusInfo.AddField("vmage",	float.Parse(fileInfo["Venus Magnitude (after extinction)"]));
			//jsonVenusInfo.AddField("elong", fileInfo["Venus Longitude"]);
			jsonVenusInfo.AddField("diameter", 0.0f); // For the naked eye, we assume point source (to switch off the impostor sphere!)
			// The following field can only be in the file with Stellarium r9130+ (2017-02-05).
			if (fileInfo.ContainsKey("Landscape Brightness"))
			{
				jsonSunInfo.AddField("ambientInt",   float.Parse(fileInfo["Landscape Brightness"]));
				jsonMoonInfo.AddField("ambientInt",  float.Parse(fileInfo["Landscape Brightness"]));
				jsonVenusInfo.AddField("ambientInt", float.Parse(fileInfo["Landscape Brightness"]));
			}


			if (float.Parse(fileInfo["Sun Altitude"]) > -3.0f)
			{
				jsonLightInfo = jsonSunInfo.Copy();
			}
			else if (float.Parse(fileInfo["Moon Altitude"]) > 0.0f)
			{
				jsonLightInfo = jsonMoonInfo.Copy();
			}
			else if (float.Parse(fileInfo["Venus Altitude"]) > 0.0f)
			{
				jsonLightInfo = jsonVenusInfo.Copy();
			}
			else
			{
				jsonLightInfo.AddField("name", "none");
				// The following field can only be in the file with Stellarium r9130+ (2017-02-05).
				if (fileInfo.ContainsKey("Landscape Brightness"))
					jsonLightInfo.AddField("ambientInt", float.Parse(fileInfo["Landscape Brightness"]));
				else
					jsonLightInfo.AddField("ambientInt", 0.05f);
			}
		}
		catch (Exception e)
		{
			// Let the user know what went wrong.
			Debug.LogWarning("info lookup failed:" + e.Message);
			jsonLightInfo.AddField("name", "none");
			jsonLightInfo.AddField("ambientInt", 0.05f);
		}

		jsonTime = new JSONObject();
		JD=double.Parse(fileInfo["JD"]);
		jsonTime.AddField("jday",	  float.Parse(fileInfo["JD"]));
		jsonTime.AddField("deltaT",	"unknown");	
		jsonTime.AddField("gmtShift",  "unknown");
		jsonTime.AddField("timeZone",  "unknown");
		jsonTime.AddField("utc",	   fileInfo["Date (UTC)"]);
		jsonTime.AddField("local",	 fileInfo["Date"]);
		jsonTime.AddField("isTimeNow", false);
		jsonTime.AddField("timerate",  0.0f);
	}
		
	private void FileCopy(){
		CreateDirAndCopyFile( pathBase+dataFile, SkydataPath+"/"+dataFile);
		CreateDirAndCopyFile( pathBase+"Unity1-north.png", SkydataPath+"/Unity1-north.png");
		CreateDirAndCopyFile( pathBase+"Unity2-east.png", SkydataPath+"/Unity2-east.png");
		CreateDirAndCopyFile( pathBase+"Unity3-south.png", SkydataPath+"/Unity3-south.png");
		CreateDirAndCopyFile( pathBase+"Unity4-west.png", SkydataPath+"/Unity4-west.png");
		CreateDirAndCopyFile( pathBase+"Unity5-top.png", SkydataPath+"/Unity5-top.png");
		CreateDirAndCopyFile( pathBase+"Unity6-bottom.png", SkydataPath+"/Unity6-bottom.png");
	}

	private void CreateDirAndCopyFile(string sourceFullPath, string distFullPath){
		if(File.Exists(sourceFullPath)){
			string distDir = Path.GetDirectoryName(distFullPath);
			if(!Directory.Exists(distDir)){
				Directory.CreateDirectory(distDir);
			}
			File.Copy(sourceFullPath, distFullPath, true);
		}
	}

}

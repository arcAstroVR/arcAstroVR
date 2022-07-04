/* Interfacing Unity3D with Stellarium. Joint project (started 2015-12) of Georg Zotti (LBI ArchPro) and John Fillwalk (IDIA Lab).
// Authors of this script: 
// Neil Zehr (IDIA Lab)
// David Rodriguez (IDIA Lab)
// Georg Zotti (LBI ArchPro)
// aAV_StreamingSkybox: Reorganize StreamingSkybox for arcAstroVR. (c) 2021 by K.Iwashiro.
 */
using UnityEngine;
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
    private string imgDir;// 岩城追加 2021-03-19 "/Users/iwashiro/Desktop/data_sannai/"
    private string dataDir;// 岩城追加 2021-03-19 "/Users/iwashiro/Library/Application Support/Stellarium/"
    private string dataFile="unityData.txt";// 岩城追加 2021-03-19 
    private string defaultSkyname = "";// 岩城修正
    private string skyName = "live";  // subdir of StreamingAssets/SkyBoxes/ where the images written by Stellarium have been moved. 

    private Dictionary<string, Texture2D> sides = new Dictionary<string, Texture2D>(); // private dictionary for Material creation.

	private bool isCreated;	// 岩城追加 2021-03-19 
	private DateTime lastModified;	// 岩城追加 2021-03-19 

    private string pathBase;          // will contain the path (directory or URL) to the SkyBoxes folder. Usually, use SkydataPath to construct file paths/URLs.
    private JSONObject jsonTime;      // a small JSON format-ident to StelController*s JSONtime that represents the time data from the unityData.txt;
    private JSONObject jsonLightInfo; // a small JSON that contains data about the current luminaire from the unityData.txt. 
    private JSONObject jsonSunInfo;   // a small JSON that contains data about Sun   from the unityData.txt. 
    private JSONObject jsonMoonInfo;  // a small JSON that contains data about Moon  from the unityData.txt. 
    private JSONObject jsonVenusInfo; // a small JSON that contains data about Venus from the unityData.txt. 
    private aAV_StelController controller; // needed for location details (rotation)
                                       // Currently we don't use it, but this can change. 
                                       // It would be better to add the light info handling to the skyboxes and just get them in the StelController.

    FileSystemWatcher watcher;          // The watcher is used to check for updates in the skybox tiles directory. As soon as a new skybox has been generated, Skybox and SunLight will be updated. 

    private void Awake()
    {

		dataDir=aAV_Public.basicInfo.filedir;  //岩城追加
		
        imgDir = "~/.stellarium/";
        if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
        {
            Regex reg = new Regex(@"(^.*?\/Application Support\/).*");
            Match match = reg.Match(Application.persistentDataPath);
            imgDir = match.Groups[1] + "Stellarium/";
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            Regex reg = new Regex(@"(^.*?\/AppData\/).*");
            Match match = reg.Match(Application.persistentDataPath);
            imgDir = match.Groups[1] + "Roaming/Stellarium/";
        }

 

        skyName = defaultSkyname;
        // prepare Material dictionary
        sides.Add("Unity1-north.png", null);
        sides.Add("Unity2-east.png", null);
        sides.Add("Unity3-south.png", null);
        sides.Add("Unity4-west.png", null);
        sides.Add("Unity5-top.png", null);
        sides.Add("Unity6-bottom.png", null);
        pathBase = imgDir;
        controller = gameObject.GetComponent<aAV_StelController>();
        watcher = new FileSystemWatcher();
    }

    private void Start()
    {
        StartCoroutine(ParseDataFile());
    }

    private void Update()
    {
    	if (isCreated) {	//岩城追加 2021-03-19 
    		isCreated = false;
			Debug.Log(dataFile+" is Changed.");
			StartCoroutine(DoGetImages(SkydataPath));
			StartCoroutine(ParseDataFile());
		}
	}

    private void OnEnable()
    {
        watcher.Path = imgDir;// 岩城修正 2021-03-19
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.Filter = "*"+dataFile;// 岩城修正 2021-03-19
        watcher.Changed += OnLiveDirectoryChanged;
        watcher.Created += OnLiveDirectoryChanged;
        watcher.EnableRaisingEvents = true;
        Debug.Log("Watch File = "+imgDir+dataFile);
    }

    void OnDisable()
    {
        watcher.Changed -= OnLiveDirectoryChanged;
        watcher.Created -= OnLiveDirectoryChanged;
        watcher.EnableRaisingEvents = false;
    }

    void OnLiveDirectoryChanged(object source, FileSystemEventArgs e)
    {
		if(lastModified != File.GetLastWriteTime(SkydataPath+dataFile)){
			lastModified = File.GetLastWriteTime(SkydataPath+dataFile);
			isCreated = true;
		}
    }

    private Material CreateSkyboxMaterial(Dictionary<string, Texture2D> sides)
    {
        Shader skyboxShader = Shader.Find("Skybox/6 Sided");
        if (!skyboxShader) Debug.LogError("Shader not found!");
        Material mat = new Material(skyboxShader);
        mat.SetTexture("_FrontTex", sides["Unity4-west.png"]);
        mat.SetTexture("_BackTex", sides["Unity2-east.png"]);
        mat.SetTexture("_LeftTex", sides["Unity1-north.png"]);
        mat.SetTexture("_RightTex", sides["Unity3-south.png"]);
        mat.SetTexture("_UpTex", sides["Unity5-top.png"]);
        mat.SetTexture("_DownTex", sides["Unity6-bottom.png"]);
        float rot = -controller.northAngle; if (rot < 0) rot += 360.0f;
        mat.SetFloat("_Rotation", Mathf.Clamp(rot, 0, 360));
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

    // Readonly: path (directory or URL) to current skybox textures and info file.
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
            skyName = value;
            StartCoroutine(DoGetImages(SkydataPath));
            StartCoroutine(ParseDataFile());
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
        //int sampleHeight = 0;
        Texture2D texture = Texture2D.whiteTexture; // new Texture2D(2, 2, TextureFormat.BGRA32, true);
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

    // Cut central square from landscape-oriented texture. 
    public static Texture2D CropTexture(Texture2D originalTexture) {
        Rect cropRect=new Rect((originalTexture.width*.5f)-(originalTexture.height * .5f),0f,originalTexture.height,originalTexture.height);
        // Make sure the crop rectangle stays within the original Texture dimensions
        //cropRect.x = Mathf.Clamp(cropRect.x, 0, originalTexture.width);
        //cropRect.width = Mathf.Clamp(cropRect.width, 0, originalTexture.width - cropRect.x);
        //cropRect.y = Mathf.Clamp(cropRect.y, 0, originalTexture.height);
        //cropRect.height = Mathf.Clamp(cropRect.height, 0, originalTexture.height - cropRect.y);
        if(cropRect.height <= 0 || cropRect.width <= 0) return null; // dont create a Texture with size 0

        Texture2D newTexture = new Texture2D((int)cropRect.width, (int)cropRect.height, TextureFormat.RGBA32, false);
        //Texture2D newTexture = new Texture2D((int)cropRect.width, (int)cropRect.height, TextureFormat.BGRA32, false); // NOTE new BGRA!
        Color[] pixels = originalTexture.GetPixels((int)cropRect.x, (int)cropRect.y, (int)cropRect.width, (int)cropRect.height, 0);
        newTexture.SetPixels(pixels);

        //TextureScale.Bilinear(newTexture, 256, 256);
        //Debug.Log("Supported Texture Formats:");
        //if (SystemInfo.SupportsTextureFormat(TextureFormat.RGBA32)) Debug.Log(" RGBA32");
        
        newTexture.Apply();
        newTexture.wrapMode = TextureWrapMode.Clamp;
        return newTexture;
    }

    public JSONObject GetJsonTime()
    {
        if (!jsonTime) StartCoroutine(ParseDataFile());
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
    private IEnumerator ParseDataFile()
    {
        // Construct a JSONObject from the output.txt file.
        Dictionary<string, string> fileInfo = new Dictionary<string, string>(); // info from output.txt comes here in key/value pairs.
        fileInfo.Clear();

        //string filePath = System.IO.Path.Combine(SkydataPath, "unityData.txt");
        string filePath = System.IO.Path.Combine(imgDir, dataFile); // 岩城修正 2021-03-19

//        Debug.Log("Trying to get " + filePath);

        if (filePath.Contains("://") )
        {
            // Mostly WebGL path...
            Debug.Log("Reading data via WebRequest: " + filePath);
            UnityWebRequest www = UnityWebRequest.Get(filePath);
            yield return www.SendWebRequest();

            string text;
            Debug.Log("Asked for text file at URL:" + www.url);
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogWarning(message: "CANNOT RETRIEVE TEXT!" + www.error);
            }
            else
            {
                // Show results as text
                Debug.Log(www.downloadHandler.text);

                // Or retrieve results as binary data
                //byte[] results = www.downloadHandler.data;
            }
            text = www.downloadHandler.text;
            // TODO: Parse into fileInfo dictionary
            try
            {
                // Create an instance of StringReader to read from the returned text.
                // The using statement also closes the StringReader.
                using (StringReader sr = new StringReader(text))
                {
                    String line;
                    // Read and display lines from the file until end of string.
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
                Debug.LogWarning("The www.text string could not be parsed:" + e.Message);
            }
            // At this point all data from the URL path have been read.
        }
        else
        {
            // LOCAL FILE
            //Debug.Log("Reading data from local file " + filePath);
            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (StreamReader sr = new StreamReader(filePath))
                {
                    String line;
                    // Read and display lines from the file until the end of 
                    // the file is reached.
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
            // At this point all data have been read.
            // Prepare the light info object. They are currently not used, but maybe later?
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
            jsonSunInfo.AddField("vmag",     float.Parse(fileInfo["Sun Magnitude"]));
            jsonSunInfo.AddField("vmage",    float.Parse(fileInfo["Sun Magnitude (after extinction)"]));
            jsonSunInfo.AddField("elong",    float.Parse(fileInfo["Sun Longitude"]));
            if (fileInfo.ContainsKey("Sun Size"))
                jsonSunInfo.AddField("diameter", float.Parse(fileInfo["Sun Size"]));
            else
                jsonSunInfo.AddField("diameter", 0.5f);
            jsonMoonInfo.AddField("name", "Moon");
            jsonMoonInfo.AddField("altitude", float.Parse(fileInfo["Moon Altitude"]));
            jsonMoonInfo.AddField("azimuth",  float.Parse(fileInfo["Moon Azimuth"]));
            jsonMoonInfo.AddField("illumination", float.Parse(fileInfo["Moon illumination"]));
            jsonMoonInfo.AddField("vmag",     float.Parse(fileInfo["Moon Magnitude"]));
            jsonMoonInfo.AddField("vmage",    float.Parse(fileInfo["Moon Magnitude (after extinction)"]));
            //jsonMoonInfo.AddField("elong",  float.Parse(fileInfo["Moon Longitude"]));
            if (fileInfo.ContainsKey("Moon Size"))
                jsonMoonInfo.AddField("diameter", float.Parse(fileInfo["Moon Size"]));
            else
                jsonMoonInfo.AddField("diameter", 0.5f);
            jsonVenusInfo.AddField("name", "Venus");
            jsonVenusInfo.AddField("altitude", float.Parse(fileInfo["Venus Altitude"]));
            jsonVenusInfo.AddField("azimuth",  float.Parse(fileInfo["Venus Azimuth"]));
            jsonVenusInfo.AddField("vmag",     float.Parse(fileInfo["Venus Magnitude"]));
            jsonVenusInfo.AddField("vmage",    float.Parse(fileInfo["Venus Magnitude (after extinction)"]));
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
            //jsonLightInfo.AddField("landscape-brt", "0.05");
        }

        jsonTime = new JSONObject();
        //jsonTime.Clear();
        //Debug.Log(fileInfo["JD"]);
        jsonTime.AddField("jday",      float.Parse(fileInfo["JD"])); //current Julian day
        jsonTime.AddField("deltaT",    "unknown");      //current deltaT as determined by the current dT algorithm  --> TODO: Make float! Or fix in skybox.ssc
        jsonTime.AddField("gmtShift",  "unknown");      //the timezone shift to GMT                                 --> TODO: Make float! Or fix in skybox.ssc
        jsonTime.AddField("timeZone",  "unknown");      //the timezone name                                         --> TODO: Fix in skybox.ssc and retrieve as string
        jsonTime.AddField("utc",       fileInfo["Date (UTC)"]); //the time in UTC time zone as ISO8601 time string
        jsonTime.AddField("local",     fileInfo["Date"]); //the time in local time zone as ISO8601 time string
        jsonTime.AddField("isTimeNow", false);        //if true, the Stellarium time equals the current real-world time
        jsonTime.AddField("timerate",  0.0f);          //the current time rate (in secs). Obviously 0 for a static skybox...
    }
        
}

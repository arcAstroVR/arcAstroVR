using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class aAV_Public : MonoBehaviour
{
	public GameObject InnerAvatar;
	public GameObject markerPrefab;
	public GameObject linePrefab;
	public GameObject labelPrefab;
	public GameObject waterPrefab;

	public class BasicInfo
	{
		public string location = "arcAstroVR";
		public string country = "";
		public string timezone = "00:00";
		public int year = 1;
		public int month = 1;
		public int day = 1;
		public int hour = 1;
		public int minute = 1;
		public int second = 1;
		public float area = 0f;				//狭域terrainのmeshサイズ(m)。
		public string type = "";				//座標系。WGS84はWG、19系はJP、UTMはUT
		public int zone = 84;					//19系、UTMのゾーン番号
		public double center_E =0d;			//東向き座標。表記は指定座標系に準拠
		public double center_N =0d;			//北向き座標。表記は指定座標系に準拠
		public float center_H =0f;				//楕円体高
		public string avatar="";
		public float avatar_H=176f;				//身長
		public string copyright_W="";
		public string copyright_N="";
		
		public string filePath="";
		public string filedir = "./";				//datasetの保存場所
		public string livedir = "";				//天球画像の保存場所：Winはfiledir、MacはApplication Support/Stellarium
	}

	public class CenterInfo
	{
		public string type = "WG";
		public double WGS_E = 0d;
		public double WGS_N = 0d;
		public double JPRCS_E = 0d;
		public double JPRCS_N = 0d;
		public int JPRCS_zone = 1;
		public double UTM_E = 0d;
		public double UTM_N = 0d;
		public int UTM_zone = 1;
	}
	
	public class RPoint
	{
		public string name = "";
		public double origin_E = 0d;			//Unity空間座標
		public double origin_N = 0d;		//Unity空間座標
		public float origin_H = 0f;				//Unity空間座標
		public string color = "#ffa500";
		public bool visible = true;
		public float cam_ROLL = 0f;			//MarkerカメラRoll回転
		public float cam_PITCH = 0f;		//MarkerカメラPitch回転
		public float cam_YAW = 0f;			//MarkerカメラYaw回転
		public float cam_FOV = 60f;			//MarkerカメラFOV
		public int cam_type = 0;				//Markerカメラ投影タイプ（0：透視投影、1：平行投影、2：魚眼投影）
		public GameObject gameobject;	//VR空間のMarkerObj
		public GameObject infoobject;		//情報WindowのMaker行Obj
	}

	public class Line
	{
		public string name = "";
		public int start_marker = 0;			//LineEditor内でのみ有効
		public int end_marker = 0;			//LineEditor内でのみ有効
		public GameObject startObj;
		public GameObject endObj;
		public string angle = "";
		public string color = "#00ff00";
		public bool visible = true;
		public GameObject gameobject;	//VR空間のLineObj
		public GameObject infoobject;		//情報WindowのLine行Obj
	}
	
	public class Dataset
	{
		public string name = "";
		public string file = "";
		public double origin_E = 0d;			//Unity空間座標
		public double origin_N = 0d;		//Unity空間座標
		public float origin_H = 0f;				//Unity空間座標
		public float rot_E = 0f;
		public float rot_N = 0f;
		public float rot_H = 0f;
		public float scale_E = 1f;
		public float scale_N = 1f;
		public float scale_H = 1f;
		public string start = "";
		public string end = "";
		public bool visible = true;
		public GameObject gameobject;	//VR空間の遺構Obj
		public GameObject infoobject;		//情報Windowの遺構行Obj
		public string copyright="";
	}

	public class Language
	{
		public string coordinate = "";
		public string lon = "";
		public string lat = "";
		public string height = "";
		public string cursor = "";
		public string azimuth = "";
		public string altitude = "";
		public string direction = "";
		public string distance = "";
		public string origin = "";
		public string rotation = "";
		public string scale = "";
		public string existences = "";
		public string close = "";
		public string show = "";
		public string xdirection = "";
		public string ydirection = "";
		public string zdirection = "";
		public string xaxis = "";
		public string yaxis = "";
		public string zaxis = "";
	}
	
	public static BasicInfo basicInfo = new BasicInfo();
	public static CenterInfo center = new CenterInfo();
	public static Language lang = new Language();
	public static List<RPoint> rplist = new List<RPoint>();
	public static List<Line> linelist = new List<Line>();
	public static List<Dataset> datalist = new List<Dataset>();
	public static bool addMarker = false;
	public static bool addLine = false;
	public static bool showCompass = false;
	public static bool uiDrag = false;
	public static int ambient = 0;
	public static int displayMode = 0;
	public static bool domeFix = false;
	public static string copyright = "";

	public async Task GetEntry(){
		var targetTableName = "LanguageTable";
		await LocalizationSettings.StringDatabase.GetTableAsync(targetTableName).Task;
		var table = LocalizationSettings.StringDatabase.GetTable(targetTableName);
		lang.coordinate = table.GetEntry("coordinate").Value;
		lang.lon = table.GetEntry("lon").Value;
		lang.lat = table.GetEntry("lat").Value;
		lang.height = table.GetEntry("height").Value;
		lang.cursor = table.GetEntry("cursor").Value;
		lang.azimuth = table.GetEntry("azimuth").Value;
		lang.altitude =table.GetEntry("altitude").Value;
		lang.direction= table.GetEntry("direction").Value;
		lang.distance= table.GetEntry("distance").Value;
		lang.origin= table.GetEntry("origin").Value;
		lang.rotation= table.GetEntry("rotation").Value;
		lang.scale= table.GetEntry("scale").Value;
		lang.existences= table.GetEntry("existences").Value;
		lang.xdirection= table.GetEntry("xdirection").Value;
		lang.ydirection= table.GetEntry("ydirection").Value;
		lang.zdirection= table.GetEntry("zdirection").Value;
		lang.xaxis= table.GetEntry("xaxis").Value;
		lang.yaxis= table.GetEntry("yaxis").Value;
		lang.zaxis= table.GetEntry("zaxis").Value;
	}

	void Start()
	{
		GetEntry();
	}
}

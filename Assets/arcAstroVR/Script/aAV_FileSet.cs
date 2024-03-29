﻿using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Rendering;
using TriLibCore;
using TriLibCore.General;
using TriLibCore.SFB;
using TriLibCore.Extensions;
using TriLibCore.Mappers;
using Fragilem17.MirrorsAndPortals;

public class aAV_FileSet : MonoBehaviour {

	[SerializeField]
	private HumanoidAvatarMapper _humanoidAvatarMapper;

	private aAV_Public aav_public;
	private aAV_GIS gis;
	private aAV_ThirdPersonOrbitCamBasic orbit_cam;
	private MirrorSurface ms;
	private GameObject errorstatus;
	private GameObject avatar;
	private GameObject cesiumObj;
	private int obj_count=0;
	private int terrain_count=0;
	
	void Awake() {
		aav_public = GameObject.Find("Main").GetComponent<aAV_Public>();
		gis = GameObject.Find("Main").GetComponent<aAV_GIS>();
		avatar = GameObject.Find("Main").transform.Find("Avatar").gameObject;
		orbit_cam = GameObject.Find("XR Origin").transform.Find("Camera Offset/Main Camera").GetComponent<aAV_ThirdPersonOrbitCamBasic>();
		errorstatus = GameObject.Find("errorMessage");
		cesiumObj = GameObject.Find("CesiumGeoreference");
	}
	
	void Start() {
		StartCoroutine(connectCheck());
		GameObject version = GameObject.Find("version");
		version.GetComponent<Text>().text = "Version：" + Application.version;
	}

	private IEnumerator connectCheck() {
		while(true){
			//Stellariumとの通信確認
			string url = "http://localhost:8090/api/main/status?propId=-2&actionId=-2";
			UnityWebRequest uwr = UnityWebRequest.Get(url);
			uwr.chunkedTransfer = false;
			yield return uwr.SendWebRequest();
			if (uwr.isNetworkError || uwr.isHttpError){
				errorstatus.GetComponent<Text>().text = "Communication with Stellarium could not be verified, please start Stellarium.";
			} else {
				errorstatus.GetComponent<Text>().text = "";
				GameObject.Find("Canvas").transform.Find("selectButton").gameObject.SetActive(true);
				break;
			}
		}
	}
	
	public void selectDataset() {
		aAV_Public.basicInfo.filePath = StandaloneFileBrowser.OpenFilePanel( "Open File", "", "txt", true )[0].Name;
		if(aAV_Public.basicInfo.filePath != ""){
			errorstatus.GetComponent<Text>().text = "";
			//Datasetの読み込み
			ReadFile(aAV_Public.basicInfo.filePath);
			string filedir = Path.GetDirectoryName(aAV_Public.basicInfo.filePath)+"/";
			if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor ) {
				Regex reg = new Regex(@"(^.*?\/Application Support\/).*");
				Match match =reg.Match(Application.persistentDataPath);
				aAV_Public.basicInfo.livedir = match.Groups[1]+"Stellarium/";
			}else if(Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor ) {
				Regex reg = new Regex(@"(^.*?\/AppData\/).*");
				Match match = reg.Match(Application.persistentDataPath);
				aAV_Public.basicInfo.livedir = match.Groups[1] + "Roaming/Stellarium/";
			}else{
				aAV_Public.basicInfo.livedir = "~/.stellarium/";
			}
			aAV_Public.basicInfo.filedir = filedir;
			Debug.Log("fileDIR="+aAV_Public.basicInfo.filedir + ", liveDIR=" +aAV_Public.basicInfo.livedir);
			
			//必須項目の確認
			CheckRequire();
			if(errorstatus.GetComponent<Text>().text != ""){
				return;
			}

			//selectButton非表示
			GameObject.Find("selectButton").SetActive(false);

			//PlayerPrefの読み込み
			LocalizationSettings.SelectedLocale = Locale.CreateLocale(PlayerPrefs.GetString ("Language", "en"));
			aav_public.GetEntry();
			if(PlayerPrefs.GetString ("Path", "") == aAV_Public.basicInfo.filePath){
				aAV_Public.center.type = PlayerPrefs.GetString ("Coordinate", aAV_Public.basicInfo.type);
				if(aAV_Public.center.type=="JP"){
					aAV_Public.center.JPRCS_zone = PlayerPrefs.GetInt ("Zone", aAV_Public.basicInfo.zone);
				}else if(aAV_Public.center.type=="UT"){
					aAV_Public.center.UTM_zone = PlayerPrefs.GetInt ("Zone", aAV_Public.basicInfo.zone);
				}
			}else{
				PlayerPrefs.SetString("Path", aAV_Public.basicInfo.filePath);
			}
			float ambient = PlayerPrefs.GetFloat ("Ambient", 0f);
			if(ambient == 0f){
				ambient = 1f;
			}
			RenderSettings.ambientIntensity = ambient;
			float scaleUI = PlayerPrefs.GetFloat("ScaleUI", 0f);
			if(scaleUI == 0f){
				scaleUI = Screen.dpi/100f;
			}
			GameObject.Find("Main").transform.Find("Menu").gameObject.GetComponent<CanvasScaler>().scaleFactor = scaleUI;
			
			//Stellarium起動
			GameObject.Find("Main").transform.Find("Stellarium").gameObject.SetActive(true);

			//Markerの設置
			SetMarker();
			
			//Auxiliary Lineの設置
			SetLine();
			
			//プログレスの初期化
			GameObject.Find("load_terrain").GetComponent<Text>().text = "Terrain loading：";
			GameObject.Find("load_object").GetComponent<Text>().text = "Object  loading：";

			//Avatarの読み込み
			LoadAvatar();
	
			//3D Objectの読み込み
			LoadObj();
			
			//Terrainの読み込み
			StartCoroutine (LoadTerrain());
	
			//3D Object読み込みの終了を確認して、Mainに移行
			StartCoroutine(loadcheck());
		}
	}

	public void ReadFile(string filePath) {
		string[] allText = File.ReadAllLines (filePath);
				
		foreach (var line in allText) {
			if (!line.Trim().StartsWith("#") || line.Trim()!=""){
				//dataset読み込みのための整形
				string[] dataVal = line.Split('=');
				string[] dataProp = dataVal[0].Split('.');
				//基本情報の読み込み
				if((dataVal.Length > 1)&&(dataVal[1].Trim() !="")){
					if (dataProp[0].Trim() == "location") aAV_Public.basicInfo.location = dataVal[1].Trim().Trim('"').Normalize();
					if (dataProp[0].Trim() == "country") aAV_Public.basicInfo.country = dataVal[1].Trim().Trim('"').Normalize();
					if (dataProp[0].Trim() == "timezone") aAV_Public.basicInfo.timezone = dataVal[1].Trim().Trim('"');
					if (dataProp[0].Trim() == "date") {
						string[] date = dataVal[1].Trim().Trim('"').Split('/');
						if(date.Length==3){
							aAV_Public.basicInfo.year = int.Parse(date[0].Trim());
							aAV_Public.basicInfo.month = int.Parse(date[1].Trim());
							aAV_Public.basicInfo.day = int.Parse(date[2].Trim());
						}
					}
					if (dataProp[0].Trim() == "time") {
						string[] time = dataVal[1].Trim().Trim('"').Split(':');
						if(time.Length==3){
							aAV_Public.basicInfo.hour = int.Parse(time[0].Trim());
							aAV_Public.basicInfo.minute = int.Parse(time[1].Trim());
							aAV_Public.basicInfo.second= int.Parse(time[2].Trim());
						}
					}
					if (dataProp[0].Trim() == "mesh") aAV_Public.basicInfo.area= float.Parse(dataVal[1].Trim().Trim('"'))*4096;
					if (dataProp[0].Trim() == "type") {
						aAV_Public.basicInfo.type= dataVal[1].Trim().Trim('"').Substring(0, 2).ToUpper();
						string zone = dataVal[1].Trim().Trim('"').Substring(dataVal[1].Trim().Trim('"').Length - 2);
						int n;
						if(Int32.TryParse(zone, out n)){
							aAV_Public.basicInfo.zone= int.Parse(zone);
						}else{
							errorstatus.GetComponent<Text>().text +="Error : 'type' setting is incorrect.\n";
						}
						aAV_Public.center.type = aAV_Public.basicInfo.type;
						if(aAV_Public.basicInfo.type == "JP"){
							aAV_Public.center.JPRCS_zone = aAV_Public.basicInfo.zone;
						}else if(aAV_Public.basicInfo.type == "UT"){
							aAV_Public.center.UTM_zone = aAV_Public.basicInfo.zone;
						}
					}
					if (dataProp[0].Trim() == "center") {
						string[] center= dataVal[1].Trim().Trim('"').Split(',');
						if (center.Length==3){
							aAV_Public.basicInfo.center_E = double.Parse(center[0].Trim());
							aAV_Public.basicInfo.center_N = double.Parse(center[1].Trim());
							aAV_Public.basicInfo.center_H= float.Parse(center[2].Trim());
						}else{
							errorstatus.GetComponent<Text>().text +="Error : 'center' setting is incorrect. (center = "+dataVal[1].Trim()+")\n";
						}
						gis.CenterCalc();
					}
					if (dataProp[0].Trim() == "avatar") aAV_Public.basicInfo.avatar = dataVal[1].Trim().Trim('"');
					if (dataProp[0].Trim() == "avatar_height") {
						aAV_Public.basicInfo.avatar_H = float.Parse(dataVal[1].Trim().Trim('"'));
						Debug.Log("Avatar Height = "+ aAV_Public.basicInfo.avatar_H + "cm");
						avatar.transform.localScale = (aAV_Public.basicInfo.avatar_H / 176f) * Vector3.one;
						orbit_cam.pivotOffset = new Vector3(0f,1.654f*aAV_Public.basicInfo.avatar_H/176f,0f);
						orbit_cam.ResetTargetOffsets();
					}
					if (dataProp[0].Trim() == "copyright"){
						string[] dataval = dataVal[1].Trim().Split(',');
						var copy = new List<string>();
						for(int i=0; i<dataval.Length; i++){
							var addstr = "";
							if(dataval[i].Trim()[0] == '"'){
								dataval[i] = dataval[i].TrimStart();
								for(int n=i; n<dataval.Length; n++){
									i = n;
									addstr += ","+dataval[n];
									if(dataval[n][dataval[n].Length-1] == '"'){
										addstr = addstr.Remove(addstr.Length-1, 1).Remove(0, 2);
										break;
									}
								}
							}else{
								addstr = dataval[i];
							}
							copy.Add(addstr);
						}
						aAV_Public.basicInfo.copyright_W = copy[0].Trim().Normalize();
						if (copy.Count >1) {
							aAV_Public.basicInfo.copyright_N = copy[1].Trim().Normalize();
						}
					}
					if (dataProp[0].Trim() == "cesium"){
						bool check;
						if(bool.TryParse(dataVal[1].Trim().Trim('"'), out check)){
							GameObject.Find("CesiumGeoreference").SetActive(check);
							GameObject.Find("CesiumCreditSystemDefault").SetActive(check);
						}
					}
					
					//マーカーの読み込み
					if (dataProp[0].StartsWith("marker")){
						Regex reg = new Regex(@"marker\[([0-9]*?)\]");
						Match match = reg.Match(dataProp[0]);
						var i = int.Parse(match.Groups[1].Value)-1;
						while (aAV_Public.rplist.Count <= i){
							var rpset = new aAV_Public.RPoint();
							aAV_Public.rplist.Add(rpset);
						}
						if(dataVal[1].Trim() != ""){
							if (dataProp[1].Trim() == "name"){
								aAV_Public.rplist[i].name = dataVal[1].Trim().Trim('"').Normalize();
							}else if (dataProp[1].Trim() == "origin"){
								string[] origin = dataVal[1].Trim().Trim('"').Split(',');
								//unity空間のXYに変換
								if (origin.Length==3){
									double[] XY=gis.UnityXY(double.Parse(origin[0].Trim()), double.Parse(origin[1].Trim()));
									aAV_Public.rplist[i].origin_E = XY[0];
									aAV_Public.rplist[i].origin_N = XY[1];
									aAV_Public.rplist[i].origin_H = float.Parse(origin[2].Trim());
								}else{
									errorstatus.GetComponent<Text>().text +="Error : "+dataProp[0]+"."+dataProp[1]+"= "+dataVal[1].Trim()+"\n";
								}
							}else if (dataProp[1].Trim() == "color"){
								aAV_Public.rplist[i].color = dataVal[1].Trim().Trim('"');
							}else if (dataProp[1].Trim() == "visible"){
								aAV_Public.rplist[i].visible = Convert.ToBoolean(dataVal[1].Trim().Trim('"'));
							}else if (dataProp[1].Trim() == "cam_rotation"){
								string[] rotaion = dataVal[1].Trim().Trim('"').Split(',');
								if (rotaion.Length==1){
									aAV_Public.rplist[i].cam_YAW = float.Parse(rotaion[0].Trim());
									aAV_Public.rplist[i].cam_PITCH = 0f;
									aAV_Public.rplist[i].cam_ROLL = 0f;
								}else if (rotaion.Length==2){
									aAV_Public.rplist[i].cam_YAW = float.Parse(rotaion[0].Trim());
									aAV_Public.rplist[i].cam_PITCH = float.Parse(rotaion[1].Trim());
									aAV_Public.rplist[i].cam_ROLL = 0f;
								}else if (rotaion.Length==3){
									aAV_Public.rplist[i].cam_YAW = float.Parse(rotaion[0].Trim());
									aAV_Public.rplist[i].cam_PITCH = float.Parse(rotaion[1].Trim());
									aAV_Public.rplist[i].cam_ROLL = float.Parse(rotaion[2].Trim());
								}else{
									errorstatus.GetComponent<Text>().text +="Error : "+dataProp[0]+"."+dataProp[1]+"= "+dataVal[1].Trim()+"\n";
								}
							}else if (dataProp[1].Trim() == "cam_fov"){
								aAV_Public.rplist[i].cam_FOV = float.Parse(dataVal[1].Trim().Trim('"'));
							}else{
								errorstatus.GetComponent<Text>().text += "Error : "+dataProp[0]+"."+dataProp[1]+"= "+dataVal[1].Trim()+"\n";
							}
						}
					}
	
					//補助線の読み込み
					if (dataProp[0].StartsWith("line")){
						Regex reg = new Regex(@"line\[([0-9]*?)\]");
						Match match = reg.Match(dataProp[0]);
						var i = int.Parse(match.Groups[1].Value)-1;
						while (aAV_Public.linelist.Count <= i){
							var lineset = new aAV_Public.Line();
							aAV_Public.linelist.Add(lineset);
						}
						if(dataVal[1].Trim() != ""){
							if (dataProp[1].Trim() == "name"){
								aAV_Public.linelist[i].name = dataVal[1].Trim().Trim('"').Normalize();
							}else if (dataProp[1].Trim() == "marker"){
								string[] marker = dataVal[1].Trim().Trim('"').Split(',');
								aAV_Public.linelist[i].start_marker = int.Parse(marker[0].Trim());
								if ((marker.Length > 1)&&(marker[1].Trim()!="")){
									aAV_Public.linelist[i].end_marker = int.Parse(marker[1].Trim());
								}
							}else if (dataProp[1].Trim() == "angle"){
								aAV_Public.linelist[i].angle = dataVal[1].Trim().Trim('"');
							}else if (dataProp[1].Trim() == "color"){
								aAV_Public.linelist[i].color = dataVal[1].Trim().Trim('"');
							}else if (dataProp[1].Trim() == "visible"){
								aAV_Public.linelist[i].visible = Convert.ToBoolean(dataVal[1].Trim().Trim('"'));
							}else{
								errorstatus.GetComponent<Text>().text += "Error : "+dataProp[0]+"."+dataProp[1]+"= "+dataVal[1].Trim()+"\n";
							}
						}
					}
	
					//オブジェクト情報の読み込み
					if (dataProp[0].StartsWith("dataset")){
						//オブジェクト情報読み取りのための整形
						Regex reg = new Regex(@"dataset\[([0-9]*?)\]");
						Match match = reg.Match(dataProp[0]);
						int i = int.Parse(match.Groups[1].Value)-1;
						while (aAV_Public.datalist.Count <= i){
							var dataset = new aAV_Public.Dataset();
							aAV_Public.datalist.Add(dataset);
						}
						if(dataVal[1].Trim() != ""){
							if (dataProp[1].Trim() == "name"){
								aAV_Public.datalist[i].name = dataVal[1].Trim().Trim('"').Normalize();
							}else if (dataProp[1].Trim() == "file"){
								aAV_Public.datalist[i].file = dataVal[1].Trim().Trim('"').Normalize();
							}else if (dataProp[1].Trim() == "type"){
								aAV_Public.datalist[i].type = dataVal[1].Trim().Trim('"').Normalize().ToLower();
							}else if (dataProp[1].Trim() == "origin"){
								string[] origin = dataVal[1].Trim().Trim('"').Split(',');
								if (origin.Length==3){
									//unity空間のXYに変換
									double[] XY=gis.UnityXY(double.Parse(origin[0].Trim()), double.Parse(origin[1].Trim()));
									aAV_Public.datalist[i].origin_E =XY[0];
									aAV_Public.datalist[i].origin_N = XY[1];
									aAV_Public.datalist[i].origin_H = float.Parse(origin[2].Trim());
								}else{
									errorstatus.GetComponent<Text>().text +="Error : "+dataProp[0]+"."+dataProp[1]+"= "+dataVal[1].Trim()+"\n";
								}
							}else if (dataProp[1].Trim() == "rotation"){
								string[] rotaion = dataVal[1].Trim().Trim('"').Split(',');
								if (rotaion.Length==1){
									aAV_Public.datalist[i].rot_H = float.Parse(rotaion[0].Trim());
									aAV_Public.datalist[i].rot_N = 0f;
									aAV_Public.datalist[i].rot_E = 0f;
								}else if (rotaion.Length==3){
									aAV_Public.datalist[i].rot_H = float.Parse(rotaion[2].Trim());
									aAV_Public.datalist[i].rot_N = float.Parse(rotaion[1].Trim());
									aAV_Public.datalist[i].rot_E = float.Parse(rotaion[0].Trim());
								}else{
									errorstatus.GetComponent<Text>().text +="Error : "+dataProp[0]+"."+dataProp[1]+"= "+dataVal[1].Trim()+"\n";
								}
							}else if (dataProp[1].Trim() == "scale"){
								string[] scale = dataVal[1].Trim().Trim('"').Split(',');
								if (scale.Length==1){
									aAV_Public.datalist[i].scale_E = float.Parse(scale[0].Trim());
									aAV_Public.datalist[i].scale_N = float.Parse(scale[0].Trim());
									aAV_Public.datalist[i].scale_H = float.Parse(scale[0].Trim());
								}else if (scale.Length==3){
									aAV_Public.datalist[i].scale_E = float.Parse(scale[0].Trim());
									aAV_Public.datalist[i].scale_N = float.Parse(scale[1].Trim());
									aAV_Public.datalist[i].scale_H = float.Parse(scale[2].Trim());
								}else{
									errorstatus.GetComponent<Text>().text +="Error : "+dataProp[0]+"."+dataProp[1]+"= "+dataVal[1].Trim()+"\n";
								}
							}else if (dataProp[1].Trim() == "exist"){
								string[] exist = dataVal[1].Trim().Trim('"').Split(',');
								if(exist.Length>=2){
									aAV_Public.datalist[i].end = exist[1].Trim();
								}
								if(exist.Length>=1){
									aAV_Public.datalist[i].start = exist[0].Trim();
								}
							}else if (dataProp[1].Trim() == "visible"){
								aAV_Public.datalist[i].visible = Convert.ToBoolean(dataVal[1].Trim().Trim('"'));
							}else if (dataProp[1].Trim() == "copyright"){
								aAV_Public.datalist[i].copyright = dataVal[1].Trim().Trim('"').Normalize();
							}else{
								errorstatus.GetComponent<Text>().text +="Error : "+dataProp[0]+"."+dataProp[1]+"= "+dataVal[1].Trim()+"\n";
							}
						}
					}
				}
			}
		}
	}
	
	private void CheckRequire(){		//必須項目の確認
		if((aAV_Public.basicInfo.type != "WG")&&(aAV_Public.basicInfo.type != "JP")&&(aAV_Public.basicInfo.type != "UT")){
			errorstatus.GetComponent<Text>().text +="Error : 'type' setting not found.\n";
		}
		if((aAV_Public.basicInfo.center_E == 0d)&&(aAV_Public.basicInfo.center_N == 0d)&&(aAV_Public.basicInfo.center_H == 0f)){
			errorstatus.GetComponent<Text>().text +="Error : 'center' setting not found.\n";
		}
		if(File.Exists(aAV_Public.basicInfo.filedir+"terrain/terrain00.raw")&&(aAV_Public.basicInfo.area==0f)){
			errorstatus.GetComponent<Text>().text +="Error : 'mesh' setting not found.\n";
		}
		for(int i = 0 ; i < aAV_Public.rplist.Count; i++){
			if((aAV_Public.rplist[i].origin_E == 0d)&&(aAV_Public.rplist[i].origin_N == 0d)&&(aAV_Public.rplist[i].origin_H == 0f)){
				errorstatus.GetComponent<Text>().text +="Error : 'marker["+(i+1)+"].origin' setting not found.\n";
			}
		}
		for(int i = 0 ; i < aAV_Public.linelist.Count; i++){
			if((aAV_Public.linelist[i].start_marker == 0)){
				errorstatus.GetComponent<Text>().text +="Error : 'line["+(i+1)+"].start_marker' setting not found.\n";
			}
		}
		for(int i = 0 ; i < aAV_Public.datalist.Count; i++){
			if((aAV_Public.datalist[i].file == "")&&((aAV_Public.datalist[i].type == "")||(aAV_Public.datalist[i].type == "normal"))){
				errorstatus.GetComponent<Text>().text +="Error : 'dataset["+(i+1)+"].file' setting not found.\n";
			}
			if((aAV_Public.datalist[i].origin_E == 0d)&&(aAV_Public.datalist[i].origin_N == 0d)&&(aAV_Public.datalist[i].origin_H == 0f)){
				errorstatus.GetComponent<Text>().text +="Error : 'dataset["+(i+1)+"].origin' setting not found.\n";
			}
		}
	}

	public void SetMarker(){		//Maker配置
		//マーカーのGameObjectを作成
		for(int i = 0 ; i < aAV_Public.rplist.Count; i++){
			//マーカーをprefabから作成
			GameObject marker = Instantiate(aav_public.markerPrefab) as GameObject;
			aAV_Public.rplist[i].gameobject = marker;
			marker.name = "Marker"+(i+1).ToString();

			//マーカーの色を設定
			Color color = default(Color);
			Renderer r = marker.GetComponent<Renderer>();
			r.material.EnableKeyword("_EMISSION");
			ColorUtility.TryParseHtmlString(aAV_Public.rplist[i].color, out color);
			r.material.SetColor("_EmissionColor", color);

			//マーカーの降下量補正を行い設置
			double[] down = gis.downXY(aAV_Public.rplist[i].origin_E,aAV_Public.rplist[i].origin_N);
			marker.transform.position = new Vector3((float)aAV_Public.rplist[i].origin_E, aAV_Public.rplist[i].origin_H-(float)down[0], (float)aAV_Public.rplist[i].origin_N);
			marker.SetActive(aAV_Public.rplist[i].visible);

			Debug.Log("Marker["+i+"]：E="+aAV_Public.rplist[i].origin_E.ToString()+", N="+aAV_Public.rplist[i].origin_N.ToString()+", down="+down[0].ToString());
		}
	}
	
	public void SetLine(){		//Auxiliary Line配置
		//LineのGameObjectを作成
		for(int i = 0 ; i < aAV_Public.linelist.Count; i++){
			int startNo;
			int endNo;
			Vector3 startPos;
			Vector3 endPos;
			Color color = default(Color);
			ColorUtility.TryParseHtmlString(aAV_Public.linelist[i].color, out color);
			LineRenderer lineRenderer;
			aAV_Lines aav_lines;
			if(aAV_Public.linelist[i].start_marker>0){
				aAV_Public.linelist[i].startObj= aAV_Public.rplist[aAV_Public.linelist[i].start_marker-1].gameobject;
			}
			if(aAV_Public.linelist[i].end_marker>0){
				aAV_Public.linelist[i].endObj= aAV_Public.rplist[aAV_Public.linelist[i].end_marker-1].gameobject;
			}
			
			//GameObjectを作成
			GameObject lineObj = new GameObject("Lines"+(i+1).ToString());
			aAV_Public.linelist[i].gameobject = lineObj;

			startPos = aAV_Public.linelist[i].startObj.transform.position;
			if(aAV_Public.linelist[i].endObj){	//マーカー指定の場合
				//Lineをprefabから作成
				GameObject lines = Instantiate(aav_public.linePrefab) as GameObject;
				lines.transform.parent = lineObj.transform;

				//色の設定
				lineRenderer = lines.GetComponent<LineRenderer>();
				lineRenderer.material.EnableKeyword("_EMISSION");
				lineRenderer.material.SetColor("_EmissionColor", color);

				//Lineの描画
				lineRenderer.numCapVertices = 10;
				lineRenderer.SetPosition(0, startPos);
				endPos = aAV_Public.linelist[i].endObj.transform.position;
				lineRenderer.SetPosition(1, endPos);
				
				//markerのgameobject保存
				aav_lines = lines.GetComponent<aAV_Lines>();
				aav_lines.startMarker = aAV_Public.linelist[i].startObj;
				aav_lines.endMarker = aAV_Public.linelist[i].endObj;
			}else{	//角度指定の場合
				float distance = 100000f;
				string[] anglelist = aAV_Public.linelist[i].angle.Trim().Split(',');
				for(int lineNo = 0 ; lineNo < anglelist.Length; lineNo++){
					var endX = Math.Sin(float.Parse(anglelist[lineNo]) * Math.PI / 180f)*distance;
					var endY = Math.Cos(float.Parse(anglelist[lineNo]) * Math.PI / 180f)*distance;

					//Lineをprefabから作成
					GameObject lines = Instantiate(aav_public.linePrefab) as GameObject;
					lines.transform.parent = lineObj.transform;

					//色の設定
					lineRenderer = lines.GetComponent<LineRenderer>();
					lineRenderer.material.EnableKeyword("_EMISSION");
					lineRenderer.material.SetColor("_EmissionColor", color);

					//Lineの描画
					lineRenderer.numCapVertices = 10;
					lineRenderer.SetPosition(0, startPos);
					endPos = new Vector3((float)endX , 0, (float)endY );
					lineRenderer.SetPosition(1, startPos+endPos);

					//endPositionの保存
					aav_lines = lines.GetComponent<aAV_Lines>();
					aav_lines.startMarker = aAV_Public.linelist[i].startObj;
					aav_lines.endAngle = endPos;
				}
			}
		}
	}

	public void LoadAvatar(){		//Avatar読込
		if(aAV_Public.basicInfo.avatar != ""){
			var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
			assetLoaderOptions.AnimationType = AnimationType.Humanoid;
			assetLoaderOptions.HumanoidAvatarMapper = _humanoidAvatarMapper;
			Debug.Log("Avatar Load="+aAV_Public.basicInfo.filedir+"object/"+aAV_Public.basicInfo.avatar);
			AssetLoader.LoadModelFromFile(aAV_Public.basicInfo.filedir+"object/"+aAV_Public.basicInfo.avatar, null, delegate(AssetLoaderContext assetLoaderContext) {
				if (assetLoaderContext.RootGameObject != null)
				{
					//古いAvatarを削除し、新しいAvatarに置き換え
					var existingInnerAvatar = aav_public.InnerAvatar;
					if (existingInnerAvatar != null)
					{
						Destroy(existingInnerAvatar);
					}
					aav_public.InnerAvatar = assetLoaderContext.RootGameObject;
					assetLoaderContext.RootGameObject.transform.SetParent(avatar.transform, false);
					
					//Avatarのサイズを調整（基本身長は176cm、視位置は165.4cm）
					if(aAV_Public.basicInfo.avatar_H == 0f){
						var bounds = assetLoaderContext.RootGameObject.CalculateBounds();
						aAV_Public.basicInfo.avatar_H = bounds.size.y*100;
					}
					
					//Animatorに読み込んだAvatarをセット
					avatar.GetComponent<Animator>().avatar = assetLoaderContext.RootGameObject.GetComponent<Animator>().avatar;
					
				}
			}, null, null, null, assetLoaderOptions, null);
		}
	}

	public void LoadObj(){		//3DObject読込
		obj_count = 0;
		AssetLoad();
	}

	public void AssetLoad(){		//3DObject読込
		var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
		//TriLib2の読み込みオプション設定
//		assetLoaderOptions.UseFileScale = true;									//true：ファイルの元のスケールを使用
//		assetLoaderOptions.ScaleFactor = 1f;										//1f：モデルスケール乗数
		assetLoaderOptions.SortHierarchyByName = false;					//true：モデル階層を名前で並べ替え
//		assetLoaderOptions.ImportVisibility = false;								//true：メッシュレンダラー/スキンメッシュレンダラーに可視性プロパティを適用
		assetLoaderOptions.Static = true;												//false：移動できないObjectかどうか 
//		assetLoaderOptions.AddAssetUnloader = true;						//true：アセットアンローダーコンポーネントを追加し、リソースを自動的に割り当て解除する
//		assetLoaderOptions.ImportMeshes = true;								//true：モデルメッシュをインポート。falseにするとモデルが消滅。必須
//		assetLoaderOptions.LimitBoneWeights = true;							//true：非推奨
//		assetLoaderOptions.ReadEnabled = true;									//true：メッシュにアクセス。アクセスするので必須。
//		assetLoaderOptions.ReadAndWriteEnabled = false;					//false：読み書き用に最適化
//		assetLoaderOptions.OptimizeMeshes = true;							//true：メッシュをGPUに最適化
		assetLoaderOptions.GenerateColliders = true;							//false：コライダー作成。必須。
		assetLoaderOptions.ConvexColliders= false;							//false：trueにすると隙間に入れなくなる。必須。
//		assetLoaderOptions.ImportBlendShapes = true;						//true：メッシュブレンドシェイプをインポート
//		assetLoaderOptions.ImportColors = true;									//true：メッシュの色をインポート
//		assetLoaderOptions.IndexFormat=indexFormat.UInt32;			//indexFormat.UInt32：メッシュの上限ビット数。非推奨
//		assetLoaderOptions.LODScreenRelativeTransitionHeightBase = 0.75f;	//0.75f：初期画面の相対遷移高さ
//		assetLoaderOptions.KeepQuads = false;									//false：DX11用
//		assetLoaderOptions.ImportNormals = true;								//true：メッシュノーマルをインポート
//		assetLoaderOptions.SmoothingAngle = 60f;								//60f：スムージング角度
//		assetLoaderOptions.ImportBlendShapeNormals = true;			//false; メッシュブレンドシェイプノーマルをインポート
//		assetLoaderOptions.CalculateBlendShapeNormals = true;		//false：メッシュブレンドシェイプのノーマルを計算
//		assetLoaderOptions.ImportTangents = true;								//false：メッシュタンジェントをインポート
//		assetLoaderOptions.SwapUVs= false;										//false：メッシュUVを交換
//		assetLoaderOptions.ImportMaterials = true;								//true：falseにするとテクスチャ消滅。必須。
//		assetLoaderOptions.AddSecondAlphaMaterial = false;				//false：半不透明/半透明マテリアル。非推奨
//		assetLoaderOptions.ImportTextures = true;								//true：テクスチャをインポート。必須。
//		assetLoaderOptions.Enforce16BitsTextures = false;					//false：テクスチャを16ビットHDRとしてインポート。非推奨
//		assetLoaderOptions.ScanForAlphaPixels = false;						//true：アルファブレンドピクセルをスキャンし、透明なマテリアルを生成
//		assetLoaderOptions.UseAlphaMaterials = false;						//false：アルファ(透明)マテリアル。非推奨
//		assetLoaderOptions.DoubleSidedMaterials = false;					//false：両面マテリアル
//		assetLoaderOptions.TextureCompressionQuality = TextureCompressionQuality.Normal;	//TextureCompressionQuality.Normal：テクスチャ圧縮
//		assetLoaderOptions.GenerateMipmaps = true;							//true：テクスチャのミップマップ生成。高速化に寄与
//		assetLoaderOptions.FixNormalMaps = true;								//false：マップチャンネルの順序をRGBAではなくABBRに変更
//		assetLoaderOptions.AnimationType = AnimationType.Legacy;	//AnimationType.Legacy：モデルリギングタイプ
//		assetLoaderOptions.SampleBindPose = false;							//false：バインドポーズにサンプリング
//		assetLoaderOptions.EnforceTPose = true;									//true：Tポーズに強制
//		assetLoaderOptions.ResampleAnimations = false;					//false：アニメーション曲線を再サンプリング。非推奨
//		assetLoaderOptions.EnforceAnimatorWithLegacyAnimations = false;	//false：nimationTypeがレガシーに設定されている場合、アニメーターを追加
//		assetLoaderOptions.AutomaticallyPlayLegacyAnimations = false;			//false：自動的に再生
//		assetLoaderOptions.ResampleFrequency = 4f;							//4f：FBX回転アニメーション曲線の再サンプリング周波数
//		assetLoaderOptions.AnimationWrapMode = WrapMode.Loop;	// WrapMode.Loop：アニメーションに適用するラップモード。
//		assetLoaderOptions.ShowLoadingWarnings = true;					//false：モデル読み込み警告表示
//		assetLoaderOptions.CloseStreamAutomatically = true;				//true：モデルローディングストリームが自動的に閉じられます。
//		assetLoaderOptions.Timeout = 180;											//180：ロードタイムアウト(秒単位)
//		assetLoaderOptions.DestroyOnError = false;								//true：読み込みエラーが発生した場合にゲームオブジェクトを自動的に破棄
//		assetLoaderOptions.EnsureQuaternionContinuity = true;			//true：四元数キーを再調整し、最短補間パスを確保;
//		assetLoaderOptions.UseMaterialKeywords = true;					//false：シェーダーキーワードを使用
//		assetLoaderOptions.ForceGCCollectionWhileLoading = true;		//true：GCコレクションを強制し、メモリを速やかに解放
//		assetLoaderOptions.MergeVertices = true;								//true：重複した頂点をマージ。falseだと早く読み込むがエラーをおこす。必須？
//		assetLoaderOptions.MarkTexturesNoLongerReadable = true;	//true：テクスチャを読み取り不可能に設定し、メモリリソースを解放
//		assetLoaderOptions.UseUnityNativeNormalCalculator = false;	//false：組み込みのUnity正規計算機を使用
//		assetLoaderOptions.GCHelperCollectionInterval = 20f;				//20f：GCHelperクラスが実行される時間(秒単位)。
//		assetLoaderOptions.LoadTexturesAsSRGB = true;						//true：テクスチャをsRGBではなく線形としてロード
//		assetLoaderOptions.ApplyTexturesOffsetAndScaling = true;		//true：テクスチャのオフセットとスケーリングを適用
//		assetLoaderOptions.UseAutodeskInteractiveMaterials= false;	//false：オートデスクインタラクティブマテリアルを使用。非推奨
//		assetLoaderOptions.DiscardUnusedTextures = true;					//true：未使用のテクスチャを保持しない
//		assetLoaderOptions.ForcePowerOfTwoTextures= false;			//false：テクスチャを読み込むときに2つの解像度を強制
//		assetLoaderOptions.EnableProfiler= true;								//false：モデルの読み込み中にプロファイラメッセージが表示。｡
//		assetLoaderOptions.UseUnityNativeTextureLoader= true;		//false：Unity組み込みテクスチャローダーを使用
		assetLoaderOptions.LoadMaterialsProgressively = true;			//false：マテリアルを徐々にロード。非同期メソッド読み込みで反応が早くなる。
//		assetLoaderOptions.ImportCameras = false;								//false：カメラの読み込みを有効
//		assetLoaderOptions.ImportLights = false;									//false：ライトのインポートを有効
//		assetLoaderOptions.DisableObjectsRenaming = false;				//false：オブジェクトの名前変更を無効

		if(obj_count < aAV_Public.datalist.Count){
			string[] filename = aAV_Public.datalist[obj_count].file.Split('.');
			GameObject typeObj;
			if(filename.Length <= 1){		//file名に拡張子がない場合（type番号等の場合）
				if(aAV_Public.datalist[obj_count].type == "water"){	//prefabからwater planeを作成（file指定がある場合は、OnMaterialsLoad内でObjのマテリアルをwaterに変更
					switch(filename[0]){
						case "2":
							typeObj = Instantiate(aav_public.water2Prefab) as GameObject;
							break;
						default:
							typeObj = Instantiate(aav_public.water1Prefab) as GameObject;
							break;
					}
					typeObj.transform.parent = GameObject.Find("Water").transform;
					aAV_Public.datalist[obj_count].gameobject = typeObj;
					SetObject(obj_count);
					obj_count += 1;
					AssetLoad();
				}else if(aAV_Public.datalist[obj_count].type == "fire"){	//prefabからfire planeを作成
					switch(filename[0]){
						case "2":
							typeObj = Instantiate(aav_public.fire2Prefab) as GameObject;
							break;
						case "3":
							typeObj = Instantiate(aav_public.fire3Prefab) as GameObject;
							break;
						case "4":
							typeObj = Instantiate(aav_public.fire4Prefab) as GameObject;
							break;
						case "5":
							typeObj = Instantiate(aav_public.fire5Prefab) as GameObject;
							break;
						default:
							typeObj = Instantiate(aav_public.fire1Prefab) as GameObject;
							break;
					}
					aAV_Public.datalist[obj_count].gameobject = typeObj;
					SetObject(obj_count);
					obj_count += 1;
					AssetLoad();
				}else if(aAV_Public.datalist[obj_count].type == "mirror"){	//prefabからmirror planeを作成（file指定がある場合は、OnMaterialsLoad内でObjのマテリアルをmirrorに変更
					switch(filename[0]){
						case "2":
							typeObj = Instantiate(aav_public.mirror2Prefab) as GameObject;
							break;
						default:
							typeObj = Instantiate(aav_public.mirror1Prefab) as GameObject;
							break;
					}
					typeObj.transform.parent = GameObject.Find("Mirror").transform;
					aAV_Public.datalist[obj_count].gameobject = typeObj;
					SetObject(obj_count);
					ms = typeObj.GetComponent<MirrorSurface>();
					GameObject.Find("Mirror").GetComponent<MirrorRenderer>().mirrorSurfaces.Add(ms);
					obj_count += 1;
					AssetLoad();
				}
			}else if(aAV_Public.datalist[obj_count].type != "fire"){
				AssetLoader.LoadModelFromFile(aAV_Public.basicInfo.filedir+"object/"+aAV_Public.datalist[obj_count].file, OnLoad, OnMaterialsLoad, OnProgress, OnError, null, assetLoaderOptions);
			}
		}
	}
	
	private void OnLoad(AssetLoaderContext assetLoaderContext)
	{
		//Debug.Log("Model loaded. Loading materials.");
	}

	private void OnMaterialsLoad(AssetLoaderContext assetLoaderContext)
	{
		//Debug.Log("Materials loaded. Model fully loaded.");
		GameObject.Find("load_object").GetComponent<Text>().text = "Object loading："+ aAV_Public.datalist[obj_count].name+" (100%)";

		//読み込んだ3Dの最上位GameObjectを取得
		GameObject myGameObject = assetLoaderContext.RootGameObject;
		
		//読み込んだ3Dのdatalist番号を取得し、GameObjectを記録
		int n =0;
		for(int i = 0; i<aAV_Public.datalist.Count; i++){
			//すでに同じfile名のgameobjectを格納していないか、file名に拡張子がついているか（type用の指定番号に使われていないか）をチェック
			string[] filename = aAV_Public.datalist[i].file.Split('.');
			if (assetLoaderContext.Filename.Contains(aAV_Public.datalist[i].file)&&(aAV_Public.datalist[i].gameobject == null)&&(filename.Length >1)){
				n = i;
				break;
			}
		}
		aAV_Public.datalist[n].gameobject = myGameObject;
		SetObject(n);

		//MeshRendererのあるgameObjectに、著作権表示スクリプトを追加
		for (int i = 0; i < myGameObject.transform.childCount; i++)
		{
			if(myGameObject.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>() != null){
				myGameObject.transform.GetChild(i).gameObject.AddComponent<aAV_Copyright>();
			}
		}

		//時系列フラグによる表示コントロール
		bool timeVisible = true;
		if(aAV_Public.datalist[n].start != ""){
			if(aAV_Public.basicInfo.year < int.Parse(aAV_Public.datalist[n].start)){
				timeVisible = false;
			}
		}
		if(aAV_Public.datalist[n].end!= ""){
			if(aAV_Public.basicInfo.year > int.Parse(aAV_Public.datalist[n].end)){
				timeVisible = false;
			}
		}
		
		//表示フラグによる表示コントロール
		myGameObject.SetActive(aAV_Public.datalist[n].visible && timeVisible);
		
		//3D読み込み終了カウント
		obj_count += 1;
		AssetLoad();
	}

	private void OnProgress(AssetLoaderContext assetLoaderContext, float progress)
	{
		GameObject loadstatus = GameObject.Find("load_object");
		
		if(obj_count < aAV_Public.datalist.Count){
			loadstatus.GetComponent<Text>().text = "Object  loading："+ aAV_Public.datalist[obj_count].name+" ("+(int)(progress*100)+"%)";
		}
	}

	private void OnError(IContextualizedError obj)
	{
		AssetLoaderContext assetLoaderContext = (TriLibCore.AssetLoaderContext)obj.GetContext();
		errorstatus.GetComponent<Text>().text +=　"Model loading Error："+assetLoaderContext.Filename+"\n";
		Debug.LogError($"An error occurred while loading your Model: {obj.GetInnerException()}");
		Debug.LogError($"Model loading Error："+assetLoaderContext.Filename);
	}


	public void SetObject(int n){
		GameObject myGameObject = aAV_Public.datalist[n].gameobject;
		myGameObject.name = "dataset"+(n+1).ToString();
		Transform myTransform = myGameObject.transform;

		//3Dの原点座標における降下量・傾きを計算
		double[] down = gis.downXY(aAV_Public.datalist[n].origin_E, aAV_Public.datalist[n].origin_N);

		//3Dを指定の座標で降下させて配置
		Vector3 pos = myTransform.position;
		pos.x = (float)aAV_Public.datalist[n].origin_E;	//E方向
		pos.z = (float)aAV_Public.datalist[n].origin_N;	//N方向
		pos.y = aAV_Public.datalist[n].origin_H - (float)down[0];	//H方向
		myTransform.position = pos; // 位置を設定

		//3Dを傾ける
		Vector3 worldAngle = myTransform.eulerAngles;
		worldAngle.x = aAV_Public.datalist[n].rot_E + (float)down[2];
		worldAngle.z = aAV_Public.datalist[n].rot_N; // + (float)EN[2]; //真北偏差
		worldAngle.y = aAV_Public.datalist[n].rot_H + (float)down[1];
		myTransform.eulerAngles = worldAngle; // 回転角度を設定
		
		//3Dのスケールを調整
		Vector3 localScale = myTransform.localScale;
		localScale.x = aAV_Public.datalist[n].scale_E; 
		localScale.y = aAV_Public.datalist[n].scale_H; 
		localScale.z = aAV_Public.datalist[n].scale_N; 
		myTransform.localScale = localScale; // スケールを設定

		Debug.Log(aAV_Public.datalist[n].name+"：標高降下="+down[0].ToString()+", E方向傾き="+down[1].ToString()+", N方向傾き="+down[2].ToString());
	}
	
	IEnumerator LoadTerrain()
	{
		int terrainResolution = 4096;
		int terrainSize = 100000;
		int terrainHeight = 10000;
		int terrainBottom = -1000;
		
		Terrain terrain;
		TerrainCollider terrainCollider;
		GameObject loadstatus = GameObject.Find("load_terrain");
		var loading = loadstatus.GetComponent<Text>();
		if(cesiumObj.activeSelf){
			loading.text = "Terrain loading：Cesium World Terrain";
		}else{
			loading.text = "Terrain loading：Terrain not found";
		}
		
		List<string> terrainList = new List<string>() {"terrain11", "terrain12", "terrain13", "terrain21", "terrain22", "terrain23", "terrain31", "terrain32", "terrain33"};
		if(File.Exists(aAV_Public.basicInfo.filedir+"terrain/terrain00.raw")){
			terrainList.Insert(0, "terrain00");
		}
				
		foreach(string terrainName in terrainList){
			if(aAV_Public.basicInfo.filedir != null)
			{
				string rawName = aAV_Public.basicInfo.filedir+"terrain/"+terrainName+".raw";
				string texName = aAV_Public.basicInfo.filedir+"terrain/"+terrainName+".jpg";
				bool m_FlipVertically = true;
				float normalize = 1.0F / (1 << 16);
				FileInfo file = new FileInfo(rawName);
				if(file.Exists){
					int heightmapRes = (int)System.Math.Sqrt(file.Length/2);
					Debug.Log("Loading="+ terrainName+" Resolution="+heightmapRes);
	
					//rawファイルの読み込み
					TerrainData terrainData = new TerrainData();
					terrainData.heightmapResolution = heightmapRes;
					terrainData.baseMapResolution = terrainResolution;
					GameObject terrainObj = Terrain.CreateTerrainGameObject(terrainData);
					terrain =terrainObj.GetComponent<Terrain>();
					terrainObj.name = terrainName;
					terrainObj.AddComponent<MeshRenderer>();
					terrainObj.AddComponent<aAV_Copyright>();
					using (BinaryReader br = new BinaryReader(File.Open(rawName, FileMode.Open, FileAccess.Read)))
					{
						byte[] rawdata;
						rawdata = br.ReadBytes(heightmapRes * heightmapRes * 2);
						br.Close();
						
					
						float[,] heights = new float[heightmapRes, heightmapRes];
						for (int y = 0; y < heightmapRes; ++y)
						{
							for (int x = 0; x < heightmapRes; ++x)
							{
								int index = Mathf.Clamp(x, 0, heightmapRes - 1) + Mathf.Clamp(y, 0, heightmapRes - 1) * heightmapRes;
								ushort compressedHeight = System.BitConverter.ToUInt16(rawdata, index * 2);
								float height = compressedHeight * normalize;
								int destY = m_FlipVertically ? heightmapRes - 1 - y : y;
								heights[destY, x] = height;
							}
							if(y % 500 == 0){
								loading.text = "Terrain loading："+ terrainName +" ("+(y*100/heightmapRes).ToString()+"%)";
								yield return null;
							}
						}
	
						if(terrainName == "terrain00"){
							terrainObj.transform.position = new Vector3(-(int)(aAV_Public.basicInfo.area/2), terrainBottom, -(int)(aAV_Public.basicInfo.area/2));
							terrain.terrainData.size = new Vector3((int)aAV_Public.basicInfo.area, terrainHeight, (int)aAV_Public.basicInfo.area);
						}else{
							float tileY=float.Parse(terrainName.Substring(7,1));
							float tileX=float.Parse(terrainName.Substring(8,1));
							terrainObj.transform.position = new Vector3((int)((-2.5 + tileX)*terrainSize), terrainBottom, (int)((1.5-tileY)*terrainSize));
							terrain.terrainData.size = new Vector3(terrainSize, terrainHeight, terrainSize);
						}
						terrain.drawInstanced = true;
						terrain.basemapDistance = 20000;
						terrain.shadowCastingMode = ShadowCastingMode.On;
						Material LitMaterial = new Material(Shader.Find("Universal Render Pipeline/Terrain/Lit"));
						terrain.materialTemplate = LitMaterial;
	//					Debug.Log(terrain.materialTemplate);
	
						//TerrainにHeightMap、Colliderをセット
						terrain.terrainData.SetHeights(0, 0, heights);
						terrainCollider = terrainObj.GetComponent<TerrainCollider>();
						terrainCollider.terrainData = terrain.terrainData;
	
						// テクスチャを読み込み
						if(File.Exists(texName)){
							using (BinaryReader bin = new BinaryReader(File.Open(texName, FileMode.Open, FileAccess.Read)))
							{
								byte[] texdata;
								texdata= bin.ReadBytes((int)bin.BaseStream.Length);
								bin.Close();
								Texture2D tex = new Texture2D(1, 1);
								tex.LoadImage(texdata);
								
								TerrainLayer[] tlayers = new TerrainLayer[1];
								tlayers[0] = new TerrainLayer();
								tlayers[0].diffuseTexture = tex;
								tlayers[0].metallic = 0f;
								tlayers[0].smoothness = 0f;
								tlayers[0].tileOffset = new Vector2();
								if(terrainName == "terrain00"){
									//terrain00のテクスチャ タイリングサイズ調整
									tlayers[0].tileSize = new Vector2((int)aAV_Public.basicInfo.area,(int)aAV_Public.basicInfo.area);
								}else{
									tlayers[0].tileSize = new Vector2(terrainSize, terrainSize);
								}
								terrain.terrainData.terrainLayers = tlayers;
	
								//Terrain読み込み終了カウント
								terrain_count += 1;
							}
						}else{
							//Terrain読み込み終了カウント
							terrain_count += 1;
						}
						
						loading.text = "Terrain loading："+ terrainName +" (100%)";
						yield return null;
					}
				}else{
					//Terrain読み込み終了カウント
					terrain_count += 1;
				}
			}
		}
	}

	private IEnumerator loadcheck() {
		int terrainNo = 9;
		if(File.Exists(aAV_Public.basicInfo.filedir+"terrain/terrain00.raw")){
			terrainNo += 1;
		}
		while(true){
			if( terrain_count + obj_count >= aAV_Public.datalist.Count + terrainNo){
				startMain();
				break;
			}
			yield return new WaitForSeconds(.2f);
		}
	}

	public void startMain(){
		//タイトルとロードメッセージ画面を消し、Menuをアクティブにする。
		GameObject.Find("Main").transform.Find("Menu").gameObject.SetActive(true);
		GameObject.Find("XR Origin").transform.Find("Camera Offset/Main Camera").gameObject.SetActive(true);
		
		//観測者高度をcenterに設定
		avatar.SetActive(true);
		avatar.transform.position = new Vector3(0f, aAV_Public.basicInfo.center_H+100, 0f);
		GameObject.Find("Load").SetActive(false);
	}

	private Bounds CalcBounds(GameObject obj, Bounds bounds)
	{
		// 指定オブジェクトの全ての子オブジェクトをチェックする
		foreach (Transform child in obj.transform)
		{
			// メッシュフィルターの存在確認
			MeshFilter filter = child.gameObject.GetComponent<MeshFilter>();

			if (filter != null)
			{
				// オブジェクトのワールド座標とサイズを取得する
				Vector3 ObjWorldPosition = child.position;
				Vector3 ObjWorldScale = child.lossyScale;

				// フィルターのメッシュ情報からバウンドボックスを取得する
				Bounds meshBounds = filter.mesh.bounds;

				// バウンドのワールド座標とサイズを取得する
				Vector3 meshBoundsWorldCenter = meshBounds.center + ObjWorldPosition;
				Vector3 meshBoundsWorldSize = Vector3.Scale(meshBounds.size, ObjWorldScale);

				// バウンドの最小座標と最大座標を取得する
				Vector3 meshBoundsWorldMin = meshBoundsWorldCenter - (meshBoundsWorldSize / 2);
				Vector3 meshBoundsWorldMax = meshBoundsWorldCenter + (meshBoundsWorldSize / 2);

				// 取得した最小座標と最大座標を含むように拡大/縮小を行う
				if (bounds.size == Vector3.zero)
				{
					// 元バウンドのサイズがゼロの場合はバウンドを作り直す
					bounds = new Bounds(meshBoundsWorldCenter, Vector3.zero);
				}
				bounds.Encapsulate(meshBoundsWorldMin);
				bounds.Encapsulate(meshBoundsWorldMax);
			}

			// 再帰処理
			bounds = CalcBounds(child.gameObject, bounds);
		}
		return bounds;
	}
	
}

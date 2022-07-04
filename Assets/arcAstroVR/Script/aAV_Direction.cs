using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TriLibCore;
using TriLibCore.SFB;

public class aAV_Direction : MonoBehaviour
{
	[SerializeField]
	private GameObject rpPrefab;
	[SerializeField]
	private GameObject lnPrefab;
	[SerializeField]
	private GameObject obPrefab;

	public AudioClip audioCopy;
	public string markerName ="";
	
	private aAV_CustomDateTime customDateTime;
	private aAV_FileSet aav_fileset;
	private aAV_Public aav_public;
	private aAV_GIS gis;
	private Camera mainCamera;
	private GameObject avator;
	private GameObject infoview;
	private GameObject scrollview;
	private GameObject setting;
	private GameObject markerEdit;
	private GameObject objectEdit;
	private GameObject lineEdit;
	private Text message;
	private Text positionText;
	private Text cursorText;
	private Text showText;
	private Text label;
	private Text info_coordinate;
	private List<Text> info_direction;
	private Text info_rotate;
	private Text info_scale;
	private Text line_marker;
	private Text line_angle;
	private Toggle view;
	private Vector3 humanPosition;
	private Vector3 eyePosition;
	private List<Vector3> marker;
	private double radian;
	private double toDeg = 180/Math.PI;
	private double[] XY = new double [2] {0d, 0d};
	private float altitude;
	private string exist;

	void Awake()
	{
		//GameObject取得
		Transform mainTrans = GameObject.Find("Main").transform;
		aav_fileset = GameObject.Find("Main").GetComponent<aAV_FileSet>();
		aav_public = GameObject.Find("Main").GetComponent<aAV_Public>();
		gis = GameObject.Find("Main").GetComponent<aAV_GIS>();
		avator = mainTrans.Find("Avatar").gameObject;
		infoview = mainTrans.Find("Menu/InfoView").gameObject;
		scrollview = mainTrans.Find("Menu/InfoView/ScrollView").gameObject;
		setting = mainTrans.Find("Menu/Setting").gameObject;
		markerEdit = mainTrans.Find("Menu/MarkerEdit").gameObject;
		objectEdit = mainTrans.Find("Menu/ObjectEdit").gameObject;
		lineEdit = mainTrans.Find("Menu/LineEdit").gameObject;
		positionText = mainTrans.Find("Menu/TopBar/positionInfo").gameObject.GetComponent<Text> ();
		cursorText = mainTrans.Find("Menu/TopBar/cursorInfo").gameObject.GetComponent<Text> ();
		showText = mainTrans.Find("Menu/TopBar/showButton/Text").gameObject.GetComponent<Text> ();
	}
	
	void Start()
	{
		mainCamera = Camera.main;
		
		//表示設定
		setting.SetActive(false);
		markerEdit.SetActive(false);
		objectEdit.SetActive(false);
		lineEdit.SetActive(false);

		//InfoViewの設定
		ViewSizeInitialize();
		ViewUpdate();
		
		//リアルタイム更新情報の開始
		StartCoroutine("Direction");
	}

	public void ViewSizeInitialize(){
		int heightsize = (aAV_Public.rplist.Count+aAV_Public.linelist.Count+aAV_Public.datalist.Count)*30+10+30+10+30+10+30+10;
		var infosize = infoview.GetComponent<RectTransform>().sizeDelta;
		if( heightsize > 500){
			heightsize = 500;
		}
		scrollview.GetComponent<RectTransform>().sizeDelta = new Vector2(infosize.x, heightsize);
		infoview.GetComponent<RectTransform>().sizeDelta = new Vector2(infosize.x, heightsize +30);
	}
	
	public void ViewUpdate(){
		if(!infoview.activeInHierarchy){
			return;
		}
		gis.CenterCalc();
		
		//InfoView：マーカーの初期設定
		Canvas.ForceUpdateCanvases();
		info_direction = new List<Text>();
		marker = new List<Vector3>();
		var parent = GameObject.Find("rpInfo").transform;
		for(int i =1; i<parent.childCount; i++){
			Destroy(parent.GetChild(i).gameObject);
		}
		for(int i = 0 ; i < aAV_Public.rplist.Count; i++){
			marker.Add(aAV_Public.rplist[i].gameobject.transform.position);

			//マーカーのGameObjectをprehubから作成
			aAV_Public.rplist[i].infoobject = (GameObject)Instantiate(rpPrefab, transform.position, Quaternion.identity, parent);
			aAV_Public.rplist[i].infoobject.GetComponent<aAV_InfoAction> ().ListNo=i;

			//マーカーの表示ON/OFFをセット
			view = aAV_Public.rplist[i].infoobject.transform.GetChild(0).gameObject.GetComponent<Toggle> ();
			view.isOn = aAV_Public.rplist[i].visible;

			//マーカーの名前をセット
			label = aAV_Public.rplist[i].infoobject.transform.Find("view/Label").gameObject.GetComponent<Text> ();
			label.text = (i+1).ToString()+":"+aAV_Public.rplist[i].name;

			//マーカーの情報(Coordinate)をセット
			double[] down = gis.downXY((double)marker[i].x, (double)marker[i].z);
			altitude = marker[i].y + (float)down[0];
			info_coordinate = aAV_Public.rplist[i].infoobject.transform.GetChild(1).gameObject.GetComponent<Text> ();
			info_direction.Add(aAV_Public.rplist[i].infoobject.transform.GetChild(2).gameObject.GetComponent<Text> ());
			if(aAV_Public.center.type=="WG"){
				XY = gis.EN2LonLat(aAV_Public.rplist[i].origin_E, aAV_Public.rplist[i].origin_N, aAV_Public.center.WGS_E, aAV_Public.center.WGS_N, 1d);
				info_coordinate.text = aAV_Public.lang.coordinate+" : "+XY[0].ToString("F6")+"°, "+XY[1].ToString("F6")+"°, "+km(altitude)+"m";
			}else if(aAV_Public.center.type=="JP"){
				XY[0]=aAV_Public.rplist[i].origin_E+aAV_Public.center.JPRCS_E;
				XY[1]=aAV_Public.rplist[i].origin_N+aAV_Public.center.JPRCS_N;
				info_coordinate.text = aAV_Public.lang.coordinate+" : "+XY[0].ToString("F2")+"m, "+XY[1].ToString("F2")+"m, "+km(altitude)+"m";
			}else if(aAV_Public.center.type=="UT"){
				XY[0]=aAV_Public.rplist[i].origin_E+aAV_Public.center.UTM_E;
				XY[1]=aAV_Public.rplist[i].origin_N+aAV_Public.center.UTM_N;
				info_coordinate.text = aAV_Public.lang.coordinate+" : "+XY[0].ToString("F2")+"m, "+XY[1].ToString("F2")+"m, "+km(altitude)+"m";
			}
		}

		//InfoView：補助線の初期設定
		Canvas.ForceUpdateCanvases();
		parent = GameObject.Find("lnInfo").transform;
		for(int i =1; i<parent.childCount; i++){
			Destroy(parent.GetChild(i).gameObject);
		}
		for(int i = 0 ; i < aAV_Public.linelist.Count; i++){
			//補助線のGameObjectをprehubから作成
			aAV_Public.linelist[i].infoobject = (GameObject)Instantiate(lnPrefab, transform.position, Quaternion.identity, parent);
			aAV_Public.linelist[i].infoobject.GetComponent<aAV_InfoAction> ().ListNo=i;

			//補助線の表示ON/OFFをセット
			view = aAV_Public.linelist[i].infoobject.transform.GetChild(0).gameObject.GetComponent<Toggle> ();
			view.isOn = aAV_Public.linelist[i].visible;

			//補助線の名前をセット
			label = aAV_Public.linelist[i].infoobject.transform.Find("view/Label").gameObject.GetComponent<Text> ();
			label.text = aAV_Public.linelist[i].name;

			//補助線の情報をセット
			line_marker = aAV_Public.linelist[i].infoobject.transform.GetChild(1).gameObject.GetComponent<Text> ();
			string s_text = "None";
			if((bool)aAV_Public.linelist[i].startObj){
				for(int n = 0; n<aAV_Public.rplist.Count; n++){
					if(aAV_Public.rplist[n].gameobject == aAV_Public.linelist[i].startObj){
						s_text = "Marker"+(n+1).ToString();
					}
				}
			}
			string e_text = "None";
			if((bool)aAV_Public.linelist[i].endObj){
				for(int n = 0; n<aAV_Public.rplist.Count; n++){
					if(aAV_Public.rplist[n].gameobject == aAV_Public.linelist[i].endObj){
						e_text = "Marker"+(n+1).ToString();
					}
				}
			}else{
				e_text = "Angle : "+aAV_Public.linelist[i].angle;
			}
			line_marker.text = s_text +" - " + e_text;
		}

		//InfoView：オブジェクトの初期設定
		Canvas.ForceUpdateCanvases();
		parent = GameObject.Find("obInfo").transform;
		for(int i =1; i<parent.childCount; i++){
			Destroy(parent.GetChild(i).gameObject);
		}
		for(int i = 0 ; i < aAV_Public.datalist.Count; i++){
			//オブジェクトのGameObjectをprehubから作成
			aAV_Public.datalist[i].infoobject = (GameObject)Instantiate(obPrefab, transform.position, Quaternion.identity, parent);
			aAV_Public.datalist[i].infoobject.GetComponent<aAV_InfoAction> ().ListNo=i;

			//オブジェクトの表示ON/OFFをセット
			view = aAV_Public.datalist[i].infoobject.transform.GetChild(0).gameObject.GetComponent<Toggle> ();
			view.isOn = aAV_Public.datalist[i].visible;

			//オブジェクトの名前をセット
			label = aAV_Public.datalist[i].infoobject.transform.Find("view/Label").gameObject.GetComponent<Text> ();
			label.text = aAV_Public.datalist[i].name;

			//オブジェクトの情報をセット
			info_coordinate = aAV_Public.datalist[i].infoobject.transform.GetChild(1).gameObject.GetComponent<Text> ();
			info_rotate = aAV_Public.datalist[i].infoobject.transform.GetChild(2).gameObject.GetComponent<Text> ();
			info_scale = aAV_Public.datalist[i].infoobject.transform.GetChild(3).gameObject.GetComponent<Text> ();
			if(aAV_Public.center.type=="WG"){
				XY = gis.EN2LonLat(aAV_Public.datalist[i].origin_E, aAV_Public.datalist[i].origin_N, aAV_Public.center.WGS_E, aAV_Public.center.WGS_N, 1d);
				info_coordinate.text = aAV_Public.lang.origin+" : "+XY[0].ToString("F6")+"°, "+XY[1].ToString("F6")+"°, "+km(aAV_Public.datalist[i].origin_H)+"m";
			}else if(aAV_Public.center.type=="JP"){
				XY[0]=aAV_Public.datalist[i].origin_E+aAV_Public.center.JPRCS_E;
				XY[1]=aAV_Public.datalist[i].origin_N+aAV_Public.center.JPRCS_N;
				info_coordinate.text = aAV_Public.lang.origin+" : "+XY[0].ToString("F2")+"m, "+XY[1].ToString("F2")+"m, "+km(aAV_Public.datalist[i].origin_H)+"m";
			}else if(aAV_Public.center.type=="UT"){
				XY[0]=aAV_Public.datalist[i].origin_E+aAV_Public.center.UTM_E;
				XY[1]=aAV_Public.datalist[i].origin_N+aAV_Public.center.UTM_N;
				info_coordinate.text = aAV_Public.lang.origin+" : "+XY[0].ToString("F2")+"m, "+XY[1].ToString("F2")+"m, "+km(aAV_Public.datalist[i].origin_H)+"m";
			}
			info_rotate.text =aAV_Public.lang.rotation+" : "+aAV_Public.datalist[i].rot_E.ToString("F2")+", "+aAV_Public.datalist[i].rot_N.ToString("F2")+", "+aAV_Public.datalist[i].rot_H.ToString("F2")+"°";

			//オブジェクトのスケール、開始年・終了年をセット
			if(aAV_Public.datalist[i].start != "" && aAV_Public.datalist[i].end != ""){
				exist = aAV_Public.datalist[i].start + "〜"+aAV_Public.datalist[i].end+"";
			}else if (aAV_Public.datalist[i].start != ""){
				exist = aAV_Public.datalist[i].start + "〜";
			}else if (aAV_Public.datalist[i].end != ""){
				exist = "〜"+aAV_Public.datalist[i].end;
			}else{
				exist = "All";
			}
			info_scale.text = aAV_Public.lang.scale+" : "+aAV_Public.datalist[i].scale.ToString("F2")+"   "+aAV_Public.lang.existences+" :"+exist;
		}

		//レイアウト計算が崩れる時があるので、強制再計算描画
//		Canvas.ForceUpdateCanvases();
//		infoview.SetActive(false);
//		infoview.SetActive(true);
	}

	//リアルタイム更新情報
	IEnumerator Direction()
	{
		while(true){
			//TopBar：現在位置のセット（人間かかと位置から計算）
			humanPosition = avator.transform.position;
			double[] down = gis.downXY((double)humanPosition.x, (double)humanPosition.z);
			altitude = humanPosition.y + (float)down[0];
			if(aAV_Public.center.type=="WG"){
				XY = gis.EN2LonLat(humanPosition.x, humanPosition.z, aAV_Public.center.WGS_E, aAV_Public.center.WGS_N, 1d);
				positionText.text = aAV_Public.lang.coordinate+" (WGS84) : "+aAV_Public.lang.lon+"="+XY[0].ToString("F6")+"°, "+aAV_Public.lang.lat+"="+XY[1].ToString("F6")+"°, "+aAV_Public.lang.height+"="+altitude+"m";
			}else if (aAV_Public.center.type=="JP"){
				XY[0] = humanPosition.x+aAV_Public.center.JPRCS_E;
				XY[1] = humanPosition.z+aAV_Public.center.JPRCS_N;
				positionText.text = aAV_Public.lang.coordinate+" (JPRCS "+aAV_Public.center.JPRCS_zone.ToString() +") : Y="+XY[0].ToString("F2")+"m, X="+XY[1].ToString("F2")+"m, "+aAV_Public.lang.height+"="+altitude+"m";
			}else if (aAV_Public.center.type=="UT"){
				XY[0] = humanPosition.x+aAV_Public.center.UTM_E;
				XY[1] = humanPosition.z+aAV_Public.center.UTM_N;
				positionText.text = aAV_Public.lang.coordinate+" (UTM "+aAV_Public.center.UTM_zone.ToString() +") : E="+XY[0].ToString("F2")+"m, N="+XY[1].ToString("F2")+"m, "+aAV_Public.lang.height+"="+altitude+"m";
			}

			//TopBar：カーソル情報のセット（カメラ位置から計算）
			Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
			radian = Math.Atan2(ray.direction.x, ray.direction.z);
			if(radian < 0){
				radian += 2*Math.PI;
			}
			cursorText.text = aAV_Public.lang.cursor+" : "+aAV_Public.lang.azimuth+"="+(radian*toDeg).ToString("F2") +"°, "+aAV_Public.lang.altitude+"="+(Math.Asin(ray.direction.y)*toDeg).ToString("F2")+"° "+markerName;

			//InfoView：マーカー方位情報のセット（人間視位置から計算）
			eyePosition = humanPosition;
			eyePosition.y += 1.6f;
			int count = aAV_Public.rplist.Count;
			if(aAV_Public.addMarker){
				count -=1;
			}
			for(int i = 0 ; i < count; i++){
				Vector3 direction = marker[i] - eyePosition;
				float r = direction.magnitude;
				radian = Math.Atan2(direction.x, direction.z);
				if(radian < 0){
					radian += 2*Math.PI;
				}
				info_direction[i].text = aAV_Public.lang.direction+" : "+(radian*toDeg).ToString("F2")+"°, "+(Math.Asin(direction.y/r)*toDeg).ToString("F2")+"°, "+km(r)+"m";
			}
			yield return new WaitForSeconds(.2f);
		}
	}
	
	public void CopyInfo(){
		string text ="Coordinate System : ";
		//Top Bar
		char[] del = {':', ','};
		var result = Regex.Replace(positionText.text, "[^0-9\\:\\.\\,\\+\\-]", "").Split(del);
		if(aAV_Public.center.type == "WG"){
			text += "WGS84";
			text += "\t\tLongitude(°)\tLatitude(°)\tEl height(m)\n";
		}else if(aAV_Public.center.type == "UT"){
			text += "UTM zone"+result[0];
			text += "\t\tEast(m)\tNorth(m)\tEl height(m)\n";
		}else if(aAV_Public.center.type == "JP"){
			text += "Japan PRCS zone"+result[0];
			text += "\t\tEast(m)\tNorth(m)\tEl height(m)\n";
		}
		text += "Observation position\t\t"+result[1]+"\t"+result[2]+"\t"+result[3]+"\n";

		//Maker
		text += "\nMarker\n";
		if(aAV_Public.center.type == "WG"){
			text += "Name\tNo.\tLongitude(°)\tLatitude(°)\tEl height(m)\tAzimuth(°)\tAltitude(°)\tDistance(m)\n";
		}else{
			text += "Name\tNo.\tEast(m)\tNorth(m)\tEl height(m)\tAzimuth(°)\tAltitude(°)\tDistance(m)\n";
		}
		for(int i = 0 ; i < aAV_Public.rplist.Count; i++){
			text += aAV_Public.rplist[i].name + "\t" + (i+1)+"\t";
			double[] down = gis.downXY((double)marker[i].x, (double)marker[i].z);
			altitude = marker[i].y + (float)down[0];
			if(aAV_Public.center.type=="WG"){
				XY = gis.EN2LonLat(aAV_Public.rplist[i].origin_E, aAV_Public.rplist[i].origin_N, aAV_Public.center.WGS_E, aAV_Public.center.WGS_N, 1d);
				text += XY[0].ToString()+"\t"+XY[1].ToString()+"\t"+altitude+"\t";
			}else if(aAV_Public.center.type=="JP"){
				XY[0]=aAV_Public.rplist[i].origin_E+aAV_Public.center.JPRCS_E;
				XY[1]=aAV_Public.rplist[i].origin_N+aAV_Public.center.JPRCS_N;
				text += XY[0].ToString()+"\t"+XY[1].ToString()+"\t"+altitude+"\t";
			}else if(aAV_Public.center.type=="UT"){
				XY[0]=aAV_Public.rplist[i].origin_E+aAV_Public.center.UTM_E;
				XY[1]=aAV_Public.rplist[i].origin_N+aAV_Public.center.UTM_N;
				text += XY[0].ToString()+"\t"+XY[1].ToString()+"\t"+altitude+"\t";
			}

			Vector3 direction = marker[i] - eyePosition;
			float r = direction.magnitude;
			radian = Math.Atan2(direction.x, direction.z);
			if(radian < 0){
				radian += 2*Math.PI;
			}

			text += (radian*toDeg).ToString()+"\t"+(Math.Asin(direction.y/r)*toDeg).ToString()+"\t"+r+"\n";
		}

	//Auxiliary line
		text += "\nAuxiliary line\n";
		text += "Name\tNo.\tStart\tEnd\tAngle(°)\n";
		for(int i = 0 ; i < aAV_Public.linelist.Count; i++){
			text += aAV_Public.linelist[i].name + "\t"+(i+1)+"\t";
			if((bool)aAV_Public.linelist[i].startObj){
				for(int n = 0; n<aAV_Public.rplist.Count; n++){
					if(aAV_Public.rplist[n].gameobject == aAV_Public.linelist[i].startObj){
						text += "Marker"+(n+1).ToString() +"\t";
					}
				}
			}else{
				text += "None"+"\t";
			}
			if((bool)aAV_Public.linelist[i].endObj){
				for(int n = 0; n<aAV_Public.rplist.Count; n++){
					if(aAV_Public.rplist[n].gameobject == aAV_Public.linelist[i].endObj){
						text += "Marker"+(n+1).ToString()+"\n";
					}
				}
			}else{
				text += "Angle→\t";
				string[] anglelist =  aAV_Public.linelist[i].angle.Split(',');
				foreach( string angle in anglelist){
					text += angle.Trim().Normalize()+"\t";
				}
				text += "\n";
			}
		}
	
		//Object
		text += "\nObject\t\tOrigin\t\t\tRotation\n";
		if(aAV_Public.center.type == "WG"){
			text += "Name\tNo.\tLongitude(°)\tLatitude(°)\tEl height(m)\tE-Axis(°)\tN-Axis(°)\tH-Axis(°)\tScale(mag.)\tExistences\n";
		}else{
			text += "Name\tNo.\tEast(m)\tNorth(m)\tEl height(m)\tE-Axis(°)\tN-Axis(°)\tH-Axis(°)\tScale(mag.)\tExistences\n";
		}
		for(int i = 0 ; i < aAV_Public.datalist.Count; i++){
			text += aAV_Public.datalist[i].name+"\t"+(i+1)+"\t";
			if(aAV_Public.center.type=="WG"){
				XY = gis.EN2LonLat(aAV_Public.datalist[i].origin_E, aAV_Public.datalist[i].origin_N, aAV_Public.center.WGS_E, aAV_Public.center.WGS_N, 1d);
				text += XY[0].ToString()+"\t"+XY[1].ToString()+"\t"+aAV_Public.datalist[i].origin_H+"\t";
			}else if(aAV_Public.center.type=="JP"){
				XY[0]=aAV_Public.datalist[i].origin_E+aAV_Public.center.JPRCS_E;
				XY[1]=aAV_Public.datalist[i].origin_N+aAV_Public.center.JPRCS_N;
				text += XY[0].ToString()+"\t"+XY[1].ToString()+"\t"+aAV_Public.datalist[i].origin_H+"\t";
			}else if(aAV_Public.center.type=="UT"){
				XY[0]=aAV_Public.datalist[i].origin_E+aAV_Public.center.UTM_E;
				XY[1]=aAV_Public.datalist[i].origin_N+aAV_Public.center.UTM_N;
				text += XY[0].ToString()+"\t"+XY[1].ToString()+"\t"+aAV_Public.datalist[i].origin_H+"\t";
			}
			text += aAV_Public.datalist[i].rot_E.ToString()+"\t"+aAV_Public.datalist[i].rot_N.ToString()+"\t"+aAV_Public.datalist[i].rot_H.ToString()+"\t";

			//オブジェクトのスケール、開始年・終了年をセット
			if(aAV_Public.datalist[i].start != "" && aAV_Public.datalist[i].end != ""){
				exist = "From "+aAV_Public.datalist[i].start + " to "+aAV_Public.datalist[i].end;
			}else if (aAV_Public.datalist[i].start != ""){
				exist = "From "+aAV_Public.datalist[i].start;
			}else if (aAV_Public.datalist[i].end != ""){
				exist = "To "+aAV_Public.datalist[i].end ;
			}else{
				exist = "All";
			}
			text += aAV_Public.datalist[i].scale.ToString("F6")+"\t"+exist+"\n";
		}
		
		GUIUtility.systemCopyBuffer = text;
		GetComponent<AudioSource>().PlayOneShot (audioCopy);
	}

	public void Restore(){
		CloseDialog();
		//マーカーをクリア
		foreach(var list in aAV_Public.rplist){
			Destroy(list.gameobject);
		}
		aAV_Public.rplist = new List<aAV_Public.RPoint>();
		//ラインをクリア
		foreach(var list in aAV_Public.linelist){
			Destroy(list.gameobject);
		}
		aAV_Public.linelist = new List<aAV_Public.Line>();
		//Objectをクリア（変更可能値のみ）
		for(int n=0; n < aAV_Public.datalist.Count; n++){
			//初期値設定
			aAV_Public.datalist[n].name = "undefined";
			aAV_Public.datalist[n].origin_E= 0d;
			aAV_Public.datalist[n].origin_N = 0d;
			aAV_Public.datalist[n].origin_H= 0f;
			aAV_Public.datalist[n].rot_E = 0f;
			aAV_Public.datalist[n].rot_N = 0f;
			aAV_Public.datalist[n].rot_H = 0f;
			aAV_Public.datalist[n].scale = 1f;
			aAV_Public.datalist[n].start = "";
			aAV_Public.datalist[n].end = "";
			aAV_Public.datalist[n].visible = true;
		}

		//データセットを読み込み
		aav_fileset.ReadFile(aAV_Public.basicInfo.filePath);
		Debug.Log("SetObject(0)="+aAV_Public.datalist[0].rot_N);
		//Markerの設置
		aav_fileset.SetMarker();
		//Auxiliary Lineの設置
		aav_fileset.SetLine();
		//Objectの再設置
		for(int n=0; n < aAV_Public.datalist.Count; n++){
			aav_fileset.SetObject(n);
		}
		//infoViewの更新
		ViewUpdate();
	}
	
	public void ShowInfo(){
		if (GameObject.Find("InfoView") == null){
			infoview.SetActive(true);
			showText.text = "Close Info";
		}else{
			showText.text = "Show Info";
			infoview.SetActive(false);
		}
	}

	public void ShowSetting(){
		CloseDialog();
		setting.SetActive(true);
	}

	//マーカーAddボタン
	public void AddMarker(){
		CloseDialog();
		markerEdit.SetActive(true);
		markerEdit.GetComponent<aAV_MarkerEdit>().MarkerAdd();
	}

	//ラインAddボタン
	public void AddLine(){
		CloseDialog();
		if(aAV_Public.rplist.Count>0){	//Lineを描くためには、Markerが1つ以上必要
			lineEdit.SetActive(true);
			lineEdit.GetComponent<aAV_LineEdit>().LineAdd();
		}
	}

	public void CloseDialog(){
		if(aAV_Public.addMarker){
			markerEdit.GetComponent<aAV_MarkerEdit>().OnCancel();
		}
		if(aAV_Public.addLine){
			lineEdit.GetComponent<aAV_LineEdit>().OnCancel();
		}
		setting.SetActive(false);
		markerEdit.SetActive(false);
		objectEdit.SetActive(false);
		lineEdit.SetActive(false);
	}

	public string km(float size){
		if(Math.Abs(size) > 1000){
			size = size/1000;
			return size.ToString("F4")+"k";
		}else{
			return size.ToString("F1");
		}
	}
	
	public void SaveDataset(){
		var extensionList = new[]
		{
			new ExtensionFilter( "Text", "txt"),
		};
		var path = StandaloneFileBrowser.SaveFilePanel( "Save File", Path.GetDirectoryName(aAV_Public.basicInfo.filePath), "NewDataset", extensionList ).Name;
		if(path != ""){
			string savetext = "##############################################################\n";
			savetext += "#Basic Setting / 基本設定 (Required : type, center, height)\n";
			savetext += "#注：2バイトコード表記はStellariumでエラーを起こすので英語表記をすること\n";
			savetext += "#typeには空間座標系を（WGS84または、JP01〜JP19、UTM01〜UTM60で）指定\n";
			savetext += "#centerには中心座標を（WGS84は経度,緯度、19系はY,X、UTMはX,Yの順で）指定\n";
			savetext += "#meshには詳細地形のメッシュ解像度(m)を指定\n";
			savetext += "##############################################################\n";
			savetext += "location = "+aAV_Public.basicInfo.location+"\n";
			savetext += "country = "+aAV_Public.basicInfo.country+"\n";
			savetext += "timezone = "+aAV_Public.basicInfo.timezone+"\n";
			savetext += "date = "+aAV_Public.basicInfo.year+"/"+string.Format("{0:D2}", aAV_Public.basicInfo.month)+"/"+string.Format("{0:D2}", aAV_Public.basicInfo.day)+"\n";
			savetext += "time = "+string.Format("{0:D2}", aAV_Public.basicInfo.hour)+":"+string.Format("{0:D2}", aAV_Public.basicInfo.minute)+":"+string.Format("{0:D2}", aAV_Public.basicInfo.second)+"\n";
			savetext += "mesh = "+(aAV_Public.basicInfo.area/4096)+"\n";
			if(aAV_Public.center.type == "WG"){
				savetext += "type = WGS84\n";
				savetext += "center = "+aAV_Public.center.WGS_E+","+aAV_Public.center.WGS_N+"\n";
			}else if(aAV_Public.center.type == "JP"){
				savetext += "type = JP"+string.Format("{0:D2}", aAV_Public.center.JPRCS_zone)+"\n";
				savetext += "center = "+aAV_Public.center.JPRCS_E+","+aAV_Public.center.JPRCS_N+"\n";
			}else if(aAV_Public.center.type == "UT"){
				savetext += "type = UTM"+string.Format("{0:D2}", aAV_Public.center.UTM_zone)+"\n";
				savetext += "center = "+aAV_Public.center.UTM_E+","+aAV_Public.center.UTM_N+"\n";
			}
			savetext += "height = "+aAV_Public.basicInfo.center_H+"\n";
			savetext += "avatar = "+aAV_Public.basicInfo.avatar+"\n\n";
	
			savetext += "##############################################################\n";
			savetext += "#Marker / マーカー (Required : marker[].origin, marker[].height)\n";
			savetext += "##############################################################\n";
			for(int i=0; i<aAV_Public.rplist.Count; i++){
				savetext += "marker["+(i+1)+"].name = "+aAV_Public.rplist[i].name+"\n";
				if(aAV_Public.center.type=="WG"){
					XY = gis.EN2LonLat(aAV_Public.rplist[i].origin_E, aAV_Public.rplist[i].origin_N, aAV_Public.center.WGS_E, aAV_Public.center.WGS_N, 1d);
					savetext += "marker["+(i+1)+"].origin = "+XY[0]+","+XY[1]+"\n";
				}else if(aAV_Public.center.type=="JP"){
					XY[0]=aAV_Public.rplist[i].origin_E+aAV_Public.center.JPRCS_E;
					XY[1]=aAV_Public.rplist[i].origin_N+aAV_Public.center.JPRCS_N;
					savetext += "marker["+(i+1)+"].origin = "+XY[0]+","+XY[1]+"\n";
				}else if(aAV_Public.center.type=="UT"){
					XY[0]=aAV_Public.rplist[i].origin_E+aAV_Public.center.UTM_E;
					XY[1]=aAV_Public.rplist[i].origin_N+aAV_Public.center.UTM_N;
					savetext += "marker["+(i+1)+"].origin = "+XY[0]+","+XY[1]+"\n";
				}
				savetext += "marker["+(i+1)+"].height = "+aAV_Public.rplist[i].origin_H+"\n";
				savetext += "marker["+(i+1)+"].color = "+aAV_Public.rplist[i].color+"\n";
				savetext += "marker["+(i+1)+"].visible = "+aAV_Public.rplist[i].visible+"\n\n";
			}
			
			savetext += "##############################################################\n";
			savetext += "#Auxiliary line / 補助線 (Required : line[].start_marker)\n";
			savetext += "##############################################################\n";
			for(int i=0; i<aAV_Public.linelist.Count; i++){
				savetext += "line["+(i+1)+"].name = "+aAV_Public.linelist[i].name+"\n";
				if((bool)aAV_Public.linelist[i].startObj){
					for(int n = 0; n<aAV_Public.rplist.Count; n++){
						if(aAV_Public.rplist[n].gameobject == aAV_Public.linelist[i].startObj){
							savetext += "line["+(i+1)+"].start_marker = "+(n+1).ToString()+"\n";
						}
					}
				}else{
					savetext += "line["+(i+1)+"].start_marker = \n";
				}
				if((bool)aAV_Public.linelist[i].endObj){
					for(int n = 0; n<aAV_Public.rplist.Count; n++){
						if(aAV_Public.rplist[n].gameobject == aAV_Public.linelist[i].endObj){
							savetext += "line["+(i+1)+"].end_marker = "+(n+1).ToString()+"\n";
						}
					}
				}else{
					savetext += "line["+(i+1)+"].end_marker = \n";
				}
				savetext += "line["+(i+1)+"].angle = "+aAV_Public.linelist[i].angle+"\n";
				savetext += "line["+(i+1)+"].color = "+aAV_Public.linelist[i].color+"\n";
				savetext += "line["+(i+1)+"].visible = "+aAV_Public.linelist[i].visible+"\n\n";
			}
			
			savetext += "##############################################################\n";
			savetext += "#3D Object (required : dataset[].file, dataset[].origin, dataset[].height)\n";
			savetext += "##############################################################\n";
			for(int i=0; i<aAV_Public.datalist.Count; i++){
				savetext += "dataset["+(i+1)+"].name = "+aAV_Public.datalist[i].name+"\n";
				savetext += "dataset["+(i+1)+"].file = "+aAV_Public.datalist[i].file+"\n";
				if(aAV_Public.center.type=="WG"){
					XY = gis.EN2LonLat(aAV_Public.datalist[i].origin_E, aAV_Public.datalist[i].origin_N, aAV_Public.center.WGS_E, aAV_Public.center.WGS_N, 1d);
					savetext += "dataset["+(i+1)+"].origin = "+XY[0]+","+XY[1]+"\n";
				}else if(aAV_Public.center.type=="JP"){
					XY[0]=aAV_Public.datalist[i].origin_E+aAV_Public.center.JPRCS_E;
					XY[1]=aAV_Public.datalist[i].origin_N+aAV_Public.center.JPRCS_N;
					savetext += "dataset["+(i+1)+"].origin = "+XY[0]+","+XY[1]+"\n";
				}else if(aAV_Public.center.type=="UT"){
					XY[0]=aAV_Public.datalist[i].origin_E+aAV_Public.center.UTM_E;
					XY[1]=aAV_Public.datalist[i].origin_N+aAV_Public.center.UTM_N;
					savetext += "dataset["+(i+1)+"].origin = "+XY[0]+","+XY[1]+"\n";
				}
				savetext += "dataset["+(i+1)+"].height = "+aAV_Public.datalist[i].origin_H+"\n";
				savetext += "dataset["+(i+1)+"].rot_E = "+aAV_Public.datalist[i].rot_E+"\n";
				savetext += "dataset["+(i+1)+"].rot_N = "+aAV_Public.datalist[i].rot_N+"\n";
				savetext += "dataset["+(i+1)+"].rot_H = "+aAV_Public.datalist[i].rot_H+"\n";
				savetext += "dataset["+(i+1)+"].scale = "+aAV_Public.datalist[i].scale+"\n";
				savetext += "dataset["+(i+1)+"].start = "+aAV_Public.datalist[i].start+"\n";
				savetext += "dataset["+(i+1)+"].end = "+aAV_Public.datalist[i].end+"\n";
				savetext += "dataset["+(i+1)+"].visible = "+aAV_Public.datalist[i].visible+"\n\n";
			}
			File.WriteAllText(path, savetext, Encoding.UTF8);
		}
	}

}

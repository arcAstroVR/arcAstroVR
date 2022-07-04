using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class aAV_MarkerEdit : MonoBehaviour
{
	private int markerNo = 0;
	private string markerName = "";
	private Vector3 markerPosition;
	private double[] XY = new double [2] {0d, 0d};
	private double[] down = new double [3] {0d, 0d, 0d};
	private double eField = 0d;
	private double nField = 0d;
	private float hField = 0f;
	private string colorField = "";
	private bool open = false;
	private Renderer r;
	private Color color = default(Color);

	private aAV_GIS gis;
	private aAV_Direction direction;
	private aAV_Public aav_public;
	private Toggle delete;
	private GameObject markerObj;
	private GameObject avator;
	
	void Awake(){
		avator = GameObject.Find("Main").transform.Find("Avatar").gameObject;

		gis = GameObject.Find("Main").GetComponent<aAV_GIS>();
		aav_public = GameObject.Find("Main").GetComponent<aAV_Public>();
		direction = GameObject.Find("Main").transform.Find("Menu").gameObject.GetComponent<aAV_Direction>();
		delete = GameObject.Find("Main").transform.Find("Menu/MarkerEdit/Delete").gameObject.GetComponent<Toggle>();
	}
	
	public void MarkerEdit(int rp_no){
		aAV_Event.textInput = true;
		if(open){
			SettingBack();
		}
		markerNo = rp_no;
		markerName = aAV_Public.rplist[rp_no].name;
		markerObj = aAV_Public.rplist[rp_no].gameobject;
		r = markerObj.GetComponent<Renderer>();
		r.material.EnableKeyword("_EMISSION");
		delete.isOn = false;
		
		XY = gis.TypeXY(aAV_Public.rplist[rp_no].origin_E, aAV_Public.rplist[rp_no].origin_N);
		eField = XY[0];			//元の座標系表示
		nField = XY[1];			//元の座標系表示
		hField = aAV_Public.rplist[rp_no].origin_H;
		colorField = aAV_Public.rplist[rp_no].color;
		open = true;
		UpdateMarker();
	}

	public void MarkerAdd(){
		aAV_Public.addMarker = true;
		var rpset = new aAV_Public.RPoint();
		aAV_Public.rplist.Add(rpset);
		int newNo = aAV_Public.rplist.Count;
		GameObject marker = Instantiate(aav_public.markerPrefab) as GameObject;
		aAV_Public.rplist[newNo-1].gameobject = marker;
		marker.name = "Marker"+newNo.ToString();
		aAV_Public.rplist[newNo-1].name = "undefined";
		Vector3 humanPosition = avator.transform.position;
		aAV_Public.rplist[newNo-1].origin_E = humanPosition.x;
		aAV_Public.rplist[newNo-1].origin_N = humanPosition.z;
		down = gis.downXY(humanPosition.x, humanPosition.z);
		aAV_Public.rplist[newNo-1].origin_H = humanPosition.y+(float)down[0];
		aAV_Public.rplist[newNo-1].visible = true;
		MarkerEdit(newNo-1);
	}

	public void OnOk(){
		XY = gis.UnityXY(eField, nField);		//Unity座標系に変換
		aAV_Public.rplist[markerNo].name = markerName;
		aAV_Public.rplist[markerNo].origin_E = XY[0];
		aAV_Public.rplist[markerNo].origin_N = XY[1];
		aAV_Public.rplist[markerNo].origin_H = hField;
		aAV_Public.rplist[markerNo].color = colorField;
		if(delete.isOn){
			aAV_Public.rplist[markerNo].gameobject = null;
			Destroy(markerObj);
			aAV_Public.rplist.RemoveAt(markerNo);
		}
		open = false;
		aAV_Event.textInput = false;
		aAV_Public.addMarker = false;
		this.gameObject.SetActive(false);
		direction.ViewUpdate();
	}

	public void OnCancel(){
		SettingBack();
		open = false;
		aAV_Event.textInput = false;
		this.gameObject.SetActive(false);
	}
	
	public void SettingBack(){	//一時的変更を戻す
		down = gis.downXY(aAV_Public.rplist[markerNo].origin_E, aAV_Public.rplist[markerNo].origin_N);
		markerObj.transform.position=new Vector3((float)aAV_Public.rplist[markerNo].origin_E, (float)aAV_Public.rplist[markerNo].origin_H-(float)down[0], (float)aAV_Public.rplist[markerNo].origin_N);
		ColorUtility.TryParseHtmlString(aAV_Public.rplist[markerNo].color, out color);
		r.material.SetColor("_EmissionColor", color);
		if(aAV_Public.addMarker){
			Destroy(markerObj);
			aAV_Public.rplist.RemoveAt(markerNo);
			aAV_Public.addMarker = false;
		}
	}

	public void ChangeField(){
		markerName = GameObject.Find("MarkerName").GetComponent<InputField>().text;
		eField = double.Parse(GameObject.Find("XField").GetComponent<InputField>().text);
		nField = double.Parse(GameObject.Find("YField").GetComponent<InputField>().text);
		hField = float.Parse(GameObject.Find("ZField").GetComponent<InputField>().text);
		colorField = "#"+GameObject.Find("ColorField").GetComponent<InputField>().text;
		UpdateMarker();
	}

	public void XUp(){
		if(aAV_Public.center.type=="WG"){
			eField += (double)Math.Abs(Math.Cos(aAV_Public.center.WGS_N))*0.000001;
		}else{
			eField += 0.1d;
		}
		UpdateMarker();
	}

	public void XDown(){
		if(aAV_Public.center.type=="WG"){
			eField -= (double)Math.Abs(Math.Cos(aAV_Public.center.WGS_N))*0.000001;
		}else{
			eField -= 0.1d;
		}
		UpdateMarker();
	}

	public void YUp(){
		if(aAV_Public.center.type=="WG"){
			nField += 0.000001d;
		}else{
			nField += 0.1d;
		}
		UpdateMarker();
	}

	public void YDown(){
		if(aAV_Public.center.type=="WG"){
			nField -= 0.000001d;
		}else{
			nField -= 0.1d;
		}
		UpdateMarker();
	}

	public void ZUp(){
		hField += 0.1f;
		UpdateMarker();
	}

	public void ZDown(){
		hField -= 0.1f;
		UpdateMarker();
	}
	
	public void UpdateMarker(){
		//座標移動
		XY = gis.UnityXY(eField, nField);
		down = gis.downXY(XY[0], XY[1]);
		markerObj.transform.position=new Vector3((float)XY[0], hField-(float)down[0], (float)XY[1]);
		//色変更
		ColorUtility.TryParseHtmlString(colorField, out color);
		r.material.SetColor("_EmissionColor", color);
		
		//ダイアログ更新
		GameObject.Find("MarkerNo").GetComponent<Text>().text= "Marker No."+(markerNo+1).ToString();
		GameObject.Find("MarkerName").GetComponent<InputField>().text= markerName;
		GameObject.Find("XField").GetComponent<InputField>().text= eField.ToString();
		GameObject.Find("YField").GetComponent<InputField>().text= nField.ToString();
		GameObject.Find("ZField").GetComponent<InputField>().text= hField.ToString();
		GameObject.Find("ColorField").GetComponent<InputField>().text= colorField;
	}
}

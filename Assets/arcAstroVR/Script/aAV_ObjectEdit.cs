using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class aAV_ObjectEdit : MonoBehaviour
{
	private int objectNo = 0;
	private string objectName = "";
	private Vector3 objectPosition;
	private double[] XY = new double [2] {0d, 0d};
	private double[] down = new double [3] {0d, 0d, 0d};
	private double eField = 0d;
	private double nField = 0d;
	private float hField = 0f;
	private float eRotate = 0f;
	private float nRotate = 0f;
	private float hRotate = 0f;
	private float eScale = 1f;
	private float nScale = 1f;
	private float hScale = 1f;
	private string start;
	private string end;
	private bool open = false;

	private aAV_GIS gis;
	private aAV_Direction direction;
	private GameObject objectObj;

	void Awake(){
		gis = GameObject.Find("Main").GetComponent<aAV_GIS>();
		direction = GameObject.Find("Main").transform.Find("Menu").gameObject.GetComponent<aAV_Direction>();
	}

	public void ObjectEdit(int ob_no){
		aAV_Event.textInput = true;
		if(open){
			SettingBack();
		}
		objectNo = ob_no;
		objectName = aAV_Public.datalist[ob_no].name;
		objectObj = aAV_Public.datalist[ob_no].gameobject;
		
		XY = gis.TypeXY(aAV_Public.datalist[ob_no].origin_E, aAV_Public.datalist[ob_no].origin_N);
		eField = XY[0];			//元の座標系表示
		nField = XY[1];			//元の座標系表示
		hField = aAV_Public.datalist[ob_no].origin_H;
		eRotate = aAV_Public.datalist[ob_no].rot_E;
		nRotate = aAV_Public.datalist[ob_no].rot_N;
		hRotate = aAV_Public.datalist[ob_no].rot_H;
		eScale= aAV_Public.datalist[ob_no].scale_E;
		nScale = aAV_Public.datalist[ob_no].scale_N;
		hScale = aAV_Public.datalist[ob_no].scale_H;
		start =aAV_Public.datalist[ob_no].start;
		end = aAV_Public.datalist[ob_no].end;
		open = true;
		UpdateObject();
	}

	public void OnOk(){
		XY = gis.UnityXY(eField, nField);		//Unity座標系に変換
		aAV_Public.datalist[objectNo].name = objectName;
		aAV_Public.datalist[objectNo].origin_E = XY[0];
		aAV_Public.datalist[objectNo].origin_N = XY[1];
		aAV_Public.datalist[objectNo].origin_H = hField;
		aAV_Public.datalist[objectNo].rot_E = eRotate;
		aAV_Public.datalist[objectNo].rot_N = nRotate;
		aAV_Public.datalist[objectNo].rot_H = hRotate;
		aAV_Public.datalist[objectNo].scale_E = eScale;
		aAV_Public.datalist[objectNo].scale_N = nScale;
		aAV_Public.datalist[objectNo].scale_H = hScale;
		aAV_Public.datalist[objectNo].start = start;
		aAV_Public.datalist[objectNo].end = end;
		direction.ViewUpdate();

		//時系列による3Dオブジェクトの表示コントロール
		for(int i = 0; i < aAV_Public.datalist.Count; i++){
			bool timeVisible = true;
			if(aAV_Public.datalist[i].start != ""){
				if(aAV_Public.basicInfo.year < int.Parse(aAV_Public.datalist[i].start)){
					timeVisible = false;
				}
			}
			if(aAV_Public.datalist[i].end != ""){
				if(aAV_Public.basicInfo.year > int.Parse(aAV_Public.datalist[i].end)){
					timeVisible = false;
				}
			}
			aAV_Public.datalist[i].gameobject.SetActive(aAV_Public.datalist[i].visible && timeVisible);
		}
		
		open = false;
		aAV_Event.textInput = false;
		this.gameObject.SetActive(false);
	}

	public void OnCancel(){
		SettingBack();
		open = false;
		aAV_Event.textInput =false;
		this.gameObject.SetActive(false);
	}

	private void SettingBack(){
		down = gis.downXY(aAV_Public.datalist[objectNo].origin_E, aAV_Public.datalist[objectNo].origin_N);
		objectObj.transform.position=new Vector3((float)aAV_Public.datalist[objectNo].origin_E, (float)aAV_Public.datalist[objectNo].origin_H-(float)down[0], (float)aAV_Public.datalist[objectNo].origin_N);
		objectObj.transform.eulerAngles=new Vector3((float)aAV_Public.datalist[objectNo].rot_E + (float)down[2], (float)aAV_Public.datalist[objectNo].rot_H, (float)aAV_Public.datalist[objectNo].rot_N+ (float)down[1]);
		objectObj.transform.localScale=new Vector3((float)aAV_Public.datalist[objectNo].scale_E, (float)aAV_Public.datalist[objectNo].scale_H, (float)aAV_Public.datalist[objectNo].scale_N);
	}

	public void ChangeField(){
		objectName = GameObject.Find("ObjectName").GetComponent<InputField>().text;
		eField = double.Parse(GameObject.Find("CE_Field").GetComponent<InputField>().text);
		nField = double.Parse(GameObject.Find("CN_Field").GetComponent<InputField>().text);
		hField = float.Parse(GameObject.Find("CH_Field").GetComponent<InputField>().text);
		eRotate = float.Parse(GameObject.Find("RE_Field").GetComponent<InputField>().text);
		nRotate = float.Parse(GameObject.Find("RN_Field").GetComponent<InputField>().text);
		hRotate = float.Parse(GameObject.Find("RH_Field").GetComponent<InputField>().text);
		eScale = float.Parse(GameObject.Find("SE_Field").GetComponent<InputField>().text);
		nScale = float.Parse(GameObject.Find("SN_Field").GetComponent<InputField>().text);
		hScale = float.Parse(GameObject.Find("SH_Field").GetComponent<InputField>().text);
		start = GameObject.Find("StartField").GetComponent<InputField>().text;
		end = GameObject.Find("EndField").GetComponent<InputField>().text;
		UpdateObject();
	}

	public void CX_Up(){
		if(aAV_Public.center.type=="WG"){
			eField += (double)Math.Abs(Math.Cos(aAV_Public.center.WGS_N))*0.000001;
		}else{
			eField += 0.1d;
		}
		UpdateObject();
	}

	public void CX_Down(){
		if(aAV_Public.center.type=="WG"){
			eField -= (double)Math.Abs(Math.Cos(aAV_Public.center.WGS_N))*0.000001;
		}else{
			eField -= 0.1d;
		}
		UpdateObject();
	}

	public void CY_Up(){
		if(aAV_Public.center.type=="WG"){
			nField += 0.000001d;
		}else{
			nField += 0.1d;
		}
		UpdateObject();
	}

	public void CY_Down(){
		if(aAV_Public.center.type=="WG"){
			nField -= 0.000001d;
		}else{
			nField -= 0.1d;
		}
		UpdateObject();
	}

	public void CZ_Up(){
		hField += 0.1f;
		UpdateObject();
	}

	public void CZ_Down(){
		hField -= 0.1f;
		UpdateObject();
	}
	
	public void RX_Up(){
		eRotate += 0.1f;
		UpdateObject();
	}

	public void RX_Down(){
		eRotate -= 0.1f;
		UpdateObject();
	}

	public void RY_Up(){
		nRotate += 0.1f;
		UpdateObject();
	}

	public void RY_Down(){
		nRotate -= 0.1f;
		UpdateObject();
	}

	public void RZ_Up(){
		hRotate += 0.1f;
		UpdateObject();
	}

	public void RZ_Down(){
		hRotate -= 0.1f;
		UpdateObject();
	}
	
	public void SX_Up(){
		eScale += 0.1f;
		UpdateObject();
	}

	public void SX_Down(){
		eScale -= 0.1f;
		UpdateObject();
	}

	public void SY_Up(){
		nScale += 0.1f;
		UpdateObject();
	}

	public void SY_Down(){
		nScale -= 0.1f;
		UpdateObject();
	}

	public void SZ_Up(){
		hScale += 0.1f;
		UpdateObject();
	}

	public void SZ_Down(){
		hScale -= 0.1f;
		UpdateObject();
	}

	public void Start_Up(){
		if(start==""){
			start="0";
		}
		start = (int.Parse(start)+1).ToString();
		UpdateObject();
	}

	public void Start_Down(){
		int n;
		if(start==""){
			start="0";
		}
		start = (int.Parse(start)-1).ToString();
		UpdateObject();
	}
	
	public void End_Up(){
		if(end==""){
			end="0";
		}
		end = (int.Parse(end)+1).ToString();;
		UpdateObject();
	}

	public void End_Down(){
		if(end==""){
			end="0";
		}
		end = (int.Parse(end)-1).ToString();;
		UpdateObject();
	}
	
	public void UpdateObject(){
		//座標移動
		XY = gis.UnityXY(eField, nField);
		down = gis.downXY(XY[0], XY[1]);
		objectObj.transform.position=new Vector3((float)XY[0], hField-(float)down[0], (float)XY[1]);
		//座標回転
		objectObj.transform.eulerAngles=new Vector3((float)eRotate + (float)down[2], (float)hRotate, (float)nRotate+ (float)down[1]);
		//スケール調整
		objectObj.transform.localScale=new Vector3((float)eScale, (float)hScale, (float)nScale);
		
		//ダイアログ更新
		GameObject.Find("ObjectNo").GetComponent<Text>().text= "Object No."+(objectNo+1).ToString();
		GameObject.Find("ObjectName").GetComponent<InputField>().text= objectName;
		GameObject.Find("CE_Field").GetComponent<InputField>().text= eField.ToString();
		GameObject.Find("CN_Field").GetComponent<InputField>().text= nField.ToString();
		GameObject.Find("CH_Field").GetComponent<InputField>().text= hField.ToString();
		GameObject.Find("RE_Field").GetComponent<InputField>().text= eRotate.ToString();
		GameObject.Find("RN_Field").GetComponent<InputField>().text= nRotate.ToString();
		GameObject.Find("RH_Field").GetComponent<InputField>().text= hRotate.ToString();
		GameObject.Find("SE_Field").GetComponent<InputField>().text= eScale.ToString();
		GameObject.Find("SN_Field").GetComponent<InputField>().text= nScale.ToString();
		GameObject.Find("SH_Field").GetComponent<InputField>().text= hScale.ToString();
		GameObject.Find("StartField").GetComponent<InputField>().text= start;
		GameObject.Find("EndField").GetComponent<InputField>().text= end;
	}
}

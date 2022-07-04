using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class aAV_InfoAction : MonoBehaviour
{
	public int ListNo;

	private Toggle toggle;
	private GameObject avatar;
	private GameObject setting;
	private GameObject markerEdit;
	private GameObject objectEdit;
	private GameObject lineEdit;
	private GameObject lineCanvas;
	private GameObject mapCam;
	private aAV_Direction direction;
	private aAV_CompassMap compass;

	void Awake(){
		Transform mainTrans = GameObject.Find("Main").transform;
		avatar = mainTrans.Find("Avatar").gameObject;
		setting = mainTrans.Find("Menu/Setting").gameObject;
		markerEdit = mainTrans.Find("Menu/MarkerEdit").gameObject;
		objectEdit = mainTrans.Find("Menu/ObjectEdit").gameObject;
		lineEdit = mainTrans.Find("Menu/LineEdit").gameObject;
		lineCanvas = mainTrans.Find("LineCanvas").gameObject;
		mapCam = mainTrans.Find("MapCamera").gameObject;

		direction = mainTrans.Find("Menu").GetComponent<aAV_Direction>();
		compass = mainTrans.Find("LineCanvas").GetComponent<aAV_CompassMap>();
	}

	//マーカーの表示/非表示
	public void rpChanged(){
		toggle = transform.GetChild(0).GetComponent<Toggle> ();
		aAV_Public.rplist[ListNo].gameobject.SetActive(toggle.isOn);
		aAV_Public.rplist[ListNo].visible = toggle.isOn;
	}

	//ラインの表示/非表示
	public void lnChanged(){
		toggle = transform.GetChild(0).GetComponent<Toggle> ();
		aAV_Public.linelist[ListNo].gameobject.SetActive(toggle.isOn);
		aAV_Public.linelist[ListNo].visible = toggle.isOn;
	}

	//オブジェクトの表示/非表示
	public void obChanged(){
		toggle = transform.GetChild(0).GetComponent<Toggle> ();
		bool timeVisible = true;
		if(aAV_Public.datalist[ListNo].start != ""){
			if(aAV_Public.basicInfo.year < int.Parse(aAV_Public.datalist[ListNo].start)){
				timeVisible = false;
			}
		}
		if(aAV_Public.datalist[ListNo].end != ""){
			if(aAV_Public.basicInfo.year > int.Parse(aAV_Public.datalist[ListNo].end)){
				timeVisible = false;
			}
		}
		aAV_Public.datalist[ListNo].gameobject.SetActive(toggle.isOn && timeVisible);
		aAV_Public.datalist[ListNo].visible = toggle.isOn;
	}

	//GOボタン
	public void JumpPosition(){
		avatar.transform.position = aAV_Public.rplist[ListNo].gameobject.transform.position;
	}

	//マーカーEditボタン
	public void ShowMarkerEdit(){
		direction.CloseDialog();
		markerEdit.SetActive(true);
		markerEdit.GetComponent<aAV_MarkerEdit>().MarkerEdit(ListNo);
	}

	//オブジェクトEditボタン
	public void ShowObjectEdit(){
		direction.CloseDialog();
		objectEdit.SetActive(true);
		objectEdit.GetComponent<aAV_ObjectEdit>().ObjectEdit(ListNo);	
	}

	//補助線Editボタン
	public void ShowLineEdit(){
		direction.CloseDialog();
		lineEdit.SetActive(true);
		lineEdit.GetComponent<aAV_LineEdit>().LineEdit(ListNo);
	}

	//CompassMapボタン
	public void ShowCompassMap(){
		direction.CloseDialog();
		lineCanvas.SetActive(true);
		compass.ShowCompassMap(ListNo);
	}

}

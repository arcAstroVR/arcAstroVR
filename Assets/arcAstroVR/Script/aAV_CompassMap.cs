using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class aAV_CompassMap : MonoBehaviour
{

	private aAV_Public aav_public;
	private aAV_Public.RPoint centerMarker;
	private LineRenderer lineRenderer;
	private GameObject lineCanvas;
	private GameObject mainCam;
	private GameObject markerCam;
	private GameObject mapCam;
	private GameObject markerObj;
	private GameObject closeButton;
	private Vector3 mapPos;
	private Vector3 markerPos;
	private Text centerLabel;

	void Awake(){
		aav_public = GameObject.Find("Main").GetComponent<aAV_Public>();
		lineCanvas = GameObject.Find("Main").transform.Find("LineCanvas").gameObject;
		mainCam = GameObject.Find("XR Origin/Camera Offset");
		markerCam = GameObject.Find("Main").transform.Find("Menu/CamEdit").gameObject;
		mapCam = GameObject.Find("Main").transform.Find("MapCamera").gameObject;
		closeButton = GameObject.Find("Main").transform.Find("Menu/CloseCompass").gameObject;
	}

	void Update(){
		//コンパスマップの中心をstartMarkerの場所に移動する
		if(markerObj != null){
			mapPos = mapCam.transform.position;
			markerPos = markerObj.transform.position;
			mapCam.transform.position = new Vector3(markerPos.x, mapPos.y, markerPos.z);
			centerLabel.text =centerMarker.name;
			ShowLabel();
		}else{
			CloseCompassMap();
		}
	}
	
	public void ShowCompassMap(int i){
		markerObj = aAV_Public.linelist[i].startObj;
		if(markerCam.activeSelf){
			markerCam.GetComponent<aAV_CamEdit>().OnCancel();
		}
		if(markerObj != null){
			mapCam.SetActive(true);
			closeButton.SetActive(true);
			aAV_Public.showCompass = true;
			markerPos = markerObj.transform.position;
			markerPos.y += 4000;
			mapCam.transform.position = markerPos;
		}else{
			CloseCompassMap();
			return;
		}
		
		//中心マーカーの名前を表示
		centerLabel = GameObject.Find("markerLabel").GetComponent<Text>();
		foreach (var rp in aAV_Public.rplist) {
			if(rp.gameobject == markerObj){
				centerMarker = rp;
			}
		}

		ShowLabel();
	}

	private void ShowLabel(){
		//既存のラベルprefabを削除
		var clones = GameObject.FindGameObjectsWithTag ("labelClone");
		foreach (var clone in clones){
			Destroy(clone);
		}

		//始点マーカーに関連するラインのラベルを表示
		foreach (var line in aAV_Public.linelist) {
			if(line.visible){
				//始点に含まれる場合
				if(line.startObj == markerObj){
					if(line.endObj != null){		//終点がMarkerの場合
						Vector3 direction = line.endObj.transform.position - line.startObj.transform.position;
						double radian = Math.Atan2(direction.x, direction.z);
						if(radian < 0){
							radian += 2*Math.PI;
						}
						float angle = (float)(radian*180/Math.PI);
						//ラベルをprefabから作成
						GameObject label = Instantiate(aav_public.labelPrefab);
						label.transform.SetParent(lineCanvas.transform);
						label.transform.localPosition = new Vector3(0f, 0f, 0f);
						label.transform.localScale = new Vector3(1f, 1f, 1f);
						label.transform.localRotation = Quaternion.Euler(0f, 0f, -1f*angle);
						label.GetComponent<Text>().text = aAV_Public.rplist[line.end_marker - 1].name+"\n"+angle.ToString("F2");
					}else{		//終点がAngleの場合)
						string[] angles = line.angle.Split(',');
						foreach (var angle in angles) {
							//ラベルをprefabから作成
							GameObject label = Instantiate(aav_public.labelPrefab);
							label.transform.SetParent(lineCanvas.transform);
							label.transform.localPosition = new Vector3(0f, 0f, 0f);
							label.transform.localScale = new Vector3(1f, 1f, 1f);
							label.transform.localRotation = Quaternion.Euler(0f, 0f, float.Parse(angle)*-1);
							label.GetComponent<Text>().text = line.name+"\n"+angle;
						}
					}
				}
				//終点に含まれる場合
				if((line.endObj == markerObj)&&(line.startObj != null)&&(aAV_Public.rplist.Count>=line.start_marker)){
					Vector3 direction = line.startObj.transform.position - line.endObj.transform.position;
					double radian = Math.Atan2(direction.x, direction.z);
					if(radian < 0){
						radian += 2*Math.PI;
					}
					float angle = (float)(radian*180/Math.PI);
					//ラベルをprefabから作成
					GameObject label = Instantiate(aav_public.labelPrefab);
					label.transform.SetParent(lineCanvas.transform);
					label.transform.localPosition = new Vector3(0f, 0f, 0f);
					label.transform.localScale = new Vector3(1f, 1f, 1f);
					label.transform.localRotation = Quaternion.Euler(0f, 0f, -1f*angle);
					label.GetComponent<Text>().text = aAV_Public.rplist[line.start_marker - 1].name+"\n"+angle.ToString("F2");
				}
			}
		}
	}
	
	public void CloseCompassMap(){
		closeButton.SetActive(false);
		mapCam.SetActive(false);
		aAV_Public.showCompass = false;
		this.gameObject.SetActive(false);
	}
}

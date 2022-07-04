using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;

public class aAV_Marker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
	private aAV_Direction direction;
	private GameObject camObj;
	private GameObject mapCam;
	private GameObject parentObj;
	
	void Awake()
	{
		direction = GameObject.Find("Main").transform.Find("Menu").gameObject.GetComponent<aAV_Direction>();
		mapCam = GameObject.Find("Main").transform.Find("MapCamera").gameObject;
	}
	
	void OnEnable()
	{
		StartCoroutine("ReMarker");
	}
	
	public void OnPointerClick (PointerEventData eventData){	//マーカークリック時にその場所へ移動
		GameObject.Find("Avatar").transform.position = this.transform.position;
	}
	
	public void OnPointerEnter(PointerEventData eventData){	//マーカー情報の表示
		foreach (var val in aAV_Public.rplist)		{
			if(val.gameobject == gameObject){
				direction.markerName = "（"+val.name+"）";
			}
		}
	}
	public void OnPointerExit(PointerEventData eventData){	//マーカー情報の非表示
		direction.markerName = "";
	}
	
	IEnumerator ReMarker() {		//カメラ位置によるマーカーサイズの調整
		float scale;
		while(true){
			if(!aAV_Public.showCompass){
				camObj = Camera.main.gameObject;
				Vector3 camera = camObj.transform.position;
				Vector3 marker = this.gameObject.transform.position;
				scale = Vector3.Distance(camera, marker) / 200;
			}else{
				scale = mapCam.GetComponent<Camera>().orthographicSize / 100;
			}
			this.gameObject.transform.localScale = new Vector3(scale, scale, scale);
			yield return new WaitForSeconds(.1f);
		}
	}
}


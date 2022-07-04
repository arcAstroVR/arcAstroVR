using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class aAV_InfoResize : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler{
	private GameObject info;
	private GameObject view;
	private GameObject log;
	
	void Awake () {
		Transform mainTrans = GameObject.Find("Main").transform;
		info = mainTrans.Find("Menu/InfoView").gameObject;
		view = mainTrans.Find("Menu/InfoView/ScrollView").gameObject;
		log = mainTrans.Find("Menu/InfoView/ScrollView/Viewport/Log").gameObject;
	}

	public void OnBeginDrag(PointerEventData e){
	}

	public void OnDrag(PointerEventData e){
		var infosize = info.GetComponent<RectTransform>().sizeDelta;
		var logsize = log.GetComponent<RectTransform>().sizeDelta;
		if( infosize.y - e.delta.y >100 && infosize.y - e.delta.y <= logsize.y +30){
			info.GetComponent<RectTransform>().sizeDelta -= new Vector2(0f, e.delta.y);
			view.GetComponent<RectTransform>().sizeDelta -= new Vector2(0f, e.delta.y);
		}
		aAV_Public.uiDrag = true;
	}

	public void OnEndDrag(PointerEventData e){
		aAV_Public.uiDrag = false;
	}
}

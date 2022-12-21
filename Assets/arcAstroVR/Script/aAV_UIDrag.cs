using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class aAV_UIDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler{

	private RectTransform obj;
	private GameObject xrRighthand;	

	void Awake(){
		xrRighthand = GameObject.Find("Camera Offset").transform.Find("RightHand Controller").gameObject;
	}
	
	public void OnBeginDrag(PointerEventData e)
    {
		if(!xrRighthand.activeSelf){
			aAV_Public.uiDrag = true;
		}
    }

	public void OnDrag(PointerEventData e){
		if((name == "DateTimeSetting")&&(!xrRighthand.activeSelf)){
			GetComponent<RectTransform>().position += new Vector3(e.delta.x, e.delta.y, 0.0f);
		}
	}

	public void OnEndDrag(PointerEventData e){
		if(!xrRighthand.activeSelf){
			aAV_Public.uiDrag = false;
		}
	}
}

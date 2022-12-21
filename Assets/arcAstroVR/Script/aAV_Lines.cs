using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class aAV_Lines : MonoBehaviour
{
	public float lineWidth = 0.1f;

	public GameObject startMarker;
	public GameObject endMarker;
	public Vector3 endAngle = new Vector3(0f,0f,0f);

	private Vector3 camera = new Vector3(0f,0f,0f);
	private Vector3 mapPos = new Vector3(0f,0f,0f);
	private Vector3 startPos = new Vector3(0f,0f,0f);
	private Vector3 endPos = new Vector3(0f,0f,0f);
	private Vector3 footPos = new Vector3(0f,0f,0f);
	private LineRenderer lineRenderer;
	private GameObject mapCamObj;
	private float scale;

	void Awake() {
		mapCamObj = GameObject.Find("Main").transform.Find("MapCamera").gameObject;
	}
	
	void OnEnable()
	{
		lineRenderer = gameObject.GetComponent<LineRenderer>();
		StartCoroutine("ReLine");
	}

	IEnumerator ReLine()
	{
		while(true){
			if(startMarker == null){
				gameObject.GetComponent<LineRenderer>().enabled = false;
			}else{
				gameObject.GetComponent<LineRenderer>().enabled = true;
				camera = Camera.main.gameObject.transform.position;
				startPos = startMarker.transform.position;
				if(endMarker){
					endPos = endMarker.transform.position;
				}else{
					endPos = startPos+endAngle;
				}
				// 垂線座標の取得
				footPos = startPos + Vector3.Project(camera - startPos, endPos - startPos);
				scale = Vector3.Distance(camera, footPos)/1000;
				
				if(!aAV_Public.showCompass){	//通常表示
					//start側の線の太さ
					lineRenderer.SetPosition(0, startPos);
					lineRenderer.startWidth = lineWidth+scale;
										
					//end側の線の太さ
					lineRenderer.SetPosition(1, endPos);
					lineRenderer.endWidth = lineWidth+scale;
				}else{	//コンパスマップ表示
					scale = mapCamObj.GetComponent<Camera>().orthographicSize / 400f;
					startPos = startMarker.transform.position;
					lineRenderer.SetPosition(0, startPos);
					lineRenderer.startWidth = scale;
					lineRenderer.SetPosition(1, endPos);
					lineRenderer.endWidth = scale;
				}	
			}
			yield return new WaitForSeconds(.1f);
		}
	}
}

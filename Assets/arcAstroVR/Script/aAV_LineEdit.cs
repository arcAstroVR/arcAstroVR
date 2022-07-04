using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class aAV_LineEdit : MonoBehaviour
{

	private class OldLine
	{
		public GameObject startMarker;
		public GameObject endMarker;
		public Vector3 endAngle;
	}
	private List<OldLine> oldList;
		
	private int lineNo = 0;
	private string lineName = "";
	private int lineStart = 0;
	private int lineEnd = 0;
	private string angle = "";
	private string colorField = "";
	private bool open = false;
	private Dropdown startDrop;
	private Dropdown endDrop;
	private LineRenderer lineRenderer;
	private Color color = default(Color);
	private Vector3 startPos;
	private Vector3 endAngle;

	private aAV_Public aav_public;
	private aAV_Direction direction;
	private aAV_Lines aav_lines;
	private Toggle delete;
	private GameObject lineObj;
	private GameObject startObj;
	private GameObject endObj;
	private GameObject lines;

	void Awake(){
		aav_public = GameObject.Find("Main").GetComponent<aAV_Public>();
		direction = GameObject.Find("Main").transform.Find("Menu").gameObject.GetComponent<aAV_Direction>();
	}

	public void LineEdit(int ln_no){
		aAV_Event.textInput = true;
		if(open){
			SettingBack();
		}
		lineNo = ln_no;
		lineName = aAV_Public.linelist[ln_no].name;
		lineObj = aAV_Public.linelist[ln_no].gameobject;
		delete = transform.Find("Delete").gameObject.GetComponent<Toggle>();
		delete.isOn = false;
		startObj = aAV_Public.linelist[ln_no].startObj;
		endObj = aAV_Public.linelist[ln_no].endObj;
		angle = aAV_Public.linelist[ln_no].angle;
		colorField = aAV_Public.linelist[ln_no].color;
		
		//Lineパラメータを保存
		oldList= new List<OldLine>();
		foreach (Transform line in lineObj.transform){
			var oldset = new OldLine();
			aav_lines = line.GetComponent<aAV_Lines>();
			oldset.startMarker = aav_lines.startMarker;
			oldset.endMarker = aav_lines.endMarker;
			oldset.endAngle = aav_lines.endAngle;
			oldList.Add(oldset);
		}
		
		//ドロップダウンリストの作成
		List<string> startlist = new List<string>();
		for(int i=0; i<aAV_Public.rplist.Count; i++){
			startlist.Add((i+1).ToString()+":"+aAV_Public.rplist[i].name);
			if(aAV_Public.rplist[i].gameobject == startObj){
				lineStart = i+1;
			}
		}
		if(!startObj){
			startlist.Add("None");
			lineStart = aAV_Public.rplist.Count+1;
		}
		startDrop = GameObject.Find("StartDropdown").GetComponent<Dropdown>();
		startDrop.ClearOptions();
		startDrop.AddOptions(startlist);
		
		List<string> endlist = new List<string>();
		endlist.Add("Angle");
		for(int i=0; i<aAV_Public.rplist.Count; i++){
			endlist.Add((i+1).ToString()+":"+aAV_Public.rplist[i].name);
			if(aAV_Public.rplist[i].gameobject == endObj){
				lineEnd = i+1;
			}
		}
		if(!endObj){
			lineEnd = 0;
		}
		endDrop = GameObject.Find("EndDropdown").GetComponent<Dropdown>();
		endDrop.ClearOptions();
		endDrop.AddOptions(endlist);

		open = true;
		UpdateLines();
	}

	public void LineAdd(){
		aAV_Public.addLine = true;
		var lineset = new aAV_Public.Line();
		aAV_Public.linelist.Add(lineset);
		int newNo = aAV_Public.linelist.Count;
		GameObject line = Instantiate(aav_public.linePrefab) as GameObject;
		aAV_Public.linelist[newNo-1].gameobject = line;
		line.name = "Line"+newNo.ToString();
		aAV_Public.linelist[newNo-1].name = "undefined";
		aAV_Public.linelist[newNo-1].startObj = aAV_Public.rplist[0].gameobject;
		aAV_Public.linelist[newNo-1].angle = "0";
		aAV_Public.linelist[newNo-1].visible = true;
		LineEdit(newNo-1);
	}
	
	public void OnOk(){
		aAV_Public.linelist[lineNo].name = lineName;
		aAV_Public.linelist[lineNo].start_marker = lineStart;
		aAV_Public.linelist[lineNo].end_marker = lineEnd;
		aAV_Public.linelist[lineNo].startObj = startObj;
		aAV_Public.linelist[lineNo].endObj = endObj;
		aAV_Public.linelist[lineNo].angle = angle;
		aAV_Public.linelist[lineNo].color = colorField;
		if(delete.isOn){
			aAV_Public.linelist[lineNo].gameobject = null;
			Destroy(lineObj);
			aAV_Public.linelist.RemoveAt(lineNo);
		}
		open = false;
		aAV_Event.textInput = false;
		aAV_Public.addLine = false;
		this.gameObject.SetActive(false);
		direction.ViewUpdate();
	}

	public void OnCancel(){
		SettingBack();
		open = false;
		aAV_Event.textInput = false;
		this.gameObject.SetActive(false);
	}

	public void SettingBack(){		//一時的変更を戻す
		foreach (Transform line in lineObj.transform){
			Destroy(line.gameObject);
		}
		ColorUtility.TryParseHtmlString(aAV_Public.linelist[lineNo].color, out color);
		for (int i = 0; i < oldList.Count; i++){
			lines = Instantiate(aav_public.linePrefab) as GameObject;
			lines.transform.parent = lineObj.transform;
			aav_lines = lines.GetComponent<aAV_Lines>();
			aav_lines.startMarker = oldList[i].startMarker;
			aav_lines.endMarker = oldList[i].endMarker;
			aav_lines.endAngle = oldList[i].endAngle;
			lineRenderer = lines.GetComponent<LineRenderer>();
			lineRenderer.material.EnableKeyword("_EMISSION");
			lineRenderer.material.SetColor("_EmissionColor", color);
			lineRenderer.numCapVertices = 10;
		}
		if(aAV_Public.addLine){
			Destroy(lineObj);
			aAV_Public.linelist.RemoveAt(lineNo);
			aAV_Public.addLine = false;
		}
	}
	
	public void ChangeStartDrop(){
		lineStart = startDrop.value+1;
		UpdateLines();
	}

	public void ChangeEndDrop(){
		lineEnd = endDrop.value;
		UpdateLines();
	}
	
	public void ChangeField(){
		lineName = GameObject.Find("LineName").GetComponent<InputField>().text;
		angle = GameObject.Find("LineAngle").GetComponent<InputField>().text;
		colorField = "#"+GameObject.Find("LineColor").GetComponent<InputField>().text;
		UpdateLines();
	}


	public void UpdateLines(){
		//色変更
		ColorUtility.TryParseHtmlString(colorField, out color);

		//line変更
		foreach (Transform line in lineObj.transform){
			Destroy(line.gameObject);
		}
		if(aAV_Public.rplist.Count > lineStart -1){
			startObj = aAV_Public.rplist[lineStart-1].gameobject;
			if (lineEnd != 0){
				endObj = aAV_Public.rplist[lineEnd-1].gameobject;
				if(endObj){
					lines = Instantiate(aav_public.linePrefab) as GameObject;
					lines.transform.parent = lineObj.transform;
					aav_lines = lines.GetComponent<aAV_Lines>();
					aav_lines.startMarker = startObj;
					aav_lines.endMarker = endObj ;
					lineRenderer = lines.GetComponent<LineRenderer>();
					lineRenderer.material.EnableKeyword("_EMISSION");
					lineRenderer.material.SetColor("_EmissionColor", color);
					lineRenderer.numCapVertices = 10;
				}
			}else{
				float distance = 100000f;
				string[] anglelist = angle.Trim().Split(',');
				for(int lineNo = 0 ; lineNo < anglelist.Length; lineNo++){
					lines = Instantiate(aav_public.linePrefab) as GameObject;
					lines.transform.parent = lineObj.transform;
					startObj = aAV_Public.rplist[lineStart-1].gameobject;
					endObj = null;
					aav_lines = lines.GetComponent<aAV_Lines>();
					aav_lines.startMarker = startObj;
					aav_lines.endMarker = endObj;
					var endX = Math.Sin(float.Parse(anglelist[lineNo]) * Math.PI / 180f)*distance;
					var endY = Math.Cos(float.Parse(anglelist[lineNo]) * Math.PI / 180f)*distance;
					aav_lines.endAngle = new Vector3((float)endX , 0, (float)endY );
					lineRenderer = lines.GetComponent<LineRenderer>();
					lineRenderer.material.EnableKeyword("_EMISSION");
					lineRenderer.material.SetColor("_EmissionColor", color);
					lineRenderer.numCapVertices = 10;
				}
			}
		}
		
		//ダイアログ更新
		GameObject.Find("LineNo").GetComponent<Text>().text= "Auxiliary Line No."+(lineNo+1).ToString();
		GameObject.Find("LineName").GetComponent<InputField>().text= lineName;
		startDrop.value = lineStart-1;
		endDrop.value = lineEnd;
		GameObject.Find("LineAngle").GetComponent<InputField>().text= angle;
		GameObject.Find("LineColor").GetComponent<InputField>().text= colorField;
	}

}

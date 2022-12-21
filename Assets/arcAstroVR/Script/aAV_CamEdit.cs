using UnityEngine;
using UnityEngine.UI;

public class aAV_CamEdit : MonoBehaviour
{
	private GameObject markerCam;
	private GameObject lineCanvas;
	private GameObject markerName;
	private GameObject rollField;
	private GameObject pitchField;
	private GameObject yawField;
	private GameObject fovField;
	private float eRotate = 0f;
	private float nRotate = 0f;
	private float hRotate = 0f;
	private float Fov = 60f;
	int rp_no;

	void Awake(){
		markerCam = GameObject.Find("Main").transform.Find("MarkerCamera").gameObject;
		lineCanvas = GameObject.Find("Main").transform.Find("LineCanvas").gameObject;
		markerName = GameObject.Find("Main").transform.Find("Menu/CamEdit/Name/MarkerName").gameObject;
		rollField = GameObject.Find("Main").transform.Find("Menu/CamEdit/Rotation/Roll_Input/Roll_Field").gameObject;
		pitchField = GameObject.Find("Main").transform.Find("Menu/CamEdit/Rotation/Pitch_Input/Pitch_Field").gameObject;
		yawField = GameObject.Find("Main").transform.Find("Menu/CamEdit/Rotation/Yaw_Input/Yaw_Field").gameObject;
		fovField = GameObject.Find("Main").transform.Find("Menu/CamEdit/FOV/FovInput/FovField").gameObject;
	}

	public void CamEdit(int cam_no){
		rp_no = cam_no;
		markerName.GetComponent<Text>().text = aAV_Public.rplist[rp_no].name;
		rollField.GetComponent<InputField>().text = (aAV_Public.rplist[rp_no].cam_ROLL).ToString("F2");
		pitchField.GetComponent<InputField>().text = (aAV_Public.rplist[rp_no].cam_PITCH).ToString("F2");
		yawField.GetComponent<InputField>().text = (aAV_Public.rplist[rp_no].cam_YAW).ToString("F2");
		fovField.GetComponent<InputField>().text = (aAV_Public.rplist[rp_no].cam_FOV).ToString("F2");
		if(lineCanvas.activeSelf){
			lineCanvas.GetComponent<aAV_CompassMap>().CloseCompassMap();
		}
	}

	public void OnClose(){
		aAV_Public.rplist[rp_no].cam_ROLL=float.Parse(rollField.GetComponent<InputField>().text);
		aAV_Public.rplist[rp_no].cam_PITCH=float.Parse(pitchField.GetComponent<InputField>().text);
		aAV_Public.rplist[rp_no].cam_YAW=float.Parse(yawField.GetComponent<InputField>().text);
		aAV_Public.rplist[rp_no].cam_FOV=float.Parse(fovField.GetComponent<InputField>().text);
		markerCam.SetActive(false);
		this.gameObject.SetActive(false);
	}

	public void OnCancel(){
		markerCam.SetActive(false);
		this.gameObject.SetActive(false);
	}

	public void rotationChange(){
		markerCam.transform.rotation = Quaternion.Euler(-1f*float.Parse(pitchField.GetComponent<InputField>().text), float.Parse(yawField.GetComponent<InputField>().text), float.Parse(rollField.GetComponent<InputField>().text));
	}
	
	public void fovChange(){
		if(float.Parse(fovField.GetComponent<InputField>().text) < 1f){
			fovField.GetComponent<InputField>().text = "1.0";
		}else if(float.Parse(fovField.GetComponent<InputField>().text) > 179f){
			fovField.GetComponent<InputField>().text = "179.0";
		}
		markerCam.GetComponent<Camera>().fieldOfView = float.Parse(fovField.GetComponent<InputField>().text);
	}
	
	public void typeChange(){
	}

	public void Roll_Up(){
		rollField.GetComponent<InputField>().text = (float.Parse(rollField.GetComponent<InputField>().text) + 1f).ToString("F2");
	}

	public void Roll_Down(){
		rollField.GetComponent<InputField>().text = (float.Parse(rollField.GetComponent<InputField>().text) - 1f).ToString("F2");
	}

	public void Pitch_Up(){
		pitchField.GetComponent<InputField>().text = (float.Parse(pitchField.GetComponent<InputField>().text) + 1f).ToString("F2");
	}

	public void Pitch_Down(){
		pitchField.GetComponent<InputField>().text = (float.Parse(pitchField.GetComponent<InputField>().text) - 1f).ToString("F2");
	}

	public void Yaw_Up(){
		yawField.GetComponent<InputField>().text = (float.Parse(yawField.GetComponent<InputField>().text) + 1f).ToString("F2");
	}

	public void Yaw_Down(){
		yawField.GetComponent<InputField>().text = (float.Parse(yawField.GetComponent<InputField>().text) - 1f).ToString("F2");
	}
	
	public void FOV_Up(){
		fovField.GetComponent<InputField>().text = (float.Parse(fovField.GetComponent<InputField>().text) + 1f).ToString("F2");
	}

	public void FOV_Down(){
		fovField.GetComponent<InputField>().text = (float.Parse(fovField.GetComponent<InputField>().text) - 1f).ToString("F2");
	}


}

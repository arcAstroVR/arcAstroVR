using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.XR.Management;

public class aAV_Setting : MonoBehaviour
{
	private GameObject zoneObj;	
	private GameObject domeObj;	
	private GameObject xrLefthand;	
	private GameObject xrRighthand;	

	private aAV_Direction direction;
	private aAV_Public publicObj;
	private aAV_MoveBehaviour movebehaviour;
	private Slider ambientSlider;
	private Dropdown langSetting;
	private Dropdown typeSetting;
	private InputField zoneField;
	private Dropdown outputSetting;
	private InputField pitchField;
	private InputField rollField;
	private InputField fovField;
	private Toggle fixToggle;
		
	private int lastLanguage;
	private int lastType;
	private int lastZone;
	private float lastSlide;
	private int lastDisplay;
	private string lastPitch;
	private string lastRoll;
	private string lastFOV;
	private bool lastFix;

	private ManualXRControl manualXRControl;

	void Awake(){
		direction = GameObject.Find("Main").transform.Find("Menu").gameObject.GetComponent<aAV_Direction>();
		publicObj = GameObject.Find("Main").gameObject.GetComponent<aAV_Public>();
		langSetting = GameObject.Find("Main").transform.Find("Menu/Setting/Language/LanguageDropdown").gameObject.GetComponent<Dropdown>();
		typeSetting = GameObject.Find("Main").transform.Find("Menu/Setting/Type/TypeDropdown").gameObject.GetComponent<Dropdown>();
		zoneObj = GameObject.Find("Main").transform.Find("Menu/Setting/Type/Zone").gameObject;
		zoneField = GameObject.Find("Main").transform.Find("Menu/Setting/Type/Zone/ZoneField").gameObject.GetComponent < InputField > ();
		ambientSlider =  GameObject.Find("Main").transform.Find("Menu/Setting/Ambient/AmbientSlider").gameObject.GetComponent<Slider>();
		outputSetting = GameObject.Find("Main").transform.Find("Menu/Setting/Output/OutputDropdown").gameObject.GetComponent<Dropdown>();
		domeObj = GameObject.Find("Main").transform.Find("Menu/Setting/Output/Dome").gameObject;
		pitchField = GameObject.Find("Main").transform.Find("Menu/Setting/Output/Dome/PitchField").gameObject.GetComponent < InputField > ();
		rollField = GameObject.Find("Main").transform.Find("Menu/Setting/Output/Dome/RollField").gameObject.GetComponent < InputField > ();
		fovField = GameObject.Find("Main").transform.Find("Menu/Setting/Output/Dome/FovField").gameObject.GetComponent < InputField > ();
		fixToggle = GameObject.Find("Main").transform.Find("Menu/Setting/Output/Dome/Fix").gameObject.GetComponent < Toggle > ();
		movebehaviour = GameObject.Find("Main").transform.Find("Avatar").gameObject.GetComponent<aAV_MoveBehaviour>();
		xrLefthand = GameObject.Find("Camera Offset").transform.Find("LeftHand Controller").gameObject;
		xrRighthand = GameObject.Find("Camera Offset").transform.Find("RightHand Controller").gameObject;

	}

	void OnEnable(){
		lastLanguage = langSetting.value;
		lastType = typeSetting.value;
		lastZone = int.Parse(zoneField.text);
		lastSlide = ambientSlider.value;
		lastDisplay = outputSetting.value;
		lastPitch = pitchField.text;
		lastRoll = rollField.text;
		lastFOV = fovField.text;
		lastFix = fixToggle.isOn;
	}
	
	void Start()
	{
		ambientSlider.value = RenderSettings.ambientIntensity;
		switch(LocalizationSettings.SelectedLocale.ToString()){
			case "English":
				langSetting.value = 0;
				break;
			case "Japanese":
				langSetting.value = 1;
				break;
			case "Spanish":
				langSetting.value = 2;
				break;
			default:
				break;
		}
		switch(aAV_Public.center.type){
			case "WG":
				typeSetting.value = 0;
				break;
			case "JP":
				typeSetting.value = 1;
				zoneField.text = aAV_Public.center.JPRCS_zone.ToString();
				break;
			case "UT":
				typeSetting.value = 2;
				zoneField.text = aAV_Public.center.UTM_zone.ToString();
				break;
			default:
				break;
		}
		changeType();
		pitchField.text = PlayerPrefs.GetString ("DomePitch", "-90");
		rollField.text = PlayerPrefs.GetString ("DomeRoll", "0");
		fovField.text = PlayerPrefs.GetString ("DomeFov", "180");
		fixToggle.isOn = (PlayerPrefs.GetInt("DomeFix", 0) == 1);
		
		#if UNITY_STANDALONE_WIN
			manualXRControl = new ManualXRControl();
		#endif
	}
	
	public void OnAmbientSlider(){
		RenderSettings.ambientIntensity = ambientSlider.value;
	}

	public void OnOk(){
		switch(langSetting.value){
			case 0:
				ChangeSelectedLocale("en");
				PlayerPrefs.SetString("Language", "en");
				break;
			case 1:
				ChangeSelectedLocale("ja");
				PlayerPrefs.SetString("Language", "ja");
				break;
			case 2:
				ChangeSelectedLocale("es");
				PlayerPrefs.SetString("Language", "es");
				break;
			default:
				break;
		}
		if(typeSetting.value==0){
			PlayerPrefs.SetString("Coordinate", "WG");
		}else if(typeSetting.value==1){
			PlayerPrefs.SetString("Coordinate", "JP");
			PlayerPrefs.SetInt("Zone",int.Parse(zoneField.text));
		}else if(typeSetting.value==2){
			PlayerPrefs.SetString("Coordinate", "UT");
			PlayerPrefs.SetInt("Zone",int.Parse(zoneField.text));
		}
		PlayerPrefs.SetFloat("Ambient", ambientSlider.value);
		PlayerPrefs.SetString("DomePitch", pitchField.text);
		PlayerPrefs.SetString("DomeRoll", rollField.text);
		PlayerPrefs.SetString("DomeFov", fovField.text);
		PlayerPrefs.SetInt("DomeFix", fixToggle.isOn? 1 : 0);
		PlayerPrefs.Save();
		this.gameObject.SetActive(false);
	}

	public void OnCancel(){
		langSetting.value=lastLanguage;
		typeSetting.value = lastType;
		switch(lastType){
			case 0:
				aAV_Public.center.type = "WG";
				break;
			case 1:
				aAV_Public.center.type = "JP";
				aAV_Public.center.JPRCS_zone = lastZone;
				break;
			case 2:
				aAV_Public.center.type = "UT";
				aAV_Public.center.UTM_zone = lastZone;
				break;
			default:
				break;
		}
		zoneField.text = lastZone.ToString();
		ambientSlider.value=lastSlide;
		RenderSettings.ambientIntensity = lastSlide;
		outputSetting.value=lastDisplay;
		pitchField.text = lastPitch;
		rollField.text = lastRoll;
		fovField.text = lastFOV;

		direction.ViewUpdate();
		this.gameObject.SetActive(false);
	}

	public void changeType(){
		switch(typeSetting.value){
			case 0:
				zoneObj.SetActive(false);
				aAV_Public.center.type = "WG";
				direction.ViewUpdate();
				break;
			case 1:
				aAV_Public.center.type = "JP";
				zoneObj.SetActive(true);
				zoneField.text=aAV_Public.center.JPRCS_zone.ToString();
				direction.ViewUpdate();
				break;
			case 2:
				aAV_Public.center.type = "UT";
				zoneObj.SetActive(true);
				zoneField.text = aAV_Public.center.UTM_zone.ToString();
				direction.ViewUpdate();
				break;
			default:
				break;
		}
	}
	
	public void changeZone(){
		switch(typeSetting.value){
			case 0:
				break;
			case 1:
				if(int.Parse(zoneField.text) < 1){
					zoneField.text = "1";
				}else if(int.Parse(zoneField.text) >19){
					zoneField.text = "19";
				}
				aAV_Public.center.JPRCS_zone = int.Parse(zoneField.text);
				direction.ViewUpdate();
				break;
			case 2:
				if(int.Parse(zoneField.text) < 1){
					zoneField.text = "1";
				}else if(int.Parse(zoneField.text) >60){
					zoneField.text = "60";
				}
				aAV_Public.center.UTM_zone = int.Parse(zoneField.text);
				direction.ViewUpdate();
				break;
			default:
				break;
		}
	}
	
	private async Task ChangeSelectedLocale(string lang){
		LocalizationSettings.SelectedLocale = Locale.CreateLocale(lang);
		await LocalizationSettings.InitializationOperation.Task;
		publicObj.GetEntry();
		direction.ViewUpdate();
	}
	
	public void changeOutput(){
		aAV_Public.displayMode = outputSetting.value;
		switch(outputSetting.value){
			case 0:
				domeObj.SetActive(false);
				GameObject.Find("MainCamera").GetComponent<aAV_DomeShader>().enabled = false;
				#if UNITY_STANDALONE_WIN
					manualXRControl.StopXR();
					xrLefthand.SetActive(false);
					xrRighthand.SetActive(false);
				#endif
				break;
			case 1:
				domeObj.SetActive(false);
				GameObject.Find("MainCamera").GetComponent<aAV_DomeShader>().enabled = false;
				#if UNITY_STANDALONE_WIN
					StartCoroutine(manualXRControl.StartXRCoroutine());
					movebehaviour.ChangeAim();
					xrLefthand.SetActive(true);
					xrRighthand.SetActive(true);
				#endif
				break;
			case 2:
				domeObj.SetActive(true);
				GameObject.Find("MainCamera").GetComponent<aAV_DomeShader>().enabled = true;
				#if UNITY_STANDALONE_WIN
					manualXRControl.StopXR();
					xrLefthand.SetActive(false);
					xrRighthand.SetActive(false);
				#endif
				break;
			default:
				break;
		}
	}

	public void changeDome(){
		GameObject.Find("MainCamera").GetComponent<aAV_DomeShader>().worldCameraPitch = float.Parse(pitchField.text);
		GameObject.Find("MainCamera").GetComponent<aAV_DomeShader>().worldCameraRoll = float.Parse(rollField.text);
		GameObject.Find("MainCamera").GetComponent<aAV_DomeShader>().FOV = int.Parse(fovField.text);
		aAV_Public.domeFix = fixToggle.isOn;
	}
}

public class ManualXRControl
{
	public IEnumerator StartXRCoroutine()
	{
		Debug.Log("Initializing XR...");
		yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

		if (XRGeneralSettings.Instance.Manager.activeLoader == null)
		{
			Debug.LogError("Initializing XR Failed. Check Editor or Player log for details.");
		}
		else
		{
			Debug.Log("Starting XR...");
			XRGeneralSettings.Instance.Manager.StartSubsystems();
		}
	}

	public void StopXR()
	{
		Debug.Log("Stopping XR...");

		XRGeneralSettings.Instance.Manager.StopSubsystems();
		XRGeneralSettings.Instance.Manager.DeinitializeLoader();
		Debug.Log("XR stopped completely.");
	}
}

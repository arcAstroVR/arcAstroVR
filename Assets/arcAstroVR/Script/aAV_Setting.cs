using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.XR.Management;
using UnityEngine.XR.Interaction.Toolkit;

public class aAV_Setting : MonoBehaviour
{
	private Transform mainTransform;
	private GameObject menuObj;	
	private GameObject zoneObj;	
	private GameObject domeObj;	
	private GameObject copyrightObj;
	private GameObject domeCamera;	
	private GameObject xrLefthand;
	private GameObject xrRighthand;
	private Transform markerEdit;
	private Transform lineEdit;
	private Transform objectEdit;

	private aAV_Public publicObj;
	private aAV_MoveBehaviour movebehaviour;
	private RectTransform topbar;	
	private RectTransform position;	
	private RectTransform cursor;	
	private RectTransform infoview;	
	private RectTransform icon;	
	private RectTransform datetime;	
	private Image topbarBack;
	private Dropdown langSetting;
	private Dropdown typeSetting;
	private InputField zoneField;
	private InputField avatarField;
	private Text avatarEye;
	private Slider ambientSlider;
	private InputField ambientField;
	private Slider scaleSlider;
	private InputField scaleField;
	private Dropdown outputSetting;
	private InputField pitchField;
	private InputField rollField;
	private InputField fovField;
	private Toggle fixToggle;
	private XRRayInteractor rightHandRay;
	private Light ambientLight;

	private int lastLanguage;
	private int lastType;
	private int lastZone;
	private float lastAvatar;
	private float lastScale;
	private float lastSlide;
	private int lastDisplay;
	private string lastPitch;
	private string lastRoll;
	private string lastFOV;
	private bool lastFix;
	private bool lastXR;
	private RenderMode lastRenderMode;
	private CanvasScaler.ScaleMode lastScaleMode;

	private ManualXRControl manualXRControl;

	void Awake(){
		publicObj = GameObject.Find("Main").gameObject.GetComponent<aAV_Public>();
		
		mainTransform = GameObject.Find("Main").transform;
		menuObj 			= mainTransform.Find("Menu").gameObject;
		topbar	 		= mainTransform.Find("Menu/TopBar").gameObject.GetComponent<RectTransform>();
		topbarBack 		= mainTransform.Find("Menu/TopBar").gameObject.GetComponent<Image>();
		position 			= mainTransform.Find("Menu/TopBar/positionInfo").gameObject.GetComponent<RectTransform>();
		cursor				= mainTransform.Find("Menu/TopBar/cursorInfo").gameObject.GetComponent<RectTransform>();
		infoview			= mainTransform.Find("Menu/InfoView").gameObject.GetComponent<RectTransform>();
		icon	 		= mainTransform.Find("Menu/ToggleSwitch").gameObject.GetComponent<RectTransform>();
		datetime	 		= mainTransform.Find("Menu/DateTimeSetting").gameObject.GetComponent<RectTransform>();
		langSetting 		= mainTransform.Find("Menu/Setting/Language/LanguageDropdown").gameObject.GetComponent<Dropdown>();
		typeSetting 		= mainTransform.Find("Menu/Setting/Type/TypeDropdown").gameObject.GetComponent<Dropdown>();
		zoneObj 			= mainTransform.Find("Menu/Setting/Type/Zone").gameObject;
		zoneField 			= mainTransform.Find("Menu/Setting/Type/Zone/ZoneField").gameObject.GetComponent < InputField > ();
		avatarField 	= mainTransform.Find("Menu/Setting/AvatarHeight/AvatarH_Field").gameObject.GetComponent<InputField>();
		avatarEye 	= mainTransform.Find("Menu/Setting/AvatarHeight/EyeHeight").gameObject.GetComponent<Text>();
		ambientSlider 	= mainTransform.Find("Menu/Setting/Ambient/AmbientSlider").gameObject.GetComponent<Slider>();
		ambientField 	= mainTransform.Find("Menu/Setting/Ambient/AmbientField").gameObject.GetComponent < InputField > ();
		scaleSlider 	= mainTransform.Find("Menu/Setting/UIscale/ScaleSlider").gameObject.GetComponent<Slider>();
		scaleField 	= mainTransform.Find("Menu/Setting/UIscale/ScaleField").gameObject.GetComponent < InputField > ();
		outputSetting 	= mainTransform.Find("Menu/Setting/Output/OutputDropdown").gameObject.GetComponent<Dropdown>();
		copyrightObj 	= mainTransform.Find("Menu/Copyright").gameObject;
		domeObj 			= mainTransform.Find("Menu/Setting/Output/Dome").gameObject;
		pitchField 			= mainTransform.Find("Menu/Setting/Output/Dome/PitchField").gameObject.GetComponent < InputField > ();
		rollField 			= mainTransform.Find("Menu/Setting/Output/Dome/RollField").gameObject.GetComponent < InputField > ();
		fovField 			= mainTransform.Find("Menu/Setting/Output/Dome/FovField").gameObject.GetComponent < InputField > ();
		fixToggle 			= mainTransform.Find("Menu/Setting/Output/Dome/Fix").gameObject.GetComponent < Toggle > ();
		movebehaviour = mainTransform.Find("Avatar").gameObject.GetComponent<aAV_MoveBehaviour>();
		domeCamera 	= GameObject.Find("Camera Offset").transform.Find("DomeCamera").gameObject;
		xrLefthand 		= GameObject.Find("Camera Offset").transform.Find("LeftHand Controller").gameObject;
		xrRighthand 		= GameObject.Find("Camera Offset").transform.Find("RightHand Controller").gameObject;
		rightHandRay = xrRighthand.GetComponent<XRRayInteractor>();
		ambientLight = GameObject.Find("AmbientLight").GetComponent<Light>();
		markerEdit = mainTransform.Find("Menu/MarkerEdit").gameObject.transform;
		lineEdit = mainTransform.Find("Menu/LineEdit").gameObject.transform;
		objectEdit = mainTransform.Find("Menu/ObjectEdit").gameObject.transform;

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
		if(aAV_Public.basicInfo.avatar_H == 0f){
			avatarField.text = (PlayerPrefs.GetFloat("AvatarHeight", aAV_Public.basicInfo.avatar_H)).ToString("F1");
		}else{
			avatarField.text = aAV_Public.basicInfo.avatar_H.ToString("F1");
		}
		avatarEye.text = (float.Parse(avatarField.text)*1.654f/176f).ToString("F1")+"cm";
		scaleSlider.value = PlayerPrefs.GetFloat("ScaleUI", Screen.dpi/100f)*100f/Screen.dpi;
		ambientSlider.value = PlayerPrefs.GetFloat("Ambient", 1f);
		pitchField.text = PlayerPrefs.GetString ("DomePitch", "-90");
		rollField.text = PlayerPrefs.GetString ("DomeRoll", "0");
		fovField.text = PlayerPrefs.GetString ("DomeFov", "180");
		fixToggle.isOn = (PlayerPrefs.GetInt("DomeFix", 0) == 1);
		domeObj.SetActive(false);
		
		#if UNITY_STANDALONE_WIN
			manualXRControl = new ManualXRControl();
		#endif
	}
	
	void OnEnable(){
		lastLanguage = langSetting.value;
		lastType = typeSetting.value;
		lastZone = int.Parse(zoneField.text);
		lastAvatar = float.Parse(avatarField.text);
		avatarEye.text = (lastAvatar*165.4f/176f).ToString("F1")+"cm";
		lastScale = scaleSlider.value;
		lastSlide = ambientSlider.value;
		ambientField.text = lastSlide.ToString("F1");
		lastDisplay = outputSetting.value;
		lastPitch = pitchField.text;
		lastRoll = rollField.text;
		lastFOV = fovField.text;
		lastFix = fixToggle.isOn;
		lastXR = xrLefthand.activeInHierarchy;
		lastRenderMode = menuObj.GetComponent<Canvas>().renderMode;
		lastScaleMode = menuObj.GetComponent<CanvasScaler>().uiScaleMode;
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
		PlayerPrefs.SetFloat("AvatarHeight", float.Parse(avatarField.text));
		PlayerPrefs.SetFloat("ScaleUI", scaleSlider.value*Screen.dpi/100f);
		PlayerPrefs.SetFloat("Ambient", ambientSlider.value);
		PlayerPrefs.SetString("DomePitch", pitchField.text);
		PlayerPrefs.SetString("DomeRoll", rollField.text);
		PlayerPrefs.SetString("DomeFov", fovField.text);
		PlayerPrefs.SetInt("DomeFix", fixToggle.isOn? 1 : 0);
		PlayerPrefs.Save();
		aAV_Event.selectStella = false;
		aAV_Event.selectInfo = false;
		aAV_Event.selectDate = false;
		mainTransform.Find("Menu/ToggleSwitch/Select").gameObject.SetActive(false);

		#if UNITY_STANDALONE_WIN
		rightHandRay.enabled = false;
		#endif

		this.gameObject.SetActive(false);
	}

	public void OnCancel(){
		langSetting.value=lastLanguage;
		typeSetting.value = lastType;
		xrLefthand.SetActive(lastXR);
		xrRighthand.SetActive(lastXR);
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
		avatarField.text = lastAvatar.ToString("F1");
		changeAvatarHeight();
		scaleSlider.value=lastScale ;
		ambientSlider.value=lastSlide;
		RenderSettings.ambientIntensity = lastSlide*7f-6f;
		ambientLight.intensity = lastSlide-1f;
		outputSetting.value=lastDisplay;
		pitchField.text = lastPitch;
		rollField.text = lastRoll;
		fovField.text = lastFOV;
		menuObj.GetComponent<Canvas>().renderMode = lastRenderMode;
		menuObj.GetComponent<CanvasScaler>().uiScaleMode = lastScaleMode;
		aAV_Event.selectStella = false;
		aAV_Event.selectInfo = false;
		aAV_Event.selectDate = false;
		mainTransform.Find("Menu/ToggleSwitch/Select").gameObject.SetActive(false);
		
		#if UNITY_STANDALONE_WIN
		rightHandRay.enabled = false;
		#endif
		
		this.gameObject.SetActive(false);
	}

	public void changeType(){
		switch(typeSetting.value){
			case 0:
				zoneObj.SetActive(false);
				aAV_Public.center.type = "WG";
				menuObj.GetComponent<aAV_Direction>().ViewUpdate();
				break;
			case 1:
				aAV_Public.center.type = "JP";
				zoneObj.SetActive(true);
				zoneField.text=aAV_Public.center.JPRCS_zone.ToString();
				menuObj.GetComponent<aAV_Direction>().ViewUpdate();
				break;
			case 2:
				aAV_Public.center.type = "UT";
				zoneObj.SetActive(true);
				zoneField.text = aAV_Public.center.UTM_zone.ToString();
				menuObj.GetComponent<aAV_Direction>().ViewUpdate();
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
				menuObj.GetComponent<aAV_Direction>().ViewUpdate();
				break;
			case 2:
				if(int.Parse(zoneField.text) < 1){
					zoneField.text = "1";
				}else if(int.Parse(zoneField.text) >60){
					zoneField.text = "60";
				}
				aAV_Public.center.UTM_zone = int.Parse(zoneField.text);
				menuObj.GetComponent<aAV_Direction>().ViewUpdate();
				break;
			default:
				break;
		}
	}
	
	private async Task ChangeSelectedLocale(string lang){
		LocalizationSettings.SelectedLocale = Locale.CreateLocale(lang);
		await LocalizationSettings.InitializationOperation.Task;
		publicObj.GetEntry();
		menuObj.GetComponent<aAV_Direction>().ViewUpdate();
	}

	public void changeAvatarHeight(){
		avatarEye.text = (float.Parse(avatarField.text)*165.4f/176f).ToString("F1")+"cm";
		GameObject.Find("Main").transform.Find("Avatar").gameObject.transform.localScale = (float.Parse(avatarField.text) / 176f) * Vector3.one;
		GameObject.Find("Main Camera").GetComponent<aAV_ThirdPersonOrbitCamBasic>().pivotOffset = new Vector3(0f,float.Parse(avatarField.text)*1.654f/176f,0f);
		GameObject.Find("Main Camera").GetComponent<aAV_ThirdPersonOrbitCamBasic>().ResetTargetOffsets();
	}

	public void OnScaleSlider(){
		scaleField.text = scaleSlider.value.ToString("F1");
		menuObj.GetComponent<CanvasScaler>().scaleFactor = scaleSlider.value*Screen.dpi/100f;
	}

	public void changeScale(){
		var scale = float.Parse(scaleField.text);
		if(scale > 1.5f){
			scale = 1.5f;
		}else if(scale <0.5f){
			scale = 0.5f;
		}
		scaleSlider.value = scale;
	}

	public void OnAmbientSlider(){
		ambientField.text = ambientSlider.value.ToString("F1");
		RenderSettings.ambientIntensity = ambientSlider.value*7f-6f;
		ambientLight.intensity = ambientSlider.value-1f;
	}

	public void changeAmbient(){
		var ambient = float.Parse(ambientField.text);
		if(ambient > 8f){
			ambient = 8f;
		}else if(ambient <1f){
			ambient = 1f;
		}
		ambientSlider.value = ambient;
	}

	public void changeOutput(){
		aAV_Public.displayMode = outputSetting.value;
		switch(outputSetting.value){
			case 0:	//PC mode
				// Active Controll
				copyrightObj.SetActive(true);
				menuObj.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
				menuObj.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
				mainTransform.Find("Menu/TopBar").gameObject.SetActive(true);
				mainTransform.Find("Menu/TopBar/showButton").gameObject.SetActive(true);
				mainTransform.Find("Menu/TopBar/settingButton").gameObject.SetActive(true);
				mainTransform.Find("Menu/TopBar/cursorInfo").gameObject.SetActive(true);
				mainTransform.Find("Menu/InfoView").gameObject.SetActive(true);
				mainTransform.Find("Menu/InfoView/SaveButton").gameObject.SetActive(true);
				mainTransform.Find("Menu/ToggleSwitch").gameObject.SetActive(true);
				mainTransform.Find("Menu/DateTimeSetting").gameObject.SetActive(true);
				mainTransform.Find("Menu/TopBar/showButton/Text").gameObject.GetComponent<Text> ().text = "Close Info";
				domeObj.SetActive(false);
				domeCamera.SetActive(false);
				#if UNITY_STANDALONE_WIN
				if(XRGeneralSettings.Instance && XRGeneralSettings.Instance.Manager.activeLoader != null){
					manualXRControl.StopXR();
					xrLefthand.SetActive(false);
					xrRighthand.SetActive(false);
				}
				#endif
				
				//UI Position
				transform.localScale = new Vector3(1f,1f,1f);
				markerEdit.localScale = new Vector3(1f,1f,1f);
				lineEdit.localScale = new Vector3(1f,1f,1f);
				objectEdit.localScale = new Vector3(1f,1f,1f);
				topbarBack.enabled = true;
				topbar.anchorMin = new Vector2(0f, 1f);
				topbar.anchorMax = new Vector2(1f, 1f);
				topbar.anchoredPosition = new Vector3(0f,0f,0f);
				position.anchoredPosition = new Vector3(120f,0f,0f);
				cursor.anchoredPosition = new Vector3(480f,0f,0f);
				infoview.anchorMin = new Vector2(0f, 1f);
				infoview.anchorMax = new Vector2(0f, 1f);
				infoview.anchoredPosition = new Vector3(0f,-18f,0f);
				infoview.localScale = new Vector3(1f,1f,1f);
				icon.anchorMin = new Vector2(0f, 0f);
				icon.anchorMax = new Vector2(0f, 0f);
				icon.anchoredPosition = new Vector3(5f,30f,0f);
				icon.localScale = new Vector3(1f,1f,1f);
				mainTransform.Find("Menu/ToggleSwitch/Toggle-C").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f,0f,0f);
				mainTransform.Find("Menu/ToggleSwitch/Toggle-V").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(50f,0f,0f);
				mainTransform.Find("Menu/ToggleSwitch/Toggle-R").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(100f,0f,0f);
				mainTransform.Find("Menu/ToggleSwitch/Toggle-E").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(150f,0f,0f);
				mainTransform.Find("Menu/ToggleSwitch/Toggle-Z").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(200f,0f,0f);
				mainTransform.Find("Menu/ToggleSwitch/Toggle-Q").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(250f,0f,0f);
				mainTransform.Find("Menu/ToggleSwitch/Toggle-T").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(300f,0f,0f);
				mainTransform.Find("Menu/ToggleSwitch/Toggle-U").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(350f,0f,0f);
				mainTransform.Find("Menu/ToggleSwitch/Toggle-P").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(400f,0f,0f);
				datetime.anchorMin = new Vector2(1f, 0f);
				datetime.anchorMax = new Vector2(1f, 0f);
				datetime.anchoredPosition = new Vector3(-5f,10f,0f);
				datetime.localScale = new Vector3(1f,1f,1f);

				menuObj.GetComponent<aAV_Direction>().ViewUpdate();
				break;
			case 1:	//XR mode
				// Active Controll
				copyrightObj.SetActive(false);
				menuObj.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
				menuObj.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPhysicalSize;
				mainTransform.Find("Menu/TopBar").gameObject.SetActive(false);
				mainTransform.Find("Menu/TopBar/showButton").gameObject.SetActive(false);
				mainTransform.Find("Menu/TopBar/settingButton").gameObject.SetActive(false);
				mainTransform.Find("Menu/TopBar/cursorInfo").gameObject.SetActive(false);
				mainTransform.Find("Menu/InfoView").gameObject.SetActive(false);
				mainTransform.Find("Menu/InfoView/SaveButton").gameObject.SetActive(false);
				mainTransform.Find("Menu/ToggleSwitch").gameObject.SetActive(false);
				mainTransform.Find("Menu/DateTimeSetting").gameObject.SetActive(false);
				mainTransform.Find("Menu/TopBar/showButton/Text").gameObject.GetComponent<Text> ().text = "Show Info";
				domeObj.SetActive(false);
				domeCamera.SetActive(false);
				#if UNITY_STANDALONE_WIN
				if(XRGeneralSettings.Instance && XRGeneralSettings.Instance.Manager.activeLoader == null){
					StartCoroutine(manualXRControl.StartXRCoroutine());
					movebehaviour.ChangeAim();
					xrLefthand.SetActive(true);
					xrRighthand.SetActive(true);
					rightHandRay.enabled = true;
				}
				#endif

				//UI Position
				transform.localScale = new Vector3(0.4f,0.4f,0.4f);
				markerEdit.localScale = new Vector3(0.4f,0.4f,0.4f);
				lineEdit.localScale = new Vector3(0.4f,0.4f,0.4f);
				objectEdit.localScale = new Vector3(0.4f,0.4f,0.4f);
				topbarBack.enabled = false;
//				topbar.anchorMin = new Vector2(0.5f, 0.5f);
//				topbar.anchorMax = new Vector2(0.5f, 0.5f);
//				topbar.anchoredPosition = new Vector3(-130f,130f,0f);
//				position.anchoredPosition = new Vector3(5f,-15f,0f);
//				cursor.anchoredPosition = new Vector3(5f,-25f,0f);
				infoview.anchorMin = new Vector2(0.5f, 0.5f);
				infoview.anchorMax = new Vector2(0.5f, 0.5f);
				infoview.anchoredPosition = new Vector3(-120f,30f,0f);
				infoview.localScale = new Vector3(0.4f,0.4f,0.4f);
				icon.anchorMin = new Vector2(0.5f, 0.5f);
				icon.anchorMax = new Vector2(0.5f, 0.5f);
				icon.anchoredPosition = new Vector3(-88f,-70f,0f);
				icon.localScale = new Vector3(0.4f,0.4f,0.4f);
				mainTransform.Find("Menu/ToggleSwitch/Toggle-C").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f,0f,0f);
				mainTransform.Find("Menu/ToggleSwitch/Toggle-V").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(50f,0f,0f);
				mainTransform.Find("Menu/ToggleSwitch/Toggle-R").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(100f,0f,0f);
				mainTransform.Find("Menu/ToggleSwitch/Toggle-E").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(150f,0f,0f);
				mainTransform.Find("Menu/ToggleSwitch/Toggle-Z").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(200f,0f,0f);
				mainTransform.Find("Menu/ToggleSwitch/Toggle-Q").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(250f,0f,0f);
				mainTransform.Find("Menu/ToggleSwitch/Toggle-T").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(300f,0f,0f);
				mainTransform.Find("Menu/ToggleSwitch/Toggle-U").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(350f,0f,0f);
				mainTransform.Find("Menu/ToggleSwitch/Toggle-P").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(400f,0f,0f);
				datetime.anchorMin = new Vector2(0.5f, 0.5f);
				datetime.anchorMax = new Vector2(0.5f, 0.5f);
				datetime.anchoredPosition = new Vector3(45f,-90f,0f);
				datetime.localScale = new Vector3(0.4f,0.4f,0.4f);
				
				menuObj.GetComponent<aAV_Direction>().ViewUpdate();
				break;
			case 2:	//Domemaster mode
				// Active Controll
				copyrightObj.SetActive(false);
				menuObj.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
				menuObj.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
				menuObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080f,1080f);
				mainTransform.Find("Menu/TopBar").gameObject.SetActive(true);
				mainTransform.Find("Menu/TopBar/showButton").gameObject.SetActive(true);
				mainTransform.Find("Menu/TopBar/settingButton").gameObject.SetActive(true);
				mainTransform.Find("Menu/TopBar/cursorInfo").gameObject.SetActive(true);
				mainTransform.Find("Menu/InfoView").gameObject.SetActive(false);
				mainTransform.Find("Menu/InfoView/SaveButton").gameObject.SetActive(true);
				mainTransform.Find("Menu/ToggleSwitch").gameObject.SetActive(true);
				mainTransform.Find("Menu/DateTimeSetting").gameObject.SetActive(true);
				mainTransform.Find("Menu/TopBar/showButton/Text").gameObject.GetComponent<Text> ().text = "Show Info";
				domeObj.SetActive(true);
				domeCamera.SetActive(true);
				#if UNITY_STANDALONE_WIN
				if(XRGeneralSettings.Instance && XRGeneralSettings.Instance.Manager.activeLoader != null){
					manualXRControl.StopXR();
					xrLefthand.SetActive(false);
					xrRighthand.SetActive(false);
				}
				#endif

				//UI Position
				transform.localScale = new Vector3(1f,1f,1f);
				markerEdit.localScale = new Vector3(1f,1f,1f);
				lineEdit.localScale = new Vector3(1f,1f,1f);
				objectEdit.localScale = new Vector3(1f,1f,1f);
				topbarBack.enabled = false;
				topbar.anchorMin = new Vector2(0f, 1f);
				topbar.anchorMax = new Vector2(1f, 1f);
				topbar.anchoredPosition = new Vector3(0f,0f,0f);
				position.anchoredPosition = new Vector3(5f,-15f,0f);
				cursor.anchoredPosition = new Vector3(5f,-25f,0f);
				infoview.anchorMin = new Vector2(0.5f, 0.5f);
				infoview.anchorMax = new Vector2(0.5f, 0.5f);
				infoview.anchoredPosition = new Vector3(-320f,-200f,0f);
				infoview.localScale = new Vector3(1f,1f,1f);
				icon.anchorMin = new Vector2(0f, 0f);
				icon.anchorMax = new Vector2(0f, 0f);
				icon.anchoredPosition = new Vector3(5f,30f,0f);
				icon.localScale = new Vector3(1f,1f,1f);
				mainTransform.Find("Menu/ToggleSwitch/Toggle-C").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f,140f,0f);
				mainTransform.Find("Menu/ToggleSwitch/Toggle-V").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(50f,140f,0f);
				mainTransform.Find("Menu/ToggleSwitch/Toggle-R").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f,70f,0f);
				mainTransform.Find("Menu/ToggleSwitch/Toggle-E").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(50f,70f,0f);
				mainTransform.Find("Menu/ToggleSwitch/Toggle-Z").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(100f,70f,0f);
				mainTransform.Find("Menu/ToggleSwitch/Toggle-Q").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f,0f,0f);
				mainTransform.Find("Menu/ToggleSwitch/Toggle-T").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(50f,0f,0f);
				mainTransform.Find("Menu/ToggleSwitch/Toggle-U").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(100f,0f,0f);
				mainTransform.Find("Menu/ToggleSwitch/Toggle-P").gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(150f,0f,0f);
				menuObj.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
				menuObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080f,1080f);
				mainTransform.Find("Menu/TopBar/showButton/Text").gameObject.GetComponent<Text> ().text = "Show Info";
				mainTransform.Find("Menu/InfoView").gameObject.SetActive(false);
				datetime.anchorMin = new Vector2(1f, 0f);
				datetime.anchorMax = new Vector2(1f, 0f);
				datetime.anchoredPosition = new Vector3(-5f,10f,0f);
				datetime.localScale = new Vector3(1f,1f,1f);
				
				menuObj.GetComponent<aAV_Direction>().ViewUpdate();
				break;
			default:
				break;
		}
	}

	public void changeDome(){
		domeCamera.GetComponent<aAV_Domemaster>().domeCameraPitch = float.Parse(pitchField.text);
		domeCamera.GetComponent<aAV_Domemaster>().domeCameraRoll = float.Parse(rollField.text);
		domeCamera.GetComponent<aAV_Domemaster>().FOV = int.Parse(fovField.text);
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

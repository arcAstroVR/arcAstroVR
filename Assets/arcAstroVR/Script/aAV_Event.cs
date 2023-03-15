using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.XR.Management;
using UnityEngine.XR.Interaction.Toolkit;

public class aAV_Event : MonoBehaviour
{
	[SerializeField]
	private InputActionReference moveXR;

	[SerializeField]
	private InputActionReference lookXR;

	[SerializeField]
	private InputActionReference sprintXR;

	[SerializeField]
	private InputActionReference flyupXR;

	public static float moveH = 0;				//Avatar移動設定：aAV_BasicBehaviour.csでこの値を参照して実行
	public static float moveV = 0;
	public static float rotateH = 0;				//Avatar回転設定：aAV_ThirdPersonOrbitCamBasic.csでこの値を参照して実行
	public static float rotateV = 0;
	public static bool sprint = false;
	public static bool rise = false;
	public static bool zoom = false;
	public static bool selectStella = false;
	public static bool selectInfo = false;
	public static bool selectDate = false;
	public static bool textInput = false;			//EditWindow操作

	public AudioClip audioSelect;
	public AudioClip audioButton;
	public AudioClip audioOpen;
	public AudioClip audioClose;

	private GameObject menuObj;	
	private GameObject topbarGameobject;
	private GameObject infoGameobject;
	private GameObject targetGameobject;
	private GameObject stellaGameobject;
	private GameObject selectGameobject;
	private GameObject dateTimeSetting;
	private GameObject tzInputField;
	private GameObject yearInputField;
	private GameObject monthInputField;
	private GameObject dayInputField;
	private GameObject hourInputField;
	private GameObject minuteInputField;
	private GameObject secondInputField;
	private GameObject updateButton;
	private GameObject settingGameobject;
	private GameObject markerCam;
	private GameObject mapCam;
	private XRRayInteractor rightHand;
	private Text copyright;

	private aAV_icon aAV_icon_script;
	private aAV_UI aAV_UI_script;
	private aAV_MoveBehaviour aAV_Move_script;
	private aAV_FlyBehaviour aAV_Fly_script;
	private aAV_StelMouseZoom aAV_Zoom_script;
	private aAV_ThirdPersonOrbitCamBasic aAV_Cam_script;
	private aAV_CamEdit aAV_MarkerCam_script;
	private aAV_CompassMap aAV_Compass_script;
	private int stellaNo = 0;
	private int dateNo = 1;
	private int infoNo = 1;
	private int infoAction = 1;

	private float checkTime = 0f;
	private float pressTime = 0f;
	private bool moveHold = false;
	private float angleH = 0;                                          // Float to store camera horizontal angle related to mouse movement.
	private float angleV = 0;                                          // Float to store camera vertical angle related to mouse movement.

	private Color inputColor = new Color(1.0f, 0.8f, 0.2f);
	private Color OnColor = new Color(0.25f, 0.6f, 0.25f, 1.0f);
	private Color OffColor = new Color(0f, 0f, 0f, 0f);
	private Color dateOffColor = new Color(0.25f, 0.25f, 0.4f, 0.8f);
	private Color infoOffColor = new Color(0f, 0f, 0f, 0.4f);
	private Color buttonOffColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
	private Color buttonOnColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);


	void Awake(){
		Transform mainTrans = GameObject.Find("Main").transform;
		menuObj 			= mainTrans.Find("Menu").gameObject;
		topbarGameobject = mainTrans.Find("Menu/TopBar").gameObject;
		infoGameobject = mainTrans.Find("Menu/InfoView").gameObject;
		stellaGameobject = mainTrans.Find("Menu/ToggleSwitch").gameObject;
		selectGameobject = mainTrans.Find("Menu/ToggleSwitch/Select").gameObject;
		dateTimeSetting= mainTrans.Find("Menu/DateTimeSetting").gameObject;
		tzInputField= mainTrans.Find("Menu/DateTimeSetting/TimeZone/TZInputField").gameObject;
		yearInputField= mainTrans.Find("Menu/DateTimeSetting/Date/YearInputField").gameObject;
		monthInputField= mainTrans.Find("Menu/DateTimeSetting/Date/MonthInputField").gameObject;
		dayInputField= mainTrans.Find("Menu/DateTimeSetting/Date/DayInputField").gameObject;
		hourInputField= mainTrans.Find("Menu/DateTimeSetting/Time/HourInputField").gameObject;
		minuteInputField= mainTrans.Find("Menu/DateTimeSetting/Time/MinuteInputField").gameObject;
		secondInputField= mainTrans.Find("Menu/DateTimeSetting/Time/SecondInputField").gameObject;
		updateButton = mainTrans.Find("Menu/DateTimeSetting/TimeZone/UpdateButton").gameObject;
		settingGameobject = mainTrans.Find("Menu/Setting").gameObject;
		rightHand =GameObject.Find("XR Origin").transform.Find("Camera Offset/RightHand Controller").gameObject.GetComponent<XRRayInteractor>();
		copyright = mainTrans.Find("Menu/Copyright").gameObject.GetComponent<Text>();
		markerCam = mainTrans.Find("MarkerCamera").gameObject;
		mapCam = mainTrans.Find("MapCamera").gameObject;
		
		aAV_UI_script = dateTimeSetting.GetComponent<aAV_UI>();
		aAV_icon_script = mainTrans.Find("Menu/ToggleSwitch").gameObject.GetComponent<aAV_icon>();
		aAV_Move_script = mainTrans.Find("Avatar").gameObject.GetComponent<aAV_MoveBehaviour>();
		aAV_Fly_script = mainTrans.Find("Avatar").gameObject.GetComponent<aAV_FlyBehaviour>();
		aAV_Zoom_script = mainTrans.Find("Stellarium").gameObject.GetComponent<aAV_StelMouseZoom>();
		aAV_Compass_script = mainTrans.Find("LineCanvas").gameObject.GetComponent<aAV_CompassMap>();
		aAV_MarkerCam_script = mainTrans.Find("Menu/CamEdit").gameObject.GetComponent<aAV_CamEdit>();
		aAV_Cam_script = GameObject.Find("XR Origin").transform.Find("Camera Offset/Main Camera").gameObject.GetComponent<aAV_ThirdPersonOrbitCamBasic>();
	}
	
	void Update(){		//押している間連続処理系（時刻変更等）
		checkTime += Time.deltaTime;
		if(!textInput){
			if(Gamepad.current != null){
				pressTime += Time.deltaTime;
				if(selectDate && checkTime > 0){
					checkTime -= 0.05f;
					if(Gamepad.current.dpad.up.isPressed && pressTime > 1f){
						InputFieldUp(dateNo);
					}else if(Gamepad.current.dpad.down.isPressed && pressTime > 1f){
						InputFieldDown(dateNo);
					}else if(Gamepad.current.dpad.left.isPressed && pressTime > 1f){
						dateNo -= 1;
						UnselectInputField();
						SelectInputField();
					}else if(Gamepad.current.dpad.right.isPressed && pressTime > 1f){
						dateNo += 1;
						UnselectInputField();
						SelectInputField();
					}
					if(Gamepad.current.buttonEast.isPressed){
						updateButton.GetComponent<Image>().color = buttonOnColor;
					}else{
						updateButton.GetComponent<Image>().color = buttonOffColor;
					}
				}
				if(selectStella && checkTime > 0){
					checkTime -= 0.05f;
					if(Gamepad.current.dpad.left.isPressed && pressTime > 1f){
						stellaNo -= 1;
						stellaSelectPos();
					}else if(Gamepad.current.dpad.right.isPressed && pressTime > 1f){
						stellaNo += 1;
						stellaSelectPos();
					}
				}
				if(selectInfo && checkTime > 0){
					checkTime -= 0.05f;
					if(Gamepad.current.dpad.up.isPressed && pressTime > 1f){
						infoNo -= 1;
						UnselectInfotarget();
						selectInfotarget();
					}else if(Gamepad.current.dpad.down.isPressed && pressTime > 1f){
						infoNo += 1;
						UnselectInfotarget();
						selectInfotarget();
					}
				}
			}
		}
		
		//XR Controll処理系(XR Controllの押し続ける系の処理はInputActionでうまくとらえられないため、こちらで記述。改善されたらInputActionに移行予定)
		if(XRGeneralSettings.Instance && XRGeneralSettings.Instance.Manager.activeLoader != null){
			moveH=0;
			moveV=0;
			rotateH=0;
			if(moveXR.action.ReadValue<Vector2>().sqrMagnitude > 0.1f){
				moveH=moveXR.action.ReadValue<Vector2>().x * 0.5f;
				moveV=moveXR.action.ReadValue<Vector2>().y * 0.5f;
			}	
			if(lookXR.action.ReadValue<Vector2>().sqrMagnitude > 0.1f){
				rotateH=lookXR.action.ReadValue<Vector2>().x * 0.5f;
			}
			if(sprintXR.action.ReadValue<float>() > 0.1f){
				sprint = true;
			}else{
				sprint = false;
			}
			if(flyupXR.action.ReadValue<float>() > 0.1f){
				rise = true;
			}else{
				rise = false;
			}
		}

		//Zoom処理
		if(zoom){
			aAV_Zoom_script.Zoom();
		}
	
		//Copyright 描画処理
		Ray ray = Camera.main.ViewportPointToRay(new Vector2(0.5f, 0f));
		checkRay(ray);
		if(aAV_Public.copyright != ""){
			copyright.text = "Copyright : "+aAV_Public.copyright;
		}else{
			copyright.text = "";
		}
		aAV_Public.copyright="";
	}

	private void checkRay(Ray ray){		//copyright確認用Ray
		foreach (RaycastHit hit in Physics.RaycastAll(ray)){
			if(hit.collider.name.Contains("terrain")){
				string tNo = hit.collider.name.Trim().Substring(hit.collider.name.Trim().Length - 2);
				if(tNo == "00"){
					aAV_Public.copyright += checkCopyright(aAV_Public.basicInfo.copyright_N);
				}else{
					aAV_Public.copyright += checkCopyright(aAV_Public.basicInfo.copyright_W);
				}
			}
		}
	}
	private string checkCopyright(string text){
		string copy = "";
		if(!aAV_Public.copyright.Contains(text)){
			if(aAV_Public.copyright !=""){
				copy = ", ";
			}
			copy += text;
		}
		return copy;
	}
	
	public void OnBack(InputAction.CallbackContext context) {		//キャンセル操作：GamePAD "×"
		if (!context.performed) return;
		if (!selectStella && !selectInfo && !selectDate && !textInput){	//Stella Menu On
			GetComponent<AudioSource>().PlayOneShot (audioOpen);
			selectGameobject.SetActive(true);
			stellaSelectPos();
			selectStella = true;
			selectInfo = false;
			selectDate = false;
			moveH=0;
			moveV=0;
		}else if(selectStella || selectInfo || selectDate){		//全Off
			GetComponent<AudioSource>().PlayOneShot (audioClose);
			if(markerCam.activeSelf){
				aAV_MarkerCam_script.OnCancel();
			}else if(mapCam.activeSelf){
				aAV_Compass_script.CloseCompassMap();
			}else{
				dateTimeSetting.GetComponent<Image>().color = dateOffColor ;
				selectGameobject.SetActive(false);
				UnselectInputField();
				UnselectInfotarget();
				stellaSelectPos();
				selectStella = false;
				selectInfo = false;
				selectDate = false;
			}
		}else{
			Debug.Log("Back button");
		}
	}
	
	public void OnDo(InputAction.CallbackContext context) {		//決定操作：GamePAD "○"
		if (!context.performed) return;
		if (!selectDate && !selectStella && !selectInfo && !textInput){	//日付メニュー On
			GetComponent<AudioSource>().PlayOneShot (audioOpen);
			dateTimeSetting.GetComponent<Image>().color = OnColor ;
			selectStella = false;
			selectInfo = false;
			selectDate = true;
			moveH=0;
			moveV=0;
			SelectInputField();
		}else if(selectStella){		//Stella実行
			GetComponent<AudioSource>().PlayOneShot (audioButton);
			switch(stellaNo){
				case 0:
					aAV_icon_script.Constellation_Line();
					break;
				case 1:
					aAV_icon_script.Constellation_Label();
					break;
				case 2:
					aAV_icon_script.Constellation_Art();
					break;
				case 3:
					aAV_icon_script.Equatorial_Grid();
					break;
				case 4:
					aAV_icon_script.Azimuthal_Grid();
					break;
				case 5:
					aAV_icon_script.Cardinal_Point();
					break;
				case 6:
					aAV_icon_script.Atmosphere();
					break;
				case 7:
					aAV_icon_script.ArchaeoLines();
					break;
				case 8:
					aAV_icon_script.Planet_Labels();
					break;
				default:
					break;
			}
		}else if(selectDate){		//Date実行
			GetComponent<AudioSource>().PlayOneShot (audioButton);
			aAV_UI_script.GenerateSkybox();
		}else if(selectInfo){		//InfoView実行
			GetComponent<AudioSource>().PlayOneShot (audioButton);
			if(infoNo != 0){
				if(infoNo <= aAV_Public.rplist.Count){
					if(infoAction == 1){
						targetGameobject.transform.GetChild(0).GetComponent<Toggle> ().isOn = !(targetGameobject.transform.GetChild(0).GetComponent<Toggle>().isOn);
					}else if(infoAction ==2){
						targetGameobject.GetComponent<aAV_InfoAction>().JumpPosition();
					}else{
						targetGameobject.GetComponent<aAV_InfoAction>().CamView();
					}
				}else if(infoNo <= aAV_Public.rplist.Count+aAV_Public.linelist.Count){
					if(infoAction == 1){
						targetGameobject.transform.GetChild(0).GetComponent<Toggle> ().isOn = !(targetGameobject.transform.GetChild(0).GetComponent<Toggle>().isOn);
					}else{
						targetGameobject.GetComponent<aAV_InfoAction>().ShowCompassMap();
					}
				}else{
						targetGameobject.transform.GetChild(0).GetComponent<Toggle> ().isOn = !(targetGameobject.transform.GetChild(0).GetComponent<Toggle>().isOn);
				}
			}
		}else{	//その他決定動作
			Debug.Log("OK button");
		}
	}

	public void OnMove(InputAction.CallbackContext context) {	//選択操作：GamePAD "←→↑↓"
		if (context.started) return; //最初の押し下げは無視
		if(!selectStella && !selectDate && !selectInfo  && !textInput){	//通常操作
			moveH=context.ReadValue<Vector2>().x;
			moveV=context.ReadValue<Vector2>().y;
		}else if(selectStella){	//Stellaメニュー操作
			pressTime = 0f;
			checkTime = 0f;
			if(Gamepad.current.dpad.right.wasPressedThisFrame){	//通常時Stellaメニュー選択：GamePAD "→"
				GetComponent<AudioSource>().PlayOneShot (audioSelect);
				stellaNo += 1;
			}else if(Gamepad.current.dpad.left.wasPressedThisFrame){	//通常時Stellaメニュー選択：GamePAD "←"
				GetComponent<AudioSource>().PlayOneShot (audioSelect);
				stellaNo -= 1;
			}else if((Gamepad.current.dpad.up.wasPressedThisFrame)&&(aAV_Public.displayMode == 2)){
				//DomeMasterStellaメニュー選択：GamePAD "↑"
				GetComponent<AudioSource>().PlayOneShot (audioSelect);
				switch(stellaNo){
					case 2:
						stellaNo = 0;
						break;
					case 3:
						stellaNo = 1;
						break;
					case 5:
						stellaNo = 2;
						break;
					case 6:
						stellaNo = 3;
						break;
					case 7:
						stellaNo = 4;
						break;
					default:
						break;
				}
			}else if((Gamepad.current.dpad.down.wasPressedThisFrame)&&(aAV_Public.displayMode == 2)){
				//DomeMasterStellaメニュー選択：GamePAD "↓"
				GetComponent<AudioSource>().PlayOneShot (audioSelect);
				switch(stellaNo){
					case 0:
						stellaNo = 2;
						break;
					case 1:
						stellaNo = 3;
						break;
					case 2:
						stellaNo = 5;
						break;
					case 3:
						stellaNo = 6;
						break;
					case 4:
						stellaNo = 7;
						break;
					default:
						break;
				}
			}
			stellaSelectPos();
		}else if(selectDate){	//日付操作：GamePAD "←→↑↓"
			pressTime = 0f;
			checkTime = 0f;
			if(Gamepad.current.dpad.right.wasPressedThisFrame){
				GetComponent<AudioSource>().PlayOneShot (audioSelect);
				dateNo += 1;
				UnselectInputField();
				SelectInputField();
			}else if(Gamepad.current.dpad.left.wasPressedThisFrame){
				GetComponent<AudioSource>().PlayOneShot (audioSelect);
				dateNo -= 1;
				UnselectInputField();
				SelectInputField();
			}else if(Gamepad.current.dpad.up.wasPressedThisFrame){
				InputFieldUp(dateNo);
			}else if(Gamepad.current.dpad.down.wasPressedThisFrame){
				InputFieldDown(dateNo);
			}
		}else if(selectInfo){	//InfoView操作：GamePAD "←→↑↓"
			pressTime = 0f;
			checkTime = 0f;
			if(Gamepad.current.dpad.up.wasPressedThisFrame){
				infoNo -=1;
			}else if(Gamepad.current.dpad.down.wasPressedThisFrame){
				infoNo +=1;
			}
			if(Gamepad.current.dpad.left.wasPressedThisFrame){
				infoAction -=1;
			}else if(Gamepad.current.dpad.right.wasPressedThisFrame){
				infoAction +=1;
			}
			if(infoAction < 1){
				infoAction = 1;
			}
			UnselectInfotarget();
			selectInfotarget();
		}
	}
	
	public void OnLook(InputAction.CallbackContext context) {
		rotateH=context.ReadValue<Vector2>().x;
		rotateV=context.ReadValue<Vector2>().y;
	}

	public void OnFly(InputAction.CallbackContext context) { //飛行モード操作：GamePAD "□"
		if (!context.started) return;	//最初の押し下げのみ認識(2連認識対策)
		if(!selectStella && !selectDate && !selectInfo && !textInput){	//通常操作
			aAV_Fly_script.ChangeFly();
		}
	}	

	public void OnView(InputAction.CallbackContext context) { //1st/3rd View操作
		if (!context.started) return;	//最初の押し下げのみ認識(2連認識対策)
		if(!selectStella && !selectDate && !selectInfo && !textInput){	//通常操作
			aAV_Move_script.ChangeAim();
		}
	}	
	
	public void OnJump(InputAction.CallbackContext context) {
		if(!selectStella && !selectDate && !selectInfo && !textInput){	//通常操作
			if(context.phase == InputActionPhase.Performed){	//アクションが実行されたばかりかどうかを判断
				rise = true;
				aAV_Move_script.DoJump();
			}else{
				rise = false;
			}
		}
	}

	public void OnSprint(InputAction.CallbackContext context) {
		if(!selectStella && !selectDate && !selectInfo && !textInput){	//通常操作
			if(context.ReadValue<float>() > 0){
				sprint = true;
			}else{
				sprint = false;
			}
		}
	}
	
	public void OnZoom(InputAction.CallbackContext context) {
		if (context.started){	//Controll Modifieredの不具合対策
			aAV_Zoom_script.stepOrFactor = context.ReadValue<float>();
			aAV_Zoom_script.Zoom();
		}else if (context.performed){	//通常のZoom処理
			aAV_Zoom_script.stepOrFactor = context.ReadValue<float>();
			zoom = true;
		}else{
			zoom = false;
		}
	}

	public void OnInfoview(InputAction.CallbackContext context) {//InfoView操作：GamePAD "△"
		if (!context.started) return;	//最初の押し下げのみ認識(2連認識対策)
		if(aAV_Public.displayMode == 1){
			if(infoGameobject.activeSelf){
				rightHand.enabled = false;
			}else{
				rightHand.enabled = true;
			}
			menuObj.GetComponent<aAV_Direction>().ShowInfo();
		}else{
			if(!selectInfo){
				selectStella = false;
				selectInfo = true;
				selectDate = false;
				moveH=0;
				moveV=0;
				GetComponent<AudioSource>().PlayOneShot (audioOpen);
				selectInfotarget();
			}
		}
	}
	
	public void OnUI(InputAction.CallbackContext context) {
		if (!context.started) return;	//最初の押し下げのみ認識(2連認識対策)
		if (menuObj.activeSelf){
			menuObj.SetActive(false);
		}else{
			menuObj.SetActive(true);
		}
	}
	
	public void OnDatetime(InputAction.CallbackContext context) {
		if(dateTimeSetting.activeSelf){
			rightHand.enabled = false;
			dateTimeSetting.SetActive(false);
		}else{
			rightHand.enabled = true;
			dateTimeSetting.SetActive(true);
			infoGameobject.SetActive(false);
			topbarGameobject.SetActive(false);
			stellaGameobject.SetActive(false);
			settingGameobject.SetActive(false);
		}
	}
	
	public void OnStellaicon(InputAction.CallbackContext context) {
		if(stellaGameobject.activeSelf){
			rightHand.enabled = false;
			stellaGameobject.SetActive(false);
		}else{
			rightHand.enabled = true;
			stellaGameobject.SetActive(true);
			infoGameobject.SetActive(false);
			topbarGameobject.SetActive(false);
			dateTimeSetting.SetActive(false);
			settingGameobject.SetActive(false);
		}
	}
	
	public void OnSetting(InputAction.CallbackContext context) {
		if(settingGameobject.activeSelf){
			rightHand.enabled = false;
			settingGameobject.SetActive(false);
		}else{
			rightHand.enabled = true;
			settingGameobject.SetActive(true);
			infoGameobject.SetActive(false);
			topbarGameobject.SetActive(false);
			dateTimeSetting.SetActive(false);
			stellaGameobject.SetActive(false);
		}
	}

	private void UnselectInfotarget(){
		GameObject[] infoArray = GameObject.FindGameObjectsWithTag("rpClone");
		foreach(GameObject obj in infoArray){
			obj.GetComponent<Image>().color = OffColor ;
			obj.transform.Find("view/Background").gameObject.GetComponent<Image>().color = Color.white;
			obj.transform.Find("GoButton").gameObject.GetComponent<Image>().color = Color.white;
			obj.transform.Find("CamButton").gameObject.GetComponent<Image>().color = Color.white;
		}
		infoArray = GameObject.FindGameObjectsWithTag("lnClone");
		foreach(GameObject obj in infoArray){
			obj.GetComponent<Image>().color = OffColor ;
			obj.transform.Find("view/Background").gameObject.GetComponent<Image>().color = Color.white;
			obj.transform.Find("MapButton").gameObject.GetComponent<Image>().color = Color.white;
		}
		infoArray = GameObject.FindGameObjectsWithTag("obClone");
		foreach(GameObject obj in infoArray){
			obj.GetComponent<Image>().color = OffColor ;
			obj.transform.Find("view/Background").gameObject.GetComponent<Image>().color = Color.white;
		}
	}

	public void selectInfotarget(){
		if(infoNo < 1){
			infoNo = 1;
		}else if(aAV_Public.rplist.Count+aAV_Public.linelist.Count+aAV_Public.datalist.Count < infoNo) {
			infoNo = aAV_Public.rplist.Count+aAV_Public.linelist.Count+aAV_Public.datalist.Count;
		}
		if(selectInfo && aAV_Public.rplist.Count+aAV_Public.linelist.Count+aAV_Public.datalist.Count > 0){
			var bottomPos = 0;
			if(infoNo <= aAV_Public.rplist.Count){
				targetGameobject = infoGameobject.transform.Find("ScrollView/Viewport/Log/rpInfo").GetChild(infoNo).gameObject;
				if(infoAction == 1){
					targetGameobject.transform.Find("view/Background").gameObject.GetComponent<Image>().color = inputColor;
				}else if(infoAction ==2){
					targetGameobject.transform.Find("GoButton").gameObject.GetComponent<Image>().color = inputColor;
				}else{
					targetGameobject.transform.Find("CamButton").gameObject.GetComponent<Image>().color = inputColor;
					infoAction = 3;
				}
				bottomPos += 22 + infoNo*12;
			}else if(infoNo <= aAV_Public.rplist.Count+aAV_Public.linelist.Count){
				targetGameobject = infoGameobject.transform.Find("ScrollView/Viewport/Log/lnInfo").GetChild(infoNo - aAV_Public.rplist.Count).gameObject;
				if(infoAction == 1){
					targetGameobject.transform.Find("view/Background").gameObject.GetComponent<Image>().color = inputColor;
				}else{
					targetGameobject.transform.Find("MapButton").gameObject.GetComponent<Image>().color = inputColor;
					infoAction = 2;
				}
				bottomPos += 22*2 + infoNo*12;
			}else{
				targetGameobject = infoGameobject.transform.Find("ScrollView/Viewport/Log/obInfo").GetChild(infoNo - aAV_Public.rplist.Count - aAV_Public.linelist.Count).gameObject;
				targetGameobject.transform.Find("view/Background").gameObject.GetComponent<Image>().color = inputColor;
				infoAction = 1;
				bottomPos += 22*3 + infoNo*12;
			}
			targetGameobject.GetComponent<Image>().color = OnColor ;
			var viewH=infoGameobject.transform.Find("ScrollView").GetComponent<RectTransform>().sizeDelta.y;
			var logY=infoGameobject.transform.Find("ScrollView/Viewport/Log").GetComponent<RectTransform>().anchoredPosition.y;
			
			//選択項目がInfoView内に収まるようにスクロール
			if(logY+viewH < bottomPos){
				infoGameobject.transform.Find("ScrollView/Viewport/Log").GetComponent<RectTransform>().anchoredPosition = new Vector3(0, bottomPos-viewH+10, 0);
			}else if(logY > bottomPos -24){
				infoGameobject.transform.Find("ScrollView/Viewport/Log").GetComponent<RectTransform>().anchoredPosition = new Vector3(0, bottomPos-34, 0);
			}
		}else{
			selectInfo = false;
		}
	}
	

	private void UnselectInputField(){
		tzInputField.GetComponent<Image>().color = Color.white;
		yearInputField.GetComponent<Image>().color = Color.white;
		monthInputField.GetComponent<Image>().color = Color.white;
		dayInputField.GetComponent<Image>().color = Color.white;
		hourInputField.GetComponent<Image>().color = Color.white;
		minuteInputField.GetComponent<Image>().color = Color.white;
		secondInputField.GetComponent<Image>().color = Color.white;
	}

	private void SelectInputField(){
		if(dateNo < 1){
			dateNo = 1;
		}else  if(dateNo > 6){
			dateNo = 6;
		}

		switch(dateNo){
			case 0:
				tzInputField.GetComponent<Image>().color = inputColor;
				break;
			case 1:
				yearInputField.GetComponent<Image>().color = inputColor;
				break;
			case 2:
				monthInputField.GetComponent<Image>().color = inputColor;
				break;
			case 3:
				dayInputField.GetComponent<Image>().color = inputColor;
				break;
			case 4:
				hourInputField.GetComponent<Image>().color = inputColor;
				break;
			case 5:
				minuteInputField.GetComponent<Image>().color = inputColor;
				break;
			case 6:
				secondInputField.GetComponent<Image>().color = inputColor;
				break;
			default:
				break;
		}
	}

	private void InputFieldUp(int number){
		switch(number){
			case 0:
				break;
			case 1:
				aAV_UI_script.yearUP();
				break;
			case 2:
				aAV_UI_script.monthUP();
				break;
			case 3:
				aAV_UI_script.dayUP();
				break;
			case 4:
				aAV_UI_script.hourUP();
				break;
			case 5:
				aAV_UI_script.minuteUP();
				break;
			case 6:
				aAV_UI_script.secondUP();
				break;
			default:
				break;
		}
	}

	private void InputFieldDown(int number){
		switch(number){
			case 0:
				break;
			case 1:
				aAV_UI_script.yearDOWN();
				break;
			case 2:
				aAV_UI_script.monthDOWN();
				break;
			case 3:
				aAV_UI_script.dayDOWN();
				break;
			case 4:
				aAV_UI_script.hourDOWN();
				break;
			case 5:
				aAV_UI_script.minuteDOWN();
				break;
			case 6:
				aAV_UI_script.secondDOWN();
				break;
			default:
				break;
			}
	}
	
	private void OnApplicationQuit()
	{
		//XRモードの時は、HMDとのコネクションを終了してからQUIT
		if(XRGeneralSettings.Instance && XRGeneralSettings.Instance.Manager.activeLoader != null){
			XRGeneralSettings.Instance.Manager.StopSubsystems();
			XRGeneralSettings.Instance.Manager.DeinitializeLoader();
		}
	}

	public void stellaSelectPos(){
		if(stellaNo < 0){
			stellaNo = 0;
		}else  if(stellaNo > 8){
			stellaNo = 8;
		}
		
		var pos = selectGameobject.transform.localPosition;
		if(aAV_Public.displayMode == 2){
			if(stellaNo < 2){
				pos.x = stellaNo*50;
				pos.y = 140;
			}else if(stellaNo < 5){
				pos.x = (stellaNo-2)*50;
				pos.y = 70;
			}else{
				pos.x = (stellaNo-5)*50;
				pos.y = 0;
			}
		}else{
			pos.x = stellaNo*50;
			pos.y = 0;
		}
		selectGameobject.transform.localPosition = pos;
	}
}

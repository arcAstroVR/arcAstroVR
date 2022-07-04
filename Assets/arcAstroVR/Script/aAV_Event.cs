using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.XR.Management;

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

	public static float moveH = 0;
	public static float moveV = 0;
	public static float rotateH = 0;
	public static float rotateV = 0;
	public static bool sprint = false;
	public static bool rise = false;
	public static bool selectStella = false;
	public static bool selectMenu = false;
	public static bool selectDate = false;
	public static bool textInput = false;			//EditWindow操作

	public AudioClip audioSelect;
	public AudioClip audioButton;
	public AudioClip audioOpen;
	public AudioClip audioClose;

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

	private aAV_icon aAV_icon_script;
	private aAV_UI aAV_UI_script;
	private aAV_MoveBehaviour aAV_Move_script;
	private aAV_FlyBehaviour aAV_Fly_script;
	private aAV_StelMouseZoom aAV_Zoom_script;
	private aAV_ThirdPersonOrbitCamBasic aAV_Cam_script;
	private int stellaNo = 0;
	private int dateNo = 1;
	private int menuNo = 0;
	
	private float checkTime = 0f;
	private float pressTime = 0f;
	private float angleH = 0;                                          // Float to store camera horizontal angle related to mouse movement.
	private float angleV = 0;                                          // Float to store camera vertical angle related to mouse movement.

	private Color inputColor = new Color(1.0f, 1.0f, 0.7f);
	private Color dateOffColor = new Color(0.25f, 0.25f, 0.4f, 0.8f);
	private Color dateOnColor = new Color(0.25f, 0.6f, 0.25f, 1.0f);
	private Color buttonOffColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
	private Color buttonOnColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);

	void Awake(){
		Transform mainTrans = GameObject.Find("Main").transform;
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
		
		aAV_UI_script = dateTimeSetting.GetComponent<aAV_UI>();
		aAV_icon_script = mainTrans.Find("Menu/ToggleSwitch").gameObject.GetComponent<aAV_icon>();
		aAV_Move_script = mainTrans.Find("Avatar").gameObject.GetComponent<aAV_MoveBehaviour>();
		aAV_Fly_script = mainTrans.Find("Avatar").gameObject.GetComponent<aAV_FlyBehaviour>();
		aAV_Zoom_script = mainTrans.Find("Stellarium").gameObject.GetComponent<aAV_StelMouseZoom>();

		aAV_Cam_script = GameObject.Find("XR Origin").transform.Find("Camera Offset/MainCamera").gameObject.GetComponent<aAV_ThirdPersonOrbitCamBasic>();
	}
	
	void Update(){		//押している間連続処理系（時刻変更等）
		checkTime += Time.deltaTime;
		if(!textInput){
			if(Gamepad.current != null){
				pressTime += Time.deltaTime;
				if(selectDate && checkTime > 0){
					checkTime -= 0.02f;
					if(Gamepad.current.buttonNorth.isPressed && pressTime > 0.5f){
						InputFieldUp(dateNo);
					}else if(Gamepad.current.buttonWest.isPressed && pressTime > 0.5f){
						InputFieldDown(dateNo);
					}
					if(Gamepad.current.buttonEast.isPressed){
						updateButton.GetComponent<Image>().color = buttonOnColor;
					}else{
						updateButton.GetComponent<Image>().color = buttonOffColor;
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
	}
	
	

	public void OnDo(InputAction.CallbackContext context) {		//決定操作
		if (!context.performed) return;
		if (!selectStella && !selectMenu && !selectDate && !textInput){	//Stella Menu On
			GetComponent<AudioSource>().PlayOneShot (audioOpen);
			selectGameobject.SetActive(true);
			selectStella = true;
			selectMenu = false;
			selectDate = false;
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
			Debug.Log("Menu Toggle");
		}else if(selectDate){		//Date実行
			GetComponent<AudioSource>().PlayOneShot (audioButton);
			aAV_UI_script.GenerateSkybox();
		}else if(selectMenu){		//メニュバー確定
			GetComponent<AudioSource>().PlayOneShot (audioButton);
			Debug.Log("Menu Toggle");
		}else{	//その他決定動作
			Debug.Log("OK button");
		}
	}

	public void OnBack(InputAction.CallbackContext context) {
		if (!context.performed) return;
		if (!selectDate && !selectStella && !selectMenu && !textInput){	//日付メニュー On
			GetComponent<AudioSource>().PlayOneShot (audioOpen);
			dateTimeSetting.GetComponent<Image>().color = dateOnColor ;
			selectStella = false;
			selectMenu = false;
			selectDate = true;
			SelectInputField(dateNo);
			Debug.Log("Date On");
		}else if(selectStella || selectMenu || selectDate){		//全Off
			GetComponent<AudioSource>().PlayOneShot (audioClose);
			dateTimeSetting.GetComponent<Image>().color = dateOffColor ;
			selectGameobject.SetActive(false);
			UnselectInputField();
			selectStella = false;
			selectMenu = false;
			selectDate = false;
		}else{
			Debug.Log("Back button");
		}
	}
	
	public void OnMove(InputAction.CallbackContext context) {
		if (context.started) return;
		if(!selectStella && !selectDate && !selectMenu  && !textInput){	//通常操作
			moveH=context.ReadValue<Vector2>().x;
			moveV=context.ReadValue<Vector2>().y;
		}else if(selectStella){	//Stellaメニュー操作
			if(Gamepad.current.dpad.right.wasPressedThisFrame){
				GetComponent<AudioSource>().PlayOneShot (audioSelect);
				stellaNo += 1;
				if(stellaNo > 8){
					stellaNo = 8;
				}
			}else if(Gamepad.current.dpad.left.wasPressedThisFrame){
				GetComponent<AudioSource>().PlayOneShot (audioSelect);
				stellaNo -= 1;
				if(stellaNo < 0){
					stellaNo = 0;
				}
			}
			var pos = selectGameobject.transform.localPosition;
			pos.x = stellaNo*110;
			selectGameobject.transform.localPosition = pos;
		}else if(selectDate){	//日付操作
			UnselectInputField();
			if(Gamepad.current.dpad.right.wasPressedThisFrame){
				GetComponent<AudioSource>().PlayOneShot (audioSelect);
				dateNo += 1;
				if(dateNo > 6){
					dateNo = 6;
				}
			}else if(Gamepad.current.dpad.left.wasPressedThisFrame){
				GetComponent<AudioSource>().PlayOneShot (audioSelect);
				dateNo -= 1;
				if(dateNo < 1){
					dateNo = 1;
				}
			}else if(Gamepad.current.dpad.up.wasPressedThisFrame){
				GetComponent<AudioSource>().PlayOneShot (audioSelect);
				if(dateNo>3){
					dateNo -= 3;
				}
			}else if(Gamepad.current.dpad.down.wasPressedThisFrame){
				GetComponent<AudioSource>().PlayOneShot (audioSelect);
				if(dateNo==0){
					dateNo = 1;
				}else if(dateNo <4){
					dateNo += 3;
				}
			}
			SelectInputField(dateNo);
		}else if(selectMenu){	//menuバー操作
			Debug.Log("menuバー");
		}
	}
	
	public void OnLook(InputAction.CallbackContext context) {
		if (context.started) return;
		rotateH=context.ReadValue<Vector2>().x;
		rotateV=context.ReadValue<Vector2>().y;
	}

	public void OnFly(InputAction.CallbackContext context) {
		if (!context.performed) return;
		if(!selectStella && !selectDate && !selectMenu && !textInput){	//通常操作
			aAV_Fly_script.ChangeFly();
		}else if (selectDate){	//CountDown操作
			pressTime = 0f;
			checkTime = 0f;
			InputFieldDown(dateNo);
		}
	}	

	public void OnView(InputAction.CallbackContext context) {
		if (!context.performed) return;
		if(!selectStella && !selectDate && !selectMenu && !textInput){	//通常操作
			aAV_Move_script.ChangeAim();
		}else if (selectDate){	//CountUp操作
			pressTime = 0f;
			checkTime = 0f;
			InputFieldUp(dateNo);
		}
	}	
	
	public void OnJump(InputAction.CallbackContext context) {
		if (context.started) return;
		if(!selectStella && !selectDate && !selectMenu && !textInput){	//通常操作
			if(context.phase == InputActionPhase.Performed){	//アクションが実行されたばかりかどうかを判断
				rise = true;
				aAV_Move_script.DoJump();
			}else{
				rise = false;
			}
		}
	}

	public void OnSprint(InputAction.CallbackContext context) {
		if (context.started) return;
		if(!selectStella && !selectDate && !selectMenu && !textInput){	//通常操作
			if(context.ReadValue<float>() > 0){
				sprint = true;
			}else{
				sprint = false;
			}
		}
	}
	
	public void OnZoom(InputAction.CallbackContext context) {
		if (!context.performed) return;
		if (context.ReadValue<float>()>0){
			aAV_Zoom_script.ZoomIn();
		}
		if (context.ReadValue<float>()<0){
			aAV_Zoom_script.ZoomOut();
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

	private void SelectInputField(int number){
		switch(number){
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
}

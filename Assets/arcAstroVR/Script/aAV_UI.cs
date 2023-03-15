using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;

public class aAV_UI : MonoBehaviour {

	[SerializeField]
	InputField tzInput;
	[SerializeField]
	InputField hourInput, minuteInput, secondInput;
	[SerializeField]
	Slider timeSlider;
	[SerializeField]
	InputField yearInput, monthInput, dayInput;
	
	public float tempRotation = 0;

	private bool usingTimeSlider = false;
	private double earthRotAngle = 7.29211515d/100000d*180/Math.PI*60*60*24;

	private double nowJD;
	private double newJD;
	
	private aAV_StelController controller;
	private aAV_StreamingSkybox streamingSkybox;
	private aAV_CustomDateTime localTime;
	private GameObject stellariumObj;
	private Material skyboxMaterial;

	void Awake() {
		Debug.Log("earthRotAngle"+ earthRotAngle);
		stellariumObj = GameObject.Find( "Stellarium" );
		controller = stellariumObj.GetComponent<aAV_StelController>();
		streamingSkybox = stellariumObj.GetComponent<aAV_StreamingSkybox>();
		localTime = stellariumObj.GetComponent<aAV_CustomDateTime>();

		tzInput = GameObject.Find("TZInputField").GetComponent<InputField>();
		yearInput = GameObject.Find("YearInputField").GetComponent<InputField>();
		monthInput = GameObject.Find("MonthInputField").GetComponent<InputField>();
		dayInput = GameObject.Find("DayInputField").GetComponent<InputField>();
		hourInput = GameObject.Find("HourInputField").GetComponent<InputField>();
		minuteInput = GameObject.Find("MinuteInputField").GetComponent<InputField>();
		secondInput = GameObject.Find("SecondInputField").GetComponent<InputField>();
	}
	
	void Start() {
		yearInput.text = "0001";
		monthInput.text = "01";
		dayInput.text = "01";
		hourInput.text = "00";
		minuteInput.text = "00";
		secondInput.text = "00";
		tzInput.text = "+00:00";
		if(aAV_Public.basicInfo.year != null) yearInput.text = aAV_Public.basicInfo.year.ToString("D4");
		if(aAV_Public.basicInfo.month != null) monthInput.text = aAV_Public.basicInfo.month.ToString("D2");
		if(aAV_Public.basicInfo.day != null) dayInput.text = aAV_Public.basicInfo.day.ToString("D2");
		if(aAV_Public.basicInfo.hour != null) hourInput.text = aAV_Public.basicInfo.hour.ToString("D2");
		if(aAV_Public.basicInfo.minute != null) minuteInput.text =aAV_Public.basicInfo.minute.ToString("D2");
		if(aAV_Public.basicInfo.second != null) secondInput.text = aAV_Public.basicInfo.second.ToString("D2");
		if(aAV_Public.basicInfo.timezone != null) tzInput.text = aAV_Public.basicInfo.timezone;
		string[] timezone = tzInput.text .Split(':');
		double tzJD = double.Parse(timezone[0])+Math.Sign(double.Parse(timezone[0]))*double.Parse(timezone[1])/60;
		int setyear = (int)Math.Abs(StringToInt(yearInput.text));
		if (StringToInt(yearInput.text)<=0){	//AD表記をBC表記に変換
			setyear = (int)Math.Abs(StringToInt(yearInput.text) -1);
		}
		SetTimebar();
		localTime.SetDateTime(
			setyear,
			StringToInt(monthInput.text),
			StringToInt(dayInput.text),
			StringToInt(hourInput.text),
			StringToInt(minuteInput.text),
			StringToInt(secondInput.text),
			(aAV_CustomDateTime.Era)(StringToInt(yearInput.text)<=0?0:1)
		);
		nowJD = localTime.ToJulianDay() - tzJD/24 + 0.5/100000;
		UpdateDateTime();
	}
	
	public void GenerateSkybox() {	//Update,1h,10minボタンでcall
		StartCoroutine(controller.SetJD(newJD));
		nowJD = newJD;

		//時系列による3Dオブジェクトの表示コントロール
		for(int i = 0; i < aAV_Public.datalist.Count; i++){
			bool timeVisible = true;
			if(aAV_Public.datalist[i].start != ""){
				if(int.Parse(yearInput.text) < int.Parse(aAV_Public.datalist[i].start)){
					timeVisible = false;
				}
			}
			if(aAV_Public.datalist[i].end != ""){
				if(int.Parse(yearInput.text) > int.Parse(aAV_Public.datalist[i].end)){
					timeVisible = false;
				}
			}
			aAV_Public.datalist[i].gameobject.SetActive(aAV_Public.datalist[i].visible && timeVisible);
		}
	}

	public void OnTimeSliderDown() {
		usingTimeSlider = true;
	}

	public void OnTimeSliderUp() {
		usingTimeSlider = false;
	}

	public void SetTimeInput() {
		int hour = (int)timeSlider.value;
		int minute = (int)((timeSlider.value - hour) * 60);
		int second = (int)((timeSlider.value -hour- (timeSlider.value - hour)) *60*60);
		hourInput.text = hour.ToString("D2");
		minuteInput.text = minute.ToString("D2");
		secondInput.text = second.ToString("D2");
	}
	
	public void SetTimebar() {
		timeSlider.value = float.Parse(hourInput.text)+float.Parse(minuteInput.text)/60+float.Parse(secondInput.text)/60/60;
	}

	public void UpdateDateTime() {
		//各InputField入力時にCall
		//各日付のInputField値からdeltaJDを計算し、パブリック値に保存
		if(!tzInput.text.Contains(":")){
			try {
				double tz = double.Parse(tzInput.text);
				string sign = "+";
				if (tz<0){
					sign = "-";
				}
				tzInput.text = sign+((int)tz).ToString("D2")+":"+((int)Math.Abs(tz % 1 *60)).ToString("D2");
			} catch {
				tzInput.text = aAV_Public.basicInfo.timezone;
			}
		}
		string[] timezone = tzInput.text .Split(':');
		double tzJD = double.Parse(timezone[0])+Math.Sign(double.Parse(timezone[0]))*double.Parse(timezone[1])/60;
		
		int setyear = (int)Math.Abs(StringToInt(yearInput.text));
		if (StringToInt(yearInput.text)<=0){	//AD表記をBC表記に変換
			setyear = (int)Math.Abs(StringToInt(yearInput.text) -1);
		}
		
		SetTimebar();
		localTime.SetDateTime(
			setyear,
			StringToInt(monthInput.text),
			StringToInt(dayInput.text),
			StringToInt(hourInput.text),
			StringToInt(minuteInput.text),
			StringToInt(secondInput.text),
			(aAV_CustomDateTime.Era)(StringToInt(yearInput.text)<0?0:1)
		);
		newJD = localTime.ToJulianDay() - tzJD/24 + 0.5/100000;
		JDsetup(newJD);
		aAV_Public.basicInfo.timezone = tzInput.text;
		aAV_Public.basicInfo.year = setyear;
		aAV_Public.basicInfo.month = int.Parse(monthInput.text);
		aAV_Public.basicInfo.day = int.Parse(dayInput.text);
		aAV_Public.basicInfo.hour = int.Parse(hourInput.text);
		aAV_Public.basicInfo.minute = int.Parse(minuteInput.text);
		aAV_Public.basicInfo.second = int.Parse(secondInput.text);
	}
	
	int StringToInt(string str) {
		int result = 0;
		if(!int.TryParse(str, out result)) {
			Debug.LogError("Could not parse int");
			return result;
		}
		return result;
	}

	public void JDsetup(double JDtime) {
		//DataUIの日付表示をJDtimeを基に設定
		string[] timezone = tzInput.text.Split(':');
		double tzJD = double.Parse(timezone[0])+Math.Sign(double.Parse(timezone[0]))*double.Parse(timezone[1])/60;
		localTime=localTime.FromJulianDay(JDtime+tzJD/24-0.5/100000);
		yearInput.text = localTime.year.ToString();
		if(localTime.era == (aAV_CustomDateTime.Era)0){	//BC表記をAD表記に変換
			var sign = "-";
			if (localTime.year == 1){
				sign="";
			}
			yearInput.text = sign+(localTime.year - 1).ToString();
		}
		monthInput.text = localTime.month.ToString();
		dayInput.text = localTime.day.ToString();
		hourInput.text = localTime.hour.ToString("D2");
		minuteInput.text = localTime.minute.ToString("D2");
		secondInput.text = ((int)Math.Floor((float)localTime.second)).ToString("D2");
		SetTimebar();
		
		//Skyboxの擬似リアルタイム回転
		skyboxMaterial = RenderSettings.skybox;
		float angle = controller.latitude*Mathf.Deg2Rad;
		skyboxMaterial.SetVector("_RotationAxis", new Vector4(0,Mathf.Sin(angle),Mathf.Cos(angle),1));
		tempRotation = Mathf.Repeat((float)((newJD - nowJD)*earthRotAngle), 360);
		skyboxMaterial.SetFloat("_Rotation", tempRotation);
		
	}
	
	public void yearUP() {
		yearInput.text = (StringToInt(yearInput.text)+1).ToString();
		UpdateDateTime();
	}
	public void yearDOWN() {
		yearInput.text = (StringToInt(yearInput.text)-1).ToString();
		UpdateDateTime();
	}
	public void monthUP() {
		monthInput.text = (StringToInt(monthInput.text)+1).ToString();
		UpdateDateTime();
	}
	public void monthDOWN() {
		monthInput.text = (StringToInt(monthInput.text)-1).ToString();
		UpdateDateTime();
	}
	public void dayUP() {
		dayInput.text = (StringToInt(dayInput.text)+1).ToString();
		UpdateDateTime();
	}
	public void dayDOWN() {
		dayInput.text = (StringToInt(dayInput.text)-1).ToString();
		UpdateDateTime();
	}
	public void hourUP() {
		hourInput.text = (StringToInt(hourInput.text)+1).ToString();
		UpdateDateTime();
	}
	public void hourDOWN() {
		hourInput.text = (StringToInt(hourInput.text)-1).ToString();
		UpdateDateTime();
	}
	public void minuteUP() {
		minuteInput.text = (StringToInt(minuteInput.text)+1).ToString();
		UpdateDateTime();
	}
	public void minuteDOWN() {
		minuteInput.text = (StringToInt(minuteInput.text)-1).ToString();
		UpdateDateTime();
	}
	public void secondUP() {
		secondInput.text = (StringToInt(secondInput.text)+1).ToString();
		UpdateDateTime();
	}
	public void secondDOWN() {
		secondInput.text = (StringToInt(secondInput.text)-1).ToString();
		UpdateDateTime();
	}
	
	public void hour1UP() {
		newJD += (double)1/24;
		JDsetup(newJD);
		GenerateSkybox();
	}
	public void hour1DOWN() {
		newJD -= (double)1/24;
		JDsetup(newJD);
		GenerateSkybox();
	}
	public void min10UP() {
		newJD += (double)10/60/24;
		JDsetup(newJD);
		GenerateSkybox();
	}
	public void min10DOWN() {
		newJD -= (double)10/60/24;
		JDsetup(newJD);
		GenerateSkybox();
	}

}
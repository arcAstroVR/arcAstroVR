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

	public static double JD;

	bool usingTimeSlider = false;

	private aAV_StelController controller;
	private aAV_StreamingSkybox streamingSkybox;
	private aAV_CustomDateTime customDateTime;
	private GameObject stellariumObj;


	void Awake() {
		stellariumObj = GameObject.Find( "Stellarium" );
		controller = stellariumObj.GetComponent<aAV_StelController>();
		streamingSkybox = stellariumObj.GetComponent<aAV_StreamingSkybox>();
		customDateTime = stellariumObj.GetComponent<aAV_CustomDateTime>();

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
		SetTimebar();
		UpdateDateTime();
		JDsetup();
	}

	public void GenerateSkybox() {	//Update,1h,10minボタンでcall
		string[] timezone =  tzInput.text.Split(':');
		double tz = double.Parse(timezone[0])+Math.Sign(double.Parse(timezone[0]))*double.Parse(timezone[1])/60;
		StartCoroutine(controller.SetJD(JD-tz/24+0.5/100000));

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

	public void GetStatus() {
		Debug.Log("GetStatus");
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

	public void UpdateDateTime() {	//各InputField入力時
		int setyear = (int)Math.Abs(StringToInt(yearInput.text));
		if (StringToInt(yearInput.text)<=0){	//AD表記をBC表記に変換
			setyear = (int)Math.Abs(StringToInt(yearInput.text) -1);
		}
		customDateTime.SetDateTime(
			setyear,
			StringToInt(monthInput.text),
			StringToInt(dayInput.text),
			StringToInt(hourInput.text),
			StringToInt(minuteInput.text),
			StringToInt(secondInput.text),
			(aAV_CustomDateTime.Era)(StringToInt(yearInput.text)<=0?0:1)
		);
		
		JD = customDateTime.ToJulianDay();
		JDsetup();
		if(!tzInput.text.Contains(":")){
			try {
				double tz = double.Parse(tzInput.text);
				string sign = "";
				if (tz>0){
					sign = "+";
				}
				tzInput.text = sign+((int)tz).ToString("D2")+":"+((int)Math.Abs(tz % 1 *60)).ToString("D2");
			} catch {
				tzInput.text = aAV_Public.basicInfo.timezone;
			}
		}
		
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

	void JDsetup() {
		customDateTime=customDateTime.FromJulianDay(JD);
		yearInput.text = customDateTime.year.ToString();
		if(customDateTime.era == (aAV_CustomDateTime.Era)0){	//BC表記をAD表記に変換
			var sign = "-";
			if (customDateTime.year == 1){
				sign="";
			}
			yearInput.text = sign+(customDateTime.year - 1).ToString();
		}
		monthInput.text = customDateTime.month.ToString();
		dayInput.text = customDateTime.day.ToString();
		hourInput.text = customDateTime.hour.ToString("D2");
		minuteInput.text = customDateTime.minute.ToString("D2");
		secondInput.text = ((int)Math.Floor((float)customDateTime.second)).ToString("D2");
		SetTimebar();
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
		JD += (double)1/24;
		JDsetup();
		GenerateSkybox();
	}
	public void hour1DOWN() {
		JD -= (double)1/24;
		JDsetup();
		GenerateSkybox();
	}
	public void min10UP() {
		JD += (double)10/60/24;
		JDsetup();
		GenerateSkybox();
	}
	public void min10DOWN() {
		JD -= (double)10/60/24;
		JDsetup();
		GenerateSkybox();
	}

}
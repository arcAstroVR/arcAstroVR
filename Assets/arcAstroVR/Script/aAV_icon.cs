using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Defective.JSON;

public static class ToggleExt
{
	public static void SetIsOnWithoutCallback( this Toggle self, bool isOn )
	{
		var onValueChanged = self.onValueChanged;
		self.onValueChanged = new Toggle.ToggleEvent();
		self.isOn = isOn;
		self.onValueChanged = onValueChanged;
	}
}

public class aAV_icon : MonoBehaviour
{

	public Toggle constellationLine;
	public Toggle constellationLabel;
	public Toggle constellationArt;
	public Toggle equatorialGrid;
	public Toggle azimuthalGrid;
	public Toggle cardinalPoint;
	public Toggle atmosphere;
	public Toggle archaeoLines;
	public Toggle planetLabels;
	
	private aAV_StelController controller;
	private aAV_icon toggle;

	void Awake()
	{
		controller = GameObject.Find("Main").transform.Find("Stellarium").gameObject.GetComponent<aAV_StelController>();
	}

	void Start()
	{
		initializeToggle();
	}

	public void initializeToggle(){
		if(controller != null){
			JSONObject jsonActions = controller.jsonActions.GetField("changes");
			constellationLine.SetIsOnWithoutCallback(jsonActions["actionShow_Constellation_Lines"].boolValue);
			constellationLabel.SetIsOnWithoutCallback(jsonActions["actionShow_Constellation_Labels"].boolValue);
			constellationArt.SetIsOnWithoutCallback(jsonActions["actionShow_Constellation_Art"].boolValue);
			equatorialGrid.SetIsOnWithoutCallback(jsonActions["actionShow_Equatorial_Grid"].boolValue);
			azimuthalGrid.SetIsOnWithoutCallback(jsonActions["actionShow_Azimuthal_Grid"].boolValue);
			cardinalPoint.SetIsOnWithoutCallback(jsonActions["actionShow_Cardinal_Points"].boolValue);
			atmosphere.SetIsOnWithoutCallback(jsonActions["actionShow_Atmosphere"].boolValue);
			archaeoLines.SetIsOnWithoutCallback(jsonActions["actionShow_ArchaeoLines"].boolValue);
			planetLabels.SetIsOnWithoutCallback(jsonActions["actionShow_Planets_Labels"].boolValue);
		}
	}
	
	public void Constellation_Line(){
		StartCoroutine(controller.DoAction("actionShow_Constellation_Lines"));
	}

	public void Constellation_Label(){
		StartCoroutine(controller.DoAction("actionShow_Constellation_Labels"));
	}
	
	public void Constellation_Art(){
		StartCoroutine(controller.DoAction("actionShow_Constellation_Art"));
	}

	public void Equatorial_Grid(){
		StartCoroutine(controller.DoAction("actionShow_Equatorial_Grid"));
	}

	public void Azimuthal_Grid(){
		StartCoroutine(controller.DoAction("actionShow_Azimuthal_Grid"));
	}

	public void Cardinal_Point(){
		StartCoroutine(controller.DoAction("actionShow_Cardinal_Points"));
	}

	public void Atmosphere(){
		StartCoroutine(controller.DoAction("actionShow_Atmosphere"));
	}

	public void ArchaeoLines(){
		StartCoroutine(controller.DoAction("actionShow_ArchaeoLines"));
	}

	public void Planet_Labels(){
		StartCoroutine(controller.DoAction("actionShow_Planets_Labels"));
	}

	
}

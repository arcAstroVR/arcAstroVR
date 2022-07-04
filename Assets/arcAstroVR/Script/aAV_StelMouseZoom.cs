// GZMouseZoom, a simple mouse zoom script. 
// (c) 2012-16 Georg Zotti
// aAV_StelMouseZoom: Reorganize StelMouseZoom for arcAstroVR. (c) 2021 by K.Iwashiro.


using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(requiredComponent: typeof(aAV_StelController))]
public class aAV_StelMouseZoom : MonoBehaviour {

    public float minFieldOfView = 2.0f;
    public float maxFieldOfView = 120.0f;
    public float stepOrFactor=5; // either n degrees per mouse rotation impulse or a scaling factor like 0.05 for 5 percent change.

    private float lastFoV=200; // keep track of this to avoid too much traffic.
    private aAV_StelController controller; // This finds the related script providing communication to Stellarium.
	private GameObject mapCamObj;

	void Awake()
	{
		controller = gameObject.GetComponent<aAV_StelController>();

		var gameObjectList = Resources.FindObjectsOfTypeAll<GameObject>();
		foreach (var obj in gameObjectList) {
			if (obj.name == "MapCamera") {
				mapCamObj = obj;
			}
		}
	}

    void Start()
    {
#if false
        if (controller && controller.spoutMode)
        {
            if (lastFoV != Camera.main.fieldOfView)
            {
                lastFoV = Camera.main.fieldOfView;
                StartCoroutine(controller.SetFoV(lastFoV));
                //Debug.Log("StelMouseZoom: NEW FOV SET: " + lastFoV);
            }
            //else
            //    Debug.Log("NEW FOV not required to SET.");
        }
#endif
    }

    void Update () {
        if (controller && controller.connectToStellarium && controller.spoutMode)
        {
            if (lastFoV != Camera.main.fieldOfView)
            {
                lastFoV = Camera.main.fieldOfView;
                StartCoroutine(controller.SetFoV(lastFoV));
                //Debug.Log("StelMouseZoom: NEW FOV SET: "+lastFoV);
            }
            //else
            //    Debug.Log("NEW FOV not required to SET.");
        }
    }
    
	public void ZoomIn(){
		if (!aAV_Public.showCompass){
			Camera.main.fieldOfView = Mathf.Max(Camera.main.fieldOfView - stepOrFactor, minFieldOfView);
		}else{
			float scale = mapCamObj.GetComponent<Camera>().orthographicSize;
			mapCamObj.GetComponent<Camera>().orthographicSize = Mathf.Max(scale / 1.2f, 1f);
		}
	}

	public void ZoomOut(){
		if (!aAV_Public.showCompass){
			Camera.main.fieldOfView = Mathf.Min(Camera.main.fieldOfView + stepOrFactor, maxFieldOfView);
		}else{
			float scale = mapCamObj.GetComponent<Camera>().orthographicSize;
			mapCamObj.GetComponent<Camera>().orthographicSize = Mathf.Min(scale * 1.2f, 150000f);
		}
	}
}
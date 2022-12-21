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
	private GameObject mainCamObj;

	void Awake()
	{
		mainCamObj = GameObject.Find("XR Origin/Camera Offset/Main Camera").gameObject;
		mapCamObj =  GameObject.Find("Main").transform.Find("MapCamera").gameObject;
		controller = gameObject.GetComponent<aAV_StelController>();
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
    
	public void Zoom(){
		if (!aAV_Public.showCompass){
			mainCamObj.GetComponent<Camera>().fieldOfView = Mathf.Clamp(mainCamObj.GetComponent<Camera>().fieldOfView + stepOrFactor, minFieldOfView, maxFieldOfView);
		}else{
			float scale = mapCamObj.GetComponent<Camera>().orthographicSize;
			if(stepOrFactor>0){
				mapCamObj.GetComponent<Camera>().orthographicSize = Mathf.Min(scale * 1.2f, 150000f);
			}else{
				mapCamObj.GetComponent<Camera>().orthographicSize = Mathf.Max(scale / 1.2f, 1f);
			}
		}
	}
}
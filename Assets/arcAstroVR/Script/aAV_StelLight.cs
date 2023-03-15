/*
 * Base Program : StelLight
 * Assign Flares, e.g. from StandardAssets 50mm for Sun and Moon, Star for Venus
   Create a directional Light (we call it Shadowmaker) in the "scene root" with the same Flare and "Draw Halo". Assign this to the SunLightObj in the script.  (c) 2017 by John Fillwalk (IDIA Lab), Georg Zotti (LBI ArchPro), Neil Zehr (IDIA Lab) and David Rodriguez (IDIA Lab).
 * Improvement program : aAV_StelLight
 * Reorganize StelController for arcAstroVR. (c) 2021 by K.Iwashiro.
 */
using UnityEngine;
using System.Collections;
using Defective.JSON;

[RequireComponent(requiredComponent: typeof(aAV_StelController), requiredComponent2: typeof(Light))]
public class aAV_StelLight : MonoBehaviour {


    public Flare sunFlare;
    public Flare moonFlare;
    public Flare venusFlare;

    private Light stelLight;
    private aAV_StelController controller;
    private JSONObject lightObjectInfo;
    private GameObject sunImpostorSphere;
    private GameObject aav_UI;

    private void Awake()
    {
        controller = gameObject.GetComponent<aAV_StelController>();
        if (controller == null)
            Debug.LogWarning("StelLight: Cannot find StelController! controller not initialized");

        stelLight = gameObject.GetComponent<Light>();
        if (stelLight == null)
            stelLight = gameObject.AddComponent<Light>();

        stelLight.type = LightType.Directional;
        
        sunImpostorSphere = transform.Find("LightImpostorSphere").gameObject;
        aav_UI = GameObject.Find("Main").transform.Find("Menu/DateTimeSetting").gameObject;
    }

    void Start () {
        SetAzAlt(135, 45);
        SetColor(1, 1, 1);
        SetLightsourcePropertiesByName("Sun");
    }

    void Update() {
        JSONObject newLightObjectInfo = controller.GetLightObjInfo();
        UpdateLightObject(newLightObjectInfo);
        transform.position = Camera.main.transform.position;
        sunImpostorSphere.SetActive(newLightObjectInfo["name"].stringValue == "Sun");
        if (newLightObjectInfo["name"].stringValue == "Sun")
        {
            float size = Mathf.Tan(Mathf.PI / 180.0f * 0.5f * newLightObjectInfo["diameter"].floatValue) * 2.0f * 250.0f;
            sunImpostorSphere.transform.localScale.Set(size, size, size);
        }
    }

    public void EnableSunImpostor(bool enable)
    {
        sunImpostorSphere.SetActive(enable);
    }

    private void UpdateLightObject(JSONObject newLightObjectInfo)
    {
        lightObjectInfo = newLightObjectInfo;

        if (!lightObjectInfo || lightObjectInfo.keys.Count==0)
        {
            Debug.LogWarning("StelLight: empty lightObject, creating dark ambient.");
            SetLightsourcePropertiesByName("none"); // switch it off.
            RenderSettings.ambientLight = new Color(0.15f, 0.15f, 0.15f);
            RenderSettings.fogColor = new Color(0.1f, 0.1f, 0.1f);
            return;
        }

        //Debug.Log("StelLight122: LightObjectInfo: " + lightObjectInfo.ToString());
        float ambientFactor = (float) lightObjectInfo["ambientInt"].doubleValue;
        RenderSettings.ambientLight = new Color(0.8f * Mathf.Min(ambientFactor, 0.3f), 0.9f * Mathf.Min(ambientFactor, 0.3f), 1.0f * Mathf.Min(ambientFactor, 0.3f));
        RenderSettings.fogColor = new Color(0.7f * ambientFactor, 0.7f * ambientFactor, 0.7f * ambientFactor);

        string lightsourceName = lightObjectInfo["name"].stringValue;
        //Debug.Log("StelLight::UpdateLightObject: Lightsource Name:" + lightsourceName+" Set flare...");
        SetLightsourcePropertiesByName(lightsourceName); // set main characteristics

        //Debug.Log("Now setting light magnitudes: " + lightObjectInfo.ToString());

        if (lightsourceName=="none")
        {
            return;
        }

        float alt = (float) lightObjectInfo["altitude"].doubleValue;
        float az = (float) lightObjectInfo["azimuth"].doubleValue + controller.northAngle;
        SetAzAlt(az, alt);

        // allow dimming and reddening close to horizon.
        float extinctedMag = (float) lightObjectInfo["vmage"].doubleValue - (float) lightObjectInfo["vmag"].doubleValue;
        float magFactorGreen = Mathf.Pow(0.85f, 0.6f * extinctedMag);
        float magFactorBlue = Mathf.Pow(0.6f, 0.5f * extinctedMag);

        if (lightsourceName == "Sun")
        {
            SetColor(1.4f, Mathf.Pow(0.75f, extinctedMag) * 1.4f, Mathf.Pow(0.42f, 0.9f * extinctedMag) * 1.4f);
        }
        else if (lightsourceName == "Moon")
        {
            float moonPower = (float) lightObjectInfo["illumination"].doubleValue * 0.01f;
            moonPower *= 0.25f*moonPower; // This should provide the "full moon peak": 25% brightness at Full Moon, much less in lower phases.
            SetColor(moonPower, magFactorGreen*moonPower, magFactorBlue*moonPower);
        }
        else if (lightsourceName == "Venus")
        {
            float venusPower = (float) lightObjectInfo["vmag"].doubleValue * -0.01f; // this is quite dim, likely still brighter than natural, but OK to make it more apparent.
            SetColor(venusPower, magFactorGreen*venusPower, magFactorBlue*venusPower);
        }

        Vector3 lightDir = new Vector3(Mathf.Cos(alt * Mathf.PI / 180.0f) * Mathf.Sin(az * Mathf.PI / 180.0f),
                                       Mathf.Sin(alt * Mathf.PI / 180.0f),
                                       Mathf.Cos(alt * Mathf.PI / 180.0f) * Mathf.Cos(az * Mathf.PI / 180.0f));
        bool lightHidden = Physics.Raycast(origin: Camera.main.transform.position, direction: lightDir);
        if (lightHidden)
        {
            //Debug.Log("Raycast to "+lightDir.x + "/"+lightDir.y+"/"+lightDir.z+" hit some target. Disabling flare.");
            stelLight.flare = null;
        }
    }

    public void SetColor(float r, float g, float b)
    {
        stelLight.color = new Color(r, g, b);
        sunImpostorSphere.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(r, g, Mathf.Min(b, 0.9f)));
    }

    public void SetAzAlt(float az, float alt)
    {
        //Debug.Log("StelLight: setAzAlt()");
        transform.eulerAngles = new Vector3(alt, az - 90.0f, 0.0f);

		//光源の擬似リアルタイム回転
        float tempRotation = aav_UI.GetComponent<aAV_UI>().tempRotation;
        float angle = controller.latitude*Mathf.Deg2Rad;
        transform.rotation = Quaternion.AngleAxis(tempRotation, new Vector3(0,Mathf.Sin(angle),Mathf.Cos(angle)))*transform.rotation;
    }

    public void SetLightsourcePropertiesByName(string name)
    {
        float foV = Camera.main.fieldOfView;
        if (name=="Sun")
        {
            //Debug.Log("StelLight:setLightsourcePropertiesByName=" + name);
            stelLight.flare = (foV < 60.0f ? null : sunFlare);
            stelLight.shadows = LightShadows.Soft;
            stelLight.enabled = true;
        }
        else if (name == "Moon")
        {
            //Debug.Log("StelLight:setLightsourcePropertiesByName=" + name);
            stelLight.flare = (foV < 60.0f ? null : moonFlare);
            stelLight.shadows = LightShadows.Soft;
            stelLight.enabled = true;
        }
        else if (name == "Venus")
        {
            //Debug.Log("StelLight:setLightsourcePropertiesByName=" + name);
            stelLight.flare = (foV < 60.0f ? null : venusFlare);
            stelLight.shadows = LightShadows.Hard;
            stelLight.enabled = true;
        }
        else
        {
            //Debug.Log("StelLight::setLightSourceName= " + name+", disabled.");
            stelLight.flare = null;
            stelLight.shadows = LightShadows.None;
            stelLight.enabled = false;
        }
    }
}

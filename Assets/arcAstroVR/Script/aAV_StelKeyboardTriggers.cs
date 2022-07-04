/* StelKeyboardTriggers: A few examples of hotkey communication with Stellarium via RemoteControl API.
 * (c) 2017 Georg Zotti
 * Part of the Stellarium Unity Bridge tools by Georg Zotti (LBI ArchPro Vienna) and John Fillwalk (IDIA Lab). 

 * aAV_KeyboardTriggers: Reorganize KeyboardTriggers for arcAstroVR. (c) 2021 by K.Iwashiro.
*/
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class aAV_StelKeyboardTriggers : MonoBehaviour
{
    private aAV_StelController controller;
    private aAV_StreamingSkybox streamingSkybox;
	private aAV_Direction direction;

    void Awake()
    {
        // just initialize the controller...
        controller = gameObject.GetComponent<aAV_StelController>();
        if (controller == null)
            Debug.LogWarning("StelKeyboardTriggers: Cannot find StelController! controller not initialized");
        streamingSkybox = gameObject.GetComponent<aAV_StreamingSkybox>();
        if (streamingSkybox == null)
            Debug.LogWarning("StelKeyboardTriggers: Cannot find StreamingSkybox! streamingSkybox not initialized");
    }

    void LateUpdate()
    {
        if (!controller)
        {
            Debug.LogWarning("StellariumTriggers: No controller found!");
            return;
        }

		if(!aAV_Event.textInput){		//Dialogが開いていない（テキスト入力がない）時、実行
	        // Any Control, but no Shift or Alt
	        if ((Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed) && !Keyboard.current.leftShiftKey.isPressed && !Keyboard.current.rightShiftKey.isPressed && !Keyboard.current.leftAltKey.isPressed && !Keyboard.current.rightAltKey.isPressed)
	        {
	            if (Keyboard.current.numpadPlusKey.wasPressedThisFrame)
	            {
	                float intensity = gameObject.GetComponent<Light>().intensity + 0.1f;
	                gameObject.GetComponent<Light>().intensity = Mathf.Min(8.0f, intensity); // allow considerable overexposure for Venus-related things.
	            }
	            if (Keyboard.current.numpadMinusKey.wasPressedThisFrame)
	            {
	                float intensity = gameObject.GetComponent<Light>().intensity - 0.1f;
	                gameObject.GetComponent<Light>().intensity = Mathf.Max(0.0f, intensity);
	            }
	        }
	
	        // NO shifts at all...
	        if (!Keyboard.current.leftCommandKey.isPressed && !Keyboard.current.rightCommandKey.isPressed && !Keyboard.current.leftCtrlKey.isPressed && !Keyboard.current.rightCtrlKey.isPressed && !Keyboard.current.leftShiftKey.isPressed && !Keyboard.current.rightShiftKey.isPressed && !Keyboard.current.leftAltKey.isPressed && !Keyboard.current.rightAltKey.isPressed)
	        {
	            if (Keyboard.current[Key.F11].wasPressedThisFrame) StartCoroutine(controller.UpdateSkyboxTiles());
	            if (Keyboard.current[Key.T].wasPressedThisFrame) StartCoroutine(controller.DoAction("actionShow_Atmosphere"));
	            if (Keyboard.current[Key.Q].wasPressedThisFrame) StartCoroutine(controller.DoAction("actionShow_Cardinal_Points"));
	            if (Keyboard.current[Key.E].wasPressedThisFrame) StartCoroutine(controller.DoAction("actionShow_Equatorial_Grid"));
	            if (Keyboard.current[Key.Z].wasPressedThisFrame) StartCoroutine(controller.DoAction("actionShow_Azimuthal_Grid"));
	            if (Keyboard.current[Key.P].wasPressedThisFrame) StartCoroutine(controller.DoAction("actionShow_Planets_Labels"));
	            if (Keyboard.current[Key.U].wasPressedThisFrame) StartCoroutine(controller.DoAction("actionShow_ArchaeoLines"));
	            if (Keyboard.current[Key.C].wasPressedThisFrame) StartCoroutine(controller.DoAction("actionShow_Constellation_Lines"));
	            if (Keyboard.current[Key.V].wasPressedThisFrame) StartCoroutine(controller.DoAction("actionShow_Constellation_Labels"));
	            if (Keyboard.current[Key.R].wasPressedThisFrame) StartCoroutine(controller.DoAction("actionShow_Constellation_Art"));
	        }else if(Keyboard.current.leftCommandKey.isPressed || Keyboard.current.rightCommandKey.isPressed || Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed){
	        	direction = GameObject.Find("Menu").GetComponent<aAV_Direction>();
	            if (Keyboard.current[Key.C].wasPressedThisFrame) direction.CopyInfo();
	        }
	    }
    }
}

# arcAstroVR
arcAstroVR is a free open source VR simulator for visualization of archaeological structures and the background celestial body.  
It is available for Windows and macOS. 

## Installation Instructions & Quick Start
Two apps and one dataset are required to operate arcAstroVR.  
 * [arcAstroVR app](https://arcastrovr.org/download.html?id=app)  
 * [Dataset for arcAstroVR](https://arcastrovr.org/download.html?id=dataset)  
 * [Stellarium app](https://www.stellarium.org/)  
  
  
Download and Setup  
 1. Download the latest version of [arcAstroVR](https://arcastrovr.org/download.html?id=app) from arcAstroVR.org. 
 2. Download the [dataset](https://arcastrovr.org/download.html?id=dataset) you wish to play from arcAstroVR.org.
 3. Download the planetarium software [Stellarium](https://www.stellarium.org/) from www.stellarium.org.
 4. Launch Stellarium and configure linkage settings.  
 4-1. Click `Configuration`[F2 Key].  
 4-2. Click the `Plugins` tab.  
 4-3. Click the `Remote Control` plugin in the left-hand list.  
 4-4. Check `Load at startup`.  
 4-5. Click the `ArchaeoLines` plugin in the left-hand list.  
 4-6. Check `Load at startup`.  
 4-7. Close and relaunch Stellarium.  
 4-8. Reopen the `Remote Control` plugin in the configuration.  
 4-9. Click `configure`.  
 4-10. Check `Server enabled` and `Enable automatically at startup`.  
 4-11. Set 8090 for `port number`.  
 4-12. Click the `Save settings`.  
 4-13. Click the `Scripts` tab.  
 4-14. Select `skybox.ssc` script in the left-hand list.  
 4-15. Check `Close window when script runs`, and click the `▶` button.<br>` (You should see the window close and the camera point to the cardinal directions, up, and down sequentially)`  
 4-16. Click the `Tool` tab.  
 4-17. Set Path for `Screenshot Directory`.<br> ` For Win, specify "C:\Users\<USERNAME>\AppData\Roaming\Stellarium".`<br> ` For Mac, specify "/Users/<USERNAME>/Library/Application Support/Stellarium".`  
 5. Launch arcAstroVR and click `select dataset.txt`.  
 6. Select the file dataset.txt in the dataset.  
 
 The observer(Camera) can be moved using the keyboard and mouse.  
 Basically, drag the left mouse button to set the direction of travel, and press the `W` and `S` keys to move forward and backward.  
 * Line of sight/direction of travel: The viewpoint moves with the `left mouse button`.
 * Change viewpoint: `tab` key can be used to switch between the own viewpoint and the third-person viewpoint.
 * Move: `W` key move forward in the direction of viewpoint, `S` key move backward from the direction of viewpoint, `A` key move left, `D` key move right.
 * High speed: You can move at high speed by holding down `Shift` key while moving.
 * Walking / Flight Mode: Press `F` key to switch to flight mode and can fly by move operations. Press `F` key again to return to walking mode.
 * Jump / Vertical Climb: `Space` key, jumps in walking mode, and vertically climbs in flight mode.
 * Zoom in / out: You can zoom in / out (change the viewing angle) with `Ctrl + mouse wheel`.

## Get & build the code  
arcAstroVR is programmed in Unity.  
This section describes the setup from a new Unity project (any template is acceptable, but 3D Core is recommended).  

### Asset Installation  
arcAstroVR is programmed in Unity and requires the following Unity Assets in addition to the arcAstroVR Unity Asset uploaded on GitHub.  

 * Input System (Unity Registry：Free)<br>The new INPUT SYSTEM of Unity is used to support various input devices. It is not installed by default and requires additional installation.
 * Localization (Unity Registry：Free)<br>The LOCALIZATION of Unity is used for multilingual support. It is not installed by default and requires additional installation.
 * XR Interaction Toolkit (Unity Registry：Free)<br>The XR INTERACTION TOOLKIT of Unity is used for HMD support. It is not installed by default and requires additional installation.
 * 3rd Person Controller + Fly Mode (Asset Store：Free)<br>This is a free 3rd-party asset for 3rd-person avatar control, available from the Asset Store.
 * JSON Object (Asset Store：Free)<br>Free 3rd party assets used for JSON communication with Stellarium, available from the Asset Store.
 * Dome Tools（Asset Store：Free）<br>Free 3rd party assets for dome projection, available from the Asset Store.
 * TriLib2 (Asset Store：$49.50)<br>3rd party paid assets for loading 3D models, available from the Asset Store.

Asset Settings
1.	Select `Window > Asset Store` from the menu bar to open the Asset Store.
2.	Search for `3rd Person Controller + Fly Mode` and press the `Add to My Asset` button to obtain the Asset.<br>Free and Mobile versions are available, but the Free version will be used.
3.	Search for `JSON Object` and press the `Add to My Asset` button to obtain the Asset.<br>Use the one created by Defective Studios.
4.	Search for `Dome Tools` and press the `Add to My Asset` button to obtain the Asset.<br>Use the one created by At-Bristol.
5.	Search for `TriLib 2` and press the `Buy Now` button to obtain the Asset.<br>Use the one created by Ricardo Reis.
6.	Select `Window＞Package Manager` from the menu bar to open the Package Manager.
7.	From the `Package` tab, select `Unity Registry`, then click `Input System` from the list on the left hand side and click the `Install` button.<br>Select `YES` when the RESTART WARNING appears.
8.	From the `+▼` tab, select `Add package by name ...` and enter `com.unity.localization` in the Name field and click the Add button.<br>You can also install by selecting `Localization` from the list as well as `Input System`, but if you have an older version (Version 1.0.5), please install the latest version by the method described above.
9.	From the `+▼` tab, select `Add package by name ...` and enter `com.unity.xr.interaction.toolkit` in the Name field and click the Add button.<br>When the Update request comes, click the `I Made a Backup, Go Ahead !` button.
10.	Click the `XR Interaction Toolkit` in the left-hand list and click the `Import` button under `Samples Starter Assets`.
11.	From the `Package` tab, select `My Assets`, then click the `JSON Object` in the left-hand list and click the `Import` button. <br>After that, when the Import list is displayed, click the `Import` button.
12.	Click the `3rd Person Controller + Fly Mode` in the left-hand list and click the `Import` button. <br>When a Warning appears asking if you want to Switch Project, click the `Import` button. After that, the Import list will be displayed, but since it is not necessary to change the project settings, remove the check mark from `ProjectSettings` and click the `Import` button.
13.	Click the `Dome Tools` in the left-hand list and click the `Import` button. 
13.	Click the `TriLib2` in the left-hand list and click the `Import` button. <br>In the Import list, we know that `Newtonsoft.Json.dll` in `Packages` causes a conflict error, so remove the check mark from `Packages` and click the `Import` button.
14.	Download the arcAstroVR unitypackage.
15.	Select the arcAstroVR unitypackage from `Assets>Import Package>Custom Package` in the menu bar and click the `open` button. After that, when the Import list is displayed, click the `Import` button.

### Project Settings
1.	Select `Edit＞Project Settings...` from the menu bar to open the Project Settings.
2.	Click the `Localization` in the left-hand list and select `Localization Settings` from ActiveSettings.
3.	Click the `Player` in the left-hand list and select `Localization Settings` from ActiveSettings.<br>Select `Input System Package (New)` for `Other Settings : Configuration : Active Input Gandling`.
4. Click the `XR Plugin Management` in the left-hand list and click `Install XR Plugin management`.<br>Check the InitializeXR on Startup checkbox. *Windows only<br>Check the OpenXR checkbox. *Windows only<br>*For Mac, all the above checks should be unchecked since it does not yet support the OpenXR.
5.	Click the `Preset Manager` in the left-hand list and click `Add Default Preset`. *Windows only<br>Select `Component>XR>XR Controller(Action based)`.<br>Make the following settings in the Filter and Preset fields displayed. <br>・Filter : `(none)`, Preset : `XRI Default Continuous Move`<br>・Filter : `(none)`, Preset : `XRI Default Continuous Turn`<br>・Filter : `(none)`, Preset : `XRI Default Snap Turn`<br>・Filter : `Left`, Preset : `XRI Default Left Controller`<br>・Filter : `Right`, Preset : `XRI Default Right Controller`<br>*For Mac, all the above XR controller settings are not necessary since the XR function is not yet supported.  

### Program execution
1. Open the scene file `Assets/arcAstroVR/Scences/arcAstroVR` from the Project folder.  
2. Click the `Run` Button

## References and Assets
The arcAstroVR asset is programmed based on the Unity Asset "stellarium-unity" developed by Georg Zotti (LBI ArchPro Vienna), John Fillwalk (IDIA Lab, Ball State University), David Rodriguez (IDIA Lab, Ball State University ), and Neil Zehr (IDIA Lab, Ball State University).  

 * Stellarium-unity-spout-JSONobject-U2017-3<br>Authors: Georg Zotti<br>Contact: https://github.com/Stellarium/stellarium-unity<br>Version: Released September 15, 2020<br>Licence: GNU General Public License v3.0  
 * 3rd Person Controller + Fly Mode<br>Authors: Vinicius Marques<br>Contact: http://www.dcc.ufmg.br/~allonman/support<br>Version: 2.1.5<br>Licence: Unity Asset Store standard EULA  
 * JSON Object<br>Authors: Defective Studios<br>Contact: http://defectivestudios.com/company<br>Version: 2.1.2<br>Licence: Unity Asset Store standard EULA  
 * Dome Tools<br>Authors: At-Bristol<br>Contact: http://at-bristol.org.uk<br>Version: 1.1<br>Licence: Unity Asset Store standard EULA  
 * TriLib2<br>Authors: Ricardo Reis<br>Contact: https://ricardoreis.net<br>Version: 2.1.0<br>Licence: Unity Asset Store standard EULA  

## License
Released April 1, 2022 under GPLv3.

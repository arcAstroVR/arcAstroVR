using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class aAV_Domemaster : MonoBehaviour
{
	public enum Resolution
	{
		Cube512	= 512,	   // Cubemap with 512x512 faces
		Cube1024   = 1024,	  // Cubemap with 1024x1024 faces
		Cube2048   = 2048,	  // etc.
		Cube4096   = 4096,
		Cube8192   = 8192,
	}

	public enum Face
	{
		None = 0,
		Everything = 63,
		PositiveX = (1 << CubemapFace.PositiveX),
		NegativeX = (1 << CubemapFace.NegativeX),
		PositiveY = (1 << CubemapFace.PositiveY),
		NegativeY = (1 << CubemapFace.NegativeY),
		PositiveZ = (1 << CubemapFace.PositiveZ),
		NegativeZ = (1 << CubemapFace.NegativeZ),
	}

	//インスペクター
	public Face cubemapFaces = Face.Everything;
	public Resolution cubeResolution = Resolution.Cube2048;
	[Range(120f, 270f)]
	public float FOV = 180.0f;
	[Range(-90.0f, +90.0f)]
	public float domeCameraPitch = -80.0f;
	[Range(-180.0f, +180.0f)]
	public float domeCameraRoll = 0f;
	[Range(0.0f, 1.0f)]
	public float backFadeIntensity = 0.1f;
	[Range(0.0f, 1.0f)]
	public float crescentFadeIntensity = 0.5f;
	[Range(0.0f, 1.0f)]
	public float crescentFadeRadius = 0.8f;
	[Range(-1.0f, +1.0f)]
	public float crescentFadeOffset = -0.2f;

	private Camera _TargetCamera;
	private RenderTexture cubeRT;
	private Material domeMaterial;

	void Awake()
	{
		if (!cubeRT)
		{
			int size = (int)cubeResolution;
			cubeRT = new RenderTexture(size, size, 24, RenderTextureFormat.ARGB32)
			{
				dimension = TextureDimension.Cube,
			};
		}
		if (!domeMaterial) domeMaterial = new Material(Shader.Find("Hidden/Domemaster"));
	}

	void Start()
	{
		_TargetCamera = gameObject.GetComponent<Camera>();
		if (_TargetCamera == null) Destroy(this);
	}

	public void LateUpdate()
	{
		StartCoroutine(RenderFrame());
	}

	IEnumerator RenderFrame()
	{
		yield return new WaitForEndOfFrame();

		// Render cubemap
		var eyesEyeSepBackup = _TargetCamera.stereoSeparation;
		_TargetCamera.transform.localRotation = Quaternion.Euler(new Vector3(domeCameraPitch, 0, domeCameraRoll));
		_TargetCamera.stereoSeparation = 0;
		_TargetCamera.RenderToCubemap(cubeRT, (int)cubemapFaces, Camera.MonoOrStereoscopicEye.Mono);
		_TargetCamera.stereoSeparation = eyesEyeSepBackup;

		Quaternion rot = Quaternion.Inverse(_TargetCamera.transform.rotation);	
		domeMaterial.SetVector("_Rotation", new Vector4(rot.x, rot.y, rot.z, rot.w));
		domeMaterial.SetFloat("_HalfFOV", FOV * 0.5f * Mathf.Deg2Rad);
		domeMaterial.SetVector("_FadeParams", new Vector4(backFadeIntensity, crescentFadeIntensity, crescentFadeRadius, crescentFadeOffset));
//		Graphics.Blit(cubeRT, null, domeMaterial);
	}
}

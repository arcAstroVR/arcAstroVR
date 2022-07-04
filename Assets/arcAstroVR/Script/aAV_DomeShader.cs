using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class aAV_DomeShader : MonoBehaviour
{
	public const float WorldCameraDefPitch = -80.0f;
	public const float WorldCameraMinPitch = -120.0f;
	public const float WorldCameraMaxPitch = +120.0f;

	public const float WorldCameraDefRoll  =	0.0f;
	public const float WorldCameraMinRoll  = -180.0f;
	public const float WorldCameraMaxRoll  = +180.0f;

	public const int DefFOV = 270;
	public const int MinFOV = 120;
	public const int MaxFOV = 270;

	public const float DefBackFadeIntensity	 =  0.1f;
	public const float DefCrescentFadeIntensity =  0.5f;
	public const float DefCrescentFadeRadius	=  0.8f;
	public const float DefCrescentFadeOffset	= -0.2f;

	public enum AntiAliasingType
	{
		Off,					// No anti-aliasing
		SSAA_2X,				// 2X super sampling
		SSAA_4X,				// 4X super sampling
	}

	public enum CubeMapType
	{
		Cube512	= 512,	   // Cubemap with 512x512 faces
		Cube1024   = 1024,	  // Cubemap with 1024x1024 faces
		Cube2048   = 2048,	  // etc.
		Cube4096   = 4096,
		Cube8192   = 8192,
	}

	private Camera m_worldCamera;
	private Material m_material;
	private RenderTexture m_cubeRT;
	
	[Range(WorldCameraMinPitch, WorldCameraMaxPitch)]
	public float worldCameraPitch = WorldCameraDefPitch;

	[Range(WorldCameraMinRoll, WorldCameraMaxRoll)]
	public float worldCameraRoll = WorldCameraDefRoll;

	[Range(MinFOV, MaxFOV)]
	public int FOV = DefFOV;

	public CubeMapType cubeMapType = CubeMapType.Cube1024;

	public AntiAliasingType antiAliasingType = AntiAliasingType.SSAA_2X;

	[Range(0.0f, 1.0f)]
	public float backFadeIntensity = DefBackFadeIntensity;

	[Range(0.0f, 1.0f)]
	public float crescentFadeIntensity = DefCrescentFadeIntensity;

	[Range(0.0f, 1.0f)]
	public float crescentFadeRadius = DefCrescentFadeRadius;

	[Range(-1.0f, +1.0f)]
	public float crescentFadeOffset = DefCrescentFadeOffset;
	
	void Start()
	{
		Shader shader = Shader.Find("Hidden/ZubrVR/DomeProjection");
		m_material = new Material(shader);
	 	m_worldCamera = GameObject.Find("WorldCamera").GetComponent<Camera>();
	}

	void LateUpdate()
	{
		int cubeMapSize = (int) cubeMapType;
		if (m_cubeRT != null) Destroy(m_cubeRT);
		m_cubeRT = new RenderTexture(cubeMapSize, cubeMapSize, 24, RenderTextureFormat.ARGB32);
		m_cubeRT.isCubemap = true;
		m_cubeRT.Create();

		m_worldCamera.transform.localRotation = Quaternion.Euler(new Vector3(worldCameraPitch, 0, worldCameraRoll));
		m_worldCamera.RenderToCubemap(m_cubeRT);
	}

	void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		Quaternion rot = Quaternion.Inverse(m_worldCamera.transform.rotation);
		m_material.SetVector("_Rotation", new Vector4(rot.x, rot.y, rot.z, rot.w));
		m_material.SetFloat("_HalfFOV", ((float)FOV) * 0.5f * Mathf.Deg2Rad);
		m_material.SetVector("_FadeParams", new Vector4(backFadeIntensity, crescentFadeIntensity, crescentFadeRadius, crescentFadeOffset));

		switch (antiAliasingType)
		{
		case AntiAliasingType.SSAA_2X:
			DoSSAA(dest, 1.414f);
			break;
		case AntiAliasingType.SSAA_4X:
			DoSSAA(dest, 2.0f);
			break;
		default:
			Graphics.Blit(m_cubeRT, dest, m_material);
			break;
		}
	}

	private void DoSSAA(RenderTexture dest, float factor)
	{
		int w = Screen.width;
		int h = Screen.height;
		int d = 24;
		RenderTextureFormat f = RenderTextureFormat.ARGB32;

		if (dest != null)
		{
			w = dest.width;
			h = dest.height;
			d = dest.depth;
			f = dest.format;
		}
			
		// Hi-res render
		w = Mathf.CeilToInt(factor * (float) w);  
		h = Mathf.CeilToInt(factor * (float) h);
		RenderTexture rt = RenderTexture.GetTemporary(w, h, d, f, RenderTextureReadWrite.Default, 1);
		Graphics.Blit(m_cubeRT, rt, m_material);

		// SSAA blit
		Graphics.Blit(rt, dest);

		RenderTexture.ReleaseTemporary(rt);
	}

}

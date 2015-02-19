using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class CRT : MonoBehaviour
{
	#region Fields
	public Shader curShader;
	public float Distortion = 0.1f;
	public float InputGamma = 2.4f;
	public float OutputGamma = 2.2f;
	public float TextureSize = 768f;
	private Material curMaterial;
	#endregion

	#region Internal Properties
	private Material material
	{
		get
		{
			if (curMaterial == null)
			{
				curMaterial = new Material(curShader);
				curMaterial.hideFlags = HideFlags.HideAndDontSave;
			}

			return curMaterial;
		}
	}
	#endregion

	#region MonoBehaviour
	private void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			enabled = false;
			return;
		}
	}

	private void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
	{
		if (curShader != null)
		{
			material.SetFloat("_Distortion", Distortion);
			material.SetFloat("_InputGamma", InputGamma);
			material.SetFloat("_OutputGamma", OutputGamma);
			material.SetVector("_TextureSize", new Vector2(TextureSize, TextureSize));
			Graphics.Blit(sourceTexture, destTexture, material);
		}
		else
		{
			Graphics.Blit(sourceTexture, destTexture);
		}
	}

	private void OnDisable()
	{
		if (curMaterial)
		{
			DestroyImmediate(curMaterial);
		}
	}
	#endregion
}
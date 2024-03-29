﻿#region

using UnityEngine;

#endregion

// Glow uses the alpha channel as a source of "extra brightness".
// All builtin Unity shaders output baseTexture.alpha * color.alpha, plus
// specularHighlight * specColor.alpha into that.
// Usually you'd want either to make base textures to have zero alpha; or
// set the color to have zero alpha (by default alpha is 0.5).

[ExecuteInEditMode, RequireComponent(typeof(Camera)), AddComponentMenu("Image Effects/Bloom and Glow/Glow (Deprecated)")]
public class GlowEffect : MonoBehaviour
{
	/// Blur iterations - larger number means more blur.
	public int blurIterations = 3;

	public Shader blurShader;

	/// Blur spread for each iteration. Lower values
	/// give better looking blur, but require more iterations to
	/// get large blurs. Value is usually between 0.5 and 1.0.
	public float blurSpread = 0.7f;

	// --------------------------------------------------------
	// The final composition shader:
	//   adds (glow color * glow alpha * amount) to the original image.
	// In the combiner glow amount can be only in 0..1 range; we apply extra
	// amount during the blurring phase.

	public Shader compositeShader;
	public Shader downsampleShader;

	/// The brightness of the glow. Values larger than one give extra "boost".
	public float glowIntensity = 1.5f;

	/// Tint glow with this color. Alpha adds additional glow everywhere.
	public Color glowTint = new Color(1, 1, 1, 0);

	private Material m_BlurMaterial;
	private Material m_CompositeMaterial;
	private Material m_DownsampleMaterial;
	// --------------------------------------------------------
	// The blur iteration shader.
	// Basically it just takes 4 texture samples and averages them.
	// By applying it repeatedly and spreading out sample locations
	// we get a Gaussian blur approximation.
	// The alpha value in _Color would normally be 0.25 (to average 4 samples),
	// however if we have glow amount larger than 1 then we increase this.

	protected Material blurMaterial
	{
		get
		{
			if (m_BlurMaterial == null)
			{
				m_BlurMaterial = new Material(blurShader);
				m_BlurMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_BlurMaterial;
		}
	}

	protected Material compositeMaterial
	{
		get
		{
			if (m_CompositeMaterial == null)
			{
				m_CompositeMaterial = new Material(compositeShader);
				m_CompositeMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_CompositeMaterial;
		}
	}

	// --------------------------------------------------------
	// The image downsample shaders for each brightness mode.
	// It is in external assets as it's quite complex and uses Cg.

	protected Material downsampleMaterial
	{
		get
		{
			if (m_DownsampleMaterial == null)
			{
				m_DownsampleMaterial = new Material(downsampleShader);
				m_DownsampleMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_DownsampleMaterial;
		}
	}

	public void BlitGlow(RenderTexture source, RenderTexture dest)
	{
		compositeMaterial.color = new Color(1F, 1F, 1F, Mathf.Clamp01(glowIntensity));
		Graphics.Blit(source, dest, compositeMaterial);
	}

	// Downsamples the texture to a quarter resolution.
	private void DownSample4x(RenderTexture source, RenderTexture dest)
	{
		downsampleMaterial.color = new Color(glowTint.r, glowTint.g, glowTint.b, glowTint.a / 4.0f);
		Graphics.Blit(source, dest, downsampleMaterial);
	}

	// Performs one blur iteration.
	public void FourTapCone(RenderTexture source, RenderTexture dest, int iteration)
	{
		var off = 0.5f + iteration * blurSpread;
		Graphics.BlitMultiTap(source, dest, blurMaterial, new Vector2(off, off), new Vector2(-off, off), new Vector2(off, -off), new Vector2(-off, -off));
	}

	// --------------------------------------------------------
	//  finally, the actual code

	protected void OnDisable()
	{
		if (m_CompositeMaterial)
			DestroyImmediate(m_CompositeMaterial);
		if (m_BlurMaterial)
			DestroyImmediate(m_BlurMaterial);
		if (m_DownsampleMaterial)
			DestroyImmediate(m_DownsampleMaterial);
	}

	// Called by the camera to apply the image effect
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		// Clamp parameters to sane values
		glowIntensity = Mathf.Clamp(glowIntensity, 0.0f, 10.0f);
		blurIterations = Mathf.Clamp(blurIterations, 0, 30);
		blurSpread = Mathf.Clamp(blurSpread, 0.5f, 1.0f);

		var rtW = source.width / 4;
		var rtH = source.height / 4;
		var buffer = RenderTexture.GetTemporary(rtW, rtH, 0);

		// Copy source to the 4x4 smaller texture.
		DownSample4x(source, buffer);

		// Blur the small texture
		var extraBlurBoost = Mathf.Clamp01((glowIntensity - 1.0f) / 4.0f);
		blurMaterial.color = new Color(1F, 1F, 1F, 0.25f + extraBlurBoost);

		for (var i = 0; i < blurIterations; i++)
		{
			var buffer2 = RenderTexture.GetTemporary(rtW, rtH, 0);
			FourTapCone(buffer, buffer2, i);
			RenderTexture.ReleaseTemporary(buffer);
			buffer = buffer2;
		}
		Graphics.Blit(source, destination);

		BlitGlow(buffer, destination);

		RenderTexture.ReleaseTemporary(buffer);
	}

	protected void Start()
	{
		// Disable if we don't support image effects
		if (!SystemInfo.supportsImageEffects)
		{
			enabled = false;
			return;
		}

		// Disable the effect if no downsample shader is setup
		if (downsampleShader == null)
		{
			Debug.Log("No downsample shader assigned! Disabling glow.");
			enabled = false;
		}
		// Disable if any of the shaders can't run on the users graphics card
		else
		{
			if (!blurMaterial.shader.isSupported)
				enabled = false;
			if (!compositeMaterial.shader.isSupported)
				enabled = false;
			if (!downsampleMaterial.shader.isSupported)
				enabled = false;
		}
	}
}
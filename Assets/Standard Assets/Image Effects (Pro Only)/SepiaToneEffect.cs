﻿#region

using UnityEngine;

#endregion

[ExecuteInEditMode, AddComponentMenu("Image Effects/Color Adjustments/Sepia Tone")]
public class SepiaToneEffect : ImageEffectBase
{
	// Called by camera to apply image effect
	private void OnRenderImage(RenderTexture source, RenderTexture destination) { Graphics.Blit(source, destination, material); }
}
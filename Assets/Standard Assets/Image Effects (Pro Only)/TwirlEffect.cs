﻿#region

using UnityEngine;

#endregion

[ExecuteInEditMode, AddComponentMenu("Image Effects/Displacement/Twirl")]
public class TwirlEffect : ImageEffectBase
{
	public float angle = 50;
	public Vector2 center = new Vector2(0.5F, 0.5F);
	public Vector2 radius = new Vector2(0.3F, 0.3F);
	// Called by camera to apply image effect
	private void OnRenderImage(RenderTexture source, RenderTexture destination) { ImageEffects.RenderDistortion(material, source, destination, angle, center, radius); }
}
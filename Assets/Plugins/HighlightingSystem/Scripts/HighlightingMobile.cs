﻿#region

using UnityEngine;

#endregion

namespace HighlightingSystem
{
	public class HighlightingMobile : HighlightingBase
	{
		// Can't use [ImageEffectOpaque] attribute here in terms of performance!
		private void OnRenderImage(RenderTexture src, RenderTexture dst)
		{
			// Render highlightingBuffer
			RenderHighlighting(src);

			// Blit highlightingBuffer to dst immediately. dst will point to the backbuffer (will be null) in case no other Image Effects used
			BlitHighlighting(src, dst);
		}
	}
}
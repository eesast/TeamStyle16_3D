﻿#region

using UnityEngine;

#endregion

public class ForceMoveCamera : MonoBehaviour
{
	private Moba_Camera mobaCamera;

	private void Start() { mobaCamera = Camera.main.GetComponentInParent<Moba_Camera>(); }

	private void Update()
	{
		if (Input.GetMouseButtonDown(0) && Data.MiniMap.MapRect.Contains(Input.mousePosition) && !Methods.GUI.MouseOver())
			mobaCamera.isForcedMoving = true;
		if (Input.GetMouseButtonUp(0))
			mobaCamera.isForcedMoving = false;
		if (!mobaCamera.isForcedMoving)
			return;
		mobaCamera.ForceDestination = Methods.Coordinates.MiniMapBasedScreenToInternal(Input.mousePosition);
	}
}
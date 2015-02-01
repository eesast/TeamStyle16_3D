﻿#region

using UnityEngine;

#endregion

public class SelectionManager : MonoBehaviour
{
	private Entity lastDownEntity;
	private Entity lastSelectedEntity;

	private void Update()
	{
		if (!Screen.lockCursor)
		{
			Entity target = null;
			RaycastHit hitInfo;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity, LayerMask.GetMask("Entity")))
				target = hitInfo.transform.GetComponentInParent<Entity>();
			if (target != null)
			{
				if (Input.GetMouseButtonDown(0))
					lastDownEntity = target;
				if (Input.GetMouseButtonUp(0) && target == lastDownEntity)
				{
					if (lastSelectedEntity)
						lastSelectedEntity.Deselect();
					(lastSelectedEntity = target).Select();
				}
				target.MouseOver();
			}
		}
		if (Input.GetMouseButtonUp(1) && lastSelectedEntity)
		{
			lastSelectedEntity.Deselect();
			lastSelectedEntity = null;
		}
	}
}
#region

using System;
using UnityEngine;

#endregion

[Serializable]
public class BezierCurvePoint
{
	public Vector3 m_anchor_point;
	public Vector3 m_handle_point;

	public Vector3 HandlePoint(bool inverse)
	{
		if (inverse)
			return m_anchor_point + (m_anchor_point - m_handle_point);
		return m_handle_point;
	}
}
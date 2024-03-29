﻿#region

using HighlightingSystem;
using UnityEditor;

#endregion

[CustomEditor(typeof(HighlightingRenderer))]
public class HighlightingRendererEditor : HighlightingBaseEditor
{
	// 
	public override void OnInspectorGUI()
	{
		EditorGUILayout.HelpBox("Don't forget to add HighlightingBlitter component to this or any other camera which has depth greater than current camera.", MessageType.Warning);
		CommonGUI();
	}
}
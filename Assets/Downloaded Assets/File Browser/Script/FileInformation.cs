﻿#region

using System.IO;
using UnityEngine;

#endregion

public class FileInformation
{
	public readonly FileInfo fileInfo;
	private readonly GUIContent guiContent;

	public FileInformation(FileInfo fileInfo, Texture fileTexture)
	{
		this.fileInfo = fileInfo;
		guiContent = new GUIContent(this.fileInfo.Name, fileTexture);
	}

	public bool Button() { return GUILayout.Button(guiContent, new GUIStyle("button") { alignment = TextAnchor.MiddleLeft }); }
}
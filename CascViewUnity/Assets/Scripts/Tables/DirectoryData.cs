using System;
using UnityEditor.IMGUI.Controls;
using SimpleSQL;

[Serializable]
public class DirectoryData
{
	[PrimaryKey]
	public int Directory { get; set; }	
	public int Directory_Parent { get; set; }
	public string Value { get; set; }
}
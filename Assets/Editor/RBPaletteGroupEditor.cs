﻿using UnityEngine;
using UnityEditor;
using System.Collections;


[CustomEditor(typeof(RBPaletteGroup))]
public class RBPaletteGroupEditor : Editor {
	
	int colorIndex = 0;
	int paletteIndex = 0;

	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector ();

		RBPaletteGroup targetRBPaletteGroup = (RBPaletteGroup) target;

		EditorGUILayout.Space ();
		EditorGUILayout.Separator ();

		if( GUILayout.Button( "Add Color", GUILayout.ExpandWidth(false)) )
		{
			targetRBPaletteGroup.AddColor ();
		}

		if( GUILayout.Button( "Add Palette", GUILayout.ExpandWidth(false)) )
		{
			targetRBPaletteGroup.AddPalette ();
		}

		colorIndex = EditorGUILayout.IntSlider ("Color index: ", colorIndex, 0, targetRBPaletteGroup.NumColorsInPalette - 1);
		if( GUILayout.Button( "Remove Color At Index", GUILayout.ExpandWidth(false)) )
		{
			targetRBPaletteGroup.RemoveColorAtIndex (colorIndex);
		}
		
		paletteIndex = EditorGUILayout.IntSlider ("Palette index: ", paletteIndex, 0, targetRBPaletteGroup.Count - 1);
		if( GUILayout.Button( "Remove Palette At Index", GUILayout.ExpandWidth(false)) )
		{
			targetRBPaletteGroup.RemovePaletteAtIndex (paletteIndex);
		}

		if( GUILayout.Button( "Export As Texture", GUILayout.ExpandWidth(false)) )
		{
			string outputPath = GetPathToAsset (targetRBPaletteGroup);
			string extension = ".png";
			string filename = targetRBPaletteGroup.GroupName + extension;
			targetRBPaletteGroup.WriteToFile (outputPath + filename, true);
		}
	}
	
	string GetPathToAsset (Object asset)
	{
		string path = AssetDatabase.GetAssetPath (asset);
		
		// Strip filename out from asset path
		string[] directories = path.Split ('/');
		path = path.TrimEnd (directories [directories.Length - 1].ToCharArray ());
		
		return path;
	}
}

using UnityEngine;
using UnityEditor;
using System.Collections;


[CustomEditor(typeof(RBPaletteGroup))]
public class RBPaletteGroupEditor : Editor {

	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector ();

		RBPaletteGroup targetRBPaletteGroup = (RBPaletteGroup) target;

		if( GUILayout.Button( "Add Color", GUILayout.ExpandWidth(false)) )
		{
			targetRBPaletteGroup.AddColor ();
		}

		if( GUILayout.Button( "Add Palette", GUILayout.ExpandWidth(false)) )
		{
			targetRBPaletteGroup.AddPalette ();
		}
	}
}

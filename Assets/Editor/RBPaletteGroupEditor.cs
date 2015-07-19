using UnityEngine;
using UnityEditor;
using System.Collections;


[CustomEditor(typeof(RBPaletteGroup))]
public class RBPaletteGroupEditor : Editor {
	
	int colorIndex = 0;

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

		colorIndex = EditorGUILayout.IntField ("Color index: ", colorIndex);
		if( GUILayout.Button( "Remove Color At Index", GUILayout.ExpandWidth(false)) )
		{
			targetRBPaletteGroup.RemoveColorAtIndex (colorIndex);
		}
	}
}

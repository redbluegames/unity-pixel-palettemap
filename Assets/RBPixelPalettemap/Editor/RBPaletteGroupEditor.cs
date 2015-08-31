using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(RBPaletteGroup))]
public class RBPaletteGroupEditor : Editor {
	
	int colorIndex = 0;
	int paletteIndex = 0;

	bool isListLocked = false;

	private ReorderableList list;
	
	private void OnEnable() {
		list = new ReorderableList(serializedObject, 
		                           serializedObject.FindProperty("palettes"), 
		                           true, true, true, true);
		list.drawElementCallback = DrawListElement;

		list.onCanRemoveCallback = CanRemovePaletteFromList;
	}

	void DrawListElement (Rect rect, int index, bool isActive, bool isFocused)
	{
		var element = list.serializedProperty.GetArrayElementAtIndex(index);
		rect.y += 2;
		float labelWidth = 100.0f;
		SerializedProperty paletteName = element.FindPropertyRelative ("PaletteName");
		Rect labelRect = new Rect (rect.x, rect.y, labelWidth, EditorGUIUtility.singleLineHeight);
		paletteName.stringValue = EditorGUI.TextField (labelRect, paletteName.stringValue);
		
		SerializedProperty listProperty = element.FindPropertyRelative ("ColorsInPalette");
		float swatchwidth = 50.0f;
		float spacing = 10.0f;
		List<SerializedProperty> colorProperties = GetListFromSerializedProperty (listProperty);
		for (int i = 0; i < colorProperties.Count; i++) {
			float startX = i * swatchwidth;
			Rect colorRect = new Rect (rect.x + labelRect.width + startX + spacing, rect.y, 
			                           swatchwidth, EditorGUIUtility.singleLineHeight);
			colorProperties[i].colorValue = EditorGUI.ColorField (colorRect, colorProperties[i].colorValue);
			
			//EditorGUIUtility.DrawColorSwatch (colorRect, Color.red);
		}
	}
	
	bool CanRemovePaletteFromList (ReorderableList list) 
	{
		// Don't let them remove the base palette
		if (list.count <= 1) {
			return false;
		}

		// Hmm this keeps them from removing but not adding
		return !isListLocked;
	}

	int GetNumColorsPerLine ()
	{
		float colorWidth = 50.0f;
		return Mathf.FloorToInt (Screen.width / colorWidth);
	}

	public override void OnInspectorGUI ()
	{
		//DrawDefaultInspector ();
		
		serializedObject.Update();
		list.DoLayoutList();

		SerializedProperty lockedProperty = serializedObject.FindProperty ("Locked");
		lockedProperty.boolValue = EditorGUILayout.Toggle ("Locked", lockedProperty.boolValue);
		isListLocked = lockedProperty.boolValue;

		RBPaletteGroup targetRBPaletteGroup = (RBPaletteGroup) target;

		EditorGUILayout.Space ();
		EditorGUILayout.LabelField ("Colors", EditorStyles.boldLabel);

		if( GUILayout.Button( "Add Color", GUILayout.ExpandWidth(false)) )
		{
			targetRBPaletteGroup.AddColor ();
		}

		colorIndex = EditorGUILayout.IntSlider ("Color index: ", colorIndex, 0, targetRBPaletteGroup.NumColorsInPalette - 1);
		if( GUILayout.Button( "Remove Color At Index", GUILayout.ExpandWidth(false)) )
		{
			targetRBPaletteGroup.RemoveColorAtIndex (colorIndex);
		}

		EditorGUILayout.Space ();
		EditorGUILayout.LabelField ("Palettes", EditorStyles.boldLabel);
		
		if( GUILayout.Button( "Add Palette", GUILayout.ExpandWidth(false)) )
		{
			targetRBPaletteGroup.AddPalette ();
		}
		
		paletteIndex = EditorGUILayout.IntSlider ("Palette index: ", paletteIndex, 0, targetRBPaletteGroup.Count - 1);
		if( GUILayout.Button( "Remove Palette At Index", GUILayout.ExpandWidth(false)) )
		{
			targetRBPaletteGroup.RemovePaletteAtIndex (paletteIndex);
		}
		EditorGUILayout.Space ();
		EditorGUILayout.LabelField ("Utilities", EditorStyles.boldLabel);

		if( GUILayout.Button( "Export As Texture", GUILayout.ExpandWidth(false)) )
		{
			string outputPath = GetPathToAsset (targetRBPaletteGroup);
			string extension = ".png";
			string filename = targetRBPaletteGroup.GroupName + extension;
			try {
				targetRBPaletteGroup.WriteToFile (outputPath + filename, false);
			} catch (System.AccessViolationException) {
				if (EditorUtility.DisplayDialog ("Warning!", 
				                                 "This will overwrite the existing file, " + filename + 
				                                 ". Are you sure you want to export the texture?", "Yes", "No")) {
					targetRBPaletteGroup.WriteToFile (outputPath + filename, true);
				}
			}
		}

		serializedObject.ApplyModifiedProperties();
	}
	
	string GetPathToAsset (Object asset)
	{
		string path = AssetDatabase.GetAssetPath (asset);
		
		// Strip filename out from asset path
		string[] directories = path.Split ('/');
		path = path.TrimEnd (directories [directories.Length - 1].ToCharArray ());
		
		return path;
	}
	
	/// <summary>
	/// Gets the Serialized Property for a List member as a List of SerializedProperties
	/// </summary>
	/// <returns>The value of the serialized property as a List.</returns>
	/// <param name="listAsProperty">The property that stores a list.</param>
	List<SerializedProperty> GetListFromSerializedProperty (SerializedProperty listAsProperty)
	{
		if (!listAsProperty.isArray) {
			return null;
		}
		
		// Apparently it's bad to iterate on the original... not sure why, but that's
		// what the StackOverflow said
		SerializedProperty listCopy = listAsProperty.Copy ();
		
		listCopy.Next (true); // Skip generic element
		listCopy.Next (true); // This is the array size
		
		int listSize = listCopy.intValue;
		int lastindex = listSize - 1;
		List<SerializedProperty> listOfElements = new List<SerializedProperty> (listSize);
		
		listCopy.Next (true); // Get first element in list
		for (int i = 0; i < listSize; i++) {
			// Copy elements or else we will just make a list of the enumerator
			SerializedProperty nextElement = listCopy.Copy ();
			listOfElements.Add (nextElement);
			if (i < lastindex) {
				listCopy.Next (false);
			}
		}
		
		return listOfElements;
	}
}

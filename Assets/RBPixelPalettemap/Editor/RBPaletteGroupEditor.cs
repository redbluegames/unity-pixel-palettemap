using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(RBPaletteGroup))]
public class RBPaletteGroupEditor : Editor {
	
	int colorIndex = 0;

	bool isListLocked = false;
	#region Sizing
	float colorPaddingX = 5.0f;
	float colorSpacing = 5.0f;
	float palettePadding = 5.0f;
	float colorWidth = 50.0f;
	float labelWidth = 100.0f;
	#endregion

	private ReorderableList list;
	List<RBPalette> lastValidPaletteList;
	
	private void OnEnable() {
		list = new ReorderableList(serializedObject, 
		                           serializedObject.FindProperty("palettes"), 
		                           true, true, true, true);
		list.onCanRemoveCallback = CanRemovePaletteFromList;
		list.drawHeaderCallback = (Rect rect) => {  
			EditorGUI.LabelField(rect, ((RBPaletteGroup)target).GroupName + " - Palette Mode");
		};
		list.drawElementCallback = DrawListElement;

		// TODO: Don't let them reorder Base Palette
	}

	bool CanRemovePaletteFromList (ReorderableList list) 
	{
		// Don't let them remove the base palette
		if (list.index == 0) {
			return false;
		}

		return !isListLocked;
	}

	float GetElementHeight ()
	{
		RBPaletteGroup targetRBPaletteGroup = (RBPaletteGroup) target;
		int numlines = Mathf.CeilToInt (targetRBPaletteGroup.NumColorsInPalette / (float) GetMaxNumColorsPerLine ());
		float sizePerLine = EditorGUIUtility.singleLineHeight + palettePadding;
		return numlines * sizePerLine;
	}
	
	int GetMaxNumColorsPerLine ()
	{
		// w = (n-1) * s + N * c + 2p  [solve for n = (w - 2p + s) / (s + c)]
		// Where n = numColors, s = spacing, c = colorwidth, p = padding, w = availableWidth
		float windowWidth = Screen.width;
		float handleWidth = list.draggable ? 25.0f : 0.0f;
		float availableWidth = windowWidth - labelWidth - handleWidth - (2 * colorPaddingX);
		int numColors = Mathf.FloorToInt ((availableWidth - (2 * colorPaddingX) + colorSpacing) / (colorSpacing + colorWidth));

		return numColors;
	}
	
	void DrawListElement (Rect rect, int index, bool isActive, bool isFocused)
	{
		var element = list.serializedProperty.GetArrayElementAtIndex(index);
		rect.y += 2;
		
		// Draw label
		SerializedProperty paletteName = element.FindPropertyRelative ("PaletteName");
		Rect labelRect = new Rect (rect.x, rect.y, labelWidth, EditorGUIUtility.singleLineHeight);
		bool isNameEditable = index > 0;
		if (isNameEditable) {
			paletteName.stringValue = EditorGUI.TextField (labelRect, paletteName.stringValue);
		} else {
			EditorGUI.LabelField (labelRect, paletteName.stringValue);
		}
		
		SerializedProperty colorsAsList = element.FindPropertyRelative ("ColorsInPalette");
		int numColorsPerLine = GetMaxNumColorsPerLine ();
		List<SerializedProperty> colorProperties = GetListFromSerializedProperty (colorsAsList);
		for (int i = 0; i < colorProperties.Count; i++) {
			int colorIndexOnLine = i % numColorsPerLine;
			float startX = labelRect.width + colorPaddingX + colorIndexOnLine * (colorWidth + colorSpacing);
			float startY = Mathf.FloorToInt (i / numColorsPerLine) * EditorGUIUtility.singleLineHeight;
			Rect colorRect = new Rect (rect.x + startX, rect.y + startY, 
			                           colorWidth, EditorGUIUtility.singleLineHeight);
			colorProperties[i].colorValue = EditorGUI.ColorField (colorRect, colorProperties[i].colorValue);
			
			//EditorGUIUtility.DrawColorSwatch (colorRect, colorProperties[i].colorValue);
		}
	}

	void SetGroupLocked (bool locked)
	{
		isListLocked = locked;
		list.displayAdd = !locked;
		list.displayRemove = !locked;
		list.draggable = !locked;
	}

	public override void OnInspectorGUI ()
	{
		RBPaletteGroup targetRBPaletteGroup = (RBPaletteGroup) target;

		//DrawDefaultInspector ();
		serializedObject.Update();
		
		list.elementHeight = GetElementHeight ();

		list.DoLayoutList();

		SerializedProperty lockedProperty = serializedObject.FindProperty ("Locked");
		lockedProperty.boolValue = EditorGUILayout.Toggle ("Locked", lockedProperty.boolValue);
		SetGroupLocked (lockedProperty.boolValue);

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

		// TODO: Remove Palette Editing once it works through + / - buttons
		/*
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
		*/
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

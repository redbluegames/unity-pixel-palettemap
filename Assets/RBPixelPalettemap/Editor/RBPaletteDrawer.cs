using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;

[CustomPropertyDrawer (typeof(RBPalette))]
public class RBPaletteDrawer : PropertyDrawer
{
	const float widthPerColor = 40.0f;
	ReorderableList colorList;

	public override float GetPropertyHeight (SerializedProperty serializedProperty, GUIContent label)
	{
		// Return 0 when using EditorGUILayout to draw Property (which is apparently not allowed,
		// but I'm a rebel.)
		return 0;
	}

	int GetNumColorsPerLine ()
	{
		return Mathf.FloorToInt (Screen.width / GetColorWidthWithPadding ());
	}

	float GetColorWidthWithPadding ()
	{
		float mysteriousPadding = widthPerColor * 0.2f;
		return widthPerColor + mysteriousPadding;
	}

	int GetNumLines (SerializedProperty property)
	{
		if (GetNumColorsPerLine () == 0) {
			// Can't draw it if it's so small we can't even fit a color.
			Debug.Log ("NumColorsPerLine = 0, Screen width: " + Screen.width);
			return 0;
		}

		SerializedProperty listProperty = property.FindPropertyRelative ("ColorsInPalette");
		List<SerializedProperty> colorProperties = GetListFromSerializedProperty (listProperty);

		int numLines = Mathf.CeilToInt ((float)colorProperties.Count / GetNumColorsPerLine ());
		return numLines;
	}

	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUILayout.BeginVertical (GUI.skin.box);
		
		// Draw the PaletteName
		EditorGUILayout.BeginHorizontal ();
		SerializedProperty nameProperty = property.FindPropertyRelative ("PaletteName");
		string paletteName = nameProperty.stringValue;
		nameProperty.stringValue = EditorGUILayout.TextField (paletteName, EditorStyles.boldLabel, GUILayout.MaxWidth (100.0f));
		
		SerializedProperty lockedProperty = property.FindPropertyRelative ("Locked");
		lockedProperty.boolValue = EditorGUILayout.Toggle ("Locked", lockedProperty.boolValue);
		EditorGUILayout.EndHorizontal ();
		
		// Draw the list of colors
		DrawColorsInPalette (property, lockedProperty.boolValue);
		
		EditorGUILayout.EndVertical (); // End Color Palette
		
		property.serializedObject.ApplyModifiedProperties ();
	}

	void DrawColorsInPalette (SerializedProperty property, bool isLocked)
	{
		// Get the colors as properties
		SerializedProperty listProperty = property.FindPropertyRelative ("ColorsInPalette");
		List<SerializedProperty> colorProperties = GetListFromSerializedProperty (listProperty);

		// Draw the table of colors
		int numColorsPerLine = GetNumColorsPerLine ();
		int numLines = GetNumLines (property);
		GUIStyle paletteStyle = new GUIStyle (GUI.skin.box);
		EditorGUILayout.BeginVertical (paletteStyle);
		for (int i = 0; i < numLines; i++) {
			int startingLineColorIndex = i * numColorsPerLine;
			int colorsRemainingInPalette = colorProperties.Count - startingLineColorIndex;
			int numColorsOnThisLine = Mathf.Min (colorsRemainingInPalette, numColorsPerLine);
			List<SerializedProperty> propertiesOnThisLine = 
				colorProperties.GetRange (startingLineColorIndex, numColorsOnThisLine);
			DrawColorPaletteLine (propertiesOnThisLine, isLocked);
		}
		EditorGUILayout.EndVertical (); // End Colors

	}

	void DrawColorPaletteLine (List<SerializedProperty> colorProperties, bool isLocked)
	{
		EditorGUILayout.BeginHorizontal ();
		for (int i = 0; i < colorProperties.Count; i++) {
			var colorPropertyAtIndex = colorProperties [i];
			Color changedColor;
			changedColor = EditorGUILayout.ColorField (GUIContent.none, colorPropertyAtIndex.colorValue,
				false, true, false, new ColorPickerHDRConfig (0.0f, 1.0f, 0.0f, 1.0f), 
				GUILayout.Width (widthPerColor));
			if (!isLocked) {
				colorPropertyAtIndex.colorValue = changedColor;
			}
		}
		EditorGUILayout.EndHorizontal (); // End Row
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

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;

[CustomPropertyDrawer (typeof(RBPalette))]
public class RBPaletteDrawer : PropertyDrawer
{
	bool isEditing = false;
	private ReorderableList colorList;
	const float widthPerColor = 40.0f;

	public override float GetPropertyHeight (SerializedProperty serializedProperty, GUIContent label)
	{
		if (isEditing) {
			SerializedProperty listProperty = serializedProperty.FindPropertyRelative ("ColorsInPalette");
			return GetReorderableList (listProperty).GetHeight ();
		} else {
			// Return 0 when using EditorGUILayout to draw Property (which is apparently not allowed,
			// but I'm a rebel.)
			return 0;
		}
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
		if (isEditing) {
			SerializedProperty listProperty = property.FindPropertyRelative ("ColorsInPalette");
			colorList = GetReorderableList (listProperty);
			colorList.DoList (position);
		} else {
			SerializedProperty listProperty = property.FindPropertyRelative ("ColorsInPalette");
			List<SerializedProperty> colorProperties = GetListFromSerializedProperty (listProperty);

			// Draw the PaletteName
			EditorGUILayout.BeginVertical (GUI.skin.box);
			SerializedProperty nameProperty = property.FindPropertyRelative ("PaletteName");
			string paletteName = nameProperty.stringValue;
			EditorGUILayout.LabelField (paletteName, EditorStyles.boldLabel, GUILayout.MaxWidth (100.0f));

			// Draw the list of colors
			int numColorsPerLine = GetNumColorsPerLine ();
			int numLines = GetNumLines (property);
			GUIStyle paletteStyle = new GUIStyle (GUI.skin.box);
			EditorGUILayout.BeginVertical (paletteStyle);
			for (int j = 0; j < numLines; j++) {
				EditorGUILayout.BeginHorizontal (GUILayout.Width (position.width));
				for (int i = 0; i < numColorsPerLine; i++) {
					int listIndex = i + j * numColorsPerLine;
					if (listIndex >= colorProperties.Count) {
						// We've printed all colors.
						break;
					}
					var colorPropertyAtIndex = colorProperties [listIndex];
					colorPropertyAtIndex.colorValue = EditorGUILayout.ColorField (GUIContent.none, colorPropertyAtIndex.colorValue,
						false, true, false, new ColorPickerHDRConfig (0.0f, 1.0f, 0.0f, 1.0f), 
						GUILayout.Width (widthPerColor));
				}
				EditorGUILayout.EndHorizontal (); // End Row
			}
			EditorGUILayout.EndVertical (); // End Colors
			EditorGUILayout.EndVertical (); // End Color Palette
		}

		property.serializedObject.ApplyModifiedProperties ();
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
	
	private ReorderableList GetReorderableList (SerializedProperty serializedProperty)
	{
		if (colorList == null) {
			colorList = new ReorderableList (serializedProperty.serializedObject, serializedProperty);
			colorList.drawElementCallback += DrawListElement;
		}
		
		return colorList;
	}

	void DrawListElement (Rect rect, int index, bool isActive, bool isFocused)
	{
		var element = colorList.serializedProperty.GetArrayElementAtIndex (index);
		rect.y += 2;

		EditorGUI.LabelField (new Rect (rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), "Color " + index);
		element.colorValue = EditorGUI.ColorField (new Rect (rect.x + 60, rect.y, rect.width - 60, EditorGUIUtility.singleLineHeight),
		                      element.colorValue);
	}
}

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
	float colorPaddingX = 10.0f;
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
		list.drawElementBackgroundCallback = DrawListElementBackground; 

		// TODO: Don't let them reorder Base Palette
	}
	
	#region Reorderable Callbacks

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
		// Get number of lines we need to show all colors given the width
		RBPaletteGroup targetRBPaletteGroup = (RBPaletteGroup) target;
		int numColorsInPalette = targetRBPaletteGroup.NumColorsInPalette;
		int numColorLines = Mathf.CeilToInt (numColorsInPalette / (float) GetMaxNumColorsPerLine ());

		// Get size we will need to show all lines
		float sizePerLine = EditorGUIUtility.singleLineHeight + palettePadding;
		float expectedSize = numColorLines * sizePerLine;
		// At minimum show a line per palette
		expectedSize = Mathf.Max (expectedSize, sizePerLine) + palettePadding;

		return expectedSize;
	}
	
	int GetMaxNumColorsPerLine ()
	{
		// w = (n-1) * s + N * c + 2p  [solve for n = (w - 2p + s) / (s + c)]
		// Where n = numColors, s = spacing, c = colorwidth, p = padding, w = availableWidth
		float windowWidth = Screen.width;
		float reorderableListDefaultPadding = ReorderableList.Defaults.padding;
		windowWidth -= (2 * reorderableListDefaultPadding);

		float handleWidth = list.draggable ? ReorderableList.Defaults.dragHandleWidth : 0.0f;
		float availableWidth = windowWidth - labelWidth - handleWidth;
		int numColors = Mathf.FloorToInt ((availableWidth - (2 * colorPaddingX) + colorSpacing) /
		                                  (colorSpacing + colorWidth));
		
		return numColors;
	}
	
	void DrawListElement (Rect rect, int index, bool isActive, bool isFocused)
	{
		var element = list.serializedProperty.GetArrayElementAtIndex(index);
		rect.y += palettePadding;
		
		// Draw label
		SerializedProperty paletteName = element.FindPropertyRelative ("PaletteName");
		Rect labelRect = new Rect (rect.x, rect.y, labelWidth, EditorGUIUtility.singleLineHeight);
		bool isNameEditable = index > 0;
		if (isNameEditable) {
			paletteName.stringValue = EditorGUI.TextField (labelRect, paletteName.stringValue);
		} else {
			EditorGUI.LabelField (labelRect, paletteName.stringValue);
		}

		// Draw colors
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

	void DrawListElementBackground (Rect rect, int index, bool isActive, bool isFocused)
	{
		// Draws a bar on top and bottom to give depth effect,
		// and box background to prevent element overlap while dragging.
		float barHeight = 1.0f;
		rect.width -= 3.0f; // Subtract and shift or it hangs over edges
		rect.x += 1.0f;
		
		Rect topRect = new Rect (rect.x, rect.y, rect.width, barHeight);
		EditorGUI.DrawRect (topRect, Color.gray);
		
		Rect bottomRect = new Rect (rect.x, rect.y + rect.height - barHeight, rect.width, barHeight);
		EditorGUI.DrawRect (bottomRect, new Color (0.2f, 0.2f, 0.2f));
		
		Vector2 padding = new Vector2 (1.0f, 1.0f);
		Rect backgroundRect = new Rect (rect.x + padding.x, rect.y + padding.y, 
			rect.width - (2 * padding.x), rect.height - (2 * padding.y));

		// Background colors approximate editor skin. This would need to change if
		// editor colors change.
		Color backgroundColor = EditorGUIUtility.isProSkin ? new Color (0.3f, 0.3f, 0.3f) :
			new Color (0.9f, 0.9f, 0.9f);
		EditorGUI.DrawRect (backgroundRect, backgroundColor);
	}
	#endregion

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

		EditorGUILayout.Space ();
		EditorGUILayout.LabelField ("Utilities", EditorStyles.boldLabel);

		if( GUILayout.Button( "Export As Texture", GUILayout.ExpandWidth(false)) )
		{
			string outputPath = AssetDatabaseUtility.GetAssetDirectory (targetRBPaletteGroup);
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

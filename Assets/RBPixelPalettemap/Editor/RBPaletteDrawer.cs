using UnityEngine;
using UnityEditorInternal;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;

[CustomPropertyDrawer (typeof(RBPalette))]
public class RBPaletteDrawer : PropertyDrawer
{
	ReorderableList colorList;
	string listPropertyName = "ColorsInPalette";
	#region Sizing
	float elementPadding = 5.0f;
	float elementSpacing = 5.0f;
	float minLabelWidth = 100.0f;
	float preferredLabelWeight = 0.4f;
	#endregion

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
	{
		var listProperty = property.FindPropertyRelative (listPropertyName);
		return BuildReorderableColorList (listProperty).GetHeight ();
	}

	float GetElementHeight ()
	{
		return EditorGUIUtility.singleLineHeight + elementPadding;
	}
	
	void DrawListElement (Rect rect, int index, bool isActive, bool isFocused)
	{
		var element = colorList.serializedProperty.GetArrayElementAtIndex (index);
		rect.y += elementPadding;
		
		// Draw label
		SerializedProperty nameProperty = element.FindPropertyRelative ("Name");
		float labelWidth = Mathf.Max (rect.width * preferredLabelWeight, minLabelWidth);
		Rect labelRect = new Rect (rect.x, rect.y, labelWidth, EditorGUIUtility.singleLineHeight);
		nameProperty.stringValue = EditorGUI.TextField (labelRect, nameProperty.stringValue);

		float remainingWidth = rect.width - (labelRect.width + elementSpacing);
		SerializedProperty colorProperty = element.FindPropertyRelative ("Color");
		Rect colorRect = new Rect (rect.x + labelRect.width + elementSpacing, rect.y, 
		                           remainingWidth, EditorGUIUtility.singleLineHeight);
		GUIContent colorGUIContent = new GUIContent (string.Empty, nameProperty.stringValue);
		colorProperty.colorValue = EditorGUI.ColorField (colorRect, colorGUIContent, colorProperty.colorValue);
	}
	
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
	{
		var listProperty = property.FindPropertyRelative (listPropertyName);
		var renderedList = BuildReorderableColorList (listProperty);
		renderedList.elementHeight = GetElementHeight ();
		
		renderedList.DoList (position);
	}
	
	ReorderableList BuildReorderableColorList (SerializedProperty property)
	{
		if (property == null) {
			Debug.LogError ("Trying to build list from null property. You may have searched" +
				" for an invalid RelativeProperty.");
		}
		if (colorList != null) {
			return colorList;
		}

		SerializedObject objectForProperty = property.serializedObject;
		colorList = new ReorderableList (objectForProperty, property, 
		                           true, true, true, true);
		colorList.drawElementCallback = DrawListElement;
		colorList.drawHeaderCallback = (Rect rect) => {  
			EditorGUI.LabelField(rect, property.displayName);
		};
		return colorList;
	}
	
	/// <summary>
	/// Gets the Serialized Property for a List member as a List of SerializedProperties.
	/// Suggested by StackOverflow: 
	/// http://answers.unity3d.com/questions/682932/using-generic-list-with-serializedproperty-inspect.html
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

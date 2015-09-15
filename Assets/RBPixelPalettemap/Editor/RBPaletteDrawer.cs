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

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
	{
		var listProperty = property.FindPropertyRelative (listPropertyName);
		return BuildReorderableColorList (listProperty).GetHeight();
	}

	float GetElementHeight ()
	{
		return EditorGUIUtility.singleLineHeight;
	}
	
	void DrawListElement (Rect rect, int index, bool isActive, bool isFocused)
	{
	}
	
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
	{
		var listProperty = property.FindPropertyRelative (listPropertyName);
		var renderedList = BuildReorderableColorList (listProperty);
		var height = 0f;
		float spacing = 1.5f;
		for (int i = 0; i < listProperty.arraySize; i++)
			height = Mathf.Max(height, EditorGUI.GetPropertyHeight(listProperty.GetArrayElementAtIndex(i))) + spacing;
		renderedList.elementHeight = height;
		
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
		colorList = new ReorderableList(objectForProperty, property, 
		                           true, true, true, true);
		//colorList.drawElementCallback = DrawListElement;

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

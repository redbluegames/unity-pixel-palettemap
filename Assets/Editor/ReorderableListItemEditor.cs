using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;

[CustomPropertyDrawer (typeof(ReorderableListItem))]
public class ReorderableListItemEditor : PropertyDrawer {

	private ReorderableList list;
	
	public override float GetPropertyHeight( SerializedProperty serializedProperty, GUIContent label )
	{
		SerializedProperty listProperty = serializedProperty.FindPropertyRelative( "ListItemColor" );
		return GetList( listProperty ).GetHeight();
	}
	
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
	{
		SerializedProperty listProperty = property.FindPropertyRelative( "ListItemColor" );
		list = GetList (listProperty);

		list.DoList (position);
	}
	
	private ReorderableList GetList( SerializedProperty serializedProperty )
	{
		if( list == null )
		{
			list = new ReorderableList( serializedProperty.serializedObject, serializedProperty );
			list.drawElementCallback =  DrawListElement;
		}
		
		return list;
	}

	void DrawListElement (Rect rect, int index, bool isActive, bool isFocused)
	{
		var element = list.serializedProperty.GetArrayElementAtIndex(index);
		rect.y += 2;
		
		EditorGUI.LabelField (new Rect (rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), "Color " + index);
		EditorGUI.ColorField (new Rect (rect.x + 60, rect.y, rect.width - 60, EditorGUIUtility.singleLineHeight),
		                      element.colorValue);
	}
}

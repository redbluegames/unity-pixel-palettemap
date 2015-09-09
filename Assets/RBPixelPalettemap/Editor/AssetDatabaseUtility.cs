using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public static class AssetDatabaseUtility {

	public static string GetDirectoryOfSelection ()
	{
		// TODO THIS DOESN"T ALWAYS WORK.
		string path = "Assets";
		foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets)) {
			path = AssetDatabase.GetAssetPath (obj);
			if (File.Exists (path)) {
				path = Path.GetDirectoryName (path);
			}
			break;
		}
		path += Path.DirectorySeparatorChar;
		return path;
	}

	public static string GetAssetDirectory (Object asset)
	{
		string path = AssetDatabase.GetAssetPath (asset);
		return Path.GetDirectoryName (path) + Path.DirectorySeparatorChar;
	}
	
	public static Object SaveObject (Object objectToSave, string path, string filename)
	{
		string fullpath = path + filename;
		
		AssetDatabase.CreateAsset (objectToSave, fullpath);
		AssetDatabase.SaveAssets ();

		return objectToSave;
	}

	public static Object SaveAndSelectObject (Object objectToSave, string path, string filename)
	{
		Object savedObject = SaveObject (objectToSave, path, filename);
		SelectObject (savedObject);
		
		return savedObject;
	}

	public static void SelectObject (Object objectToSelect)
	{
		EditorUtility.FocusProjectWindow ();
		Selection.activeObject = objectToSelect;
	}
}

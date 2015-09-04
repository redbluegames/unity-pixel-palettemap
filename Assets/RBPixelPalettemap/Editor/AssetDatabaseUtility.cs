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
	
	/// <summary>
	/// Appends increasing numbers to the end of filename until it finds the first available.
	/// This mimicks the way Unity creates assets.
	/// </summary>
	/// <returns>The next unused filename.</returns>
	/// <param name="directory">Directory to search</param>
	/// <param name="filename">Filename with extension.</param>
	public static string GetNextUnusedFilename (string directory, string filename)
	{
		// If there is no existing file by that name, just use it
		string path = directory + filename;
		if (!System.IO.File.Exists (path)) {
			return filename;
		}
		
		// Find the next available number to use as a suffix
		string[] splitFilename = filename.Split ('.');
		string filenameWithoutExtensions = splitFilename [0];
		string extensions = "";
		for (int i = 1; i < splitFilename.Length; i++) {
			extensions += "." + splitFilename[i];
		}
		string unformattedFilename = filenameWithoutExtensions + " {0}" + extensions;
		string formattedFilename = string.Empty;
		int maxFilenameSuffix = 1000;
		for (int i = 1; i < maxFilenameSuffix; i++) {
			formattedFilename = string.Format (unformattedFilename, i);
			path = directory + formattedFilename;
			if (!System.IO.File.Exists (path)) {
				return formattedFilename;
			}
		}
		
		// All 1000 consecutive numbers failed. Just generate a random one.
		int randomSuffix = Random.Range (maxFilenameSuffix, int.MaxValue);
		formattedFilename = string.Format (unformattedFilename, randomSuffix);
		return formattedFilename;
	}
}

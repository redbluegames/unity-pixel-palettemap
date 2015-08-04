using UnityEngine;
using UnityEditor;
using System.IO;

public static class RBPaletteCreator {

	const string suffix = "_PaletteGroup";

	[MenuItem ("Assets/Create/RBPalette")]
	public static RBPaletteGroup CreatePalette ()
	{
		int fileAppendix = -1;
		string unformattedFilename = "RBPaletteGroup{0}.asset";
		string formattedFilename = string.Empty;
		string path = GetPathOfSelection ();
		for (int i = 0; i < 100; i++) {
			formattedFilename = string.Format (unformattedFilename, i);
			if (!File.Exists (path + "/" + formattedFilename)) {
				fileAppendix = i;
				break;
			}
		}
		if (fileAppendix >= 0) {
			return CreatePalette (GetPathOfSelection (), formattedFilename);
		} else {
			Debug.LogError ("Failed to create file. Too many generic RBPaletteGroups exist in save location.");
			return null;
		}
	}

	static RBPaletteGroup CreatePalette (string path, string filename)
	{
		RBPaletteGroup paletteAsset = RBPaletteGroup.CreateInstance ();
		return SaveRBPalette (paletteAsset, path, filename);
	}
	
	static RBPaletteGroup SaveRBPalette (RBPaletteGroup palette, string path, string filename)
	{
		string fullpath = path + "/" + filename;
		AssetDatabase.CreateAsset (palette, fullpath);
		AssetDatabase.SaveAssets ();
		
		EditorUtility.FocusProjectWindow ();
		Selection.activeObject = palette;
		
		return palette;
	}

	static string GetPathOfSelection ()
	{
		// TODO THIS DOESN"T ALWAYS WORK.
		string path = "Assets";
		foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
		{
			path = AssetDatabase.GetAssetPath(obj);
			if (File.Exists(path))
			{
				path = Path.GetDirectoryName(path);
			}
			break;
		}

		return path;
	}
	
	[MenuItem ("Assets/ExtractPalette")]
	public static RBPaletteGroup ExtractPalleteFromTexture ()
	{
		Texture2D selectedTexture = (Texture2D) Selection.activeObject;
		string selectionPath = GetPathOfSelection ();
		string filename = selectedTexture.name + suffix + ".asset";

		// Check for existing file and warn for overwrite
		bool writeFile = false;
		if (File.Exists (selectionPath + "/" + filename)) {
			if (EditorUtility.DisplayDialog ("Warning!", 
			                                "This will overwrite the existing file, " + filename + 
			                                 ". Are you sure you want to extract the palette?", "Yes", "No")) {
				writeFile = true;
			}
		} else {
			writeFile = true;
		}

		// Extract and write the texture
		if (writeFile) {
			RBPalette paletteFromTexture = RBPalette.CreatePaletteFromTexture (selectedTexture);
			RBPaletteGroup paletteGroup = RBPaletteGroup.CreateInstance (paletteFromTexture);
			return SaveRBPalette (paletteGroup, selectionPath, filename);
		}

		return null;
	}
	
	[MenuItem ("Assets/ExtractPalette", true)]
	public static bool IsValidTargetForPalette ()
	{
		if (Selection.activeObject == null) {
			return false;
		}

		// TODO: Support multi-texture palette group extraction
		if (Selection.objects.Length > 1) {
			return false;
		}

		return Selection.activeObject.GetType() == typeof(Texture2D);
	}
}

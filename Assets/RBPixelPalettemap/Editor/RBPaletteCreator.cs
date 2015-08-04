using UnityEngine;
using UnityEditor;
using System.IO;

public static class RBPaletteCreator {

	const string suffix = "_PaletteGroup";

	[MenuItem ("Assets/Create/RBPalette")]
	public static RBPaletteGroup CreateEmptyPaletteGroup ()
	{
		string unformattedFilename = "RBPaletteGroup{0}.asset";
		string formattedFilename = string.Empty;
		string path = GetPathOfSelection ();
		RBPaletteGroup createdGroup = null;
		for (int i = 0; i < 100; i++) {
			formattedFilename = string.Format (unformattedFilename, i);
			try {
				createdGroup = CreatePaletteGroup (path, formattedFilename, false);
				break;
			} catch (IOException){
				// Do nothing, move on to next index
			}
		}

		if (createdGroup == null) {
			Debug.LogError ("Failed to create file. Too many generic RBPaletteGroups exist in save location.");
		}
		return createdGroup;
	}

	public static RBPaletteGroup CreatePaletteGroup (string path, string filename, bool overwriteExisting)
	{
		ValidateSaveLocation (path + filename, overwriteExisting);

		RBPaletteGroup paletteAsset = RBPaletteGroup.CreateInstance ();
		return SaveRBPalette (paletteAsset, path, filename);
	}

	public static RBPaletteGroup CreatePaletteGroup (string path, string filename, Texture2D sourceTexture, bool overwriteExisting)
	{
		ValidateSaveLocation (path + filename, overwriteExisting);

		// Create a base palette from the Texture
		RBPalette paletteFromTexture = RBPalette.CreatePaletteFromTexture (sourceTexture);
		paletteFromTexture.PaletteName = "Base Palette";

		// Create the paletteGroup with the base Palette
		RBPaletteGroup paletteGroup = RBPaletteGroup.CreateInstance (paletteFromTexture);
		paletteGroup.GroupName = sourceTexture.name + suffix;

		return SaveRBPalette (paletteGroup, path, filename);
	}
	
	static RBPaletteGroup SaveRBPalette (RBPaletteGroup palette, string path, string filename)
	{
		string fullpath = path + filename;

		AssetDatabase.CreateAsset (palette, fullpath);
		AssetDatabase.SaveAssets ();
		
		EditorUtility.FocusProjectWindow ();
		Selection.activeObject = palette;
		
		return palette;
	}
	
	static void ValidateSaveLocation (string fullPathToFile, bool allowOverwrite)
	{
		if (!allowOverwrite && File.Exists (fullPathToFile)) {
			throw new IOException ("File exists at save location: " + fullPathToFile);
		}
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
		path += "/";
		return path;
	}
	
	[MenuItem ("Assets/ExtractPalette")]
	public static RBPaletteGroup ExtractPaletteFromSelection ()
	{
		Texture2D selectedTexture = (Texture2D) Selection.activeObject;
		string selectionPath = GetPathOfSelection ();

		return ExtractPaletteFromTexture (selectedTexture, selectionPath);
	}

	public static RBPaletteGroup ExtractPaletteFromTexture (Texture2D extractTexture, string savePath, string filename = "")
	{
		if (string.IsNullOrEmpty (filename)) {
			filename = extractTexture.name + suffix + ".asset";
		}

		RBPaletteGroup createdGroup = null;
		try {
			createdGroup = CreatePaletteGroup (savePath, filename, extractTexture, false);
		} catch (IOException) {
			if (EditorUtility.DisplayDialog ("Warning!", 
			                                 "This will overwrite the existing file, " + filename + 
			                                 ". Are you sure you want to extract the palette?", "Yes", "No")) {
				createdGroup = CreatePaletteGroup (savePath, filename, extractTexture, true);
			}
		}

		return createdGroup;
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

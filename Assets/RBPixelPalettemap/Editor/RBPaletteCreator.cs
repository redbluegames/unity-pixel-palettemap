using UnityEngine;
using UnityEditor;
using System.IO;

public static class RBPaletteCreator {

	const string suffix = "_PaletteGroup";

	[MenuItem ("Assets/Create/RBPaletteMap/RBPalette")]
	static RBPaletteGroup CreatePaletteGroup ()
	{
		string unformattedFilename = "RBPaletteGroup{0}.asset";
		string formattedFilename = string.Empty;
		string path = AssetDatabaseUtility.GetDirectoryOfSelection ();
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
			throw new IOException ("Failed to create file. Too many generic RBPaletteGroups exist in save location.");
		}

		// Unlock standalone palette groups by default.
		createdGroup.Locked = false;

		AssetDatabaseUtility.SelectObject (createdGroup);

		return createdGroup;
	}

	public static RBPaletteGroup CreatePaletteGroup (string path, string filename, bool overwriteExisting)
	{
		ValidateSaveLocation (path + filename, overwriteExisting);

		RBPaletteGroup paletteAsset = RBPaletteGroup.CreateInstance ();
		return (RBPaletteGroup) AssetDatabaseUtility.SaveObject (paletteAsset, path, filename);
	}

	public static RBPaletteGroup CreatePaletteGroup (string path, string filename, Texture2D sourceTexture, bool overwriteExisting)
	{
		ValidateSaveLocation (path + filename, overwriteExisting);

		// Create a base palette from the Texture
		RBPalette paletteFromTexture = RBPalette.CreatePaletteFromTexture (sourceTexture);
		paletteFromTexture.PaletteName = "Base Palette";

		// Create the paletteGroup with the base Palette
		RBPaletteGroup paletteGroup = RBPaletteGroup.CreateInstance (sourceTexture.name + suffix, paletteFromTexture);

		return (RBPaletteGroup) AssetDatabaseUtility.SaveObject (paletteGroup, path, filename);
	}
	
	static void ValidateSaveLocation (string fullPathToFile, bool allowOverwrite)
	{
		if (!allowOverwrite && File.Exists (fullPathToFile)) {
			throw new IOException ("File exists at save location: " + fullPathToFile);
		}
	} 
	
	[MenuItem ("Assets/RBPaletteMap/ExtractPalette")]
	public static RBPaletteGroup ExtractPaletteFromSelection ()
	{
		Texture2D selectedTexture = (Texture2D) Selection.activeObject;
		string selectionPath = AssetDatabaseUtility.GetDirectoryOfSelection ();

		RBPaletteGroup extractedPaletteGroup = ExtractPaletteFromTexture (selectedTexture, selectionPath);
		AssetDatabaseUtility.SelectObject (extractedPaletteGroup);

		return extractedPaletteGroup;
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
	
	[MenuItem ("Assets/RBPaletteMap/ExtractPalette", true)]
	static bool IsSelectionValidTargetForPalette ()
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

using UnityEngine;
using UnityEditor;
using System.IO;

public static class RBPaletteCreator {

	const string suffix = "_PaletteGroup";

	[MenuItem ("Assets/Create/RBPaletteMap/RBPalette")]
	static RBPaletteGroup CreatePaletteGroup ()
	{
		string path = AssetDatabaseUtility.GetDirectoryOfSelection ();
		string filename = "RBPaletteGroup.asset";
		string uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath (path + filename);
		RBPaletteGroup createdGroup = null;
		try {
			createdGroup = CreatePaletteGroup (path, Path.GetFileName(uniqueAssetPath), false);
		} catch (IOException e){
			throw new IOException ("Failed to create file. Encountered exception: " + e);
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

	public static RBPaletteGroup CreatePaletteGroupFromTexture (string path, string filename, Texture2D sourceTexture, bool overwriteExisting)
	{
		ValidateSaveLocation (path + filename, overwriteExisting);
		
		// Create a base palette from the Texture
		RBPalette paletteFromTexture = RBPalette.CreatePaletteFromTexture (sourceTexture);
		paletteFromTexture.PaletteName = "Base Palette";
		
		// Create the paletteGroup with the base Palette
		RBPaletteGroup paletteGroup = RBPaletteGroup.CreateInstance (sourceTexture.name + suffix, paletteFromTexture);
		
		return (RBPaletteGroup) AssetDatabaseUtility.SaveObject (paletteGroup, path, filename);
	}
	
	public static RBPaletteGroup CreatePaletteGroupFromPaletteTexture (string path, string filename, Texture2D sourceTexture, bool overwriteExisting)
	{
		ValidateSaveLocation (path + filename, overwriteExisting);

		RBPaletteGroup paletteGroup = RBPaletteGroup.CreateInstanceFromPaletteTexture (sourceTexture, sourceTexture.name + suffix);
		
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
			createdGroup = CreatePaletteGroupFromTexture (savePath, filename, extractTexture, false);
		} catch (IOException) {
			if (EditorUtility.DisplayDialog ("Warning!", 
			                                 "This will overwrite the existing file, " + filename + 
			                                 ". Are you sure you want to extract the palette?", "Yes", "No")) {
				createdGroup = CreatePaletteGroupFromTexture (savePath, filename, extractTexture, true);
			}
		}

		return createdGroup;
	}
	
	
	[MenuItem ("Assets/RBPaletteMap/Palette To Palette Group")]
	public static RBPaletteGroup ExtractPaletteGroupFromPaletteTexture ()
	{
		Texture2D selectedTexture = (Texture2D) Selection.activeObject;
		string selectionPath = AssetDatabaseUtility.GetDirectoryOfSelection ();
		
		RBPaletteGroup extractedPaletteGroup = ExtractPaletteGroupFromPaletteTexture (selectedTexture, selectionPath);
		AssetDatabaseUtility.SelectObject (extractedPaletteGroup);
		
		return extractedPaletteGroup;
	}

	public static RBPaletteGroup ExtractPaletteGroupFromPaletteTexture (Texture2D extractTexture, string savePath, string filename = "")
	{
		if (string.IsNullOrEmpty (filename)) {
			filename = extractTexture.name + suffix + ".asset";
		}
		
		RBPaletteGroup createdGroup = null;
		try {
			createdGroup = CreatePaletteGroupFromPaletteTexture (savePath, filename, extractTexture, false);
		} catch (IOException) {
			if (EditorUtility.DisplayDialog ("Warning!", 
			                                 "This will overwrite the existing file, " + filename + 
			                                 ". Are you sure you want to extract the palette group?", "Yes", "No")) {
				createdGroup = CreatePaletteGroupFromPaletteTexture (savePath, filename, extractTexture, true);
			}
		}
		
		return createdGroup;
	}

	
	[MenuItem ("Assets/RBPaletteMap/ExtractPalette", true)]
	static bool IsSelectionValidTargetForPalette ()
	{
		if (!IsSelectionValidTexture ()) {
			return false;
		}

		// TODO: Support multi-texture palette group extraction
		if (Selection.objects.Length > 1) {
			return false;
		}
		return true;
	}
	
	static bool IsSelectionValidTexture ()
	{
		if (Selection.activeObject == null) {
			return false;
		}
		
		return Selection.activeObject.GetType() == typeof(Texture2D);
	}
}

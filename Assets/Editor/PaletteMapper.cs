using UnityEngine;
using UnityEditor;
using System.Collections;

class MyWindow : EditorWindow
{
	Object inSourceTexture = null;
	bool overwriteExistingFiles = true;

	[MenuItem ("RedBlueTools/PaletteMapper")]
	public static void  ShowWindow ()
	{
		EditorWindow.GetWindow (typeof(MyWindow));
	}
	
	void OnGUI ()
	{
		GUILayout.Label ("Palette Mapper", EditorStyles.boldLabel);

		inSourceTexture = EditorGUILayout.ObjectField ("Texture", inSourceTexture, typeof(Texture2D), false);
		overwriteExistingFiles = EditorGUILayout.Toggle("Overwite Existing files", overwriteExistingFiles);

		if (GUILayout.Button ("Build")) {
			if (inSourceTexture == null) {
				Debug.LogError ("PaletteMapper Error: No source texture specified");
				return;
			}

			try {
				Texture2D inTexture = (Texture2D)inSourceTexture;
				ValidateSourceTexture (inTexture);
				try {
					string path = GetPathToAsset(inTexture);
					try {
						WritePaletteMapTextureToDisk (path, inTexture);
					} catch (System.Exception e) {
						Debug.LogError("PaletteMap Error: Encountered file error when trying to write PaletteMap: " 
						               + e.Message);
					}
				} catch (System.Exception e) {
					Debug.LogError (e.Message);
				}
			} catch (System.Exception e) {
				Debug.LogError (e.Message);
			}
		}
	}

	void ValidateSourceTexture (Texture2D sourceTexture)
	{
		if (sourceTexture.filterMode != FilterMode.Point) {
			throw new System.ArgumentException ("PaletteMapper Error: Source texture must have Point filter mode.");
		}

		if (sourceTexture.format != TextureFormat.RGBA32) {
			throw new System.ArgumentException ("PaletteMapper Error: Source texture must be uncompressed (RGBA32)");
		}
	}

	string GetPathToAsset(Object asset)
	{
		string path = AssetDatabase.GetAssetPath(asset);

		// Strip filename out from asset path
		string[] directories = path.Split('/');
		path = path.TrimEnd(directories[directories.Length -1].ToCharArray());

		return path;
	}

	void WritePaletteMapTextureToDisk (string outputPath, Texture2D inTexture)
	{
		// Create full path to output file
		string paletteMapSuffix = "_PaletteMap.png";
		string filename = inTexture.name + paletteMapSuffix;
		string fullPathToOutputFile = outputPath + filename;

		// Handle file overwriting
		if(System.IO.File.Exists(fullPathToOutputFile)) {
			if(!overwriteExistingFiles) {
				Debug.LogError("PaletteMap Error: Tried to write " + filename + " but file already exists. " +
				               "\nFile Path: " + outputPath);
				return;
			}
			Debug.LogWarning("PaletteMap: Overwriting file " + filename);
		}

		// Create the PaletteMap texture
		Texture2D outTexture = new Texture2D (inTexture.width, inTexture.height, TextureFormat.Alpha8, false);
		outTexture.hideFlags = HideFlags.HideAndDontSave;
		byte[] outTextureData = outTexture.EncodeToPNG ();

		// Write the PaletteMap to disk
		try {
			System.IO.File.WriteAllBytes (fullPathToOutputFile, outTextureData);

			// Assign correct settings to the file
			TextureImporter textureImporter = AssetImporter.GetAtPath(fullPathToOutputFile) as TextureImporter; 
			textureImporter.filterMode = FilterMode.Point;
			textureImporter.textureFormat = TextureImporterFormat.Alpha8;
			textureImporter.alphaIsTransparency = true;
			textureImporter.npotScale = TextureImporterNPOTScale.None;
			
			// Force Unity to see the file and use the new import settings
			AssetDatabase.ImportAsset(fullPathToOutputFile); 

			Debug.Log ("<color=green>Palette Map " + filename + " created successfully</color>");
		} catch {
			throw;
		}
	}
}
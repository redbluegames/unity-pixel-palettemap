using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public static class PaletteMapper {

	public static void ValidateSourceTexture (Texture2D sourceTexture)
	{
		TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sourceTexture)) as TextureImporter;
		if(!textureImporter.isReadable) {
			throw new System.BadImageFormatException("Source texture must be Read/Write enabled.");
		}
		
		if (sourceTexture.filterMode != FilterMode.Point) {
			throw new System.BadImageFormatException ("Source texture must have Point filter mode.");
		}
		
		if (sourceTexture.format != TextureFormat.RGBA32) {
			throw new System.BadImageFormatException ("Source texture must be uncompressed (RGBA32)");
		}
	}

	public static void CreateAndSavePaletteMap (string outputPath, Texture2D inTexture, bool overwriteExistingFiles)
	{
		// Create full path to output file
		string paletteMapSuffix = "_PaletteMap.png";
		string filename = inTexture.name + paletteMapSuffix;
		string fullPathToOutputFile = outputPath + filename;
		
		// Handle file overwriting
		if(File.Exists(fullPathToOutputFile)) {
			if(!overwriteExistingFiles) {
				throw new System.AccessViolationException ("Tried to write " + filename + " but file already exists. " +
				                                           "\nFile Path: " + outputPath);
			}
			Debug.LogWarning("PaletteMap: Overwriting file " + filename);
		}
		
		// Create the PaletteMap texture
		Texture2D outTexture = CreatePaletteMapFromSource(inTexture);
		
		// Write the PaletteMap to disk
		try {
			WritePaletteMapFile(fullPathToOutputFile, outTexture);
		} catch {
			throw;
		}
	}
	
	static Texture2D CreatePaletteMapFromSource(Texture2D sourceTexture)
	{
		Texture2D outTexture = new Texture2D (sourceTexture.width, sourceTexture.height, TextureFormat.Alpha8, false);
		outTexture.hideFlags = HideFlags.HideAndDontSave;
		
		Color[] sourcePixels = sourceTexture.GetPixels ();
		List<Color> uniqueColorsInSource = new List<Color> ();
		
		// Get all unique colors
		for(int i = 0; i < sourcePixels.Length; i++) {
			if(!uniqueColorsInSource.Contains(sourcePixels[i])) {
				uniqueColorsInSource.Add (sourcePixels[i]);
			}
		}
		
		// Remap original colors to point to indeces in the palette
		Color[] paletteMapPixels = new Color[sourcePixels.Length];
		for(int i = 0; i < sourcePixels.Length; i++) {
			int paletteIndex = uniqueColorsInSource.IndexOf(sourcePixels[i]);
			float alpha;
			if(uniqueColorsInSource.Count == 1) {
				alpha = 0.0f;
			} else {
				alpha = paletteIndex / (float)(uniqueColorsInSource.Count - 1);
			}
			paletteMapPixels[i] = new Color(0.0f, 0.0f, 0.0f, alpha);
		}
		
		outTexture.SetPixels(paletteMapPixels);
		outTexture.Apply ();
		
		return outTexture;
	}

	static void WritePaletteMapFile (string fullPath, Texture2D texture)
	{
		try {
			byte[] outTextureData = texture.EncodeToPNG ();
			File.WriteAllBytes (fullPath, outTextureData);
		} catch {
			throw;
		}

		// Assign correct settings to the file
		TextureImporter textureImporter = AssetImporter.GetAtPath(fullPath) as TextureImporter; 
		textureImporter.filterMode = FilterMode.Point;
		textureImporter.textureFormat = TextureImporterFormat.Alpha8;
		textureImporter.alphaIsTransparency = true;
		textureImporter.mipmapEnabled = false;
		textureImporter.npotScale = TextureImporterNPOTScale.None;
		
		// Force Unity to see the file and use the new import settings
		AssetDatabase.ImportAsset(fullPath); 
	}
}

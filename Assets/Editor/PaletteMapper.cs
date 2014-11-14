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

	public static void CreatePaletteMapAndKey(string outputPath, Texture2D sourceTexture, bool overwriteExistingFiles)
	{
		CreateEverything(outputPath, sourceTexture);
		//CreatePaletteKeyFromTexture(outputPath, sourceTexture, overwriteExistingFiles);
		//CreateAndSavePaletteMap (outputPath, sourceTexture, overwriteExistingFiles);
	}

	static void CreateEverything(string outputPath, Texture2D sourceTexture)
	{
		PaletteKey paletteKey = CreatePaletteKeyForTexture(sourceTexture);
		Texture2D palettemap = CreatePaletteMapWithKey(sourceTexture, paletteKey);
		
		string paletteKeySuffix = "_PaletteKey.png";
		string filename = sourceTexture.name + paletteKeySuffix;
		string fullPathToOutputFile = outputPath + filename;
		paletteKey.WriteToFile(fullPathToOutputFile);

		string paletteMapSuffix = "_PaletteMap.png";
		string palettemapfilename = sourceTexture.name + paletteMapSuffix;
		string fullPathToPaletteMapFile = outputPath + palettemapfilename;
		WritePaletteMapFile(fullPathToPaletteMapFile, palettemap);
	}
	
	static Texture2D CreatePaletteMapWithKey (Texture2D sourceTexture, PaletteKey paletteKey)
	{
		Color[] sourcePixels = sourceTexture.GetPixels ();

		// Remap original colors to point to indeces in the palette
		Color[] paletteMapPixels = new Color[sourcePixels.Length];
		for(int i = 0; i < sourcePixels.Length; i++) {
			int paletteIndex = paletteKey.IndexOf(sourcePixels[i]);
			float alpha;
			if(paletteKey.Count == 1) {
				alpha = 0.0f;
			} else {
				alpha = paletteIndex / (float)(paletteKey.Count - 1);
				// For some reason, 1.0f wraps around in the shader. Maybe it's epsilon issues.
				alpha = Mathf.Clamp(alpha, 0.0f, 0.99f);
			}
			paletteMapPixels[i] = new Color(0.0f, 0.0f, 0.0f, alpha);
		}
		
		Texture2D paletteMapAsTexture = new Texture2D (sourceTexture.width, sourceTexture.height, TextureFormat.Alpha8, false);
		paletteMapAsTexture.hideFlags = HideFlags.HideAndDontSave;
		paletteMapAsTexture.SetPixels(paletteMapPixels);
		paletteMapAsTexture.Apply ();
		
		return paletteMapAsTexture;
	}

	static void CreatePaletteKeyFromTexture(string outputPath, Texture2D sourceTexture, bool overwriteExistingFiles)
	{
		// Create full path to output file
		string paletteKeySuffix = "_PaletteKey.png";
		string filename = sourceTexture.name + paletteKeySuffix;
		string fullPathToOutputFile = outputPath + filename;

		// Handle file overwriting
		try {
			AssertForOverwriteAccess(fullPathToOutputFile, filename, overwriteExistingFiles);
		} catch {
			throw;
		}

		PaletteKey paletteKey = CreatePaletteKeyForTexture(sourceTexture);
		try {
			paletteKey.WriteToFile(fullPathToOutputFile);
		} catch {
			throw;
		}
	}

	static PaletteKey CreatePaletteKeyForTexture (Texture2D sourceTexture)
	{
		Color[] sourcePixels = sourceTexture.GetPixels ();
		PaletteKey paletteKey = new PaletteKey();
		
		// Get all unique colors
		for(int i = 0; i < sourcePixels.Length; i++) {
			if(!paletteKey.ContainsColor(sourcePixels[i])) {
				paletteKey.AddColor (sourcePixels[i]);
			}
		}

		return paletteKey;
	}

	static void WritePaletteKeyFile (string fullPath, Texture2D texture)
	{
		try {
			byte[] outTextureData = texture.EncodeToPNG ();
			File.WriteAllBytes (fullPath, outTextureData);
		} catch {
			throw;
		}

		// Force refresh so that we can import it immediately
		AssetDatabase.ImportAsset(fullPath); 
		
		// Assign correct settings to the file
		TextureImporter textureImporter = AssetImporter.GetAtPath(fullPath) as TextureImporter; 
		textureImporter.filterMode = FilterMode.Point;
		textureImporter.textureFormat = TextureImporterFormat.RGBA32;
		textureImporter.alphaIsTransparency = true;
		textureImporter.mipmapEnabled = false;
		textureImporter.npotScale = TextureImporterNPOTScale.None;
		
		// Force Unity to see the file and use the new import settings
		AssetDatabase.ImportAsset(fullPath); 
	}


	static void CreateAndSavePaletteMap (string outputPath, Texture2D inTexture, bool overwriteExistingFiles)
	{
		// Create full path to output file
		string paletteMapSuffix = "_PaletteMap.png";
		string filename = inTexture.name + paletteMapSuffix;
		string fullPathToOutputFile = outputPath + filename;
		
		// Handle file overwriting
		try {
			AssertForOverwriteAccess(fullPathToOutputFile, filename, overwriteExistingFiles);
		} catch {
			throw;
		}
		
		// Create the PaletteMap texture
		Texture2D paletteMap = CreatePaletteMap(inTexture);

		try {
			WritePaletteMapFile(fullPathToOutputFile, paletteMap);
		} catch {
			throw;
		}
	}

	static Texture2D CreatePaletteMap (Texture2D sourceTexture)
	{
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
				// For some reason, 1.0f wraps around in the shader. Maybe it's epsilon issues.
				alpha = Mathf.Clamp(alpha, 0.0f, 0.99f);
			}
			paletteMapPixels[i] = new Color(0.0f, 0.0f, 0.0f, alpha);
		}

		Texture2D paletteMapAsTexture = new Texture2D (sourceTexture.width, sourceTexture.height, TextureFormat.Alpha8, false);
		paletteMapAsTexture.hideFlags = HideFlags.HideAndDontSave;
		paletteMapAsTexture.SetPixels(paletteMapPixels);
		paletteMapAsTexture.Apply ();

		return paletteMapAsTexture;
	}

	static void WritePaletteMapFile (string fullPath, Texture2D texture)
	{
		try {
			byte[] outTextureData = texture.EncodeToPNG ();
			File.WriteAllBytes (fullPath, outTextureData);
		} catch {
			throw;
		}

		// Force refresh so that we can import it immediately
		AssetDatabase.ImportAsset(fullPath); 

		// Assign correct settings to the file
		TextureImporter textureImporter = AssetImporter.GetAtPath(fullPath) as TextureImporter; 
		textureImporter.filterMode = FilterMode.Point;
		textureImporter.textureFormat = TextureImporterFormat.Alpha8;
		textureImporter.alphaIsTransparency = false;
		textureImporter.mipmapEnabled = false;
		textureImporter.npotScale = TextureImporterNPOTScale.None;
		
		// Force Unity to see the file and use the new import settings
		AssetDatabase.ImportAsset(fullPath); 
	}

	static void AssertForOverwriteAccess(string path, string filename, bool allowOverwriting)
	{
		if(File.Exists(path)) {
			if(!allowOverwriting) {
				throw new System.AccessViolationException ("Tried to write " + filename + " but file already exists. " +
				                                           "\nFile Path: " + path);
			}
			Debug.LogWarning("PaletteMap: Overwriting file " + filename);
		}
	}
	
	class PaletteKey
	{
		string filename = "";
		const string suffix = "_PaletteKey";

		List<Color> colorsInPalette;
		public int Count {
			get {
				return colorsInPalette.Count;
			}
		}
		
		public PaletteKey()
		{
			colorsInPalette = new List<Color> ();
		}
		
		public PaletteKey(List<Color> colorsInPalette)
		{
			this.colorsInPalette = new List<Color> (colorsInPalette);
		}
		
		public void AddColor(Color color)
		{
			colorsInPalette.Add(color);
		}
		
		public bool ContainsColor(Color colorToFind)
		{
			return colorsInPalette.Contains(colorToFind);
		}

		public int IndexOf(Color colorInPalette)
		{
			return colorsInPalette.IndexOf(colorInPalette);
		}
		
		Texture2D CreateTextureFromKey ()
		{
			// Write the colors into a texture
			Texture2D paletteKeyAsTexture = new Texture2D (colorsInPalette.Count, 1, TextureFormat.RGBA32, false);
			paletteKeyAsTexture.hideFlags = HideFlags.HideAndDontSave;
			paletteKeyAsTexture.SetPixels(colorsInPalette.ToArray());
			paletteKeyAsTexture.Apply ();
			
			return paletteKeyAsTexture;
		}
		
		public void WriteToFile(string fullPathToFile)
		{
			Texture2D keyAsTexture = CreateTextureFromKey ();
			try {
				byte[] outTextureData = keyAsTexture.EncodeToPNG ();
				File.WriteAllBytes (fullPathToFile, outTextureData);
			} catch {
				throw;
			}
			
			// Force refresh so that we can import it immediately
			AssetDatabase.ImportAsset(fullPathToFile); 
			
			// Assign correct settings to the file
			TextureImporter textureImporter = AssetImporter.GetAtPath(fullPathToFile) as TextureImporter; 
			textureImporter.filterMode = FilterMode.Point;
			textureImporter.textureFormat = TextureImporterFormat.RGBA32;
			textureImporter.alphaIsTransparency = true;
			textureImporter.mipmapEnabled = false;
			textureImporter.npotScale = TextureImporterNPOTScale.None;
			
			// Force Unity to see the file and use the new import settings
			AssetDatabase.ImportAsset(fullPathToFile); 
		}
	}

}

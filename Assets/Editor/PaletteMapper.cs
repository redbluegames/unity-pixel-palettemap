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
		PaletteKey paletteKey = PaletteKey.CreatePaletteKeyFromTexture(sourceTexture);
		paletteKey.SortByLumosity ();
		PaletteMap palettemap = new PaletteMap(sourceTexture, paletteKey);
		
		string paletteKeySuffix = "_PaletteKey.png";
		string paletteKeyFilename = sourceTexture.name + paletteKeySuffix;
		string fullPathToPaletteKeyFile = outputPath + paletteKeyFilename;
		paletteKey.WriteToFile(fullPathToPaletteKeyFile, overwriteExistingFiles);
		
		string paletteMapSuffix = "_PaletteMap.png";
		string paletteMapFilename = sourceTexture.name + paletteMapSuffix;
		string fullPathToPaletteMapFile = outputPath + paletteMapFilename;
		palettemap.WriteToFile(fullPathToPaletteMapFile, overwriteExistingFiles);
	}

	class PaletteKey
	{
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
		
		Texture2D CreateAsTexture ()
		{
			// Write the colors into a texture
			Texture2D paletteKeyAsTexture = new Texture2D (colorsInPalette.Count, 1, TextureFormat.RGBA32, false);
			paletteKeyAsTexture.hideFlags = HideFlags.HideAndDontSave;
			paletteKeyAsTexture.SetPixels(colorsInPalette.ToArray());
			paletteKeyAsTexture.Apply ();
			
			return paletteKeyAsTexture;
		}
		
		public void WriteToFile(string fullPathToFile, bool allowOverwriting)
		{
			if(File.Exists(fullPathToFile) && !allowOverwriting) {
				throw new System.AccessViolationException ("Tried to write PaletteKey but file already exists. " +
				                                           "\nFile Path: " + fullPathToFile);
			}

			Texture2D keyAsTexture = CreateAsTexture ();
			try {
				byte[] outTextureData = keyAsTexture.EncodeToPNG ();
				File.WriteAllBytes (fullPathToFile, outTextureData);
			} catch {
				throw;
			}
			
			// Force refresh so that we can set its Import settings immediately
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

		public static PaletteKey CreatePaletteKeyFromTexture (Texture2D sourceTexture)
		{
			Color[] sourcePixels = sourceTexture.GetPixels ();
			PaletteKey paletteKey = new PaletteKey();
			
			// Get all unique colors
			for(int i = 0; i < sourcePixels.Length; i++) {
				Color colorAtSource = sourcePixels[i];
				if(Mathf.Approximately(colorAtSource.a, 0.0f)) {
					// Only store full alpha in the palette
					colorAtSource.r = 0.0f;
					colorAtSource.g = 0.0f;
					colorAtSource.b = 0.0f;
				}
				if(!paletteKey.ContainsColor(colorAtSource)) {
					paletteKey.AddColor (colorAtSource);
				}
			}
			
			return paletteKey;
		}

		public void SortByLumosity()
		{
			colorsInPalette.Sort(CompareColorsByGrayscale);
		}

		// Returns the "smaller" of the two colors by grayscale
		static int CompareColorsByGrayscale (Color colorA, Color colorB)
		{
			// When one is alpha and the other isn't, the alpha'ed color is smaller
			if(colorA.a < 1.0f && Mathf.Approximately(colorB.a, 1.0f)) {
				return -1;
			} else if(colorB.a < 1.0f && Mathf.Approximately(colorA.a, 1.0f)) {
				return 1;
			}

			if(colorA.grayscale < colorB.grayscale) {
				return -1;
			} else if(colorA.grayscale > colorB.grayscale) {
				return 1;
			} else {
				// Colors are equal - decide ties by alpha (usually happens with black)
				if(colorA.a < colorB.a) {
					return -1;
				} else {
					return 1;
				}
			}
		}
	}
	
	class PaletteMap 
	{
		Color[] pixels;
		int width;
		int height;
		
		public PaletteMap(Texture2D sourceTexture, PaletteKey paletteKey)
		{
			this.width = sourceTexture.width;
			this.height = sourceTexture.height;
			
			Color[] sourcePixels = sourceTexture.GetPixels ();
			pixels = new Color[sourcePixels.Length];
			
			// Remap original colors to point to indeces in the palette
			for(int i = 0; i < sourcePixels.Length; i++) {
				// Get the alpha value by looking it up in the paletteKey
				int paletteIndex = paletteKey.IndexOf(sourcePixels[i]);
				float alpha;
				if(paletteKey.Count == 1) {
					alpha = 0.0f;
				} else {
					alpha = paletteIndex / (float)(paletteKey.Count - 1);
					// For some reason, 1.0f wraps around in the shader. Maybe it's epsilon issues.
					alpha = Mathf.Clamp(alpha, 0.0f, 0.99f);
				}
				pixels[i] = new Color(0.0f, 0.0f, 0.0f, alpha);
			}
		}
		
		Texture2D CreateAsTexture()
		{
			Texture2D paletteMapAsTexture = new Texture2D (width, height, TextureFormat.Alpha8, false);
			paletteMapAsTexture.hideFlags = HideFlags.HideAndDontSave;
			paletteMapAsTexture.SetPixels(pixels);
			paletteMapAsTexture.Apply ();
			
			return paletteMapAsTexture;
		}
		
		public void WriteToFile(string fullPath, bool allowOverwriting)
		{
			if(File.Exists(fullPath) && !allowOverwriting) {
				throw new System.AccessViolationException ("Tried to write PaletteKey but file already exists. " +
				                                           "\nFile Path: " + fullPath);
			}

			Texture2D texture = CreateAsTexture ();
			try {
				byte[] outTextureData = texture.EncodeToPNG ();
				File.WriteAllBytes (fullPath, outTextureData);
			} catch {
				throw;
			}
			
			// Force refresh so that we can set its Import settings immediately
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
	}
}

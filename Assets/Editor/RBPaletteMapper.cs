/*****************************************************************************
 *  Palette Mapper is a Red Blue Tool used to create indexed color images from
 *  source images.
 *  Copyright (C) 2014 Red Blue Games, LLC
 *  
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 ****************************************************************************/
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace RedBlueTools
{
	public static class RBPaletteMapper
	{

		public static void ValidateSourceTexture (Texture2D sourceTexture)
		{
			TextureImporter textureImporter = AssetImporter.GetAtPath (AssetDatabase.GetAssetPath (sourceTexture)) as TextureImporter;
			if (!textureImporter.isReadable) {
				throw new System.BadImageFormatException ("Source texture must be Read/Write enabled.");
			}
		
			if (sourceTexture.filterMode != FilterMode.Point) {
				throw new System.BadImageFormatException ("Source texture must have Point filter mode.");
			}
		
			if (sourceTexture.format != TextureFormat.RGBA32) {
				throw new System.BadImageFormatException ("Source texture must be uncompressed (RGBA32)");
			}
		}
	
		public static void ValidatePaletteKeyTexture (Texture2D paletteKeyTexture)
		{
			TextureImporter textureImporter = AssetImporter.GetAtPath (AssetDatabase.GetAssetPath (paletteKeyTexture)) as TextureImporter;
			if (!textureImporter.isReadable) {
				throw new System.BadImageFormatException ("PaletteKey texture must be Read/Write enabled.");
			}
		
			if (paletteKeyTexture.filterMode != FilterMode.Point) {
				throw new System.BadImageFormatException ("PaletteKey texture must have Point filter mode.");
			}
		
			if (paletteKeyTexture.format != TextureFormat.RGBA32) {
				throw new System.BadImageFormatException ("PaletteKey texture must be uncompressed (RGBA32)");
			}
		}

		public static void CreatePaletteMapAndKey (string outputPath, Texture2D sourceTexture, Texture2D paletteKeyTexture, 
		                                           bool sortPaletteKey, bool overwriteExistingFiles, string paletteKeyFileName, string paletteMapFilename)
		{
			// If no palette key texture is provided, create a new one from the source image
			PaletteKey paletteKey;
			if (paletteKeyTexture == null) {
				paletteKey = PaletteKey.CreatePaletteKeyFromTexture (sourceTexture);
				if (sortPaletteKey) {
					paletteKey.SortByGrayscale ();
				}
			} else {
				paletteKey = PaletteKey.CreatePaletteKeyFromTexture (paletteKeyTexture);
			}

			// Create the palette map from the palette key
			PaletteMap palettemap = new PaletteMap (sourceTexture, paletteKey);

			// Write PaletteKey to disk
			string paletteKeyFilenameWithExtension = paletteKeyFileName + ".png";
			string fullPathToPaletteKeyFile = outputPath + paletteKeyFilenameWithExtension;
			paletteKey.WriteToFile (fullPathToPaletteKeyFile, overwriteExistingFiles);
		
			// Write PaletteMap to disk
			string paletteMapFilenameWithExtension = paletteMapFilename + ".png";
			string fullPathToPaletteMapFile = outputPath + paletteMapFilenameWithExtension;
			palettemap.WriteToFile (fullPathToPaletteMapFile, overwriteExistingFiles);
		}

		class PaletteKey
		{
			List<Color> colorsInPalette;

			public int Count {
				get {
					return colorsInPalette.Count;
				}
			}
		
			public PaletteKey ()
			{
				colorsInPalette = new List<Color> ();
			}
		
			public PaletteKey (List<Color> colorsInPalette)
			{
				this.colorsInPalette = new List<Color> (colorsInPalette);
			}
		
			public void AddColor (Color color)
			{
				if (colorsInPalette.Count > byte.MaxValue) {
					throw new System.NotSupportedException ("Tried to Add more colors to palette key than are currently" +
						" supported due to PaletteMap's Alpha8 format.");
				}
				colorsInPalette.Add (color);
			}
		
			public bool ContainsColor (Color colorToFind)
			{
				return colorsInPalette.Contains (colorToFind);
			}

			public int IndexOf (Color colorInPalette)
			{
				Color colorToLookup = ClearRGBIfNoAlpha (colorInPalette);
				return colorsInPalette.IndexOf (colorToLookup);
			}
		
			public static PaletteKey CreatePaletteKeyFromTexture (Texture2D sourceTexture)
			{
				Color[] sourcePixels = sourceTexture.GetPixels ();
				PaletteKey paletteKey = new PaletteKey ();
			
				// Get all unique colors
				for (int i = 0; i < sourcePixels.Length; i++) {
					Color colorAtSource = ClearRGBIfNoAlpha (sourcePixels [i]);
					if (!paletteKey.ContainsColor (colorAtSource)) {
						paletteKey.AddColor (colorAtSource);
					}
				}
			
				return paletteKey;
			}

			// Clears out the RGB when fully transparent so that we don't get lots of versions of transparent in the palette
			static Color ClearRGBIfNoAlpha (Color colorToClear)
			{
				Color clearedColor = colorToClear;
				if (Mathf.Approximately (clearedColor.a, 0.0f)) {
					clearedColor = Color.clear;
				}
				return clearedColor;
			}
		
			Texture2D CreateAsTexture ()
			{
				// Write the colors into a texture
				Texture2D paletteKeyAsTexture = new Texture2D (colorsInPalette.Count, 1, TextureFormat.RGBA32, false);
				paletteKeyAsTexture.hideFlags = HideFlags.HideAndDontSave;
				paletteKeyAsTexture.SetPixels (colorsInPalette.ToArray ());
				paletteKeyAsTexture.Apply ();
			
				return paletteKeyAsTexture;
			}
		
			public void WriteToFile (string fullPathToFile, bool allowOverwriting)
			{
				if (File.Exists (fullPathToFile) && !allowOverwriting) {
					throw new System.AccessViolationException ("Tried to write PaletteKey but file already exists. " +
						"\nFile Path: " + fullPathToFile);
				}

				Texture2D keyAsTexture = CreateAsTexture ();
				try {
					byte[] outTextureData = keyAsTexture.EncodeToPNG ();
					File.WriteAllBytes (fullPathToFile, outTextureData);
				} catch (System.Exception e) {
					throw new System.IO.IOException ("Encountered IO exception during PaletteKey write: " + e.Message);
				}
			
				// Force refresh so that we can set its Import settings immediately
				AssetDatabase.ImportAsset (fullPathToFile); 
			
				// Assign correct settings to the file
				TextureImporter textureImporter = AssetImporter.GetAtPath (fullPathToFile) as TextureImporter;
				if(textureImporter == null) {
					throw new System.NullReferenceException ("Failed to import file at specified path: " + fullPathToFile);
				}
				textureImporter.filterMode = FilterMode.Point;
				textureImporter.textureFormat = TextureImporterFormat.RGBA32;
				textureImporter.alphaIsTransparency = false;
				textureImporter.mipmapEnabled = false;
				textureImporter.npotScale = TextureImporterNPOTScale.None;
				textureImporter.maxTextureSize = 256;
			
				// Force Unity to see the file and use the new import settings
				AssetDatabase.ImportAsset (fullPathToFile); 
			}

			public void SortByGrayscale ()
			{
				colorsInPalette.Sort (CompareColorsByGrayscale);
			}

			// Returns the "smaller" of the two colors by grayscale
			static int CompareColorsByGrayscale (Color colorA, Color colorB)
			{
				// When one is alpha and the other isn't, the alpha'ed color is smaller
				if (colorA.a < 1.0f && Mathf.Approximately (colorB.a, 1.0f)) {
					return -1;
				} else if (colorB.a < 1.0f && Mathf.Approximately (colorA.a, 1.0f)) {
					return 1;
				}

				if (colorA.grayscale < colorB.grayscale) {
					return -1;
				} else if (colorA.grayscale > colorB.grayscale) {
					return 1;
				} else {
					// Colors are equal - decide ties by alpha (usually happens with black)
					if (colorA.a < colorB.a) {
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
		
			public PaletteMap (Texture2D sourceTexture, PaletteKey paletteKey)
			{
				this.width = sourceTexture.width;
				this.height = sourceTexture.height;
			
				Color[] sourcePixels = sourceTexture.GetPixels ();
				pixels = new Color[sourcePixels.Length];
			
				// Remap original colors to point to indeces in the palette
				for (int i = 0; i < sourcePixels.Length; i++) {
					// Get the alpha value by looking it up in the paletteKey
					int paletteIndex = paletteKey.IndexOf (sourcePixels [i]);
					if (paletteIndex < 0) {
						throw new System.ArgumentException ("Encountered color in source PaletteMap image that is not in the PaletteKey." +
							"Color in PaletteMap: " + (Color32)sourcePixels [i]);
					}
					float alpha;
					if (paletteKey.Count == 1) {
						alpha = 0.0f;
					} else {
						alpha = paletteIndex / (float)(paletteKey.Count - 1);
						// For some reason, 1.0f wraps around in the shader. Maybe it's epsilon issues.
						alpha = Mathf.Clamp (alpha, 0.0f, 0.99f);
					}
					pixels [i] = new Color (0.0f, 0.0f, 0.0f, alpha);
				}
			}
		
			Texture2D CreateAsTexture ()
			{
				Texture2D paletteMapAsTexture = new Texture2D (width, height, TextureFormat.Alpha8, false);
				paletteMapAsTexture.hideFlags = HideFlags.HideAndDontSave;
				paletteMapAsTexture.SetPixels (pixels);
				paletteMapAsTexture.Apply ();
			
				return paletteMapAsTexture;
			}
		
			public void WriteToFile (string fullPath, bool allowOverwriting)
			{
				if (File.Exists (fullPath) && !allowOverwriting) {
					throw new System.AccessViolationException ("Tried to write PaletteKey but file already exists. " +
						"\nFile Path: " + fullPath);
				}

				Texture2D texture = CreateAsTexture ();
				try {
					byte[] outTextureData = texture.EncodeToPNG ();
					File.WriteAllBytes (fullPath, outTextureData);
				} catch (System.Exception e) {
					throw new System.IO.IOException ("Encountered IO exception during PaletteMap write: " + e.Message);
				}
			
				// Force refresh so that we can set its Import settings immediately
				AssetDatabase.ImportAsset (fullPath); 

				// Assign correct settings to the file
				TextureImporter textureImporter = AssetImporter.GetAtPath (fullPath) as TextureImporter;
				if(textureImporter == null) {
					throw new System.NullReferenceException ("Failed to import file at specified path: " + fullPath);
				}
				textureImporter.textureType = TextureImporterType.Advanced;
				textureImporter.npotScale = TextureImporterNPOTScale.None;
				textureImporter.alphaIsTransparency = false;
				textureImporter.mipmapEnabled = false;
				textureImporter.filterMode = FilterMode.Point;
				textureImporter.textureFormat = TextureImporterFormat.Alpha8;
			
				// Force Unity to see the file and use the new import settings
				AssetDatabase.ImportAsset (fullPath);
			}
		}
	}
}
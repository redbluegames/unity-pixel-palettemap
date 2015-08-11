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

		public static void ValidatePaletteGroup (RBPaletteGroup paletteGroup)
		{
			// Nothing to validate as of right now.
		}

		public static void CreatePaletteMapAndKey (string outputPath, Texture2D sourceTexture, RBPaletteGroup suppliedPaletteGroup, 
		                                           bool sortPaletteKey, bool overwriteExistingFiles, string paletteKeyFileName, string paletteMapFilename)
		{
			// If no palette key texture is provided, create a new one from the source image
			RBPalette basePalette = null;
			if (suppliedPaletteGroup == null) {
				basePalette = RBPalette.CreatePaletteFromTexture (sourceTexture);
				if (sortPaletteKey) {
					basePalette.SortByGrayscale ();
				}
			} else {
				// Sync the palette group up with the texture
				suppliedPaletteGroup.SyncWithTexture (sourceTexture);
				basePalette = suppliedPaletteGroup.BasePalette;
			}

			// Create the palette map from the palette key
			PaletteMap palettemap;
			palettemap = new PaletteMap (sourceTexture, basePalette);

			if (suppliedPaletteGroup == null) {
				// Write PaletteGroup to disk
				try {
					string paletteGroupFilename = paletteKeyFileName + ".asset";
					RBPaletteGroup createdGroup = RBPaletteCreator.CreatePaletteGroup (outputPath, paletteGroupFilename, overwriteExistingFiles);
					createdGroup.GroupName = paletteKeyFileName;
					createdGroup.SetBasePalette (basePalette);
					createdGroup.BasePalette.Locked = true;
					createdGroup.Locked = true;
				} catch (IOException) {
					throw new System.IO.IOException ("Failed to write PaletteMap. A PaletteGroup already exists by the name: " + paletteKeyFileName);
				}
			}

			// Write PaletteMap to disk
			string paletteMapFilenameWithExtension = paletteMapFilename + ".png";
			string fullPathToPaletteMapFile = outputPath + paletteMapFilenameWithExtension;
			palettemap.WriteToFile (fullPathToPaletteMapFile, overwriteExistingFiles);
		}
	
		class PaletteMap
		{
			Color[] pixels;
			int width;
			int height;
		
			public PaletteMap (Texture2D sourceTexture, RBPalette basePalette)
			{
				this.width = sourceTexture.width;
				this.height = sourceTexture.height;
				
				Color[] sourcePixels = sourceTexture.GetPixels ();
				pixels = new Color[sourcePixels.Length];
				
				// Remap original colors to point to indeces in the palette
				for (int i = 0; i < sourcePixels.Length; i++) {
					// Get the alpha value by looking it up in the paletteKey
					int paletteIndex = basePalette.IndexOf (sourcePixels [i]);
					if (paletteIndex < 0) {
						Vector2 coordinateFromBottomLeft = new Vector2 (i % width, i / height);
						Vector2 coordinateFromTopLeft = new Vector2 (coordinateFromBottomLeft.x, height - coordinateFromBottomLeft.y);
						throw new System.ArgumentException ("Encountered color in source PaletteMap image that is not in the base palette." +
						                                    " Color in PaletteMap: " + (Color32)sourcePixels [i] + 
						                                    " At coordinate: " + coordinateFromTopLeft);
					}
					float alpha;
					if (basePalette.Count == 1) {
						alpha = 0.0f;
					} else {
						alpha = paletteIndex / (float)(basePalette.Count - 1);
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
					throw new System.AccessViolationException ("Tried to write PaletteMap but file already exists. " +
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
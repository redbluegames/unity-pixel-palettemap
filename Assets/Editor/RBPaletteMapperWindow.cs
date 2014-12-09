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
using System.Collections;

namespace RedBlueTools
{
	class RBPaletteMapperWindow : EditorWindow
	{
		Object sourceTexture = null;
		Object lastSourceTexture;
		string paletteKeyFilename;
		string paletteMapFilename;
		Object suppliedPalleteKey = null;
		bool overwriteExistingFiles = true;
		bool sortPalette = false;
		PaletteKeyOption paletteKeyOption;

		enum PaletteKeyOption
		{
			GenerateNewPaletteKey,
			SupplyCustomPaletteKey
		}

		[MenuItem ("RedBlueTools/Palette Mapper")]
		public static void  ShowWindow ()
		{
			EditorWindow.GetWindow<RBPaletteMapperWindow> ("Palette Mapper");
		}
	
		void OnGUI ()
		{
			GUILayout.Label ("Palette Map", EditorStyles.boldLabel);
			sourceTexture = EditorGUILayout.ObjectField ("Source Texture", sourceTexture, typeof(Texture2D), false);

			bool sourceTextureChanged = lastSourceTexture != sourceTexture;
			if(sourceTextureChanged) {
				lastSourceTexture = sourceTexture;

				paletteMapFilename = sourceTexture.name + "_PaletteMap";
				paletteKeyFilename = sourceTexture.name + "_PaletteKey";
			}

			if(sourceTexture != null) {
				paletteMapFilename = EditorGUILayout.TextField("Output Filename", paletteMapFilename);
			}

			GUILayout.Label ("Palette Key", EditorStyles.boldLabel);
			paletteKeyOption = (PaletteKeyOption)EditorGUILayout.EnumPopup ("Palette Key Creation: ", paletteKeyOption);
			switch (paletteKeyOption) {
			case PaletteKeyOption.GenerateNewPaletteKey:
				suppliedPalleteKey = null;
				sortPalette = EditorGUILayout.Toggle ("Sort PaletteKey", sortPalette);
				break;
			case PaletteKeyOption.SupplyCustomPaletteKey:
				suppliedPalleteKey = EditorGUILayout.ObjectField ("Palette Key Texture", suppliedPalleteKey, typeof(Texture2D), false);
				sortPalette = false;
				break;
			}
			if(sourceTexture != null) {
				paletteKeyFilename = EditorGUILayout.TextField("Output Filename", paletteKeyFilename);
			}

			GUILayout.Label ("Options", EditorStyles.boldLabel);
			overwriteExistingFiles = EditorGUILayout.Toggle ("Overwite Existing files", overwriteExistingFiles);

			if (GUILayout.Button ("Build")) {
				if (sourceTexture == null) {
					Debug.LogError ("RBPaletteMapper Error: No source texture specified");
					return;
				}

				// Validate source texture
				Texture2D inTexture = (Texture2D)sourceTexture;
				try {
					RBPaletteMapper.ValidateSourceTexture (inTexture);
				} catch (System.BadImageFormatException e) {
					Debug.LogError ("RBPaletteMapper Error: " + e.Message);
					return;
				}

				// Validate or skip Palette Key
				Texture2D inPaletteKey = null;
				if (paletteKeyOption == PaletteKeyOption.SupplyCustomPaletteKey) {
					if (suppliedPalleteKey == null) {
						Debug.LogError ("RBPaletteMapper Error: Trying to use custom palette key but no palette key specified." +
							"\nPlease select a texture to use as the Palette Key.");
						return;
					}
					inPaletteKey = (Texture2D)suppliedPalleteKey;
					try {
						RBPaletteMapper.ValidatePaletteKeyTexture (inPaletteKey);
					} catch (System.BadImageFormatException e) {
						Debug.LogError ("RBPaletteMapper Error: " + e.Message);
						return;
					}
				}

				string path = GetPathToAsset (inTexture);
				try {
					RBPaletteMapper.CreatePaletteMapAndKey (path, inTexture, inPaletteKey, sortPalette, overwriteExistingFiles, paletteKeyFilename, paletteMapFilename);
				
					Debug.Log ("<color=green>Palette Map and Key for file " + inTexture.name + " created successfully</color>");
				} catch (System.NotSupportedException e) {
					LogError (e.Message);
				} catch (System.ArgumentException e) {
					LogError (e.Message);
				} catch (System.AccessViolationException e) {
					LogError (e.Message);
				} catch (System.IO.IOException e) {
					LogError ("Encountered IO Exception: " + e.Message);
				} catch (System.Exception e) {
					LogError ("Encountered unknown error: " + e.Message);
				}
			}
		}

		void LogError (string message)
		{
			Debug.LogError ("PaletteMap Error: " + message);
		}

		string GetPathToAsset (Object asset)
		{
			string path = AssetDatabase.GetAssetPath (asset);

			// Strip filename out from asset path
			string[] directories = path.Split ('/');
			path = path.TrimEnd (directories [directories.Length - 1].ToCharArray ());

			return path;
		}
	}
}
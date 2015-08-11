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
				paletteKeyFilename = sourceTexture.name + "_PaletteGroup";
			}

			if(sourceTexture != null) {
				paletteMapFilename = EditorGUILayout.TextField("Output Filename", paletteMapFilename);
			}

			GUILayout.Label ("Palette Key", EditorStyles.boldLabel);
			paletteKeyOption = (PaletteKeyOption)EditorGUILayout.EnumPopup ("Palette Key Creation: ", paletteKeyOption);
			switch (paletteKeyOption) {
			case PaletteKeyOption.GenerateNewPaletteKey:
				suppliedPalleteKey = null;
				if(sourceTexture != null) {
					paletteKeyFilename = EditorGUILayout.TextField("Output Filename", paletteKeyFilename);
				}
				sortPalette = EditorGUILayout.Toggle ("Sort PaletteKey", sortPalette);
				break;
			case PaletteKeyOption.SupplyCustomPaletteKey:
				suppliedPalleteKey = EditorGUILayout.ObjectField ("Linked Palette Group", suppliedPalleteKey, typeof(RBPaletteGroup), false);
				sortPalette = false;
				break;
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
				RBPaletteGroup inPaletteKey = null;
				if (paletteKeyOption == PaletteKeyOption.SupplyCustomPaletteKey) {
					if (suppliedPalleteKey == null) {
						Debug.LogError ("RBPaletteMapper Error: Trying to use custom palette key but no palette key specified." +
							"\nPlease select a RBPaletteGroup to use as the Palette Key.");
						return;
					}
					inPaletteKey = (RBPaletteGroup)suppliedPalleteKey;
					try {
						RBPaletteMapper.ValidatePaletteGroup (inPaletteKey);
					} catch {
						Debug.LogError ("Unknown PaletteMap Error encountered while Validating supplied PaletteGroup.");
						return;
					}
				}

				string path = AssetDatabaseUtility.GetAssetDirectory (inTexture);
				try {
					RBPaletteMapper.CreatePaletteMapAndKey (path, inTexture, inPaletteKey, sortPalette, overwriteExistingFiles, paletteKeyFilename, paletteMapFilename);
				
					Debug.Log ("<color=green>Palette Map and Key for file " + inTexture.name + " created successfully</color>");
					// TODO: Better error handling messages
				} catch (System.NotSupportedException e) {
					LogError (e.Message, e);
				} catch (System.ArgumentException e) {
					LogError (e.Message, e);
				} catch (System.AccessViolationException e) {
					LogError (e.Message, e);
				} catch (System.IO.IOException e) {
					LogError ("Encountered IO Exception: " + e.Message, e);
				} catch (System.Exception e) {
					LogError ("Encountered unknown error: " + e.Message, e);
				}
			}
		}

		void LogError (string message, System.Exception e)
		{
			Debug.LogError ("PaletteMap Error: " + message + "Error: " + e);
		}
	}
}
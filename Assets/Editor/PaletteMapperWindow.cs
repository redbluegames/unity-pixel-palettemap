using UnityEngine;
using UnityEditor;
using System.Collections;

class PaletteMapperWindow : EditorWindow
{
	Object sourceTexture = null;
	Object suppliedPalleteKey = null;
	bool overwriteExistingFiles = true;

	[MenuItem ("RedBlueTools/PaletteMapper/Generator")]
	public static void  ShowWindow ()
	{
		EditorWindow.GetWindow<PaletteMapperWindow> ("Palette Mapper");
	}
	
	void OnGUI ()
	{
		GUILayout.Label ("Palette Mapper", EditorStyles.boldLabel);

		sourceTexture = EditorGUILayout.ObjectField ("Texture", sourceTexture, typeof(Texture2D), false);
		suppliedPalleteKey = EditorGUILayout.ObjectField ("Palette Key (Optional)", suppliedPalleteKey, typeof(Texture2D), false);
		overwriteExistingFiles = EditorGUILayout.Toggle("Overwite Existing files", overwriteExistingFiles);

		if (GUILayout.Button ("Build")) {
			if (sourceTexture == null) {
				Debug.LogError ("PaletteMapper Error: No source texture specified");
				return;
			}

			// Validate source texture
			Texture2D inTexture = (Texture2D)sourceTexture;
			try {
				PaletteMapper.ValidateSourceTexture (inTexture);
			} catch (System.BadImageFormatException e) {
				Debug.LogError ("PaletteMapper Error: " + e.Message);
				return;
			}

			// Validate or skip Palette Key
			Texture2D inPaletteKey = null;
			if(suppliedPalleteKey != null) {
				inPaletteKey = (Texture2D)suppliedPalleteKey;
				try {
					PaletteMapper.ValidatePaletteKeyTexture (inPaletteKey);
				} catch (System.BadImageFormatException e) {
					Debug.LogError ("PaletteMapper Error: " + e.Message);
					return;
				}
			}

			string path = GetPathToAsset(inTexture);
			try {
				PaletteMapper.CreatePaletteMapAndKey (path, inTexture, inPaletteKey, overwriteExistingFiles);
				
				Debug.Log ("<color=green>Palette Map and Key for file " + inTexture.name + " created successfully</color>");
			} catch (System.NotSupportedException e) {
				LogError(e.Message);
			} catch (System.ArgumentException e) {
				LogError(e.Message);
			} catch (System.AccessViolationException e) {
				LogError(e.Message);
			} catch (System.IO.IOException e) {
				LogError("Encountered IO Exception: " + e.Message);
			}catch (System.Exception e) {
				LogError("Encountered unknown error: " + e.Message);
			}
		}
	}

	void LogError (string message)
	{
		Debug.LogError("PaletteMap Error: " + message);
	}

	string GetPathToAsset(Object asset)
	{
		string path = AssetDatabase.GetAssetPath(asset);

		// Strip filename out from asset path
		string[] directories = path.Split('/');
		path = path.TrimEnd(directories[directories.Length -1].ToCharArray());

		return path;
	}
}
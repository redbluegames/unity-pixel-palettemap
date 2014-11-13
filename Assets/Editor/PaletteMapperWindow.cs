using UnityEngine;
using UnityEditor;
using System.Collections;

class PaletteMapperWindow : EditorWindow
{
	Object inSourceTexture = null;
	bool overwriteExistingFiles = true;

	[MenuItem ("RedBlueTools/PaletteMapper")]
	public static void  ShowWindow ()
	{
		EditorWindow.GetWindow<PaletteMapperWindow> ("Palette Mapper");
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
				PaletteMapper.ValidateSourceTexture (inTexture);
				try {
					string path = GetPathToAsset(inTexture);
					try {
						PaletteMapper.WritePaletteMapTextureToDisk (path, inTexture, overwriteExistingFiles);
						
						Debug.Log ("<color=green>Palette Map and Key for file " + inTexture.name + " created successfully</color>");
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

	string GetPathToAsset(Object asset)
	{
		string path = AssetDatabase.GetAssetPath(asset);

		// Strip filename out from asset path
		string[] directories = path.Split('/');
		path = path.TrimEnd(directories[directories.Length -1].ToCharArray());

		return path;
	}
}
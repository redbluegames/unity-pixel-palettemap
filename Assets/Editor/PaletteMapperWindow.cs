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
			
			Texture2D inTexture = (Texture2D)inSourceTexture;
			try {
				PaletteMapper.ValidateSourceTexture (inTexture);
			} catch (System.BadImageFormatException e) {
				Debug.LogError ("PaletteMapper Error: " + e.Message);
				return;
			}
			
			string path = GetPathToAsset(inTexture);
			try {
				PaletteMapper.CreatePaletteMapAndKey (path, inTexture, overwriteExistingFiles);
				
				Debug.Log ("<color=green>Palette Map and Key for file " + inTexture.name + " created successfully</color>");
			} catch (System.NotSupportedException e) {
				LogError(e.Message);
			} catch (System.AccessViolationException e) {
				LogError(e.Message);
			} catch (System.IO.IOException e) {
				LogError("Encountered IO Exception: " + e.Message);
			}catch (System.Exception e) {
				LogError("Encountered unknown error: " + e.Message);
			}
		}

		if (GUILayout.Button("Megan")) {
			int sideSize = 2408;
			Texture2D hugeTexture = new Texture2D(sideSize, sideSize, TextureFormat.RGBA32, false);
			hugeTexture.hideFlags = HideFlags.HideAndDontSave;
			Color[] lotsOfColors = new Color[sideSize * sideSize];
			for(int i = 0; i < lotsOfColors.Length; i++) {
				int r = i % 255;
				lotsOfColors[i] = new Color(r / 255.0f, 0.0f, 0.0f);
			}
			hugeTexture.SetPixels (lotsOfColors);
			byte[] bytes = hugeTexture.EncodeToPNG();
			System.IO.File.WriteAllBytes(GetPathToAsset(inSourceTexture) + "Test_BigTexture.PNG", bytes);
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
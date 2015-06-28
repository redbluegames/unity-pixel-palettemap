using UnityEngine;
using UnityEditor;
using System.IO;

public static class RBPaletteCreator {

	[MenuItem ("Assets/Create/RBPalette")]
	public static RBPalette CreatePalette ()
	{
		// TODO: Needs to support creating multiple at this location
		return CreatePalette (GetPathOfSelection(), "RBPalette.asset");
	}

	static string GetPathOfSelection ()
	{
		// TODO THIS DOESN"T ALWAYS WORK.
		string path = "Assets";
		foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
		{
			path = AssetDatabase.GetAssetPath(obj);
			if (File.Exists(path))
			{
				path = Path.GetDirectoryName(path);
			}
			break;
		}

		return path;
	}
	
	static RBPalette CreatePalette (string path, string filename)
	{
		RBPalette paletteAsset = ScriptableObject.CreateInstance<RBPalette> ();

		string fullpath = path + "/" + filename;
		AssetDatabase.CreateAsset (paletteAsset, fullpath);
		AssetDatabase.SaveAssets ();
		
		EditorUtility.FocusProjectWindow ();
		Selection.activeObject = paletteAsset;
		
		return paletteAsset;
	}
}

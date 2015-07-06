using UnityEngine;
using UnityEditor;
using System.IO;

public static class RBPaletteCreator {

	[MenuItem ("Assets/Create/RBPalette")]
	public static RBPalette CreatePalette ()
	{
		// TODO: Needs to support creating duplicates (count) at this location - RBPalette0, RBPalette1 etc.
		return CreatePalette (GetPathOfSelection(), "RBPalette.asset");
	}

	static RBPalette CreatePalette (string path, string filename)
	{
		RBPalette paletteAsset = ScriptableObject.CreateInstance<RBPalette> ();
		return SaveRBPalette (paletteAsset, path, filename);
	}
	
	static RBPalette SaveRBPalette (RBPalette palette, string path, string filename)
	{
		string fullpath = path + "/" + filename;
		AssetDatabase.CreateAsset (palette, fullpath);
		AssetDatabase.SaveAssets ();
		
		EditorUtility.FocusProjectWindow ();
		Selection.activeObject = palette;
		
		return palette;
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
	
	[MenuItem ("Assets/ExtractPalette")]
	public static RBPalette ExtractPalleteFromTexture ()
	{
		Texture2D selectedTexture = (Texture2D) Selection.activeObject;
		RBPalette paletteFromTexture = RBPalette.CreatePaletteFromTexture (selectedTexture);
		return SaveRBPalette (paletteFromTexture, GetPathOfSelection (), "RBPalette.asset");
	}
	
	[MenuItem ("Assets/ExtractPalette", true)]
	public static bool IsValidTargetForPalette ()
	{
		if (Selection.activeObject == null) {
			return false;
		}

		return Selection.activeObject.GetType() == typeof(Texture2D);
	}
}

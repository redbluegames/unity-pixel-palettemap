using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using RedBlueTools;

public class PaletteMapJob : ScriptableObject
{
	public string PaletteMapName;
	public Texture2D SourceTexture;
	public RBPaletteGroup PaletteGroup;
	public bool OverwriteExistingPaletteMap;
	public RBPaletteDiff CurrentDiff;

	[MenuItem ("Assets/Create/RBPaletteMap/PaletteMapJob")]
	public static PaletteMapJob CreatePaletteMapJob ()
	{
		PaletteMapJob job = PaletteMapJob.CreateInstance ();
		string directory = AssetDatabaseUtility.GetDirectoryOfSelection ();
		string filename = "RBPaletteMapJob.asset";
		string desiredPath = directory + filename;
		string uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath (desiredPath);
		return (PaletteMapJob) AssetDatabaseUtility.SaveAndSelectObject (job, directory, Path.GetFileName(uniqueAssetPath));
	}
	
	public static PaletteMapJob CreateInstance ()
	{
		PaletteMapJob job = ScriptableObject.CreateInstance<PaletteMapJob> ();
		return job;
	}

	public void CreatePaletteGroupForTexture ()
	{
		RBPaletteGroup paletteGroup = RBPaletteCreator.ExtractPaletteFromTexture (SourceTexture, AssetDatabaseUtility.GetAssetDirectory (this),
		                                                                          name + "_PaletteGroup.asset");
		if (paletteGroup != null) {
			paletteGroup.BasePalette.Locked = true;
			paletteGroup.Locked = true;
			PaletteGroup = paletteGroup;
		}
	}

	public void DiffPaletteGroupWithTexture ()
	{
		CurrentDiff = PaletteGroup.DiffWithTexture (SourceTexture);
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(PaletteMapJob))]
public class PaletteMapJobEditor : Editor {

	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector ();

		string path = AssetDatabaseUtility.GetAssetDirectory (target);
		PaletteMapJob targetJob = (PaletteMapJob) target;

		bool disableDiff = targetJob.SourceTexture == null || targetJob.PaletteGroup == null;
		EditorGUI.BeginDisabledGroup (disableDiff);
		if (GUILayout.Button ("Diff PaletteGroup")) {
			targetJob.DiffPaletteGroupWithTexture ();
		}
		EditorGUI.EndDisabledGroup ();

		bool generationDisabled = targetJob.PaletteGroup == null;
		// Palette Group Button
		EditorGUI.BeginDisabledGroup (!generationDisabled);
		if (GUILayout.Button ("Create New PaletteGroup from Texture")) {
			targetJob.CreatePaletteGroupForTexture ();
		}
		EditorGUI.EndDisabledGroup ();

		EditorGUILayout.Separator ();

		// Generate Palette Map Button
		EditorGUI.BeginDisabledGroup (generationDisabled);
		if (GUILayout.Button ("Generate PaletteMap")) {
			try {
				bool creationComplete = RBPaletteMapper.CreatePaletteMapAndKey (
					path, targetJob.SourceTexture, targetJob.PaletteGroup, targetJob.OverwriteExistingPaletteMap, 
				    "Key", targetJob.PaletteMapName);
				if (creationComplete) {
					Debug.Log ("<color=green>Palette Map created successfully for job: </color>" + targetJob.name + 
					           "\n<color=green>Updated PaletteGroup: </color>" + targetJob.PaletteGroup);
				}
			} catch (System.Exception e) {
				Debug.LogError (e, targetJob);
			}
		}
		EditorGUI.EndDisabledGroup ();
	}
}
#endif
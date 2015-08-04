using UnityEngine;
using UnityEditor;
using System.Collections;

public class PackageExporter : UnityEditor.EditorWindow {

	static string assetPathName = "Assets/RBPixelPalettemap";

	[MenuItem ("PackageExporter/Export as Package")]
	public static void ExportPRBScripts ()
	{
		AssetDatabase.ExportPackage (assetPathName, "RBPixelPalettemap.unitypackage", ExportPackageOptions.Recurse |
		                             ExportPackageOptions.IncludeDependencies);
		Debug.Log ("Exported!");
	}
}

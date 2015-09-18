using UnityEngine;
using UnityEditor;
using System.Collections;

public class PackageExporter : UnityEditor.EditorWindow {

	static string assetPathName = "Assets/RBPixelPalettemap";
	static string testsPathName = "Assets/Tests";

	[MenuItem ("PackageExporter/Export Release")]
	public static void ExportPackage ()
	{
		AssetDatabase.ExportPackage (assetPathName, "RBPixelPalettemap.unitypackage", ExportPackageOptions.Recurse |
		                             ExportPackageOptions.IncludeDependencies);
		Debug.Log ("Exported!");
	}
	
	[MenuItem ("PackageExporter/Export with Tests")]
	public static void ExportPackageDebug ()
	{
		AssetDatabase.ExportPackage (new string[] {assetPathName, testsPathName} , "RBPixelPalettemapDebug.unitypackage", ExportPackageOptions.Recurse |
		                             ExportPackageOptions.IncludeDependencies);
		Debug.Log ("Exported!");
	}
}

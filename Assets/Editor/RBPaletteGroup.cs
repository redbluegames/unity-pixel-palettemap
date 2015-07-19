using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class RBPaletteGroup : ScriptableObject
{
	public string GroupName = "RBPaletteGroup";
	public List<RBPalette> palettes;

	RBPalette basePalette { 
		get {
			return palettes [0];
		}
		set {
			palettes [0] = value;
		}
	}

	public int NumColorsInPalette {
		get {
			if (palettes == null || palettes.Count == 0) {
				return 0;
			} else {
				return basePalette.Count;
			}
		}
	}

	public int Count {
		get {
			if (palettes == null) {
				return 0;
			} else {
				return palettes.Count;
			}
		}
	}
	
	public static RBPaletteGroup CreateInstance ()
	{
		RBPaletteGroup paletteGroup = ScriptableObject.CreateInstance<RBPaletteGroup> ();
		paletteGroup.Initialize ();
		return paletteGroup;
	}
	
	public static RBPaletteGroup CreateInstance (RBPalette basePalette)
	{
		RBPaletteGroup paletteGroup = RBPaletteGroup.CreateInstance ();
		paletteGroup.SetBasePalette (basePalette);
		return paletteGroup;
	}

	void Initialize ()
	{
		palettes = new List<RBPalette> ();
		RBPalette basePalette = new RBPalette ("Base Palette");
		palettes.Add (basePalette);
	}

	public void SetBasePalette (RBPalette basePalette)
	{
		this.basePalette = basePalette;
		this.basePalette.PaletteName = "Base Palette";
		// TODO: Extend or truncate existing palettes
	}

	public void AddPalette ()
	{
		RBPalette newPalette = new RBPalette (basePalette);
		newPalette.PaletteName = "Unnamed";

		palettes.Add (newPalette);
	}

	public void AddColor ()
	{
		foreach (RBPalette palette in palettes) {
			palette.AddColor (Color.white);
		}
	}

	public void RemoveColorAtIndex (int index)
	{
		if (index < 0 || index >= NumColorsInPalette) {
			throw new System.IndexOutOfRangeException 
				(string.Format ("Trying to remove color at invalid index, {0}", index));
		}
		foreach (RBPalette palette in palettes) {
			palette.RemoveColorAtIndex (index);
		}
	}

	public void RemovePaletteAtIndex (int index)
	{
		if (index < 0 || index >= Count) {
			throw new System.IndexOutOfRangeException 
				(string.Format ("Trying to remove palette at invalid index, {0}", index));
		}
		palettes.RemoveAt (index);
	}

	Texture2D CreateAsTexture ()
	{
		// Write the colors into a texture
		Texture2D paletteKeyAsTexture = new Texture2D (NumColorsInPalette, Count, TextureFormat.RGBA32, false);
		paletteKeyAsTexture.hideFlags = HideFlags.HideAndDontSave;
		paletteKeyAsTexture.SetPixels (GetColorsAsArray ());
		paletteKeyAsTexture.Apply ();
		
		return paletteKeyAsTexture;
	}

	Color[] GetColorsAsArray ()
	{
		Color[] colorsAsArray = new Color [NumColorsInPalette * Count];
		for (int paletteIndex = 0; paletteIndex < Count; paletteIndex++) {
			for (int colorIndex = 0; colorIndex < NumColorsInPalette; colorIndex++) {
				int i = paletteIndex * NumColorsInPalette + colorIndex;
				colorsAsArray[i] = palettes[paletteIndex][colorIndex];
			}
		}
		return colorsAsArray;
	}
	
	public void WriteToFile (string fullPathToFile, bool allowOverwriting)
	{
		if (File.Exists (fullPathToFile) && !allowOverwriting) {
			throw new System.AccessViolationException ("Tried to write PaletteGroup but file already exists. " +
			                                           "\nFile Path: " + fullPathToFile);
		}
		
		Texture2D keyAsTexture = CreateAsTexture ();
		try {
			byte[] outTextureData = keyAsTexture.EncodeToPNG ();
			File.WriteAllBytes (fullPathToFile, outTextureData);
		} catch (System.Exception e) {
			throw new System.IO.IOException ("Encountered IO exception during PaletteKey write: " + e.Message);
		}
		
		// Force refresh so that we can set its Import settings immediately
		AssetDatabase.ImportAsset (fullPathToFile); 
		
		// Assign correct settings to the file
		TextureImporter textureImporter = AssetImporter.GetAtPath (fullPathToFile) as TextureImporter;
		if(textureImporter == null) {
			throw new System.NullReferenceException ("Failed to import file at specified path: " + fullPathToFile);
		}
		textureImporter.filterMode = FilterMode.Point;
		textureImporter.textureFormat = TextureImporterFormat.RGBA32;
		textureImporter.alphaIsTransparency = false;
		textureImporter.mipmapEnabled = false;
		textureImporter.npotScale = TextureImporterNPOTScale.None;
		textureImporter.maxTextureSize = 256;
		
		// Force Unity to see the file and use the new import settings
		AssetDatabase.ImportAsset (fullPathToFile); 
	}
}

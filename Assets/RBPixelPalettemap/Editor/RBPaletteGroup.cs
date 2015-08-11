using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class RBPaletteGroup : ScriptableObject
{
	public const string DefaultGroupName = "RBPaletteGroup";
	public string GroupName;
	public bool Locked = true;
	[SerializeField]
	List<RBPalette>
		palettes;

	public RBPalette BasePalette { 
		get {
			return palettes [0];
		}
		private set {
			palettes [0] = value;
		}
	}

	public int NumColorsInPalette {
		get {
			if (palettes == null || palettes.Count == 0) {
				return 0;
			} else {
				return BasePalette.Count;
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

	public static RBPaletteGroup CreateInstance (string groupPaletteName = DefaultGroupName, RBPalette basePalette = null)
	{
		RBPaletteGroup paletteGroup = ScriptableObject.CreateInstance<RBPaletteGroup> ();
		paletteGroup.Initialize (groupPaletteName, basePalette);
		return paletteGroup;
	}

	public void Initialize (string groupPaletteName = DefaultGroupName, RBPalette basePalette = null)
	{
		palettes = new List<RBPalette> ();
		GroupName = groupPaletteName;
		if (basePalette == null) {
			basePalette = new RBPalette ("Base Palette");
		}
		palettes.Add (basePalette);
	}

	public void SetBasePalette (RBPalette basePalette)
	{
		this.BasePalette = basePalette;
		this.BasePalette.PaletteName = "Base Palette";
		// TODO: Extend or truncate existing palettes
	}

	public void AddPalette ()
	{
		RBPalette newPalette = new RBPalette (BasePalette);
		newPalette.PaletteName = "Unnamed";

		palettes.Add (newPalette);
	}

	public void AddColor ()
	{
		if (Locked) {
			throw new System.AccessViolationException ("Can't Add Color to RBPaletteGroup. PaletteGroup is Locked.");
		}

		foreach (RBPalette palette in palettes) {
			palette.AddColor (Color.magenta);
		}
	}

	public void RemoveColorAtIndex (int index)
	{
		if (Locked) {
			throw new System.AccessViolationException ("Can't Add Color to RBPaletteGroup. PaletteGroup is Locked.");
		}
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
		if (Locked && index == 0) {
			throw new System.AccessViolationException ("Trying to remove base palette from Locked palette group. Locked palette groups " +
				"must have a Base Palette.");
		}

		if (index < 0 || index >= Count) {
			throw new System.IndexOutOfRangeException 
				(string.Format ("Trying to remove palette at invalid index, {0}", index));
		}
		palettes.RemoveAt (index);
	}

	public void SyncWithTexture (Texture2D sourceTexture)
	{
		Color[] sourcePixels = sourceTexture.GetPixels ();

		// Unlock the PaletteGroup so that we can edit it.
		bool wasLocked = Locked;
		Locked = false;
		bool wasPaletteLocked = BasePalette.Locked;
		BasePalette.Locked = false;
		
		// Add new colors from the texture into the Palette
		List<Color> seenColors = new List<Color>();
		for (int i = 0; i < sourcePixels.Length; i++) {
			Color colorAtSource = RBPalette.ClearRGBIfNoAlpha (sourcePixels [i]);
			int index = BasePalette.IndexOf (colorAtSource);
			bool colorNotFound = index < 0;
			if (colorNotFound) {
				AddColor ();
				BasePalette [BasePalette.Count - 1] = colorAtSource; // Note this assumes color is added to the end...
			} else {
				// Add unique seen colors to list of seen colors
				if (!seenColors.Contains (colorAtSource)) {
					seenColors.Add (colorAtSource);
				}
			}
		}

		// Remove unused colors, back to front to avoid shifting indeces
		for (int i = BasePalette.Count -1; i >= 0; i--) {
			bool colorWasSeen = seenColors.Contains(BasePalette[i]);
			if (!colorWasSeen) {
				RemoveColorAtIndex (i);
			}
		}

		// Relock the palette group
		Locked = wasLocked;
		BasePalette.Locked = wasPaletteLocked;
	}

	#region Output Functions
	public void WriteToFile (string fullPathToFile, bool allowOverwriting)
	{
		if (File.Exists (fullPathToFile) && !allowOverwriting) {
			throw new System.AccessViolationException ("Tried to write PaletteGroup but file already exists. " +
				"\nFile Path: " + fullPathToFile);
		}
		
		Texture2D paletteGroupAsTexture = CreateAsTexture ();
		try {
			byte[] outTextureData = paletteGroupAsTexture.EncodeToPNG ();
			File.WriteAllBytes (fullPathToFile, outTextureData);
		} catch (System.Exception e) {
			throw new System.IO.IOException ("Encountered IO exception during PaletteGroup write: " + e.Message);
		}
		
		// Force refresh so that we can set its Import settings immediately
		AssetDatabase.ImportAsset (fullPathToFile); 
		
		// Assign correct settings to the file
		TextureImporter textureImporter = AssetImporter.GetAtPath (fullPathToFile) as TextureImporter;
		if (textureImporter == null) {
			throw new System.NullReferenceException ("Failed to import file at specified path: " + fullPathToFile);
		}
		textureImporter.textureType = TextureImporterType.Advanced;
		textureImporter.spriteImportMode = SpriteImportMode.None;
		textureImporter.filterMode = FilterMode.Point;
		textureImporter.textureFormat = TextureImporterFormat.RGBA32;
		textureImporter.alphaIsTransparency = false;
		textureImporter.mipmapEnabled = false;
		textureImporter.npotScale = TextureImporterNPOTScale.None;
		textureImporter.maxTextureSize = 256;

		// Force Unity to see the file and use the new import settings
		AssetDatabase.ImportAsset (fullPathToFile, ImportAssetOptions.ForceUpdate); 
	}
	
	Texture2D CreateAsTexture ()
	{
		// Write the colors into a texture
		Texture2D paletteGroupAsTexture = new Texture2D (NumColorsInPalette, Count, TextureFormat.RGBA32, false);
		paletteGroupAsTexture.hideFlags = HideFlags.HideAndDontSave;
		paletteGroupAsTexture.SetPixels (GetColorsAsArray ());
		paletteGroupAsTexture.Apply ();
		
		return paletteGroupAsTexture;
	}
	
	Color[] GetColorsAsArray ()
	{
		Color[] colorsAsArray = new Color [NumColorsInPalette * Count];
		for (int paletteIndex = 0; paletteIndex < Count; paletteIndex++) {
			for (int colorIndex = 0; colorIndex < NumColorsInPalette; colorIndex++) {
				int i = paletteIndex * NumColorsInPalette + colorIndex;
				colorsAsArray [i] = palettes [paletteIndex] [colorIndex];
			}
		}
		return colorsAsArray;
	}
	#endregion
}

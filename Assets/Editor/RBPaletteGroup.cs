using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RBPaletteGroup : ScriptableObject
{
	public List<RBPalette> palettes;

	RBPalette basePalette { 
		get {
			return palettes [0];
		} set {
			palettes[0] = value;
		}
	}

	int NumColorsInPalette {
		get {
			if (palettes == null || palettes.Count == 0) {
				return 0;
			} else {
				return basePalette.Count;
			}
		}
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
		foreach (RBPalette palette in palettes) {
			palette.RemoveColorAtIndex (index);
		}
	}

	public void RemovePaletteAtIndex (int index)
	{
		palettes.RemoveAt (index);
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
}

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;

public class RBPalette : ScriptableObject {

	public List<RBPaletteEntry> ColorsInPalette = new List<RBPaletteEntry> ();

	[System.Serializable]
	public class RBPaletteEntry {
		public Color color;
		public string label;

		public RBPaletteEntry (Color color, string label)
		{
			this.color = color;
			this.label = label;
		}
	}
	
	public int Count {
		get {
			return ColorsInPalette.Count;
		}
	}

	public static RBPalette CreatePaletteFromTexture (Texture2D sourceTexture)
	{
		Color[] sourcePixels = sourceTexture.GetPixels ();
		RBPalette palette = new RBPalette ();
		
		// Get all unique colors
		for (int i = 0; i < sourcePixels.Length; i++) {
			Color colorAtSource = ClearRGBIfNoAlpha (sourcePixels [i]);
			if (!palette.ContainsColor (colorAtSource)) {
				palette.ColorsInPalette.Add ( new RBPaletteEntry (colorAtSource, i.ToString ()));
			}
		}
		
		return palette;
	}
	
	public bool ContainsColor (Color colorToFind)
	{
		for (int i = 0; i < ColorsInPalette.Count; i++) {
			if (ColorsInPalette[i].color == colorToFind) {
				return true;
			}
		}

		return false;
	}

	// Clears out the RGB when fully transparent so that we don't get lots of versions of transparent in the palette
	static Color ClearRGBIfNoAlpha (Color colorToClear)
	{
		Color clearedColor = colorToClear;
		if (Mathf.Approximately (clearedColor.a, 0.0f)) {
			clearedColor = Color.clear;
		}
		return clearedColor;
	}
}

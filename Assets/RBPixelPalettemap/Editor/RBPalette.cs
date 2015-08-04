using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class RBPalette
{
	public string PaletteName;
	[SerializeField]
	List<Color> ColorsInPalette;

	public Color this [int index] {
		get {
			return GetColor (index);
		}
		set {
			SetColor (index, value);
		}
	}

	Color GetColor (int index)
	{
		return ColorsInPalette [index];
	}

	void SetColor (int index, Color color)
	{
		ColorsInPalette [index] = color;
	}
	
	public RBPalette () : this ("RBPalette")
	{
	}

	public RBPalette (string paletteName)
	{
		this.PaletteName = paletteName;
		this.ColorsInPalette = new List<Color> ();
	}

	public RBPalette (RBPalette paletteToCopy)
	{
		this.PaletteName = paletteToCopy.PaletteName;
		this.ColorsInPalette = new List<Color> ();
		this.ColorsInPalette.AddRange (paletteToCopy.ColorsInPalette);
	}

	public void AddColor (Color color)
	{
		ColorsInPalette.Add (color);
	}

	public void RemoveColorAtIndex (int index)
	{
		ColorsInPalette.RemoveAt (index);
	}

	public bool ContainsColor (Color colorToFind)
	{
		return IndexOf (colorToFind) >= 0;
	}

	public int IndexOf (Color colorToFind)
	{
		int index = -1;
		for (int i = 0; i < ColorsInPalette.Count; i++) {
			if (ColorsInPalette [i] == colorToFind) {
				index = i;
				break;
			}
		}

		return index;
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
		palette.PaletteName = sourceTexture.name;
		
		// Get all unique colors
		for (int i = 0; i < sourcePixels.Length; i++) {
			Color colorAtSource = ClearRGBIfNoAlpha (sourcePixels [i]);
			if (!palette.ContainsColor (colorAtSource)) {
				palette.AddColor (colorAtSource);
			}
		}
		
		return palette;
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

	public void SortByGrayscale ()
	{
		ColorsInPalette.Sort (CompareColorsByGrayscale);
	}
	
	// Returns the "smaller" of the two colors by grayscale
	static int CompareColorsByGrayscale (Color colorA, Color colorB)
	{
		// When one is alpha and the other isn't, the alpha'ed color is smaller
		if (colorA.a < 1.0f && Mathf.Approximately (colorB.a, 1.0f)) {
			return -1;
		} else if (colorB.a < 1.0f && Mathf.Approximately (colorA.a, 1.0f)) {
			return 1;
		}
		
		if (colorA.grayscale < colorB.grayscale) {
			return -1;
		} else if (colorA.grayscale > colorB.grayscale) {
			return 1;
		} else {
			// Colors are equal - decide ties by alpha (usually happens with black)
			if (colorA.a < colorB.a) {
				return -1;
			} else {
				return 1;
			}
		}
	}
}

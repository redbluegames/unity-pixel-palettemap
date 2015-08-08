using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class RBPalette
{
	public string PaletteName;
	public bool Locked = false;
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
		if (Locked) {
			throw new System.AccessViolationException ("Can't Set Color to on RBPalette. RBPalette is Locked.");
		}

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
			bool colorToFindIsZeroAlpha = Mathf.Approximately (colorToFind.a, 0.0f);
			bool currentColorIsZeroAlpha = Mathf.Approximately (ColorsInPalette [i].a, 0.0f);
			if ((colorToFindIsZeroAlpha && currentColorIsZeroAlpha) || ColorsInPalette [i] == colorToFind) {
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


	/// <summary>
	/// Clears the RGB if there is no alpha. This is useful to keep duplicate full-transparent colors out of the palette
	/// </summary>
	/// <returns>If color has no alpha returns black with 0 alpha, otherwise returns the original color.</returns>
	/// <param name="colorToClear">Color to clear.</param>
	public static Color ClearRGBIfNoAlpha (Color colorToClear)
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

	public override string ToString ()
	{
		string fullString = "";
		fullString += "[RBPalette: Name=" + PaletteName  + " Count=" + Count + " Colors=";
		for (int i =0; i < Count; i++) {
			string colorString = "{";
			colorString += (Color32) ColorsInPalette[i];
			colorString += "}";
			fullString += colorString;
		}
		return fullString;
	}
}

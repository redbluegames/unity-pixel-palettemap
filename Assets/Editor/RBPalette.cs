using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class RBPalette
{
	public bool IsShowingDetails;
	public ReorderableList AsReorderableList;

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
		for (int i = 0; i < ColorsInPalette.Count; i++) {
			if (ColorsInPalette [i] == colorToFind) {
				return true;
			}
		}
		
		return false;
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
}

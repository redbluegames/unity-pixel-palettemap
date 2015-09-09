using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class RBPaletteDiff
{
	public List<Color> Insertions;
	public List<Color> Deletions;

	public int NumChanges
	{
		get {
			return Insertions.Count + Deletions.Count;
		}
	}
	
	public RBPaletteDiff ()
	{
		Insertions = new List<Color> ();
		Deletions = new List<Color> ();
	}
	
	public static RBPaletteDiff Diff (RBPalette paletteA, RBPalette paletteB)
	{
		RBPaletteDiff diffResults = new RBPaletteDiff ();
		
		// Find deletions (colors in A that aren't in B)
		for (int i = 0; i < paletteA.Count; i++) {
			if (!paletteB.ContainsColor (paletteA[i])) {
				diffResults.Deletions.Add (paletteA[i]);
			}
		}
		
		// Find insertions (colors in B that aren't in A)
		for (int i = 0; i < paletteB.Count; i++) {
			if (!paletteA.ContainsColor (paletteB[i])) {
				diffResults.Insertions.Add (paletteB[i]);
			}
		}
		
		return diffResults;
	}
}
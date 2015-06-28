using UnityEngine;
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
}

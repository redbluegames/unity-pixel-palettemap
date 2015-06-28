/*****************************************************************************
 *  Copyright (C) 2014 Red Blue Games, LLC
 *  
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 ****************************************************************************/
using UnityEngine;
using System.Collections;

namespace RedBlueTools
{
	/// <summary>
	/// SetTextureVariableExample is simply an example of how to set a shader variable from script.
	/// </summary>
	public class SetTextureVariableExample : MonoBehaviour
	{

		public Texture[] palettes;
		public int currentIndex = 0;
		string paletteVariable = "_Palette";

		void OnEnable ()
		{
			if (palettes.Length == 0) {
				Debug.LogError ("No Palette textures assigned to GameObject: " + gameObject.name);
				enabled = false;
			}
		}

		void Update ()
		{
			if (currentIndex > palettes.Length || currentIndex < 0) {
				Debug.LogError ("Tried to set palette index beyond the number of supplied Palettes.");
				currentIndex = 0;
			}

			// Set the palette for the given material.
			GetComponent<Renderer>().material.SetTexture (paletteVariable, palettes [currentIndex]);

			// This would change the palette for all objects using this material. Careful with this,
			// since changes will persist across Play mode.
			//renderer.sharedMaterial.SetTexture(paletteVariable, palettes[currentIndex]);

		}
	}
}
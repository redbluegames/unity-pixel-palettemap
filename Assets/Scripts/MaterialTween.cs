/*****************************************************************************
 *  MaterialTween is a RedBlueTool used to swap between materials on a gameobject.
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
	public class MaterialTween : MonoBehaviour
	{
	
		public Material[] tweenMaterials;
		[Range(0.01f, 10.0f)]
		public float
			swapInterval;
		public float duration;
		public static float INFINITE_DURATION = -1.0f;
	
		// State tracking members
		Material originalMaterial;
		float timeUntilSwap;
		float totalTimeRunning;
		int currentIndex = 0;

		void OnEnable ()
		{
			if (tweenMaterials.Length <= 0) {
				Debug.LogError ("No textures assigned to Material Tween on GameObject: " + gameObject.name 
					+ "\nMust have at least one texture.");
				enabled = false;
				return;
			}
		
			originalMaterial = renderer.material;

			timeUntilSwap = swapInterval;
			totalTimeRunning = 0.0f;
			currentIndex = 0;
			renderer.material = tweenMaterials [currentIndex];
		}
	
		void OnDisable ()
		{
			// Restore original material in case we end on a weird material
			renderer.material = originalMaterial;
		}
	
		void Update ()
		{
			timeUntilSwap -= Time.deltaTime;
			if (timeUntilSwap <= 0.0f) {
				IncrementMaterial ();
				// Reset timer
				timeUntilSwap = swapInterval;
			}
		
			totalTimeRunning += Time.deltaTime;
			if (!Mathf.Approximately (duration, INFINITE_DURATION) && totalTimeRunning >= duration) {
				StopTweening ();
			}
		}
	
		void IncrementMaterial ()
		{
			currentIndex = currentIndex + 1;
			// Wrap back around when done looping through materials.
			if (currentIndex >= tweenMaterials.Length) {
				currentIndex = 0;
			}
			renderer.material = tweenMaterials [currentIndex];
		}
	
		public void StopTweening ()
		{
			enabled = false;
		}
	}
}
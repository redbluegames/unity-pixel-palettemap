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
	/// MaterialTween is a RedBlueTool used to swap between materials on a gameobject.
	/// </summary>
	public class MaterialTween : MonoBehaviour
	{
		public Material[] tweenMaterials;
		public float swapInterval;
		public float duration;
		public bool TweenOnAwake;
		bool IsTweening;
		float timeTweening;
		public static float INFINITE_DURATION = -1.0f;
	
		// State tracking members
		Material originalMaterial;
		int currentIndex = 0;

		IEnumerator tweenCoroutine;

		void Awake ()
		{
			if(TweenOnAwake) {
				BeginTweening ();
			}
		}

		public void BeginTweening ()
		{
			if (tweenMaterials == null || tweenMaterials.Length <= 0) {
				Debug.LogError ("No textures assigned to Material Tween on GameObject: " + gameObject.name 
				                + "\nMust have at least one texture.");
				enabled = false;
				return;
			}

			if(Mathf.Approximately(duration, 0.0f)) {
				Debug.LogWarning("Trying to begin a material tween with no duration. Please assign a duration.");
				enabled = false;
				return;
			}

			// If told to tween while already tweening, reset duration.
			if(IsTweening) {
				timeTweening = 0.0f;
				return;
			}
			
			tweenCoroutine = TweenForDuration();
			StartCoroutine(tweenCoroutine);
		}

		IEnumerator TweenForDuration ()
		{
			IsTweening = true;

			currentIndex = 0;
			originalMaterial = GetComponent<Renderer>().material;
			GetComponent<Renderer>().material = tweenMaterials [currentIndex];

			float timeUntilSwap = swapInterval;
			timeTweening = 0.0f;
			while (true) {

				timeUntilSwap -= Time.deltaTime;
				// Every swap interval, go to the next Material
				if (timeUntilSwap <= 0.0f) {
					IncrementMaterial ();
					// Reset timer, carrying over extra deltaTime
					timeUntilSwap += swapInterval;
				}

				// Check if duration time has elapsed
				if(duration > 0.0f) {
					timeTweening += Time.deltaTime;
					if(timeTweening >= duration) {
						break;
					}
				}

				yield return null;
			}

			FinishTween ();
		}
		void IncrementMaterial ()
		{
			currentIndex = currentIndex + 1;
			// Wrap back around when done looping through materials.
			if (currentIndex >= tweenMaterials.Length) {
				currentIndex = 0;
			}
			GetComponent<Renderer>().material = tweenMaterials [currentIndex];
		}
		
		void FinishTween ()
		{
			StopTweening ();
		}

		public void StopTweening ()
		{
			IsTweening = false;

			// Restore original material
			GetComponent<Renderer>().material = originalMaterial;

			StopCoroutine(tweenCoroutine);
			tweenCoroutine = null;
		}
	}
}
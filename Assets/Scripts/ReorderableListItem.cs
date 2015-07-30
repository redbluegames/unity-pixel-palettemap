using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ReorderableListItem
{
	[SerializeField]
	public string
		ListItemName;
	[SerializeField]
	// ReorderableListItem contains a reorderable list
	public List<Color> ListItemColor;
}

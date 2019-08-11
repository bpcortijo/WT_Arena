using UnityEngine;
using System.Collections;

public class TileScript : MonoBehaviour {
    public MapMaker map;
    public int tileX, tileY, tileZ;
    public bool floor, ceiling, northViable, westViable, eastViable, southViable;

	void OnMouseUp()
	{
		if (map.selectedUnit != null)
		{
			if (map.selectedUnit.GetComponent<ShotScript>() != null)
				if (map.selectedUnit.GetComponent<ShotScript>().set)
					return;

			if (map.selectedUnit.GetComponent<CharacterStats>() != null)
				if (!map.selectedUnit.GetComponent<UnitBasics>().full)
					map.selectedUnit.GetComponent<CharacterStats>().actions.Add("Move");

			map.GeneratePathTo(tileX, tileY, tileZ);
		}
	}
}

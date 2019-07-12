using UnityEngine;
using System.Collections;

public class TileScript : MonoBehaviour {
    public MapMaker map;
    public int tileX, tileY, tileZ;
    public bool floor, ceiling, northViable, westViable, eastViable, southViable;

	void OnMouseUp() {
		Debug.Log ("Click!");

		map.GeneratePathTo(tileX, tileY, tileZ);
		map.selectedUnit.GetComponent<CharacterStats>().currentAction = "move";
	}

}

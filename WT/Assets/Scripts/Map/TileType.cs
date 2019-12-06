using UnityEngine;
using System.Collections;

[System.Serializable]
public class TileType {
	public string name;
	public float movementcost = 1;
    public GameObject tileVisualPrefab;
	public enum typeForArt { flat, empty, nWall, eWall, sWall, wWall, neCorner, seCorner, swCorner, nwCorner }
	public enum tilePreset { };
}

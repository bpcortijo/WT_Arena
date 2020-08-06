using UnityEngine;
using System.Collections;

[System.Serializable]
public class TileType {
	public string name;
	public int movementcost = 10;
    public GameObject tileVisualPrefab;
	public enum typeForArt { flat, empty, nWallFC, eWallFC, sWallFC, wWallFC, neCorner, seCorner, swCorner, nwCorner, nWallNone, eWallNone, sWallNone, wWallNone }
	public enum tilePreset { };
	public enum stairBottom { none, north, east, west, south}
}

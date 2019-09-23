using UnityEngine;
using System.Collections;

public class TileScript : MonoBehaviour {
    public MapMaker map;
	public string effect = null;
	public int tileX, tileY, tileZ;
	public bool floor, ceiling, northViable, westViable, eastViable, southViable;
	public int defendFloor, defendCeiling, defendNorth, defendWest, defendEast, defendSouth;

	[HideInInspector]
	public TileType tt;
	public float movementcost = 1f;
	public TileType.typeForArt direction;
	

	void Start()
	{
		DefenceReset();
		gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x,
									gameObject.transform.localPosition.y + .9f,
									gameObject.transform.localPosition.z);
		tt.movementcost = movementcost;
		if (direction == TileType.typeForArt.flat)
			gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x,
												gameObject.transform.localPosition.y - .45f,
												gameObject.transform.localPosition.z);

		else if (direction == TileType.typeForArt.nWall || direction == TileType.typeForArt.nwCorner)
			gameObject.transform.localRotation = Quaternion.Euler(gameObject.transform.localRotation.x,
																	gameObject.transform.localRotation.y + 180,
																	gameObject.transform.localRotation.z);
		else if (direction == TileType.typeForArt.wWall || direction == TileType.typeForArt.swCorner)
			gameObject.transform.localRotation = Quaternion.Euler(gameObject.transform.localRotation.x,
																	gameObject.transform.localRotation.y + 90,
																	gameObject.transform.localRotation.z);
		else if (direction == TileType.typeForArt.eWall || direction == TileType.typeForArt.neCorner)
			gameObject.transform.localRotation = Quaternion.Euler(gameObject.transform.localRotation.x,
																	gameObject.transform.localRotation.y - 90,
																	gameObject.transform.localRotation.z);
	}



	void OnMouseUp()
	{
		if (map.selectedUnit != null)
		{
			if (map.selectedUnit.GetComponent<ShotScript>() != null)
				if (!map.selectedUnit.GetComponent<ShotScript>().set)
					map.GeneratePathTo(tileX, tileY, tileZ);

			if (map.selectedUnit.GetComponent<CharacterStats>() != null)
				if (map.selectedUnit.GetComponent<CharacterStats>().defending)
					map.selectedUnit.GetComponent<CharacterStats>().selectedTile = this;
				else if (!map.selectedUnit.GetComponent<UnitBasics>().full)
				{
					map.selectedUnit.GetComponent<CharacterStats>().actions.Add("Move");
					map.GeneratePathTo(tileX, tileY, tileZ);
				}
		}
	}

	public void DefenceReset()
	{
		defendFloor = 0;
		defendCeiling = 0;
		defendNorth = 0;
		defendWest = 0;
		defendEast = 0;
		defendSouth = 0;
	}
}

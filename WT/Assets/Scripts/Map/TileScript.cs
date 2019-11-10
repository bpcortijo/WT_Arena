using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

public class TileScript : NetworkBehaviour {
    public MapMaker map;
	public string effect = null;
	public int tileX, tileY, tileZ;
	public GameObject shieldPrefab;
	public bool floor, ceiling, northViable, westViable, eastViable, southViable;

	[SyncVar]
	public bool hasUnit = false;
	public int defendFloor, defendCeiling, defendNorth, defendWest, defendEast, defendSouth;
	[SyncVar]
	public int finalFloorDef, finalCeilingDef, finalNorthDef, finalWestDef, finalEastDef, finalSouthDef;

	[HideInInspector]
	public TileType tt;
	public float movementcost = 1f;
	public TileType.typeForArt direction;

	void Start()
	{
		CmdDefenceReset();

		gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x,
									gameObject.transform.localPosition.y + .9f,
									gameObject.transform.localPosition.z);
		tt.movementcost = movementcost;
		if (NetworkServer.active)
		{
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
			NetworkServer.Spawn(gameObject);
		}
		else
			gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x,
												gameObject.transform.localPosition.y - .9f,
												gameObject.transform.localPosition.z);
	}

	[ClientRpc]
	void RpcFindMap()
	{
		name = "("+ tileX + ", " + tileY + ", " + tileZ + ")";
		transform.parent = map.gameObject.transform;
	}

	[ClientRpc]
	public void RpcSendDef()
	{
		finalWestDef += defendWest;
		finalEastDef += defendEast;
		finalNorthDef += defendNorth;
		finalSouthDef += defendSouth;
		finalFloorDef += defendFloor;
		finalCeilingDef += defendCeiling;
	}

	void OnMouseUp()
	{
		if (!EventSystem.current.IsPointerOverGameObject()&& map.selectedUnit != null)
			if (map.selectedUnit.GetComponent<UnitBasics>().LocalCheck())
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

	private void Update()
	{
		if (defendWest < finalWestDef)
			defendWest = finalWestDef;
		if (defendEast < finalEastDef)
			defendEast = finalEastDef;
		if (defendNorth < finalNorthDef)
			defendNorth = finalNorthDef;
		if (defendSouth < finalSouthDef)
			defendSouth = finalSouthDef;
		if (defendFloor < finalFloorDef)
			defendFloor = finalFloorDef;
		if (defendCeiling < finalCeilingDef)
			defendCeiling = finalCeilingDef;
	}

	[Command]
	public void CmdDefenceReset()
	{
		defendWest = 0;
		defendEast = 0;
		defendNorth = 0;
		defendSouth = 0;
		defendFloor = 0;
		defendCeiling = 0;
		finalWestDef = 0;
		finalEastDef = 0;
		finalNorthDef = 0;
		finalSouthDef = 0;
		finalFloorDef = 0;
		finalCeilingDef = 0;
	}

	public void CreateVisualShields (string dir)
	{
		if (transform.Find(dir + " shield") == null)
		{
			GameObject shield = Instantiate(shieldPrefab, transform);
			shield.name = dir + " shield";
			switch (dir)
			{
				case "North":
					shield.transform.localPosition = new Vector3(0, .05f, .0035f);
					shield.transform.localScale = new Vector3(0.8f, 8, .01f);
					break;

				case "South":
					shield.transform.localPosition = new Vector3(0, .05f, -.0035f);
					shield.transform.localScale = new Vector3(0.8f, 8, .01f);
					break;

				case "West":
					shield.transform.localPosition = new Vector3(-.0035f, .05f, 0);
					shield.transform.localScale = new Vector3(0.8f, 8, .01f);
					shield.transform.localEulerAngles = new Vector3(0, 90, 0);
					break;

				case "East":
					shield.transform.localPosition = new Vector3(.0035f, .05f, 0);
					shield.transform.localScale = new Vector3(0.8f, 8, .01f);
					shield.transform.localEulerAngles = new Vector3(0, 90, 0);
					break;

				case "Up":
					shield.transform.localPosition = new Vector3(0, .05f, .005f);
					shield.transform.localScale = new Vector3(0.8f, .1f, .8f);
					break;

				case "Down":
					shield.transform.localPosition = new Vector3(0, .05f, .005f);
					shield.transform.localScale = new Vector3(0.8f, .1f, .8f);
					break;

				default:
					break;
			}
		}
	}

	public void RemoveShield(string dir)
	{
		//play shield off animation
		Destroy(transform.Find(dir + " shield").gameObject);
	}

	public void BreakShield(string dir)
	{
		//play breakshield animation
		Destroy(transform.Find(dir + " shield").gameObject);
	}
}

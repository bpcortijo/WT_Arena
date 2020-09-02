using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class TileScript : MonoBehaviour {
	int xRot=0, yRot = 0, zRot = 0;
	float xPos = 0, yPos = 0, zPos = 0;
	public MapMaker map;
	public string effect = null;
	public int tileX, tileY, tileZ;
	public GameObject shieldPrefab;
	public bool floor, ceiling, northViable, westViable, eastViable, southViable, stairs;
	public TileType.stairBottom stairBottom;
	public MapMaker.Node node;

	//public int defendFloor, defendCeiling, defendNorth, defendWest, defendEast, defendSouth;

	//public Dictionary<CharacterStats, string> defenders = new Dictionary<CharacterStats, string>();

	[HideInInspector]
	public TileType tt;
	public bool full = false;
	public int movementcost = 1;
	public TileType.typeForArt direction;

	public string prefabName;

	MeshRenderer rend;
	public float r,g,b;
	public bool inView;

	void Start()
	{
		node = map.graph[tileX, tileY, tileZ];
		DefenceReset();
		tt.movementcost = movementcost;
		rend = GetComponent<MeshRenderer>();
		r = rend.material.color.r;
		g = rend.material.color.g;
		b = rend.material.color.b;

		yPos = .9f;
		if (direction == TileType.typeForArt.flat)
			yPos -= .45f;
		else if (direction == TileType.typeForArt.nWallFC || direction == TileType.typeForArt.nWallNone || direction == TileType.typeForArt.nwCorner)
			yRot = 180;
		else if (direction == TileType.typeForArt.wWallFC || direction == TileType.typeForArt.wWallNone || direction == TileType.typeForArt.swCorner)
			yRot = 90;
		else if (direction == TileType.typeForArt.eWallFC || direction == TileType.typeForArt.eWallNone || direction == TileType.typeForArt.neCorner)
			yRot = -90;
		else if (direction == TileType.typeForArt.empty && ceiling && !floor)
			yPos += .45f;

		if (!floor)
		{
			if (direction >= TileType.typeForArt.neCorner && direction <= TileType.typeForArt.nwCorner)
				zRot += 90;
			else if (ceiling && direction >= TileType.typeForArt.nWallFC && direction <= TileType.typeForArt.wWallFC)
				xRot += 90;
			else if (direction == TileType.typeForArt.nWallFC || direction == TileType.typeForArt.nWallNone)
				zPos += .45f;
			else if (direction == TileType.typeForArt.wWallFC || direction == TileType.typeForArt.wWallNone)
				xPos -= .45f;
			else if (direction == TileType.typeForArt.eWallFC || direction == TileType.typeForArt.eWallNone)
				xPos += .45f;
			else if (direction == TileType.typeForArt.sWallFC || direction == TileType.typeForArt.sWallNone)
				zPos -= .45f;
		}

		if (prefabName.Contains("Stair"))
		{
			if (prefabName.Contains("North"))
			{
				yRot = 180;
				stairBottom = TileType.stairBottom.north;
			}
			else if (prefabName.Contains("West"))
			{
				yRot = 90;
				stairBottom = TileType.stairBottom.west;
			}
			else if (prefabName.Contains("East"))
			{
				yRot = -90;
				stairBottom = TileType.stairBottom.east;
			}
			else
				stairBottom = TileType.stairBottom.south;

			if (prefabName.Contains("End"))
				yPos = 1f;
			else
			{
				yPos += .34f;
				zPos += .09f;
			}
		}
		else
			stairBottom = TileType.stairBottom.none;
	

	gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x + xPos,
															gameObject.transform.localPosition.y + yPos,
															gameObject.transform.localPosition.z + zPos);
		gameObject.transform.localRotation = Quaternion.Euler(gameObject.transform.localRotation.x + xRot,
																gameObject.transform.localRotation.y + yRot,
																gameObject.transform.localRotation.z + zRot);
	}

	void OnMouseUp()
	{
		if (!EventSystem.current.IsPointerOverGameObject()&& map.selectedUnit != null)
			if (map.selectedUnit.GetComponent<UnitBasics>().LocalCheck())
			{
				if (map.selectedUnit.GetComponent<ShotScript>() != null)
					if (map.selectedUnit.GetComponent<ShotScript>().owner.GetComponent<CharacterStats>().player.isLocalPlayer)
						if (!map.selectedUnit.GetComponent<ShotScript>().set)
							map.GeneratePathTo(tileX, tileY, tileZ);

				if (map.selectedUnit.GetComponent<CharacterStats>() != null)
					if (map.selectedUnit.GetComponent<CharacterStats>().player.isLocalPlayer)
						if (map.selectedUnit.GetComponent<CharacterStats>().defending)
							map.selectedUnit.GetComponent<CharacterStats>().selectedTile = this;
						else if (!map.selectedUnit.GetComponent<UnitBasics>().fullPath)
						{
							map.selectedUnit.GetComponent<CharacterStats>().actions.Add("Move");
							map.GeneratePathTo(tileX, tileY, tileZ);
						}
			}
	}

	public void DefenceReset()
	{
		//defendWest = 0;
		//defendEast = 0;
		//defendNorth = 0;
		//defendSouth = 0;
		//defendFloor = 0;
		//defendCeiling = 0;
		//defenders.Clear();
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

	private void Update()
	{
		if (inView)
			rend.material.color = new Color(r, g, b);
		else
			rend.material.color = new Color(r / 1.25f, g / 1.25f, b / 1.25f);

		RaycastHit hitInfo = new RaycastHit();
		if (Physics.Raycast(Camera.main.transform.position, transform.position - Camera.main.transform.position,
			1000, 8))
			Debug.Log("hi");
	}

	public void CheckView()
	{
		inView = false;
		foreach (CharacterStats character in FindObjectsOfType<CharacterStats>())
			if (character.GetComponent<UnitBasics>().fov.Contains(node))
				inView = true;
	}

}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShotScript : MonoBehaviour
{
	MapMaker map;
	public GameObject owner;
	public UnitBasics basics;
	public PlayerScript player;
	public MapMaker.Node origin;
	public int speed, power, range;
	public bool set = false, firstTurn = true;

	void Start()
	{
		map = owner.GetComponent<UnitBasics>().map;
		basics = gameObject.GetComponent<UnitBasics>();
		basics.map = map;
		basics.speed = speed;
		basics.turns = range/speed;

		basics.timePerMove = FindObjectOfType<ManagementScript>().resultsTimer / speed;

		player.Spawn(gameObject);
	}

	public void DamageOwner()
	{
		firstTurn = false;
		owner.GetComponent<CharacterStats>().health -= Mathf.RoundToInt(Mathf.Pow(2, power));
		Debug.Log("DamageOwner");
	}

	void Update()
    {
		if (map.selectedUnit != gameObject)
			if (basics.plannedPath.Count == 1)
			{
				owner.GetComponent<CharacterStats>().CancelAction("shot");
				Debug.Log("RIP");
				Destroy(gameObject);
			}
		if (Input.GetMouseButtonUp(1) && !set)
		{
			if (map.selectedUnit = gameObject)
				ShotClear();
		}
	}

	void ThroughWall(TileScript ts, string direction)
	{
		int priorPower = power;

		switch (direction)
		{
			case "North":
				power -= ts.defendNorth;
				ts.defendNorth -= priorPower;
				break;
			case "West":
				power -= ts.defendWest;
				ts.defendWest -= priorPower;
				break;
			case "East":
				power -= ts.defendEast;
				ts.defendEast -= priorPower;
				break;
			case "South":
				power -= ts.defendSouth;
				ts.defendSouth -= priorPower;
				break;
			case "Up":
				power -= ts.defendCeiling;
				ts.defendCeiling -= priorPower;
				break;
			case "Down":
				power -= ts.defendFloor;
				ts.defendFloor -= priorPower;
				break;
		}

		if (priorPower != power)
			foreach (CharacterStats defender in ts.defenders.Keys)
				if (ts.defenders[defender] == direction)
					defender.freeShots++;

		Debug.Log("ThruWall");
	}

	public void ShotClear()
	{
		// Destroy path
		int n = basics.plannedPath.Count - 1;

		if (basics.keyPoints.Count > 2)
		{
			while (basics.plannedPath[n] != basics.keyPoints[basics.keyPoints.Count - 2])
			{
				basics.plannedPath.Remove(basics.plannedPath[n]);
				n--;
			}
			basics.keyPoints.Remove(basics.keyPoints[basics.keyPoints.Count - 1]);
		}
		else
		{
			basics.plannedPath = null;
			basics.keyPoints.Clear();
		}

		basics.CheckPath();
		Debug.Log("ShotClr");
	}

	void CheckHit(MapMaker.Node currentSpace, int index)
	{
		// If the shot his a wall lose 1 power
		TileScript currentTile = map.GetTileFromNode(currentSpace);
		if (currentSpace.x < basics.plannedPath[index + 1].x && !currentTile.eastViable ||
			currentSpace.x > basics.plannedPath[index + 1].x && !currentTile.westViable ||
			currentSpace.y < basics.plannedPath[index + 1].y && !currentTile.ceiling ||
			currentSpace.y > basics.plannedPath[index + 1].y && !currentTile.floor ||
			currentSpace.z < basics.plannedPath[index + 1].z && !currentTile.northViable ||
			currentSpace.z > basics.plannedPath[index + 1].z && !currentTile.southViable)
			power--;
		Debug.Log("CheckHit");
	}

	public void Impact(CharacterStats character, float percent, bool canBeCrit) // Deal damage
	{
		int damage = Mathf.CeilToInt(Mathf.Pow(2, 2 + power) * percent);
		if (canBeCrit)
			if (percent == 1f)
				character.DamageCharacter(damage, CharacterStats.DamageTypes.Shot, 3, owner.GetComponent<CharacterStats>());
			else if (percent >= .66f)
				character.DamageCharacter(damage, CharacterStats.DamageTypes.Shot, 2, owner.GetComponent<CharacterStats>());
			else if (percent >= .25f)
				character.DamageCharacter(damage, CharacterStats.DamageTypes.Shot, 1, owner.GetComponent<CharacterStats>());
			else
				character.DamageCharacter(damage, CharacterStats.DamageTypes.Shot, 0, owner.GetComponent<CharacterStats>());
		else
			character.DamageCharacter(damage, CharacterStats.DamageTypes.Shot, 0, owner.GetComponent<CharacterStats>());
		Debug.Log("Impact");
	}

	public void AfterTurn()
	{
		set = true;
		basics.CheckPath();
		Debug.Log("AfterTurn");
	}
}

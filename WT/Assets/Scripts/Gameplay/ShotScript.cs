using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotScript : MonoBehaviour
{
	MapMaker map;
	public bool set = false;
	public UnitBasics basics;
	public int speed, power, range;
	public GameObject target, owner;

	void Start()
	{
		map = owner.GetComponent<UnitBasics>().map;
		basics.map = map;
		basics.speed = speed;
		basics.turns = range;
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

	public void Attack()
	{
		basics.Move();
		basics.turns--;
		if (gameObject.name == "Hunter")
		{
			Hunter();
			basics.CheckPath();
		}
	}

	public void Hunter()
	{
		int targetX, targetY, targetZ;
		GameObject prevUnit = map.selectedUnit;
		map.selectedUnit = gameObject;
		if (target.GetComponent<UnitBasics>() != null)
		{
			targetX = target.GetComponent<UnitBasics>().tileX;
			targetY = target.GetComponent<UnitBasics>().tileY;
			targetZ = target.GetComponent<UnitBasics>().tileZ;
		}
		else
		{
			targetX = target.GetComponent<TileScript>().tileX;
			targetY = target.GetComponent<TileScript>().tileY;
			targetZ = target.GetComponent<TileScript>().tileZ;
		}

		map.GeneratePathTo(targetX, targetY, targetZ);

		map.selectedUnit = prevUnit;
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
	}

	void CheckHit(MapMaker.Node currentSpace, int index)
	{
		// If the shot his a wall lose 1 power
		TileScript currentTile = map.GetTileFromNode(currentSpace);
		if (currentSpace.x < basics.plannedPath[index + 1].x && !currentTile.eastViable||
			currentSpace.x > basics.plannedPath[index + 1].x && !currentTile.westViable ||
			currentSpace.y < basics.plannedPath[index + 1].y && !currentTile.ceiling ||
			currentSpace.y > basics.plannedPath[index + 1].y && !currentTile.floor ||
			currentSpace.z < basics.plannedPath[index + 1].z && !currentTile.northViable ||
			currentSpace.z > basics.plannedPath[index + 1].z && !currentTile.southViable)
			power--;
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
	}
}

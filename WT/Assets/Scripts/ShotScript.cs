using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotScript : MonoBehaviour
{
	MapMaker map;
	public UnitBasics basics;
	public GameObject target, owner;
	public int speed, power, range;

	void Start()
	{
		map = owner.GetComponent<UnitBasics>().map;
		basics.map = map;
	}

	void Update()
    {
		if (map.selectedUnit != gameObject)
			if (basics.currentPath == null || basics.currentPath.Count == 1)
			{
				owner.GetComponent<CharacterStats>().CancelAction();
				Destroy(gameObject);
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

	public void VectorClear()
	{

	}

	public void Hit()
	{

	}
}

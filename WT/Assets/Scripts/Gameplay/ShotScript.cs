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
			if (basics.currentPath.Count == 1)
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
		int n = basics.currentPath.Count - 1;

		if (basics.keyPoints.Count > 2)
		{
			while (basics.currentPath[n] != basics.keyPoints[basics.keyPoints.Count - 2])
			{
				basics.currentPath.Remove(basics.currentPath[n]);
				n--;
			}
			basics.keyPoints.Remove(basics.keyPoints[basics.keyPoints.Count - 1]);
		}
		else
		{
			basics.currentPath = null;
			basics.keyPoints.Clear();
		}

		basics.CheckPath();
	}

	public void Impact(GameObject hit, int defense)
	{
		if (hit.tag == "Player")
		{
			hit.GetComponent<CharacterStats>().DamageCharacter(power, CharacterStats.DamageTypes.Shot);
			Destroy(gameObject);
		} else { 
			if (name == "Hunter" || name == "Composite" || power <= 0)
				Destroy(gameObject);
			else
			power -= defense;
		}
	}
}

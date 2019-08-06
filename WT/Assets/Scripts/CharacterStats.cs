using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
	public int age;
	public GameObject headShot, bodyShot;

	int speed, health;

	[HideInInspector]
	public int actions = 2;
	[HideInInspector]
	public UnitBasics basics;
	[HideInInspector]
	public bool fours = false;

	public List<string> loadout;
	public GameObject[] shotTypes;
	public bool attacking, bleeding = false;
	public int agility = 1, energy = 1, experience = 1;
	public enum DamageTypes {Shot, Stabbed, Slashed, Crushed}

	void Start()
    {
		speed = agility;
		health = energy * 15;
		basics = gameObject.GetComponent<UnitBasics>();
		basics.speed = speed;
    }

	// Update is called once per frame
	void Update()
	{
		//if (Input.GetKeyUp(KeyCode.Return))
		//{
		//	if (actions.Count <= 2)
		//		actions.Add(currentAction);
		//	else
		//		Debug.Log("Too many actions");
		//}

		if (basics.map.selectedUnit == gameObject)
		{
			if (Input.GetKeyUp(KeyCode.Z))
				LoadChamber(shotTypes[0], "Z");
			if (Input.GetKeyUp(KeyCode.X))
				LoadChamber(shotTypes[1],"X");
			if (Input.GetKeyUp(KeyCode.C))
				LoadChamber(shotTypes[2],"Composite");
			if (Input.GetKeyUp(KeyCode.V))
				LoadChamber(shotTypes[3], "Vector");
		}
	}

	public void TakeActions()
	{
		while (actions>0)
		{ 
			basics.Move();
			actions--;
			basics.currentPath = null;
			basics.CheckPath();
		}
	}

	void Undo()
	{
		basics.CheckPath();
	}

	public void LoadChamber(GameObject ammo, string attackName)
	{
		actions--;
		Vector3 adjustedPos = new Vector3(transform.position.x, 
											transform.position.y + basics.unitHeight / 2, 
											transform.position.z);
		GameObject shot = Instantiate(ammo, adjustedPos, Quaternion.identity);
		ShotScript ss = shot.GetComponent<ShotScript>();

		ss.owner = gameObject;

		basics.map.selectedUnit = shot;

		shot.GetComponent<UnitBasics>().tileX = basics.tileX;
		shot.GetComponent<UnitBasics>().tileY = basics.tileY;
		shot.GetComponent<UnitBasics>().tileZ = basics.tileZ;
		shot.transform.position = basics.map.TileCoordToWorldCoord(basics.tileX, 
																	basics.tileY, 
																	basics.tileZ);
		shot.name = attackName;
		basics.CheckPath();
	}

	public void CancelAction()
	{
		actions++;
		basics.CheckPath();
	}

	public void DamageCharacter(int damage, DamageTypes damageType)
	{
		health -= damage * 4;
		if (health <= 0)
			Die(damageType);
	}

	void Die(DamageTypes causeOfDeath)
	{
		Destroy(gameObject);
	}
}

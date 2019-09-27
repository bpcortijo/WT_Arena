using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterStats : MonoBehaviour
{
	public int age;
	public GameObject headShot, bodyShot;

	public int speed, health, shot, ammo, defence = 2;

	[HideInInspector]
	public int movementActions = 2;
	[HideInInspector]
	public UnitBasics basics;
	[HideInInspector]
	public bool fours = false;
//	[HideInInspector]
	public List<string> actions = new List<string>();

	public List<string> loadout;
	public GameObject[] shotTypes;
	public int agility = 1, energy = 1, experience = 1;
	public bool attacking = false, reloading = false, defending = false, severed=false;
	public int bleeding = 0, disabled = 0;

	List<GameObject> shooting = new List<GameObject>();
	List<string> defendingDirections = new List<string>();
	public List<TileScript> defendingTiles = new List<TileScript>();
	Dictionary<CharacterStats, int> damageTracker = new Dictionary<CharacterStats, int>();

	public TileScript selectedTile = null;

	public enum CritFX { Bleed, Sever, True }
	public enum DamageTypes { Shot, Stabbed, Slashed, Bleed }

	void Start()
    {
		shot = 0;
		ammo = 4;
		speed = agility;
		health = energy * 15;
		basics = gameObject.GetComponent<UnitBasics>();
		basics.speed = speed;
		basics.vector = true;
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
			//applies to all reloads except 4shot aka pistol
			if (reloading)
			{
				if (Input.GetKeyUp(KeyCode.Alpha1))
				{
					shot = 0;
					reloading = false;
				}
					if (Input.GetKeyUp(KeyCode.Alpha2)) { }
				{
					shot = 1;
					reloading = false;
				}
				if (Input.GetKeyUp(KeyCode.Alpha3)) { }
				{
					shot = 2;
					reloading = false;
				}
				if (Input.GetKeyUp(KeyCode.Alpha4)) { }
				{
					shot = 3;
					reloading = false;
				}
			}
			else if (movementActions > 0)
			{
				if (Input.GetKeyUp(KeyCode.S))
				{
					if (loadout.Contains("4shot"))
						PistolShot();
					if (loadout.Contains("SMG"))
						SubShot();
					if (loadout.Contains("AR"))
						RifleShot();
					if (loadout.Contains("Sniper"))
						SniperShot();
					if (loadout.Contains("Shotgun"))
						ShotgunShot();
					actions.Add("Shoot");
				}
				if (Input.GetKeyUp(KeyCode.R))
				{
					actions.Add("Reload");
					if (loadout.Contains("4shot"))
						Reload(0);
					else
						reloading = true;
				}
			}

			if (Input.GetKeyUp(KeyCode.D))
				if (defending)
					defending = false;
				else
					defending = true;

			if (defending && defendingTiles.Count >= defence)
			{
				defending = false;
				selectedTile = null;
			}

			if (Input.GetMouseButtonUp(1))
				if (actions.Count > 0)
					CancelAction(actions[actions.Count - 1]);
		}
	}

	public void TakeActions()
	{
		defending = false;
		while (movementActions>0)
		{
			basics.Move();
			movementActions--;
		}

		basics.CheckPath();
		basics.full = false;
		basics.plannedPath = null;
		basics.keyPoints.Clear();
		movementActions = 2;
	}

	void Undo()
	{
		basics.CheckPath();
	}

	void PistolShot()
	{
		shot++;
		movementActions--;
		if (ammo > 0)
		{
			CockTrigger(shotTypes[shot % 4], shotTypes[shot % 4].name, 4, 5, 4);
			ammo--;
		}
		else
			Debug.Log("Out of Ammo");
	}

	void SuperPistolShot()
	{
		CockTrigger(shotTypes[shot % 4], shotTypes[shot % 4].name, 6, 6, 6);
	}

	void SubShot()
	{
		movementActions--;
		if (ammo > 0)
		{
			CockTrigger(shotTypes[shot], shotTypes[shot].name, 6, 2, 6);
			ammo--;
		}
		else
			Debug.Log("Out of Ammo");
	}

	void SuperSubShot()
	{
		CockTrigger(shotTypes[shot], shotTypes[shot].name, 6, 2, 6);
		CockTrigger(shotTypes[shot], shotTypes[shot].name, 6, 2, 6);
	}

	void RifleShot()
	{
		movementActions--;
		if (ammo > 0)
		{
			CockTrigger(shotTypes[shot], shotTypes[shot].name, 4, 3, 8);
			ammo--;
		}
		else
			Debug.Log("Out of Ammo");
	}

	void SuperRifleShot()
	{
		CockTrigger(shotTypes[shot], shotTypes[shot].name, 6, 6, 6);
	}

	void ShotgunShot()
	{
		movementActions--;
		if (ammo > 0)
		{
			CockTrigger(shotTypes[shot], shotTypes[shot].name, 3, 7, 3);
			ammo--;
		}
		else
			Debug.Log("Out of Ammo");
	}

	void SuperShotgunShot()
	{
		CockTrigger(shotTypes[shot], shotTypes[shot].name, 3, 7, 3);
	}

	void SniperShot()
	{
		movementActions--;
		if (ammo > 0)
		{
			CockTrigger(shotTypes[shot], shotTypes[shot].name, 5, 6, 15);
			ammo--;
		}
		else
			Debug.Log("Out of Ammo");
	}

	void SuperSniperShot()
	{
		CockTrigger(shotTypes[shot], shotTypes[shot].name, 15, 6, 15);
	}

	void Reload(int shotType)
	{
		if (loadout.Contains("4shot"))
		{
			movementActions--;
			ammo = 4;
			shot = shot % 4;
		}

		if (loadout.Contains("AR"))
		{
			movementActions--;
			ammo = 10;
			shot = shotType;
		}

		if (loadout.Contains("SMG"))
		{
			movementActions--;
			ammo = 15;
			shot = shotType;
		}

		if (loadout.Contains("Shotgun"))
		{
			movementActions--;
			ammo = 2;
			shot = shotType;
		}

		if (loadout.Contains("Sniper"))
		{
			movementActions--;
			ammo = 1;
			shot = shotType;
		}

		actions.Add("Reload");
	}

	void CockTrigger(GameObject ammo, string attackName, int speed, int power, int range)
	{
		Vector3 adjustedPos = new Vector3(transform.position.x, 
											transform.position.y + basics.unitHeight / 2, 
											transform.position.z);
		GameObject shot = Instantiate(ammo, adjustedPos, Quaternion.identity);
		ShotScript ss = shot.GetComponent<ShotScript>();

		ss.speed = speed;
		ss.power = power;
		ss.range = range;
		ss.owner = gameObject;

		basics.map.selectedUnit = shot;

		shot.GetComponent<UnitBasics>().tileX = basics.tileX;
		shot.GetComponent<UnitBasics>().tileY = basics.tileY;
		shot.GetComponent<UnitBasics>().tileZ = basics.tileZ;
		shot.transform.position = basics.map.TileCoordToWorldCoord(basics.tileX, 
																	basics.tileY, 
																	basics.tileZ);
		shot.name = attackName;
		shooting.Add(shot);
		basics.CheckPath();
	}

	void UncockTrigger()
	{
		Destroy(shooting[shooting.Count - 1]);
		shooting.RemoveAt(shooting.Count - 1);
	}

	public void BlockTile(string direction)
	{
		actions.Add("Block");
		defendingTiles.Add(selectedTile);
		defendingDirections.Add(direction);

		switch (direction)
		{
			case "North":
				selectedTile.defendNorth++;
				selectedTile.CreateVisualShields(direction);
				break;
			case "West":
				selectedTile.defendWest++;
				selectedTile.CreateVisualShields(direction);
				break;
			case "East":
				selectedTile.defendEast++;
				selectedTile.CreateVisualShields(direction);
				break;
			case "South":
				selectedTile.defendSouth++;
				selectedTile.CreateVisualShields(direction);
				break;
			case "Up":
				selectedTile.defendCeiling++;
				selectedTile.CreateVisualShields(direction);
				break;
			case "Down":
				selectedTile.defendFloor++;
				selectedTile.CreateVisualShields(direction);
				break;
			default:
				break;
		}
	}

	public void StopBlocking()
	{
		int i = defendingTiles.Count - 1;
		switch (defendingDirections[i])
		{
			case "North":
				defendingTiles[i].defendNorth--;
				if (defendingTiles[i].defendNorth == 0)
					defendingTiles[i].RemoveShield(false, defendingDirections[i]);
				break;
			case "West":
				defendingTiles[i].defendWest--;
				if (defendingTiles[i].defendWest == 0)
					defendingTiles[i].RemoveShield(false, defendingDirections[i]);
				break;
			case "East":
				defendingTiles[i].defendEast--;
				if (defendingTiles[i].defendEast == 0)
					defendingTiles[i].RemoveShield(false, defendingDirections[i]);
				break;
			case "South":
				defendingTiles[i].defendSouth--;
				if (defendingTiles[i].defendSouth == 0)
					defendingTiles[i].RemoveShield(false, defendingDirections[i]);
				break;
			case "Up":
				defendingTiles[i].defendCeiling--;
				if (defendingTiles[i].defendCeiling == 0)
					defendingTiles[i].RemoveShield(false, defendingDirections[i]);
				break;
			case "Down":
				defendingTiles[i].defendFloor--;
				if (defendingTiles[i].defendFloor == 0)
					defendingTiles[i].RemoveShield(false, defendingDirections[i]);
				break;
			default:
				break;
		}

		defendingTiles.RemoveAt(i);
		defendingDirections.RemoveAt(i);
	}

	public void CancelAction(string act)
	{
		if (act != null)
		{
			if (act == "Move")
				MoveClear();

			else if (act == "Block")
				StopBlocking();

			else if (act=="Reload")
				reloading = false;

			else
			{
				movementActions++;
				basics.CheckPath();
			}

			if (act == "Shoot")
			{
				if (loadout.Contains("4shot"))
					shot--;
				ammo++;
				UncockTrigger();
			}
		}
		actions.Remove(act);
	}

	public void MoveClear()
	{
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

	public void DamageCharacter(float damage, DamageTypes damageType, int crit, CharacterStats damageDealer)
	{
		switch(crit)
		{
			case 1:
				bleeding++;
				break;
			case 2:
				bleeding++;
				severed=true;
				break;
			case 3:
				damage*=4;
				bleeding += 4;
				break;
			default:
				break;
		}

		int recievingDamage;

		if (damageType != DamageTypes.Bleed)
			recievingDamage = Mathf.RoundToInt(damage * 4);
		else
			recievingDamage = Mathf.RoundToInt(damage);

		if (!damageTracker.ContainsKey(damageDealer))
			damageTracker.Add(damageDealer, recievingDamage);
		else
			damageTracker[damageDealer] += recievingDamage;

		health -= recievingDamage;
		if (health <= 0)
			Die(damageType);
	}

	void Die(DamageTypes causeOfDeath)
	{
		Destroy(gameObject);
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterStats : MonoBehaviour
{
	public int age;
	public GameObject headShot, bodyShot;

	int speed, health, shot, ammo;

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
	public bool attacking, bleeding = false, reloading = false;

	List<GameObject> shooting = new List<GameObject>();

	public enum DamageTypes {Shot, Stabbed, Slashed, Crushed}

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
					if (loadout.Contains("4shot"))
						Reload(0);
					else
						reloading = true;
				}
			}

			if (Input.GetMouseButtonUp(1))
				CancelAction(actions[actions.Count-1]);
		}
	}

	public void TakeActions()
	{
		while (movementActions>0)
		{ 
			basics.Move();
			movementActions--;
			basics.currentPath = null;
			basics.CheckPath();
		}
		basics.keyPoints.Clear();
		shot = Mathf.RoundToInt(Random.Range(-100f, 100f) / Time.deltaTime);
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

	public void CancelAction(string act)
	{
		if (act != null)
		{
			if (act == "Move")
			{
				MoveClear();
			}
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
		int n = basics.currentPath.Count - 1;

		if (basics.keyPoints.Count >= 2)
			while (basics.currentPath[n] != basics.keyPoints[basics.keyPoints.Count - 2])
			{
				basics.currentPath.Remove(basics.currentPath[n]);
				n--;
			}
		else
			while (n >= 0)
			{
				basics.currentPath.Remove(basics.currentPath[n]);
				n--;
			}

		basics.CheckPath();
		basics.keyPoints.Remove(basics.keyPoints[basics.keyPoints.Count - 1]);
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

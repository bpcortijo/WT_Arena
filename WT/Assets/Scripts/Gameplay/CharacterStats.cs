using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterStats : MonoBehaviour
{
	public int age;
	public GameObject headShot, bodyShot;

	public PlayerScript player;

	public int speed, health, shot, ammo;

	//Perks
	public bool fours = false, trust = false, quickstep = false, leadingShot = false, spare = false,
				sprint = false, disengage = false, energyRepurpse = false, recon = false,
				slowShot = false, longShot = false, meleeMaster = false;

	[HideInInspector]
	public UnitBasics basics;
	[HideInInspector]
	public int movementActions = 2;
	//[HideInInspector]
	public List<string> actions = new List<string>();

	public int disabled = 0;
	public GameObject shotType;
	public List<string> loadout;
	//Stats
	public int agility = 1, energy = 1, experience = 1, vision = 0;
	public bool attacking = false, reloading = false, defending = false, severed = false;

	public bool canHit;
	public MapMaker.Node meleeThis;
	List<GameObject> shooting = new List<GameObject>();
	public List<string> defendingDirections = new List<string>();
	public List<TileScript> defendingTiles = new List<TileScript>();

	List<CharacterStats> bleedCausers = new List<CharacterStats>();
	Dictionary<CharacterStats, int> damageTracker = new Dictionary<CharacterStats, int>();

	public TileScript selectedTile = null;

	public enum CritFX { Bleed, Sever, True }
	public enum DamageTypes { Shot, Slashed, Bleed, Choked }

	//Statuses
	public bool wrapped = false, stunned = false;

	public int freeShots = 0;

	void Start()
	{
		shot = 0;
		ammo = 4;
		speed = agility;
		health = energy * 15;
		basics = gameObject.GetComponent<UnitBasics>();
		basics.speed = speed;

		vision = experience;
		if (loadout.Contains("pistol"))
			vision += 5;
		else if (loadout.Contains("sniper"))
			vision += 8;
		//else if (loadout.Contains("shotgun"))
		//	vision += 4;

		if (fours)
			trust = true;
		if (spare)
			freeShots++;

		foreach (GameObject p in FindObjectOfType<ManagementScript>().players)
			if (basics.playerNum == p.GetComponent<PlayerScript>().num)
				player = p.GetComponent<PlayerScript>();

		transform.parent = player.transform;
		player.units.Add(gameObject);

		basics.timePerMove = FindObjectOfType<ManagementScript>().resultsTimer / speed;
	}

	// Update is called once per frame
	void Update()
	{
		if (basics.map.selectedUnit!=null)
		if (basics.map.selectedUnit == gameObject && basics.LocalCheck() && player.isLocalPlayer)
		{
			if (movementActions > 0)
			{
				if (!wrapped)
				{
					if (Input.GetKeyUp(KeyCode.S))
						CockTrigger(shotType, 10, 10, 10);
					//if (Input.GetKey(KeyCode.LeftShift) && disabled <= 0)
					//{
					//	if (loadout.Contains("pistol"))
					//		SuperPistolShot();
					//	if (loadout.Contains("sniper"))
					//		SuperSniperShot();
					//	if (loadout.Contains("shotgun"))
					//		SuperShotgunShot();

					//	actions.Add("Shoot");
					//}
					//else
					//{
					//	if (loadout.Contains("pistol"))
					//		PistolShot();
					//	if (loadout.Contains("sniper"))
					//		SniperShot();
					//	if (loadout.Contains("shotgun"))
					//		ShotgunShot();
					//	actions.Add("Shoot");
					//}

					if (Input.GetKeyUp(KeyCode.A))
						if (!attacking)
							attacking = true;
				}

				if (Input.GetKeyUp(KeyCode.R))
					Reload();

			}

			if (Input.GetKeyUp(KeyCode.D))
				if (defending)
					defending = false;
				else
					defending = true;

			if (attacking)
				attacking = false;

			if (Input.GetMouseButtonUp(1))
				if (actions.Count > 0)
					CancelAction(actions[actions.Count - 1]);
		}
	}

	public void AfterTurnReset()
	{
		defending = false;
		wrapped = false;

		foreach (CharacterStats bleedTick in bleedCausers)
			DamageCharacter(4, DamageTypes.Bleed, 0, bleedTick);

		basics.bonus = basics.nextBonus;
		basics.nextBonus = 0;
		movementActions = 2;

		basics.fullPath = false;
		basics.keyPoints.Clear();
		basics.shortPath.Clear();
		basics.plannedPath.Clear();
		basics.CheckPath();

	}

	void Undo()
	{
		basics.CheckPath();
	}

	void SpotAttack(MapMaker.Node node)
	{
		actions.Add("meleeFirst");
		meleeThis = node;
		attacking = false;
	}

	public void CheckMelee()
	{
		if (loadout.Contains("sword"))
			if (basics.map.Distance(basics.map.graph[basics.tileX, basics.tileY, basics.tileZ], meleeThis) == 1)
				foreach (CharacterStats ch in basics.map.characterPaths)
					if (basics.map.graph[ch.basics.tileX, ch.basics.tileY, ch.basics.tileZ] == meleeThis)
						if (meleeMaster)
							DamageCharacter(8, DamageTypes.Slashed, 2, this);
						else
							DamageCharacter(8, DamageTypes.Slashed, 1, this);

		if (loadout.Contains("wire"))
			if (basics.map.Distance(basics.map.graph[basics.tileX, basics.tileY, basics.tileZ], meleeThis) == 1)
				foreach (CharacterStats ch in basics.map.characterPaths)
					if (basics.map.graph[ch.basics.tileX, ch.basics.tileY, ch.basics.tileZ] == meleeThis)
						if (meleeMaster)
							DamageCharacter(2, DamageTypes.Choked, 1, this);
						else
							DamageCharacter(2, DamageTypes.Choked, 0, this);

		if (loadout.Contains("dagger"))
			foreach (CharacterStats ch in basics.map.characterPaths)
				if (basics.map.Distance(basics.map.graph[basics.tileX, basics.tileY, basics.tileZ],
					basics.map.graph[ch.basics.tileX, ch.basics.tileY, ch.basics.tileZ]) == 1)
					if (meleeMaster)
						DamageCharacter(4, DamageTypes.Slashed, 1, this);
					else
						DamageCharacter(4, DamageTypes.Slashed, 0, this);
	}

	void PistolShot()
	{
		shot++;
		movementActions--;
		int range = 5;
		if (longShot)
			range *= 2;


		if (ammo > 0)
		{
			if (slowShot)
				CockTrigger(shotType, 1, 2, range);
			else
				CockTrigger(shotType, 4, 2, range);
			ammo--;
		}
		else
			Debug.Log("Out of Ammo");
	}

	void SuperPistolShot()
	{
		movementActions--;
		int range = 6, power = 3;
		if (longShot)
			range *= 2;

		if (freeShots > 0)
		{
			CockTrigger(shotType, 6, power, range);
			freeShots--;
		}
		else if (health > power * 2)
		{
			CockTrigger(shotType, 6, power, range);
			health -= power * 2;
		}
	}

	void ShotgunShot()
	{
		movementActions--;
		int range = 4;
		if (longShot)
			range *= 2;

		if (ammo > 0)
		{
			if (slowShot)
				CockTrigger(shotType, 1, 3, range);
			else
				CockTrigger(shotType, 2, 3, range);
			ammo--;
		}
		else
			Debug.Log("Out of Ammo");
	}

	void SuperShotgunShot()
	{
		movementActions--;
		int range = 4, power = 3;
		if (longShot)
			range *= 2;

		if (freeShots > 0)
		{
			CockTrigger(shotType, 4, power, range);
			freeShots--;
		}
		else if (health > power * 2)
		{
			CockTrigger(shotType, 4, power, range);
			health -= power * 2;
		}
	}

	void SniperShot()
	{
		movementActions--;
		int range = 8;
		if (longShot)
			range *= 2;

		if (ammo > 0)
		{
			if (slowShot)
				CockTrigger(shotType,  1, 3, range);
			else
				CockTrigger(shotType, 4, 3, range);
			ammo--;
		}
		else
			Debug.Log("Out of Ammo");
	}

	void SuperSniperShot()
	{
		movementActions--;
		int range = 8, power = 4;
		if (longShot)
			range *= 2;

		if (freeShots > 0)
		{
			CockTrigger(shotType, 8, power, range);
			freeShots--;
		}
		else if (health > power * 2)
		{
			CockTrigger(shotType, 8, power, range);
			health -= power * 2;
		}
	}

	void Reload()
	{
		movementActions--;
		actions.Add("Reload");

		if (loadout.Contains("pistol"))
			ammo = 4;

		if (loadout.Contains("sniper"))
			ammo = 1;

		actions.Add("Reload");
	} 

	void CockTrigger(GameObject shotType, int speed, int power, int range)
	{
		if (longShot)
			range *= 2;

		Vector3 adjustedPos = new Vector3(transform.position.x, 
											transform.position.y + basics.unitHeight, 
											transform.position.z);

		GameObject shot = Instantiate(shotType);
		shot.transform.position = adjustedPos;
		shot.transform.localScale = new Vector3(.25f, .25f, .25f);
		ShotScript ss = shot.GetComponent<ShotScript>();
		ss.player = player;

		ss.speed = speed;
		ss.power = power;
		ss.range = range;
		ss.owner = gameObject;
		ss.basics.unitHeight = basics.unitHeight;

		basics.map.selectedUnit = shot;

		shot.GetComponent<UnitBasics>().tileX = basics.tileX;
		shot.GetComponent<UnitBasics>().tileY = basics.tileY;
		shot.GetComponent<UnitBasics>().tileZ = basics.tileZ;

		shot.name = "Vector";
		shooting.Add(shot);
		basics.CheckPath();
	}

	void UncockTrigger()
	{
		Destroy(shooting[shooting.Count - 1]);
		basics.map.selectedUnit = gameObject;
	}

	public void BlockTile(string direction)
	{
		//actions.Add("Block");
		//defendingTiles.Add(selectedTile);
		//defendingDirections.Add(direction);

		//if (energyRepurpse)
		//	selectedTile.defenders.Add(this, direction);

		//switch (direction)
		//{
		//	case "North":
		//		if (turtle)
		//		selectedTile.defendNorth+=3;
		//		else
		//			selectedTile.defendNorth++;
		//		selectedTile.CreateVisualShields(direction);
		//		break;
		//	case "West":
		//		if (turtle)
		//			selectedTile.defendWest += 3;
		//		else
		//			selectedTile.defendWest++;
		//		selectedTile.CreateVisualShields(direction);
		//		break;
		//	case "East":
		//		if (turtle)
		//			selectedTile.defendEast += 3;
		//		else
		//			selectedTile.defendEast++;
		//		selectedTile.CreateVisualShields(direction);
		//		break;
		//	case "South":
		//		if (turtle)
		//			selectedTile.defendSouth += 3;
		//		else
		//			selectedTile.defendSouth++;
		//		selectedTile.CreateVisualShields(direction);
		//		break;
		//	case "Up":
		//		if (turtle)
		//			selectedTile.defendCeiling += 3;
		//		else
		//			selectedTile.defendCeiling++;
		//		selectedTile.CreateVisualShields(direction);
		//		break;
		//	case "Down":
		//		if (turtle)
		//			selectedTile.defendFloor += 3;
		//		else
		//			selectedTile.defendFloor++;
		//		selectedTile.CreateVisualShields(direction);
		//		break;
		//	default:
		//		break;
		//}
	}

	public void StopBlocking()
	{
		//int i = defendingTiles.Count - 1;
		//switch (defendingDirections[i])
		//{
		//	case "North":
		//		defendingTiles[i].defendNorth--;
		//		if (defendingTiles[i].defendNorth == 0)
		//			defendingTiles[i].RemoveShield(defendingDirections[i]);
		//		break;
		//	case "West":
		//		defendingTiles[i].defendWest--;
		//		if (defendingTiles[i].defendWest == 0)
		//			defendingTiles[i].RemoveShield(defendingDirections[i]);
		//		break;
		//	case "East":
		//		defendingTiles[i].defendEast--;
		//		if (defendingTiles[i].defendEast == 0)
		//			defendingTiles[i].RemoveShield(defendingDirections[i]);
		//		break;
		//	case "South":
		//		defendingTiles[i].defendSouth--;
		//		if (defendingTiles[i].defendSouth == 0)
		//			defendingTiles[i].RemoveShield(defendingDirections[i]);
		//		break;
		//	case "Up":
		//		defendingTiles[i].defendCeiling--;
		//		if (defendingTiles[i].defendCeiling == 0)
		//			defendingTiles[i].RemoveShield(defendingDirections[i]);
		//		break;
		//	case "Down":
		//		defendingTiles[i].defendFloor--;
		//		if (defendingTiles[i].defendFloor == 0)
		//			defendingTiles[i].RemoveShield(defendingDirections[i]);
		//		break;
		//	default:
		//		break;
		//}

		//defendingTiles.RemoveAt(i);
		//defendingDirections.RemoveAt(i);
	}

	public void CancelAction(string act)
	{
		if (act != null)
		{
			if (act == "Move")
				MoveClear();

			else if (act == "meleeFirst")
				meleeThis = null;

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
				if (loadout.Contains("pistol"))
					shot--;

				if (shot <= 0)
					shot = 3;

				ammo++;
				UncockTrigger();
			}
		}
		actions.Remove(act);
	}

	public void MoveClear()
	{
		int n = basics.shortPath.Count - 1;

		if (basics.keyPoints.Count > 2)
		{
			while (basics.shortPath[n] != basics.keyPoints[basics.keyPoints.Count - 2])
			{
				basics.shortPath.Remove(basics.plannedPath[n]);
				n--;
			}
			basics.keyPoints.Remove(basics.keyPoints[basics.keyPoints.Count - 1]);
		}

		else
		{
			basics.plannedPath.Clear();
			basics.keyPoints.Clear();
		}

		basics.CheckPath();
	}

	public void MeleeHit(CharacterStats target)
	{
		Debug.Log(name + " punched " + target.name + " in the face");
		//determine melee weapon
		//target.DamageCharacter(0, DamageTypes.Slashed, 0, this);
	}

	public void DamageCharacter(int damage, DamageTypes damageType, int crit, CharacterStats damageDealer)
	{
		switch(crit)
		{
			case 1:
				bleedCausers.Add(damageDealer);
				break;
			case 2:
				bleedCausers.Add(damageDealer);
				severed = true;
				break;
			case 3:
				damage*=4;
				bleedCausers.Add(damageDealer);
				bleedCausers.Add(damageDealer);
				bleedCausers.Add(damageDealer);
				bleedCausers.Add(damageDealer);
				break;
			default:
				break;
		}

		if (damageType == DamageTypes.Slashed && disengage)
			basics.nextBonus += 3;

		if (damageDealer != null)
		{
			if (!damageTracker.ContainsKey(damageDealer))
				damageTracker.Add(damageDealer, damage);
			else
				damageTracker[damageDealer] += damage;

			if (damageDealer.recon)
				if (damageDealer.player.markedCharachters.ContainsKey(this))
					damageDealer.player.markedCharachters[this] = 3;
				else
					damageDealer.player.markedCharachters.Add(this, 3);
		}
		health -= damage;
		if (health <= 0)
			Die(damageType);
	}

	void Die(DamageTypes causeOfDeath)
	{
		foreach (PlayerScript player in FindObjectsOfType<PlayerScript>())
			if (player.markedCharachters.ContainsKey(this))
				player.markedCharachters.Remove(this);

		Destroy(gameObject);
	}
}

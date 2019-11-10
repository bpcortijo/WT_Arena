using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerScript : NetworkBehaviour
{
	public bool canEnd;
	public string pName;
	ManagementScript gm;
	public MapMaker map;
	public List<GameObject> units, characters;
	public bool s;

	private void Start()
	{
		name = pName;
		if (SceneManager.GetActiveScene().name=="Game")
		{
			gm = transform.parent.gameObject.GetComponent<ManagementScript>();
			Spawn();
		}
	}

	void Spawn()
	{
		foreach (GameObject character in characters)
		{
			GameObject spawnPoint = gm.map.GetComponent<MapMaker>().SpawnPoint();
			GameObject unit = Instantiate(character, new Vector3(spawnPoint.transform.position.x,
																	spawnPoint.transform.position.y,
																	spawnPoint.transform.position.z),
																	Quaternion.identity);

			unit.transform.parent = this.transform;

			unit.GetComponent<UnitBasics>().map = gm.map.GetComponent<MapMaker>();
			unit.GetComponent<UnitBasics>().tileX = (int)unit.transform.position.x;
			unit.GetComponent<UnitBasics>().tileY = (int)unit.transform.position.y;
			unit.GetComponent<UnitBasics>().tileZ = (int)unit.transform.position.z;
			units.Add(unit);
		}
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Return) && canEnd)
		{
			canEnd = false;
			PlayerEndTurn();
		}

		if (Input.GetKeyUp(KeyCode.Escape) && !canEnd)
		{
			canEnd = true;
			gm.endTurnRequests--;
		}

		if (characters.Count > 3)
			if (!CheckFour() || characters.Count > 4)
				characters.Remove(characters[0]);

	}

	public void PlayerEndTurn()
	{
		Debug.Log("pEnd");
		GameObject go = map.selectedUnit;
		foreach (GameObject unit in units)
		{
			UnitBasics basics = unit.GetComponent<UnitBasics>();
			if (basics.shortPath == null)
				map.AddToPath(basics, basics.tileX, basics.tileY, basics.tileZ);
		}

		map.selectedUnit = go;
		gm.endTurnRequests++;
	}

	public void TurnOver()
	{
		if (gm != null)
			canEnd = true;
		foreach (GameObject unit in units)
		{
			unit.GetComponent<CharacterStats>().AfterTurnReset();
			while (unit.GetComponent<CharacterStats>().defendingTiles.Count > 0)
				unit.GetComponent<CharacterStats>().StopBlocking();
		}
	}

	bool CheckFour()
	{
		int can4 = 0;
		foreach (GameObject character in characters)
		{
			if (character.GetComponent<CharacterStats>().fours)
				can4++;
		}
		if (can4 == 4)
			return true;
		else
			return false;
	}
}

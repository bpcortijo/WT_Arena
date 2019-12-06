using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class PlayerScript : NetworkBehaviour
{
	[SyncVar]
	public int num;
	public bool canEnd;
	public string pName;
	public MapMaker map;
	public ManagementScript gm;
	public List<GameObject> units, team;

	private void Start()
	{
		DontDestroyOnLoad(gameObject);
		gm = FindObjectOfType<ManagementScript>();

		if (isLocalPlayer)
		{
			FindObjectOfType<TeamSelect>().localPlayer = this;
			FindObjectOfType<DetailScript>().localPlayer = this;
			gm.localPlayer = this;
		}

		transform.parent = gm.transform;
		gm.players.Add(gameObject);
		gm.startingPlayers.Add(pName);
	}

	[ClientRpc]
	public void RpcEditTeam(GameObject character)
	{
		if (team.Contains(character))
			team.Remove(character);
		else
			team.Add(character);
	}
	 
	public void FillPlayerBlanks()
	{
		if (pName == null)
			pName = "No Name";

		PreGameScript freeAgency = FindObjectOfType<PreGameScript>();
		while (team.Count < 3)
		{
			int r = Random.Range(0, freeAgency.availableCharacters.Count - 1);
			if (!team.Contains(freeAgency.availableCharacters[r]))
				team.Add(freeAgency.availableCharacters[r]);
		}
	}

	public void SpawnTeam()
	{
		foreach (GameObject character in team)
		{
			GameObject spawnPoint = gm.map.GetComponent<MapMaker>().SpawnPoint();
			GameObject unit = Instantiate(character, new Vector3(spawnPoint.transform.position.x,
																	spawnPoint.transform.position.y,
																	spawnPoint.transform.position.z),
																	Quaternion.identity);

			unit.GetComponent<UnitBasics>().playerNum = num;
			unit.GetComponent<UnitBasics>().map = gm.map.GetComponent<MapMaker>();
			unit.GetComponent<UnitBasics>().tileX = (int)unit.transform.position.x;
			unit.GetComponent<UnitBasics>().tileY = (int)unit.transform.position.y;
			unit.GetComponent<UnitBasics>().tileZ = (int)unit.transform.position.z;

			NetworkServer.Spawn(unit);
			Debug.Log("Spawn");

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

		if (team.Count > 3)
			if (!CheckFour() || team.Count > 4)
				team.Remove(team[0]);

	}

	public void PlayerEndTurn()
	{
		Debug.Log("pEnd");
		foreach (GameObject unit in units)
		{
			UnitBasics basics = unit.GetComponent<UnitBasics>();
			if (basics.shortPath == null || basics.shortPath.Count == 0)
				basics.shortPath.Add(map.graph[basics.tileX, basics.tileY, basics.tileZ]);
		}
		gm.endTurnRequests++;
	}

	public void TurnOver()
	{
		if (gm != null)
			canEnd = true;
		foreach (GameObject unit in units)
		{
			if (isLocalPlayer)
				unit.GetComponent<CharacterStats>().AfterTurnReset();
			while (unit.GetComponent<CharacterStats>().defendingTiles.Count > 0)
				unit.GetComponent<CharacterStats>().StopBlocking();
		}
	}

	bool CheckFour()
	{
		int can4 = 0;
		foreach (GameObject character in team)
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

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
	public List<MapMaker.Node> visibleTiles = new List<MapMaker.Node>();
	public Dictionary<CharacterStats, int> markedCharachters = new Dictionary<CharacterStats, int>();

	[SyncVar]
	public string highlighted;

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
	public void RpcEditTeam()
	{
		GameObject character = null;
		foreach (GameObject ch in FindObjectOfType<PreGameScript>().availableCharacters)
			if (ch.name == highlighted)
				character = ch;

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

	public void Spawn(GameObject go)
	{
		NetworkServer.Spawn(go);
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

		foreach (CharacterStats character in markedCharachters.Keys)
		{
			markedCharachters[character]--;
			if (markedCharachters[character] <= 0)
				markedCharachters.Remove(character);
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

	public void CheckVisibility()
	{
		foreach (CharacterStats character in markedCharachters.Keys)
			map.GetTileFromNode(map.graph[character.basics.tileX,
									character.basics.tileY,
									character.basics.tileZ]).inView = true;

		foreach (UnitBasics unit in FindObjectsOfType<UnitBasics>())
			if (!units.Contains(unit.gameObject))
				if (map.GetTileFromNode(map.graph[unit.tileX, unit.tileY, unit.tileZ]).inView)
					Debug.Log("I can see " + unit.gameObject.name);
				else
					Debug.Log("I can't see " + unit.gameObject.name);
	}
}

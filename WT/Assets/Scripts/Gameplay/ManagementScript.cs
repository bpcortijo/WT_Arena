using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ManagementScript : NetworkBehaviour {
	MapMaker mapCode;
	public GameObject map;

	public PlayerScript localPlayer;
	public List<GameObject> players;
	public List<string> startingPlayers;

	bool inGame = false;
	public bool playResults = false;
	public float turnTimer = 90f, turnTime = 999f;
	public float resultsTime = 0, resultsTimer = 5f;
	[SyncVar]
	public int turn = 1, maxTurns = 60, endTurnRequests = 0;


	public void GameStart()
	{
		foreach (PlayerScript player in FindObjectsOfType<PlayerScript>())
				player.FillPlayerBlanks();
		NetworkManager.singleton.ServerChangeScene("Game");
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.name == "Game")
			if (NetworkServer.active)
			{
				map = Instantiate(map);
				mapCode = map.GetComponent<MapMaker>();
				mapCode.gm = this;
				foreach (PlayerScript player in FindObjectsOfType<PlayerScript>())
				{
					if (!players.Contains(player.gameObject))
						players.Add(player.gameObject);

					player.GetComponent<PlayerScript>().map = mapCode;
					player.GetComponent<PlayerScript>().num = players.IndexOf(player.gameObject);
				}
				NetworkServer.Spawn(map);

				turnTime = turnTimer + 30f;
				inGame = true;
				// Get Light
				CmdSpawnPlayers();
			}
	}

	[Command]
	void CmdSpawnPlayers()
	{
		NetworkServer.Spawn(localPlayer.gameObject);
		players.Add(localPlayer.gameObject);
		startingPlayers.Add(localPlayer.pName);
	}

	void Update()
    {
		if (inGame && !playResults)
			turnTime -= Time.deltaTime;

		if (playResults)
		{
			resultsTime += Time.deltaTime;

			if (resultsTime >= resultsTimer)
			{
				playResults = false;
				endTurnRequests = 0;
				resultsTime = 0;
				NextTurn();
			}
		}

		if (endTurnRequests == players.Count && endTurnRequests != 0 || turnTime <= 0)
			if (!playResults)
				TurnResults();
	}

	void TurnResults()
	{
		//GetAttackPaths();
		//GetCharacterPaths();
		//mapCode.GetAttackPaths();
		//foreach (CharacterStats ch in mapCode.characterPaths)
		//{
		//	ch.basics.ReMapMovement();
		//	ch.basics.SetPosition(resultsTimer);
		//	ch.basics.DrawLines();
		//	for (int i=0; i<ch.defendingTiles.Count; i++)
		//		RpcSendDef(ch.defendingTiles[i].name,ch.defendingDirections[i]);
		//}

		//foreach (ShotScript atk in mapCode.attackPaths)
		//{
		//	atk.basics.ReMapMovement();
		//	atk.basics.SetPosition(resultsTimer);
		//	atk.basics.DrawLines();
		//}


		//List<ShotScript> shots = new List<ShotScript>(FindObjectsOfType<ShotScript>());
		//foreach (ShotScript shot in shots)
		//{
		//	if (shot.player == localPlayer)
		//		foreach (MapMaker.Node node in shot.basics.shortPath)
		//			foreach (CharacterStats character in FindObjectsOfType<CharacterStats>())
		//				if (character.player != shot.player)
		//				{
		//					character.basics.TargetTimeToReach(connectionToClient,
		//														node.x,
		//														node.y,
		//														node.z,
		//														shot.basics.shortPath.IndexOf(node)
		//														);

		//					shot.basics.TargetRecieveTPM(connectionToClient, character.basics.tpm);
		//				}
		//	Debug.Log(shot.basics.timePerMove);
		//}

		playResults = true;
	}

	[ClientRpc]
	public void RpcSendDef(string name, string dir)
	{
		TileScript tile = map.transform.Find(name).GetComponent<TileScript>();
		switch (dir)
		{
			case "West":
				tile.defendWest++;
				break;
			case "East":
				tile.defendEast++;
				break;
			case "North":
				tile.defendNorth++;
				break;
			case "South":
				tile.defendSouth++;
				break;
			case "Floor":
				tile.defendFloor++;
				break;
			case "Ceiling":
				tile.defendCeiling++;
				break;
			default:
				break;
		}
	}

	float CheckForHit(ShotScript atk)
	{
		float timeToImpact = resultsTimer;

		foreach (CharacterStats unit in mapCode.characterPaths)
			if (unit.transform.parent != atk.owner.transform.parent)
				foreach (MapMaker.Node node in atk.basics.shortPath)
					if (unit.basics.shortPath.Contains(node))
					{
						float newTime = unit.basics.shortPath.IndexOf(node) * unit.basics.lerpTimePerSpace;
						if (timeToImpact > newTime)
							timeToImpact = newTime;
					}
		return timeToImpact;
	}

	void NextTurn()
	{
		Debug.Log("Next");

		// rotate light -.125 degrees on the x axis

		foreach (ShotScript atk in mapCode.attackPaths)
			atk.AfterTurn();

		turn++;
		StartCoroutine(EndTurn());
//		if (turn >= maxTurns || players.Count <= 1)
//			EndGame();
//		else
//		{
			foreach (GameObject p in players)
			{
				PlayerScript player = p.GetComponent<PlayerScript>();
				player.TurnOver();
				player.canEnd = true;
			}
			turnTime = turnTimer;
//		}
	}

	void GetAttackPaths()
	{
		foreach (ShotScript shot in FindObjectsOfType<ShotScript>())
				mapCode.attackPaths.Add(shot);
	}

	void GetCharacterPaths()
	{
		foreach (GameObject player in players)
			foreach (GameObject unit in player.GetComponent<PlayerScript>().units)
				mapCode.characterPaths.Add(unit.GetComponent<CharacterStats>());
	}

	public void EndGame()
	{
		foreach (Transform child in transform)
			Destroy(child.gameObject);
	}

	IEnumerator EndTurn()
	{
		mapCode.characterPaths.Clear();
		mapCode.attackPaths.Clear();
		yield return new WaitForSeconds(1);
	}
}

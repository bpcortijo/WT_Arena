using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ManagementScript : NetworkBehaviour {
	MapMaker mapCode;
	public GameObject map;

	public GameObject playerPrefab;
	public PreGamePlayerScript localTemp;

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
		NetworkManager.singleton.ServerChangeScene("Game");
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.name == "Game")
		{
			if (NetworkServer.active)
			{
				map = Instantiate(map);
				mapCode = map.GetComponent<MapMaker>();
				mapCode.gm = this;

				NetworkServer.Spawn(map);

				CreatePlayer();

				turnTime = turnTimer + 30f;
				inGame = true;
				// Get Light
			}
		}
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
			

		if (playResults)
		{
			foreach (CharacterStats ch in mapCode.characterPaths)
				if (ch.basics.shortPath.Count > 0)
					ch.basics.Move(resultsTime);
			foreach (ShotScript atk in mapCode.attackPaths)
				atk.basics.Move(resultsTime);
		}
	}

	void TurnResults()
	{
		GetCharacterPaths();
		//mapCode.GetAttackPaths();
		//foreach (CharacterStats ch in mapCode.characterPaths)
		//{
		//	ch.basics.ReMapMovement();
		//	ch.basics.SetPosition(resultsTimer);
		//	ch.basics.DrawLines();
		//}

		//foreach (ShotScript atk in mapCode.attackPaths)
		//{
		//	atk.basics.ReMapMovement();
		//	atk.basics.SetPosition(resultsTimer);
		//	atk.basics.DrawLines();
		//}

		//foreach (CharacterStats ch in mapCode.characterPaths)
		//{
		//	foreach (CharacterStats enemy in mapCode.characterPaths)
		//		if (ch.meleeThis == mapCode.graph[ch.basics.tileX, ch.basics.tileY, ch.basics.tileZ])
		//				ch.MeleeHit(enemy);
		//}
		Debug.Break();
		playResults = true;
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
		if (turn >= maxTurns /*|| players.Count <= 1*/)
			EndGame();
		else
		{
			foreach (GameObject p in players)
			{
				PlayerScript player = p.GetComponent<PlayerScript>();
				player.TurnOver();
				player.canEnd = true;
			}
			turnTime = turnTimer;
		}
	}

	void CreatePlayer()
	{
		GameObject p = Instantiate(playerPrefab);
		p.transform.parent = transform;
		PlayerScript pScript = p.GetComponent<PlayerScript>();
		pScript.map = mapCode;
		pScript.characters = localTemp.myTeam;
		pScript.pName = localTemp.localPlayerName;
		startingPlayers.Add(localTemp.localPlayerName);
		players.Add(p);
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

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ManagementScript : MonoBehaviour {
	MapMaker mapCode;
	public GameObject map;
	bool activeTurn = false;
	public float turnTimer=999f, turnTime = 90f;
	public List<GameObject> startingPlayers, players;
	public int turn = 1, maxTurns = 60, playersLeft = 1, endTurnRequests = 0;

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.name == "Game")
		{
			playersLeft = startingPlayers.Count;
			map = Instantiate(map);
			mapCode = map.GetComponent<MapMaker>();
			CreatePlayers();

			turnTimer = turnTime + 30f;
			activeTurn = true;
			// Get Light
		}
	}

    void Update()
    {
		if (activeTurn)
			turnTimer -= Time.deltaTime;
		if (endTurnRequests == playersLeft && endTurnRequests != 0 || turnTimer <= 0)
			NextTurn();
    }

	void NextTurn()
	{
		GameObject go = mapCode.selectedUnit;
		endTurnRequests = 0;

		CharacterStats[] NotMovingCharacters = FindObjectsOfType<CharacterStats>();
		foreach (CharacterStats characterCode in NotMovingCharacters)
		{
			if (characterCode.basics.plannedPath == null)
			{
				characterCode.basics.plannedPath = new List<MapMaker.Node>();
				characterCode.basics.plannedPath.Add(mapCode.graph[characterCode.basics.tileX,
																	characterCode.basics.tileY,
																	characterCode.basics.tileZ]);
			}
			else if (characterCode.basics.plannedPath.Count == 0)
				characterCode.basics.plannedPath.Add(mapCode.graph[characterCode.basics.tileX,
																	characterCode.basics.tileY,
																	characterCode.basics.tileZ]);
		}

		mapCode.selectedUnit = go;

		GetCharacterPaths();
		mapCode.GetAttackPaths();
		mapCode.CheckAllPaths();

		// rotate light -.125 degrees on the x axis

		foreach (GameObject p in players)
		{
			PlayerScript player = p.GetComponent<PlayerScript>();
			player.TakeTurn();
		}

		foreach (GameObject a in GameObject.FindGameObjectsWithTag("Shot"))
		{
			a.GetComponent<ShotScript>().set = true;
			a.GetComponent<ShotScript>().Attack();
		}

		turn++;
		StartCoroutine(EndTurn());
		if (turn >= maxTurns || playersLeft <= 1)
			EndGame();
		else
		{
			foreach (GameObject p in players)
			{
				PlayerScript player = p.GetComponent<PlayerScript>();
				player.TurnOver();
				player.canEnd = true;
			}
			turnTimer = turnTime;
		}
	}

	void CreatePlayers()
	{
		foreach (GameObject player in startingPlayers)
		{
			GameObject p = Instantiate(player);
			p.transform.parent = this.transform;
			players.Add(p);
		}
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
		yield return new WaitForSeconds(1);
	}
}

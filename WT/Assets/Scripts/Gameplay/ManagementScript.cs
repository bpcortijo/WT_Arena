using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ManagementScript : MonoBehaviour {
	MapMaker mapCode;
	public bool playResults;
	public float resultsTime = 0, resultsTimer = 5f;
	public GameObject map;
	bool activeTurn = false;
	public List<GameObject> startingPlayers, players;
	public float turnTimer = 90f, turnTime = 999f;
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

			turnTime = turnTimer + 30f;
			activeTurn = true;
			// Get Light
		}
	}

    void Update()
    {
		if (activeTurn)
			turnTime -= Time.deltaTime;
		if (endTurnRequests == playersLeft && endTurnRequests != 0 || turnTime <= 0)
			if (!playResults)
				TurnResults();

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
		mapCode.GetAttackPaths();
		//GetNetworkMap();
		foreach (CharacterStats ch in mapCode.characterPaths)
		{
			ch.basics.ReMapMovement();
			ch.basics.SetPosition(resultsTimer);
		}

		foreach (ShotScript atk in mapCode.attackPaths)
		{
			atk.basics.ReMapMovement();
			atk.basics.SetPosition(resultsTimer);
		}
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
			turnTime = turnTimer;
		}
	}

	void CreatePlayers()
	{
		foreach (GameObject player in startingPlayers)
		{
			GameObject p = Instantiate(player);
			p.transform.parent = transform;
			p.GetComponent<PlayerScript>().map = mapCode;
			players.Add(p);
			player.SetActive(false);
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

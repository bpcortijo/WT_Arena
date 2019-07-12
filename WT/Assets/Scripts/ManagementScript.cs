﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagementScript : MonoBehaviour {
	PlayerScript pCode;
	public GameObject map;
	public MapMaker mapCode;
	public List<GameObject> startingPlayers, players;

	public int turn = 0, playersLeft = 1, endTurnRequests = 0;

	void Start()
	{
		playersLeft = startingPlayers.Count;
		map = Instantiate(map);
		mapCode = map.GetComponent<MapMaker>();
		CreatePlayers();
	}

    void Update()
    {
		if (endTurnRequests == playersLeft && endTurnRequests != 0)
			NextTurn();
			
    }

    void NextTurn()
    {
		endTurnRequests = 0;
		foreach (GameObject p in players)
		{
			PlayerScript player = p.GetComponent<PlayerScript>();
			player.TakeTurn();
		}
		turn++;
		StartCoroutine(EndTurn());
		foreach (GameObject p in players)
		{
			PlayerScript player = p.GetComponent<PlayerScript>();
			player.canEnd = true;
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

	IEnumerator EndTurn()
	{
		yield return new WaitForSeconds(1);
	}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ManagementScript : MonoBehaviour {
	MapMaker mapCode;
	public GameObject map;
	public List<GameObject> startingPlayers, players;

	public int turn = 1, playersLeft = 1, endTurnRequests = 0;

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
		}
	}

    void Update()
    {
		if (endTurnRequests == playersLeft && endTurnRequests != 0)
			NextTurn();
    }

    void NextTurn()
    {
		GameObject go = mapCode.selectedUnit;
		endTurnRequests = 0;

		CharacterStats[] NotMovingCharacters = FindObjectsOfType<CharacterStats>();
		foreach (CharacterStats characterCode in NotMovingCharacters)
		{
			if (characterCode.basics.currentPath == null)
			{
				characterCode.basics.currentPath = new List<MapMaker.Node>();
				characterCode.basics.currentPath.Add(mapCode.graph[characterCode.basics.tileX,
																	characterCode.basics.tileY,
																	characterCode.basics.tileZ]);
			}
			else if (characterCode.basics.currentPath.Count == 0)
				characterCode.basics.currentPath.Add(mapCode.graph[characterCode.basics.tileX,
																	characterCode.basics.tileY,
																	characterCode.basics.tileZ]);
		}

		mapCode.selectedUnit = go;

		mapCode.GetCharacterPaths();
		mapCode.GetAttackPaths();
		mapCode.CheckCossPaths();
		EditPaths(mapCode.characterPaths);

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
		Debug.Log("It's a NEW TURN");
		foreach (GameObject p in players)
		{
			PlayerScript player = p.GetComponent<PlayerScript>();
			player.canEnd = true;
		}
	}

	void EditPaths(Dictionary<List<MapMaker.Node>, int> characterPaths)
	{
		foreach (List<MapMaker.Node> path in characterPaths.Keys)
			foreach (GameObject player in players)
				foreach (GameObject unit in player.GetComponent<PlayerScript>().units)
				{
					UnitBasics unitCode = unit.GetComponent<UnitBasics>();
					if (path[0] == mapCode.graph[unitCode.tileX, unitCode.tileY, unitCode.tileZ])
					{
						unitCode.currentPath = path;
						break;
					}
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

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerScript : MonoBehaviour
{
	public bool canEnd;
	ManagementScript gm;
	public List<GameObject> units, characters;

	private void Start()
	{
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
		if (gm != null)
			gm.endTurnRequests++;
	}

	public void Replace()
	{

	}

	public void TakeTurn()
	{
		if (gm != null)
			canEnd = true;
		foreach (GameObject unit in units)
			unit.GetComponent<CharacterStats>().TakeActions();
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

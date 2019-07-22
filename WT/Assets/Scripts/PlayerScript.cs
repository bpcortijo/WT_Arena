using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
	ManagementScript gm;
	public List<GameObject> units, characters;
	public bool canEnd;

	private void Start()
	{
		gm = transform.parent.gameObject.GetComponent<ManagementScript>();
		Spawn();
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
	}
	public void PlayerEndTurn()
	{
		gm.endTurnRequests++;
	}

	public void Replace()
	{

	}

	public void TakeTurn()
	{
		foreach (GameObject unit in units)
			unit.GetComponent<CharacterStats>().TakeActions();
	}
}

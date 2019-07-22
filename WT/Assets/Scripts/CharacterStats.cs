using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
	public bool attacking;
	public UnitBasics basics;
	public List<string> loadout;
	public GameObject[] shotTypes;
	public int speed = 2, actions=2;

	void Start()
    {
		basics = gameObject.GetComponent<UnitBasics>();
		basics.speed = speed;
    }

	// Update is called once per frame
	void Update()
	{
		//if (Input.GetKeyUp(KeyCode.Return))
		//{
		//	if (actions.Count <= 2)
		//		actions.Add(currentAction);
		//	else
		//		Debug.Log("Too many actions");
		//}

		if (basics.map.selectedUnit == gameObject)
		{
			if (Input.GetKeyUp(KeyCode.Z))
				LoadChamber(shotTypes[0], "Z");
			if (Input.GetKeyUp(KeyCode.X))
				LoadChamber(shotTypes[1],"X");
			if (Input.GetKeyUp(KeyCode.C))
				LoadChamber(shotTypes[2],"Composite");
			if (Input.GetKeyUp(KeyCode.V))
				LoadChamber(shotTypes[3], "Vector");
		}
	}

	public void TakeActions()
	{
		while (actions>0)
		{ 
			basics.Move();
			actions--;
			basics.currentPath = null;
			basics.shortPath = null;
		}
	}

	void Undo()
	{
		basics.CheckPath();
	}

	public void LoadChamber(GameObject ammo, string attackName)
	{
		actions--;
		GameObject shot = Instantiate(ammo, transform.position, Quaternion.identity);
		ShotScript ss = shot.GetComponent<ShotScript>();

		ss.owner = gameObject;

		basics.map.selectedUnit = shot;

		shot.GetComponent<UnitBasics>().tileX = basics.tileX;
		shot.GetComponent<UnitBasics>().tileY = basics.tileY;
		shot.GetComponent<UnitBasics>().tileZ = basics.tileZ;
		shot.transform.position = basics.map.TileCoordToWorldCoord(basics.tileX, 
																	basics.tileY, 
																	basics.tileZ);
		shot.name = attackName;
		basics.CheckPath();
	}

	public void CancelAction()
	{
		actions++;
		basics.CheckPath();
	}
}

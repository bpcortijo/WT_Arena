using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
	UnitBasics basics;
	public int speed = 2;
	public List<string> loadout, actions;
	public string currentAction, nextAction, attack;
	public bool attacking;

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
	}

	public void TakeActions()
	{
		string act;
		while (actions.Count < 2 && basics.currentPath!=null)
				actions.Add("move");

		while (actions.Count>0)
			{
			act = actions[0];
			currentAction = act;
			if (actions.Count > 1)
				nextAction = actions[1];
				switch (act)
				{
					case "move":
						basics.Move();
						break;
					default:
						break;
				}
			Debug.Log(act);
			actions.RemoveAt(0);
			}

		nextAction = "";
		currentAction = "";
	}
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DetailScript : MonoBehaviour
{
	public Color empty, full;

	public TeamSelect teamPage;
	public PlayerScript P1, P2;
	bool P1has = false, P2has = false;
	public Text selectTeam1, selectTeam2, cName, cAge;

	[HideInInspector]
	public GameObject currentCharacter;
	[HideInInspector]
	public List<GameObject> AG, EN, EXP;


	public void UpdatePage(int agility, int energy, int experience)
	{
		EditDetails();
		FillDots(AG, agility);
		FillDots(EN, energy);
		FillDots(EXP, experience);
	}

	void FillDots(List<GameObject> dots, int stat)
	{
		int count = 1;
		foreach (GameObject dot in dots)
		{
			if (count <= stat)
				EditDot(dot, full, "full");
			else
				EditDot(dot, empty, "empty");
			count++;
		}
	}

	void EditDot(GameObject dot, Color status, string n)
	{
		dot.GetComponent<Image>().color = status;

		if (n == "full")
			dot.GetComponent<RectTransform>().localScale = new Vector3(.075f, .075f, .075f);
		else
			dot.GetComponent<RectTransform>().localScale = new Vector3(.05f, .05f, .05f);
	}

	void EditDetails()
	{
		if (P1.characters.Contains(currentCharacter))
		{
			P1has = true;
			selectTeam1.text = "Remove from Team 1";
		}
		else
		{
			P1has = false;
			selectTeam1.text = "Add to Team 1";
		}

		if (P2.characters.Contains(currentCharacter))
		{
			P2has = true;
			selectTeam2.text = "Remove from Team 2";
		}
		else
		{
			P2has = false;
			selectTeam2.text = "Add to Team 2";
		}

		/*		if (P2.characters.Contains(currentCharacter))
				{
					P2has = true;
					select.text = "Remove from Team 2";
				}
				else
				{
					P2has = false;
					select.text = "Add to Team 2";
				}*/

		cName.text = "Name: " + currentCharacter.name;
		cAge.text = "Age: " + currentCharacter.GetComponent<CharacterStats>().age;
	}

	public void EditTeam1()
	{
		if (P1has)
			P1.characters.Remove(currentCharacter);
		else
			P1.characters.Add(currentCharacter);
		teamPage.UpdateTeam1();
		EditDetails();
	}

	public void EditTeam2()
	{
		if (P2has)
			P2.characters.Remove(currentCharacter);
		else
			P2.characters.Add(currentCharacter);
		teamPage.UpdateTeam2();
		EditDetails();
	}
}

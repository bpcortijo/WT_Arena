using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DetailScript : MonoBehaviour
{
	public Color empty, full;

	bool playerHas = false;
	public TeamSelect teamPage;
	public PlayerScript localPlayer;
	public Text selectLocalTeam, cName, cAge;

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
		cName.text = "Name: " + currentCharacter.name;
		cAge.text = "Age: " + currentCharacter.GetComponent<CharacterStats>().age;
	}

	public void EditLocalTeam()
	{
		localPlayer.highlighted = currentCharacter.name;
		localPlayer.RpcEditTeam();
		teamPage.UpdateLocalTeam();
	}

	private void Update()
	{
		if (localPlayer != null)
			if (localPlayer.team.Contains(currentCharacter))
				selectLocalTeam.text = "Remove from Team 1";
			else
				selectLocalTeam.text = "Add to Team 1";
	}
}

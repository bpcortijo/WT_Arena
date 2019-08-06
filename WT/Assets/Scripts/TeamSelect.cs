using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TeamSelect : MonoBehaviour
{
	public MenuScript menu;
	public PlayerScript p1, p2;

	public void UpdateTeam1()
	{
		foreach (GameObject go in GameObject.FindGameObjectsWithTag("Team1"))
			Destroy(go);

		for (int i=0; i<p1.characters.Count; i++)
		{
			GameObject image = Instantiate(p1.characters[i].GetComponent<CharacterStats>().bodyShot);
			image.transform.parent = gameObject.transform;
			image.tag = "Team1";
			image.GetComponent<RectTransform>().anchoredPosition = new Vector3(-60 - 90 * i, 65f);
			image.GetComponent<RectTransform>().localScale = Vector3.one;
		}
	}

	public void UpdateTeam2()
	{
		foreach (GameObject go in GameObject.FindGameObjectsWithTag("Team2"))
			Destroy(go);

		for (int i = 0; i < p2.characters.Count; i++)
		{
			GameObject image = Instantiate(p2.characters[i].GetComponent<CharacterStats>().bodyShot);
			image.transform.parent = gameObject.transform;
			image.tag = "Team2";
			image.GetComponent<RectTransform>().anchoredPosition = new Vector3(60 + 90 * i, 65f);
			image.GetComponent<RectTransform>().localScale = Vector3.one;
		}
	}

	public void LetsPlay()
	{
		if (p1.characters.Count >= 3 && p2.characters.Count >= 3)
			menu.Play();
		else
			Debug.Log("3-4 Players per team");
	}
}

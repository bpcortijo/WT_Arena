using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TeamSelect : MonoBehaviour
{
	public MenuScript menu;
	public PreGamePlayerScript localTemp;

	public void UpdateLocalTeam()
	{
		foreach (GameObject go in GameObject.FindGameObjectsWithTag("Team1"))
			Destroy(go);

		for (int i=0; i<localTemp.characters.Count; i++)
		{
			GameObject image = Instantiate(localTemp.characters[i].GetComponent<CharacterStats>().bodyShot);
			image.transform.parent = gameObject.transform;
			image.tag = "Team1";
			image.GetComponent<RectTransform>().anchoredPosition = new Vector3(-60 - 90 * i, 65f);
			image.GetComponent<RectTransform>().localScale = Vector3.one;
		}
	}

	public void LetsPlay()
	{
		if (localTemp.characters.Count >= 3)
			menu.Play();
		else
			Debug.Log("3-4 Players per team");
	}
}

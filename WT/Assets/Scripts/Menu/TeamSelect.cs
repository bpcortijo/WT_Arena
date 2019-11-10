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

		for (int i=0; i<localTemp.myTeam.Count; i++)
		{
			GameObject image = Instantiate(localTemp.myTeam[i].GetComponent<CharacterStats>().bodyShot);
			image.transform.parent = gameObject.transform;
			image.tag = "Team1";
			image.GetComponent<RectTransform>().anchoredPosition = new Vector3(-60 - 90 * i, 65f);
			image.GetComponent<RectTransform>().localScale = Vector3.one;
		}
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
	float x;
	public int currentPage;
	public float menuSpeed;
	List<Vector3> currentPos;
	public List<GameObject> Menus;

	private void Awake()
	{
		currentPos = new List<Vector3>();
		foreach (GameObject page in Menus)
			currentPos.Add(page.GetComponent<RectTransform>().anchoredPosition);
	}
	
	public void Next()
	{
		currentPage++;
		currentPage = Mathf.Clamp(currentPage, 0, 2);
		x = -820 * currentPage;
	}

	public void Prev()
	{
		currentPage--;
		currentPage = Mathf.Clamp(currentPage, 0, 2);
		x = -820 * currentPage;
	}

	public void Credits()
    {
        
    }

	private void Update()
	{
		Transition();
	}

	void Transition()
	{
		for (int i = 0; i < Menus.Count; i++)
		{
			Menus[i].GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(
																					Menus[i].GetComponent<RectTransform>().anchoredPosition,
																					new Vector3(currentPos[i].x + x,
																								currentPos[i].y),
																					Time.deltaTime * menuSpeed);
		}

		if ((Vector3)Menus[0].GetComponent<RectTransform>().anchoredPosition == new Vector3(currentPos[0].x + x, currentPos[0].y))
		{
			x = 0;
			for (int i = 0; i < Menus.Count; i++)
			{
				currentPos[i]=(Menus[i].GetComponent<RectTransform>().anchoredPosition);
			}
		}
	}

	public void Play()
	{
		DontDestroyOnLoad(GameObject.Find("GameManager"));
		SceneManager.LoadScene("Game");
	}
}

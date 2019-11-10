using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PreGamePlayerScript : MonoBehaviour
{
	public string localPlayerName;
	public List<GameObject> myTeam, availableCharacters;

	public void Start()
	{
		SceneManager.activeSceneChanged += ChangedActiveScene;
	}

	//if team is not full add random characters
	private void ChangedActiveScene(Scene current, Scene next)
	{
		if (localPlayerName == null)
			localPlayerName = "No Name";
		while (myTeam.Count < 3)
		{
			int r = Random.Range(0, availableCharacters.Count - 1);
			if (!myTeam.Contains(availableCharacters[r]))
				myTeam.Add(availableCharacters[r]);
		}
	}
}

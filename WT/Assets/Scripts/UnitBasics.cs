using UnityEngine;
using System.Collections.Generic;

public class UnitBasics : MonoBehaviour {
	public int speed, bonus;
	public float PieceMovementTime = 3, tempTime;

	public MapMaker map;
	public float unitHeight = 1f;
	public bool vector = false, full = false;
	public int tileX, tileY, tileZ, turns;

	public List<MapMaker.Node> shortPath = null;
	public List<MapMaker.Node> currentPath = null;
	public List<MapMaker.Node> keyPoints = new List<MapMaker.Node>();

	public void CheckPath()
	{
		full = false;
		if (currentPath != null)
		{
			shortPath = new List<MapMaker.Node>(currentPath);
			if (gameObject.tag == "Player")
				if (gameObject.GetComponent<CharacterStats>().actions.Contains("Reload"))
				{
					//reloading is a free action while moving
					while (shortPath.Count > speed * (gameObject.GetComponent<CharacterStats>().movementActions + 1) + 1 + bonus)
						shortPath.RemoveAt(shortPath.Count - 1);
					if (shortPath.Count == speed * (gameObject.GetComponent<CharacterStats>().movementActions + 1) + 1 + bonus)
						full = true;
				}
				else
				{
					while (shortPath.Count > speed * gameObject.GetComponent<CharacterStats>().movementActions + 1 + bonus)
						shortPath.RemoveAt(shortPath.Count - 1);
					if (shortPath.Count == speed * gameObject.GetComponent<CharacterStats>().movementActions + 1 + bonus)
						full = true;
				}

			if (gameObject.tag == "Shot")
			{
				while (shortPath.Count > speed + 1)
					shortPath.RemoveAt(shortPath.Count - 1);

				while (currentPath.Count > speed * turns + 1)
					currentPath.RemoveAt(currentPath.Count - 1);

				if (currentPath.Count == speed * turns + 1)
					full = true;
			}
		}
	}

	private void Update()
    {
		if (currentPath != null && tag == "Shot")
		{
			for (int currentStep = 0; currentStep < currentPath.Count - 1; currentStep++)
			{
				Vector3 start = map.TileCoordToWorldCoord(currentPath[currentStep].x,
																currentPath[currentStep].y,
																currentPath[currentStep].z);
				Vector3 end = map.TileCoordToWorldCoord(currentPath[currentStep + 1].x,
																currentPath[currentStep + 1].y,
																currentPath[currentStep + 1].z);
				start.y += unitHeight;
				end.y += unitHeight;
				Debug.DrawLine(start, end, Color.black);
			}
		}

		if (shortPath != null)
		{
			for (int currentStep = 0; currentStep < shortPath.Count - 1; currentStep++)
			{
				Vector3 start = map.TileCoordToWorldCoord(shortPath[currentStep].x,
															shortPath[currentStep].y,
															shortPath[currentStep].z);
				Vector3 end = map.TileCoordToWorldCoord(shortPath[currentStep + 1].x,
															shortPath[currentStep + 1].y,
															shortPath[currentStep + 1].z);
				start.y += unitHeight;
				end.y += unitHeight;
				if (tag == "Shot")
				{
					Debug.DrawLine(start, end, Color.red);
				}
				else
					Debug.DrawLine(start, end, Color.blue);
			}
		}
    }

	public void Move()
	{
		for (int s = 0; s < speed; s++)
		{
			if (currentPath == null)
				return;
			currentPath.RemoveAt(0);

			transform.position = map.TileCoordToWorldCoord(currentPath[0].x,
															currentPath[0].y,
															currentPath[0].z);

			if (currentPath.Count == 1)
			{
				tileX = currentPath[0].x;
				tileY = currentPath[0].y;
				tileZ = currentPath[0].z;
				currentPath = null;
			}
		}
	}

	public bool CheckSlope(float x, float y, float z)
	{
		if (tileY - y == 0)
		{
			if (tileX - x == 0 || tileZ - z == 0)
				return true;
			else if ((tileX - x) / (tileZ - z) == 1 || (tileX - x) / (tileZ - z) == -1)
				return true;
		}
		else if ((tileX - x) / (tileY - y) == 1 || (tileX - x) / (tileY - y) == -1 || (tileX - x) / (tileY - y) == 0)
			if ((tileZ - z) / (tileY - y) == 1 || (tileZ - z) / (tileY - y) == -1 || (tileZ - z) / (tileY - y) == 0)
				return true;
		return false;
	}
}

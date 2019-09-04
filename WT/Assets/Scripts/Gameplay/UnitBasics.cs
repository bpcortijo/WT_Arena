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
	public List<MapMaker.Node> plannedPath = null;
	public List<MapMaker.Node> keyPoints = new List<MapMaker.Node>();

	public void CheckPath()
	{
		// Create short path (the path the character or bullet plans to take this turn) and Remove any points past max distance
		full = false;
		if (plannedPath != null)
		{
			shortPath = new List<MapMaker.Node>(plannedPath);
			if (gameObject.tag == "Player")
				if (gameObject.GetComponent<CharacterStats>().actions.Contains("Reload"))
				{
					// Reloading is a free action while moving
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

				while (plannedPath.Count > speed * turns + 1)
					plannedPath.RemoveAt(plannedPath.Count - 1);

				if (plannedPath.Count == speed * turns + 1)
					full = true;
			}
		}
		else
			shortPath = null;
	}

	private void Update()
    {
		if (plannedPath != null && tag == "Shot")
		{
			// Drawing the lines to show the path the bullet will take after this turn (PLACEHOLDER)
			for (int currentStep = 0; currentStep < plannedPath.Count - 1; currentStep++)
			{
				Vector3 start = map.TileCoordToWorldCoord(plannedPath[currentStep].x,
																plannedPath[currentStep].y,
																plannedPath[currentStep].z);
				Vector3 end = map.TileCoordToWorldCoord(plannedPath[currentStep + 1].x,
																plannedPath[currentStep + 1].y,
																plannedPath[currentStep + 1].z);
				start.y += unitHeight;
				end.y += unitHeight;
				Debug.DrawLine(start, end, Color.black);
			}
		}

		if (shortPath != null)
		{
			// Drawing the lines to show the path the character or bullet will take this turn (PLACEHOLDER)
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
		int s = 1;  // Spaces

		if (shortPath != null)	// Check if character is movin
			if (shortPath.Count > 1)
			{
				while (s < speed && s<shortPath.Count)
				{
					transform.position = map.TileCoordToWorldCoord(shortPath[s].x,
																	shortPath[s].y,
																	shortPath[s].z);
					s++;
				}
				if (s == shortPath.Count)
				{
					tileX = shortPath[s - 1].x;
					tileY = shortPath[s - 1].y;
					tileZ = shortPath[s - 1].z;
				}
				else // If they didn't finish moving remove the spaces they already went
					while (s >= 0)
					{
						shortPath.RemoveAt(0);
						s--;
					}
			}
	}

	public bool CheckSlope(float x, float y, float z) // Check if the shot is going straight in any direction
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

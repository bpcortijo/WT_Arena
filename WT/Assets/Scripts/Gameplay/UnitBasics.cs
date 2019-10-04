using UnityEngine;
using System.Collections.Generic;

public class UnitBasics : MonoBehaviour {
	public int speed, bonus, nextBonus;
	public float lerpTimePerSpace = 5;

	public MapMaker map;
	public float unitHeight = 1f;
	public int tileX, tileY, tileZ, turns;
	public bool vector = false, full = false;

	public List<MapMaker.Node> keyPoints = new List<MapMaker.Node>();
	public List<MapMaker.Node> shortPath = new List<MapMaker.Node>();
	public List<MapMaker.Node> plannedPath = new List<MapMaker.Node>();

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
					if (gameObject.GetComponent<CharacterStats>().movementActions + 1 == 2 && gameObject.GetComponent<CharacterStats>().sprinter)
						bonus += 2;

					while (shortPath.Count > speed * (gameObject.GetComponent<CharacterStats>().movementActions + 1) + 1 + bonus)
						shortPath.RemoveAt(shortPath.Count - 1);
					if (shortPath.Count == speed * (gameObject.GetComponent<CharacterStats>().movementActions + 1) + 1 + bonus)
						full = true;
				}
				else
				{
					if (gameObject.GetComponent<CharacterStats>().movementActions == 2 && gameObject.GetComponent<CharacterStats>().sprinter)
						bonus += 2;
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
			shortPath = new List<MapMaker.Node>();
	}

	private void Update()
    {
		if (tag == "Shot")
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
				Debug.DrawLine(start, end, Color.red);
			else
				Debug.DrawLine(start, end, Color.blue);
		}
    }

	public void ReMapMovement()
	{
		foreach (MapMaker.Node node in shortPath)
			CheckEachMovementTile(node);
	}

	void CheckEachMovementTile(MapMaker.Node node)
	{
		TileScript standingTile = map.GetTileFromNode(node);

			if (standingTile.effect != null && GetComponent<CharacterStats>() != null)
			{
				switch (standingTile.effect)
				{
					case "spider":
						bonus--;
						CheckPath();
						break;
					case "python":
						if (gameObject.GetComponent<CharacterStats>() != null)
							gameObject.GetComponent<CharacterStats>().disabled = 3;
						break;
					default:
						break;
				}
			}

			if (shortPath.IndexOf(node) < shortPath.Count - 1)
			{
				MapMaker.Node nextNode = shortPath[shortPath.IndexOf(node)+1];
				if (!map.CanEnter(node.x, node.y, node.z, nextNode.x, nextNode.y, nextNode.z))
					if (GetComponent<CharacterStats>() != null)
						while (shortPath.Count + 1 > shortPath.IndexOf(node))
							shortPath.Remove(shortPath[shortPath.Count - 1]);
					//else
					//{
					//	GetComponent<ShotScript>().movingPower--;
					//	if (GetComponent<ShotScript>().movingPower<=0)
					//		while (shortPath.Count + 1 > shortPath.IndexOf(node))
					//			shortPath.Remove(shortPath[shortPath.Count - 1]);
					//}
			}
	}

	public void SetPosition(float timeToFinalPos)
	{
		if (shortPath.Count > 0)
		{
			tileX = shortPath[shortPath.Count - 1].x;
			tileY = shortPath[shortPath.Count - 1].y;
			tileZ = shortPath[shortPath.Count - 1].z;
			lerpTimePerSpace = timeToFinalPos / shortPath.Count;
		}
	}

	public void Move(float timePast)
	{
		int space = Mathf.CeilToInt(timePast / lerpTimePerSpace);
		if (space < shortPath.Count)
		{
			Vector3 nextSpace = map.TileCoordToWorldCoord(shortPath[space].x, shortPath[space].y, shortPath[space].z);
			if (GetComponent<ShotScript>() != null)
				nextSpace.y += unitHeight;

			transform.position = Vector3.Lerp(transform.position, nextSpace, (timePast - space + 1) / lerpTimePerSpace);
		}
	}

	public void ClearLastShotMove()
	{
		turns--;
		if (turns <= 0)
			Destroy(gameObject);

		while (0 < plannedPath.IndexOf(map.graph[tileX, tileY, tileZ]))
			plannedPath.RemoveAt(0);
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

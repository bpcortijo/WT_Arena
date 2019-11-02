using UnityEngine;
using System.Collections.Generic;

public class UnitBasics : MonoBehaviour {
	GameObject visibleShortPath, visiblePlannedPath;
	public int speed, bonus, nextBonus;
	public float lerpTimePerSpace = 5;
	public GameObject path;

	public MapMaker map;
	public float unitHeight = 1f;
	public int tileX, tileY, tileZ, turns;
	public bool vector = false, full = false;

	MapMaker.Node currentSpace = null;
	public List<MapMaker.Node> keyPoints = new List<MapMaker.Node>();
	public List<MapMaker.Node> shortPath = new List<MapMaker.Node>();
	public List<MapMaker.Node> plannedPath = new List<MapMaker.Node>();

	private void Start()
	{
		CharacterSetCurrentSpace(map.graph[tileX, tileY, tileZ],GetComponent<CharacterStats>());
	}

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

		//foreach (MapMaker.Node node in shortPath)
		//	if (shortPath.IndexOf(node) != shortPath.LastIndexOf(node))
		//	{
		//		int i = shortPath.LastIndexOf(node);
		//		while (i < shortPath.Count)
		//			shortPath.RemoveAt(shortPath.Count - 1);
		//		while (i < plannedPath.Count)
		//			plannedPath.RemoveAt(plannedPath.Count - 1);
		//	}
		DrawLines();
	}

	public void DrawLines()
	{
		Destroy(visibleShortPath);
		Destroy(visiblePlannedPath);
		// Drawing the lines to show the path the character or bullet will take this turn (PLACEHOLDER)
		visibleShortPath = Instantiate(path);
		visibleShortPath.GetComponent<VisualPathScript>().map = map;
		visibleShortPath.GetComponent<VisualPathScript>().speed = speed;
		visibleShortPath.GetComponent<VisualPathScript>().nodePath = shortPath;

		if (tag == "Shot")
		{
			visibleShortPath.GetComponent<LineRenderer>().startColor = Color.red;
			visibleShortPath.GetComponent<LineRenderer>().endColor = Color.red;

			visiblePlannedPath = Instantiate(path);
			VisualPathScript fullLine = visiblePlannedPath.GetComponent<VisualPathScript>();

			visiblePlannedPath.GetComponent<LineRenderer>().widthMultiplier = fullLine.size / 2;
			visiblePlannedPath.GetComponent<LineRenderer>().startColor = Color.black;
			visiblePlannedPath.GetComponent<LineRenderer>().endColor = Color.black;
			fullLine.nodePath = shortPath;
			fullLine.speed = speed;
		}
		else
		{
			visibleShortPath.GetComponent<LineRenderer>().startColor = Color.blue;
			visibleShortPath.GetComponent<LineRenderer>().endColor = Color.blue;
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
			MapMaker.Node nextNode = shortPath[shortPath.IndexOf(node) + 1];
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
			lerpTimePerSpace = timeToFinalPos / shortPath.Count;
	}

	public void Move(float timePast)
	{
		int space = Mathf.CeilToInt(timePast / lerpTimePerSpace);

		if (space < shortPath.Count)
			if (GetComponent<CharacterStats>() != null && shortPath[space] != currentSpace)
			{
				CharacterSetCurrentSpace(shortPath[space], GetComponent<CharacterStats>());

				transform.position = map.TileCoordToWorldCoord(tileX, tileY, tileZ, true);

				//Vector3 nextSpace = map.TileCoordToWorldCoord(shortPath[space].x, shortPath[space].y, shortPath[space].z, true);
				//if (GetComponent<ShotScript>() != null)
				//	nextSpace.y += unitHeight;

				//transform.position = Vector3.Lerp(transform.position, nextSpace, (timePast - space + 1) / lerpTimePerSpace);
			}
	}

	void CharacterSetCurrentSpace(MapMaker.Node nextStep, CharacterStats me)
	{
		if (!map.GetTileFromNode(nextStep).hasUnit)
		{
			Debug.Log(name + " stepped on to (" + nextStep.x + "," + nextStep.y + "," + nextStep.z + ")");
			if (currentSpace != null)
				map.GetTileFromNode(currentSpace).hasUnit = false;
			currentSpace = nextStep;

			foreach (ShotScript shot in map.attackPaths)
				if (shot.basics.currentSpace == currentSpace)
					if (shot.owner.transform.parent != transform.parent)
						shot.Impact(me, shortPath.IndexOf(currentSpace) + 1 / shortPath.Count, false);
					else if (!shot.owner.GetComponent<CharacterStats>().trust && !me.trust)
						shot.Impact(me, shortPath.IndexOf(currentSpace) + 1 / shortPath.Count, false);
			tileX = currentSpace.x;
			tileY = currentSpace.y;
			tileZ = currentSpace.z;
			map.GetTileFromNode(currentSpace).hasUnit = true;
		}
		else
		{
			Debug.Log(name + " tried to step on to (" + nextStep.x + "," + nextStep.y + "," + nextStep.z + ")");
			Debug.Break();
			while (shortPath.Count > shortPath.IndexOf(currentSpace) + 1)
				shortPath.RemoveAt(shortPath.Count - 1);
		}
	}

	void ShotSetCurrentSpace(MapMaker.Node nextStep, ShotScript myBullet)
	{
		foreach (ShotScript shot in map.attackPaths)
			if (shot.owner.transform.parent != myBullet.owner.transform.parent
					&& shot.basics.currentSpace == currentSpace)
				Debug.Log("Shot out of the air");
		foreach (CharacterStats character in map.characterPaths)
			if (character.basics.currentSpace == currentSpace)
				if (character.transform.parent != myBullet.owner.transform.parent)
				{
					myBullet.Impact(character,
						character.basics.shortPath.IndexOf(currentSpace) / character.basics.shortPath.Count,
						false);
				}
				else if (!character.trust && !GetComponent<ShotScript>().owner.GetComponent<CharacterStats>().trust)
				{
					myBullet.Impact(character,
						character.basics.shortPath.IndexOf(currentSpace) / character.basics.shortPath.Count,
						false);
				}
				else
					Debug.Log("Trust");

		tileX = currentSpace.x;
		tileY = currentSpace.y;
		tileZ = currentSpace.z;
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

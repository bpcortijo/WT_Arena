using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class UnitBasics : NetworkBehaviour {
	GameObject visibleShortPath, visiblePlannedPath;
	public int speed, bonus, nextBonus;
	public float lerpTimePerSpace = 5;
	public GameObject path;

	[SyncVar]
	public int playerNum;
	[SyncVar]
	public int tileX, tileY, tileZ;

	public int turns;
	public MapMaker map;
	public float unitHeight = 1f;

	public bool fullPath = false;

	MapMaker.Node currentSpace = null;
	public List<MapMaker.Node> keyPoints = new List<MapMaker.Node>();
	public List<MapMaker.Node> shortPath = new List<MapMaker.Node>();
	public List<MapMaker.Node> plannedPath = new List<MapMaker.Node>();

	public List<MapMaker.Node> fov = new List<MapMaker.Node>();

	public LayerMask tileMask;

	private void Start()
	{
		if (gameObject.GetComponent<CharacterStats>() != null)
			CharacterSetCurrentSpace(map.graph[tileX, tileY, tileZ], GetComponent<CharacterStats>());
	}

	public void CheckPath()
	{
		// Create short path (the path the character or bullet plans to take this turn) and Remove any points past max distance
		fullPath = false;
		if (plannedPath != null)
		{
			shortPath = new List<MapMaker.Node>(plannedPath);
			if (gameObject.tag == "Player")
				if (!gameObject.GetComponent<CharacterStats>().wrapped)
					if (gameObject.GetComponent<CharacterStats>().actions.Contains("Reload"))
					{
						// Reloading is a free action while moving
						if (gameObject.GetComponent<CharacterStats>().movementActions + 1 == 2 && gameObject.GetComponent<CharacterStats>().sprint)
							bonus += 2;

						while (shortPath.Count > speed * (gameObject.GetComponent<CharacterStats>().movementActions + 1) + 1 + bonus)
							shortPath.RemoveAt(shortPath.Count - 1);
						if (shortPath.Count == speed * (gameObject.GetComponent<CharacterStats>().movementActions + 1) + 1 + bonus)
							fullPath = true;
					}
					else
					{
						if (gameObject.GetComponent<CharacterStats>().movementActions == 2 && gameObject.GetComponent<CharacterStats>().sprint)
							bonus += 2;
						while (shortPath.Count > speed * gameObject.GetComponent<CharacterStats>().movementActions + 1 + bonus)
							shortPath.RemoveAt(shortPath.Count - 1);
						if (shortPath.Count == speed * gameObject.GetComponent<CharacterStats>().movementActions + 1 + bonus)
							fullPath = true;
					}
				else
				{
					plannedPath = new List<MapMaker.Node>();
					shortPath = new List<MapMaker.Node>();
				}

			if (gameObject.tag == "Shot")
			{
				while (shortPath.Count > speed + 1)
					shortPath.RemoveAt(shortPath.Count - 1);

				while (plannedPath.Count > speed * turns + 1)
					plannedPath.RemoveAt(plannedPath.Count - 1);

				if (plannedPath.Count == speed * turns + 1)
					fullPath = true;
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
		visibleShortPath.GetComponent<VisualPathScript>().represents = this;
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
			NetworkServer.Spawn(visiblePlannedPath);
		}
		else
		{
			visibleShortPath.GetComponent<LineRenderer>().startColor = Color.blue;
			visibleShortPath.GetComponent<LineRenderer>().endColor = Color.blue;
		}

		NetworkServer.Spawn(visibleShortPath);
	}

	public void ReMapMovement()
	{
		foreach (MapMaker.Node node in shortPath)
			CheckEachMovementTile(node);
	}

	public bool LocalCheck()
	{
		return NetworkClient.active;
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
		}
	}

	public void SetPosition(float timeToFinalPos)
	{
		if (shortPath.Count > 1)
			lerpTimePerSpace = timeToFinalPos / shortPath.Count;
		//else
		//	AdjustFOV();
	}

	public void Move(float timePast)
	{
		Debug.Log(name + shortPath.Count);
		int space = Mathf.CeilToInt(timePast / lerpTimePerSpace);
		if (space < shortPath.Count && shortPath[space] != currentSpace)
			if (GetComponent<CharacterStats>() != null)
			{
				CharacterSetCurrentSpace(shortPath[space], GetComponent<CharacterStats>());

				transform.position = map.TileCoordToWorldCoord(tileX, tileY, tileZ, true);
				if (GetComponent<CharacterStats>().canHit)
					GetComponent<CharacterStats>().CheckMelee();
			}
			else if (GetComponent<ShotScript>() != null)
			{
				ShotSetCurrentSpace(shortPath[space], GetComponent<ShotScript>());

				transform.position = map.TileCoordToWorldCoord(tileX, tileY, tileZ, false);
			}
	}

	void CharacterSetCurrentSpace(MapMaker.Node nextStep, CharacterStats me)
	{
		if (CheckAllPositions())
		{
			currentSpace = nextStep;

			foreach (ShotScript shot in map.attackPaths)
				if (shot.basics.currentSpace == currentSpace)
					if (shot.player.transform != transform.parent || !shot.owner.GetComponent<CharacterStats>().trust && !me.trust)
						if(shot.owner.GetComponent<CharacterStats>().leadingShot)
							shot.Impact(me, shortPath.IndexOf(currentSpace) + 2 / shortPath.Count, false);
						else
							shot.Impact(me, shortPath.IndexOf(currentSpace) + 1 / shortPath.Count, false);
			tileX = currentSpace.x;
			tileY = currentSpace.y;
			tileZ = currentSpace.z;
			AdjustFOV();
		}
		else
		{
			Debug.Log(name + " tried to step on to (" + nextStep.x + "," + nextStep.y + "," + nextStep.z + ")");
			while (shortPath.Count > shortPath.IndexOf(currentSpace) + 1)
				shortPath.RemoveAt(shortPath.Count - 1);
		}
	}

	bool CheckAllPositions()
	{
		CharacterStats[] characters = FindObjectsOfType<CharacterStats>();
		foreach (CharacterStats character in characters)
			if (character.gameObject != gameObject)
				if (character.basics.tileX != tileX || character.basics.tileY != tileY || character.basics.tileZ != tileZ)
					return true;
		return false;
	}

	void ShotSetCurrentSpace(MapMaker.Node nextStep, ShotScript myBullet)
	{
		ShotScript thisShot = GetComponent<ShotScript>();
		currentSpace = nextStep;

		foreach (ShotScript shot in map.attackPaths)
			if (shot.owner.transform.parent != myBullet.owner.transform.parent
					&& shot.basics.currentSpace == currentSpace)
				Debug.Log("Shot out of the air");
		foreach (CharacterStats character in map.characterPaths)
			if (character.basics.currentSpace == currentSpace)
				if (character.transform.parent != myBullet.player.transform || !character.trust && !thisShot.owner.GetComponent<CharacterStats>().trust)
				{
					if (character.basics.shortPath.IndexOf(currentSpace) != 0 || !character.quickstep)
						if (thisShot.owner.GetComponent<CharacterStats>().leadingShot)
							myBullet.Impact(character,
								character.basics.shortPath.IndexOf(currentSpace) + 2 / character.basics.shortPath.Count,
								false);
						else
							myBullet.Impact(character,
								character.basics.shortPath.IndexOf(currentSpace) + 1 / character.basics.shortPath.Count,
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

	public void AdjustFOV()
	{
		CharacterStats ch = GetComponent<CharacterStats>();

		List<MapMaker.Node> vision = new List<MapMaker.Node>();
		for (int x = tileX - ch.vision; x <= tileX + ch.vision; x++)
			if (x >= 0 && x < map.mapSizeX)
				for (int y = tileY - ch.vision; y <= tileY + ch.vision; y++)
					if (y >= 0 && y < map.mapSizeY)
						for (int z = tileZ - ch.vision; z <= tileZ + ch.vision; z++)
							if (z >= 0 && z < map.mapSizeZ)
								if (PerceptionCheck(map.graph[x, y, z]))
								{
									vision.Add(map.graph[x, y, z]);
									map.GetTileFromNode(map.graph[x, y, z]).inView = true;
								}

		List<MapMaker.Node> temp = new List<MapMaker.Node>();
		foreach (MapMaker.Node node in fov)
			if (!vision.Contains(node))
				temp.Add(node);

		fov = vision;

		foreach (MapMaker.Node node in temp)
			map.GetTileFromNode(node).CheckView();

		ch.player.CheckVisibility();
	}

	public bool PerceptionCheck(MapMaker.Node node)
	{
		if (Physics.Linecast(new Vector3(transform.position.x, transform.position.y + unitHeight, transform.position.z),
								new Vector3(node.x, node.y + 1, node.z), tileMask))
			return false;
		return true;
	}
}

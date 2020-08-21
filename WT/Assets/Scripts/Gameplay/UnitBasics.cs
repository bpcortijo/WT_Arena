using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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

	float moveTime = 0;
	public float timePerMove;
	public LayerMask tileMask;

	[SyncVar]
	public float tpm;

	private void Start()
	{
		map = FindObjectOfType<MapMaker>();
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

	private void Update()
	{
		if (FindObjectOfType<ManagementScript>().playResults)
			moveTime += Time.deltaTime;

		if (moveTime >= timePerMove&& shortPath.IndexOf(map.graph[tileX, tileY, tileZ])+1<shortPath.Count)
		{
			if (GetComponent<CharacterStats>() != null)
				NextStep(shortPath[shortPath.IndexOf(map.graph[tileX, tileY, tileZ]) + 1]);
			else
				NextStep(shortPath[shortPath.IndexOf(map.graph[tileX, tileY, tileZ])+1]);

			moveTime -= timePerMove;
		}
	}

	void NextStep(MapMaker.Node nextStep)
	{
		if (GetComponent<CharacterStats>()!=null&& IsPositionOccupied(nextStep))
		{
			Debug.Log(name + " tried to step on to (" + nextStep.x + "," + nextStep.y + "," + nextStep.z + ")");
			while (shortPath.Count > shortPath.IndexOf(currentSpace))
				shortPath.RemoveAt(shortPath.Count - 1);
		}
		else
		{
			RpcMoveUnit(nextStep.x, nextStep.y, nextStep.z);				
			AdjustFOV();
		}
	}

	[ClientRpc]
	void RpcMoveUnit(int x, int y, int z)
	{
		tileX = x;
		tileY = y;
		tileZ = z;

		if (GetComponent<CharacterStats>() != null)
			transform.position = map.TileCoordToWorldCoord(tileX, tileY, tileZ, true);
		else
			transform.position = map.TileCoordToWorldCoord(tileX, tileY, tileZ, false);
	}

	bool IsPositionOccupied(MapMaker.Node step)
	{
		CharacterStats[] characters = FindObjectsOfType<CharacterStats>();
		foreach (CharacterStats character in characters)
			if (character.gameObject != gameObject)
				if (character.basics.tileX == step.x && character.basics.tileY == step.y && character.basics.tileZ == step.z)
					return true;
		return false;
	}

	[TargetRpc]
	public void TargetTimeToReach(NetworkConnection target, int x, int y, int z, int index)
	{
		MapMaker.Node node = map.graph[x, y, z];

		if (shortPath.Contains(node))
			tpm = timePerMove * shortPath.IndexOf(node) / index;
		else
			tpm = float.MaxValue;
	}

	[TargetRpc]
	public void TargetRecieveTPM(NetworkConnection target, float f)
	{
		if (timePerMove > f)
			timePerMove = f;
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
		if (GetComponent<CharacterStats>() != null)
		{
			int vision = GetComponent<CharacterStats>().vision;

			List<MapMaker.Node> visionArea = new List<MapMaker.Node>();
			for (int x = tileX - vision; x <= tileX + vision; x++)
				if (x >= 0 && x < map.mapSizeX)
					for (int y = tileY - vision; y <= tileY + vision; y++)
						if (y >= 0 && y < map.mapSizeY)
							for (int z = tileZ - vision; z <= tileZ + vision; z++)
								if (z >= 0 && z < map.mapSizeZ)
									if (PerceptionCheck(map.graph[x, y, z]))
									{
										visionArea.Add(map.graph[x, y, z]);
										map.GetTileFromNode(map.graph[x, y, z]).inView = true;
									}

			List<MapMaker.Node> temp = new List<MapMaker.Node>();
			foreach (MapMaker.Node node in fov)
				if (!visionArea.Contains(node))
					temp.Add(node);

			fov = visionArea;

			foreach (MapMaker.Node node in temp)
				map.GetTileFromNode(node).CheckView();
		}

		if (GetComponent<ShotScript>()!=null)
			GetComponent<ShotScript>().player.CheckVisibility();
		else 
			GetComponent<CharacterStats>().player.CheckVisibility();
	}

	public bool PerceptionCheck(MapMaker.Node node)
	{
		if (Physics.Linecast(new Vector3(transform.position.x, transform.position.y + unitHeight, transform.position.z),
								new Vector3(node.x, node.y + 1, node.z), tileMask))
			return false;
		return true;
	}
}

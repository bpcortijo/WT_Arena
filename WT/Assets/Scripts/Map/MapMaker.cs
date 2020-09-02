using System.IO;
using UnityEngine;
using System.Linq;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class MapMaker : NetworkBehaviour
{
	string fileName = "";
	public ManagementScript gm;

	int[,,] tiles;
	public Node[,,] graph;
	int focusedHeight = 0;
	public string mapName = null;
	public int minSpawnDist = 3;

	public int mapSizeX = 10, mapSizeY = 1, mapSizeZ = 10;

	public TileType[] tileTypes;

	public GameObject selectedUnit;
	public List<GameObject> units;
	public List<GameObject> viableSpawns;

	public List<ShotScript> attackPaths;
	public List<CharacterStats> characterPaths;

	public float turnMovementTime = 5;

	public GameObject mapCamera;

	void Start()
	{
		ReadMap();
		GeneratePathfindingGraph();
		GenerateMapVisual();
		AdjustPathfindingGraph();

		PlayerScript[] players = FindObjectsOfType<PlayerScript>();
		foreach (PlayerScript player in players)
		{
			player.map = this;
			player.SpawnTeam();
		}

		if (gm == null)
			gm = FindObjectOfType<ManagementScript>();
	}

	CameraScript CameraAction()
	{
		GameObject cam = Instantiate(mapCamera);
		Camera.main.gameObject.SetActive(false);
		return cam.GetComponent<CameraScript>();
	}

	void ReadMap()
	{
		if (mapName == null || mapName == "")
			mapName = "Test";
		string[] mapNameWords = mapName.Split(' ');
		foreach (string word in mapNameWords)
			fileName += word;
		fileName += "Map";
		var mapFile = Resources.Load<TextAsset>("MapFiles/" + fileName);
		List<string> mapData = mapFile.text.Split('\n').ToList();

		if (NetworkServer.active)
			foreach (string line in mapData)
				RpcCheckData(mapData.IndexOf(line), line);

		string[] mapDimensions = mapData[1].Split(' ');
		mapSizeX = int.Parse(mapDimensions[0]);
		mapSizeY = int.Parse(mapDimensions[1]);
		mapSizeZ = int.Parse(mapDimensions[2]);

		GenerateMapData();

		CameraScript cam = CameraAction();
		cam.SetPositions(mapSizeX, mapSizeY, mapSizeZ);
		string[] camRotations = mapData[4].Split(' ');
		cam.swRot.eulerAngles = new Vector3(float.Parse(camRotations[1]), float.Parse(camRotations[2]), float.Parse(camRotations[3]));
		camRotations = mapData[5].Split(' ');
		cam.seRot.eulerAngles = new Vector3(float.Parse(camRotations[1]), float.Parse(camRotations[2]), float.Parse(camRotations[3]));
		camRotations = mapData[6].Split(' ');
		cam.neRot.eulerAngles = new Vector3(float.Parse(camRotations[1]), float.Parse(camRotations[2]), float.Parse(camRotations[3]));
		camRotations = mapData[7].Split(' ');
		cam.nwRot.eulerAngles = new Vector3(float.Parse(camRotations[1]), float.Parse(camRotations[2]), float.Parse(camRotations[3]));
		cam.transform.rotation = cam.swRot;

		for (int i = 11; i < mapData.Count; i++)
		{
			if (!mapData[i].Contains("//") && mapData[i].Trim() != "")
			{
				List<string> nums;
				List<string> line = mapData[i].Split(' ').ToList();
				if (!mapData[i].Contains('-'))
					tiles[int.Parse(line[0]), int.Parse(line[1]), int.Parse(line[2])] = int.Parse(line[4]);
				else
				{
					int x1 = 0, x2 = 0, y1 = 0, y2 = 0, z1 = 0, z2 = 0;
					if (line[0].Contains('-'))
					{
						nums = line[0].Split('-').ToList();
						x1 = int.Parse(nums[0]);
						x2 = int.Parse(nums[1]);
					}
					else
						x1 = int.Parse(line[0]);

					if (line[1].Contains('-'))
					{
						nums = line[1].Split('-').ToList();
						y1 = int.Parse(nums[0]);
						y2 = int.Parse(nums[1]);
					}
					else
						y1 = int.Parse(line[1]);

					if (line[2].Contains('-'))
					{
						nums = line[2].Split('-').ToList();
						z1 = int.Parse(nums[0]);
						z2 = int.Parse(nums[1]);
					}
					else
						z1 = int.Parse(line[2]);

					if (x1 > x2)
						x2 = x1;
					if (y1 > y2)
						y2 = y1;
					if (z1 > z2)
						z2 = z1;

					for (int x = x1; x <= x2; x++)
						for (int y = y1; y <= y2; y++)
							for (int z = z1; z <= z2; z++)
								tiles[x, y, z] = int.Parse(line[4]);

				}
			}
		}
	}

	[ClientRpc]
	void RpcCheckData(int i, string line)
	{
		var mapFile = Resources.Load<TextAsset>("MapFiles/" + fileName);
		List<string> localMapData = mapFile.text.Split('\n').ToList();

		if (line == localMapData[i])
			Debug.Log("Cheater!");
	}

	void GenerateMapData()
	{
		// Allocate our map tiles
		tiles = new int[mapSizeX, mapSizeY, mapSizeZ];

		int x, y, z;

		// Initialize our map tiles to be Temp
		for (x = 0; x < mapSizeX; x++)
			for (y = 0; y < mapSizeY; y++)
				for (z = 0; z < mapSizeZ; z++)
					if (y == 0)
						tiles[x, y, z] = 1;
					else
						tiles[x, y, z] = 0;
	}

	void GeneratePathfindingGraph()
	{
		// Initialize the array
		graph = new Node[mapSizeX, mapSizeY, mapSizeZ];

		// Initialize a Node for each spot in the array
		for (int x = 0; x < mapSizeX; x++)
			for (int y = 0; y < mapSizeY; y++)
				for (int z = 0; z < mapSizeZ; z++)
				{
					graph[x, y, z] = new Node();
					graph[x, y, z].x = x;
					graph[x, y, z].y = y;
					graph[x, y, z].z = z;
				}

		// Now that all the nodes exist, calculate their neighbors
		for (int x = 0; x < mapSizeX; x++)
			for (int y = 0; y < mapSizeY; y++)
				for (int z = 0; z < mapSizeZ; z++)
				{
					if (x > 0)
					{
						graph[x, y, z].neighbors.Add(graph[x - 1, y, z]);
						if (y > 0)
						{
							graph[x, y, z].neighbors.Add(graph[x - 1, y - 1, z]);
							if (z > 0)
								graph[x, y, z].neighbors.Add(graph[x - 1, y - 1, z - 1]);
							if (z < mapSizeZ - 1)
								graph[x, y, z].neighbors.Add(graph[x - 1, y - 1, z + 1]);
						}
						if (y < mapSizeY - 1)
						{
							graph[x, y, z].neighbors.Add(graph[x - 1, y + 1, z]);
							if (z > 0)
								graph[x, y, z].neighbors.Add(graph[x - 1, y + 1, z - 1]);
							if (z < mapSizeZ - 1)
								graph[x, y, z].neighbors.Add(graph[x - 1, y + 1, z + 1]);
						}
						if (z > 0)
							graph[x, y, z].neighbors.Add(graph[x - 1, y, z - 1]);
						if (z < mapSizeZ - 1)
							graph[x, y, z].neighbors.Add(graph[x - 1, y, z + 1]);
					}

					if (x < mapSizeX - 1)
					{
						graph[x, y, z].neighbors.Add(graph[x + 1, y, z]);
						if (y > 0)
						{
							graph[x, y, z].neighbors.Add(graph[x + 1, y - 1, z]);
							if (z > 0)
								graph[x, y, z].neighbors.Add(graph[x + 1, y - 1, z - 1]);
							if (z < mapSizeZ - 1)
								graph[x, y, z].neighbors.Add(graph[x + 1, y - 1, z + 1]);
						}
						if (y < mapSizeY - 1)
						{
							graph[x, y, z].neighbors.Add(graph[x + 1, y + 1, z]);
							if (z > 0)
								graph[x, y, z].neighbors.Add(graph[x + 1, y + 1, z - 1]);
							if (z < mapSizeZ - 1)
								graph[x, y, z].neighbors.Add(graph[x + 1, y + 1, z + 1]);
						}
						if (z > 0)
							graph[x, y, z].neighbors.Add(graph[x + 1, y, z - 1]);
						if (z < mapSizeZ - 1)
							graph[x, y, z].neighbors.Add(graph[x + 1, y, z + 1]);
					}

					if (y > 0)
					{
						graph[x, y, z].neighbors.Add(graph[x, y - 1, z]);
						if (z > 0)
							graph[x, y, z].neighbors.Add(graph[x, y - 1, z - 1]);
						if (z < mapSizeZ - 1)
							graph[x, y, z].neighbors.Add(graph[x, y - 1, z + 1]);
					}
					if (y < mapSizeY - 1)
					{
						graph[x, y, z].neighbors.Add(graph[x, y + 1, z]);
						if (z > 0)
							graph[x, y, z].neighbors.Add(graph[x, y + 1, z - 1]);
						if (z < mapSizeZ - 1)
							graph[x, y, z].neighbors.Add(graph[x, y + 1, z + 1]);
					}

					if (z > 0)
						graph[x, y, z].neighbors.Add(graph[x, y, z - 1]);
					if (z < mapSizeZ - 1)
						graph[x, y, z].neighbors.Add(graph[x, y, z + 1]);
				}
	}

	void GenerateMapVisual()
	{
		for (int x = 0; x < mapSizeX; x++)
			for (int y = 0; y < mapSizeY; y++)
				for (int z = 0; z < mapSizeZ; z++)
				{
					TileType tt = tileTypes[tiles[x, y, z]];
					GameObject tile = Instantiate(tt.tileVisualPrefab, new Vector3(x, y, z), Quaternion.identity);
					tile.transform.parent = gameObject.transform;
					tile.transform.localRotation = Quaternion.identity;
					TileScript ts = tile.GetComponent<TileScript>();
					ts.prefabName = tile.name;
					tile.name = "(" + x.ToString() + ", " + y.ToString() + ", " + z.ToString() + ") ";
					ts.tt = tt;
					ts.tileX = x;
					ts.tileY = y;
					ts.tileZ = z;
					ts.map = this;
					if (ts.floor && !ts.full)
						viableSpawns.Add(tile);
				}
	}

	void AdjustPathfindingGraph()
	{
		for (int x = 0; x < mapSizeX; x++)
			for (int y = 0; y < mapSizeY; y++)
				for (int z = 0; z < mapSizeZ; z++)
				{
					TileScript ct = transform.Find("(" + x.ToString() + ", "
													+ y.ToString() + ", "
													+ z.ToString() + ") ").gameObject.GetComponent<TileScript>();
					AdjustNeighbors(ct, x, y, z);
				}
	}

	void AdjustNeighbors(TileScript currentTile, int x, int y, int z)
	{
		TileScript neighbor;

		if (x > 0)
			if (!currentTile.westViable)
			{
				neighbor = transform.Find("(" + (x - 1).ToString() + ", "
									+ y.ToString() + ", "
									+ z.ToString() + ") ").gameObject.GetComponent<TileScript>();
				neighbor.eastViable = false;
			}

		if (x < mapSizeX - 1)
			if (!currentTile.eastViable)
			{
				neighbor = transform.Find("(" + (x + 1).ToString() + ", "
									+ y.ToString() + ", "
									+ z.ToString() + ") ").gameObject.GetComponent<TileScript>();
				neighbor.westViable = false;
			}

		if (y > 0)
			if (currentTile.floor)
			{
				neighbor = transform.Find("(" + x.ToString() + ", "
									+ (y - 1).ToString() + ", "
									+ z.ToString() + ") ").gameObject.GetComponent<TileScript>();
				neighbor.ceiling = true;
			}

		if (y < mapSizeY - 1)
			if (currentTile.ceiling)
			{
				neighbor = transform.Find("(" + x.ToString() + ", "
									+ (y + 1).ToString() + ", "
									+ z.ToString() + ") ").gameObject.GetComponent<TileScript>();
				neighbor.floor = true;
			}

		if (z > 0)
			if (!currentTile.southViable)
			{
				neighbor = transform.Find("(" + x.ToString() + ", "
									+ y.ToString() + ", "
									+ (z - 1).ToString() + ") ").gameObject.GetComponent<TileScript>();
				neighbor.northViable = false;
			}

		if (z < mapSizeZ - 1)
			if (!currentTile.northViable)
			{
				neighbor = transform.Find("(" + x.ToString() + ", "
									+ y.ToString() + ", "
									+ (z + 1).ToString() + ") ").gameObject.GetComponent<TileScript>();
				neighbor.southViable = false;
			}
	}

	public Vector3 TileCoordToWorldCoord(int x, int y, int z, bool putAtTileHeight)
	{
		if (putAtTileHeight)
			return new Vector3(x, y, z);
		else
			return new Vector3(x, y + .75f, z);
	}

	public TileScript GetTileFromNode(Node space)
	{
		TileScript[] tiles = FindObjectsOfType<TileScript>();
		foreach (TileScript ts in tiles)
		{
			if (ts.tileX == space.x)
				if (ts.tileY == space.y)
					if (ts.tileZ == space.z)
						return ts;
		}
		return null;
	}

	public int Distance(Node origin, Node target)
	{
		int r = origin.x - target.x;
		if (r < origin.y - target.y)
			r = origin.y - target.y;
		if (r < origin.z - target.z)
			r = origin.z - target.z;
		return r;
	}

	public void AddToPath(UnitBasics unit, int x, int y, int z)
	{
		unit.shortPath.Add(graph[x, y, z]);
	}

	public void GeneratePathTo(int x, int y, int z)
	{
		if (gm.playResults)
			return;

		Node source = null;
		List<Node> path = new List<Node>();

		UnitBasics unit = selectedUnit.GetComponent<UnitBasics>();

		if (unit.plannedPath == null)
			source = graph[
					unit.tileX,
					unit.tileY,
					unit.tileZ
					];

		else if (unit.plannedPath.Count <= 1)
		{
			source = graph[
					unit.tileX,
					unit.tileY,
					unit.tileZ
					];
			unit.plannedPath = new List<Node>();
		}
		else
			source = unit.plannedPath[unit.plannedPath.Count - 1];

		Dictionary<Node, float> dist = new Dictionary<Node, float>();
		Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

		// Setup the "Q" -- the list of nodes we haven't checked yet.
		List<Node> unvisited = new List<Node>();

		Node target = graph[x, y, z];

		if (selectedUnit.GetComponent<UnitBasics>() != null)
		{
			if (selectedUnit.GetComponent<UnitBasics>().keyPoints.Count == 0)
				selectedUnit.GetComponent<UnitBasics>().keyPoints.Add(source);
			if (!selectedUnit.GetComponent<UnitBasics>().fullPath)
				selectedUnit.GetComponent<UnitBasics>().keyPoints.Add(target);
		}

		dist[source] = 0;
		prev[source] = null;

		// Initialize everything to have INFINITY distance, since
		// we don't know any better right now. Also, it's possible
		// that some nodes CAN'T be reached from the source,
		// which would make INFINITY a reasonable value
		foreach (Node v in graph)
		{
			if (v != source)
			{
				dist[v] = Mathf.Infinity;
				prev[v] = null;
			}

			unvisited.Add(v);
		}

		while (unvisited.Count > 0)
		{
			// "nxt" is going to be the unvisited node with the smallest distance.
			Node nxt = null;

			foreach (Node potentialNext in unvisited)
			{
				if (nxt == null || dist[potentialNext] < dist[nxt])
				{
					nxt = potentialNext;
				}
			}

			if (nxt == target)
			{
				break;  // Exit the while loop!
			}

			unvisited.Remove(nxt);

			foreach (Node neighbor in nxt.neighbors)
			{
				float alt = dist[nxt] + nxt.DistanceTo(neighbor)
										+ CostToEnter(neighbor.x, neighbor.y, neighbor.z, nxt.x, nxt.y, nxt.z);

				if (alt < dist[neighbor])
				{
					dist[neighbor] = alt;
					prev[neighbor] = nxt;
				}
			}
		}

		// If we get there, the either we found the shortest route
		// to our target, or there is no route at ALL to our target.

		if (prev[target] == null)
		{
			// No route between our target and the source
			return;
		}

		Node currentStep = target;

		// Step through the "prev" chain and add it to our path
		while (currentStep != null)
		{
			path.Add(currentStep);
			currentStep = prev[currentStep];
		}

		// Right now, currentPath describes a route from out target to our source
		// So we need to invert it!

		path.Add(graph[unit.tileX, unit.tileY, unit.tileZ]);
		path.Reverse();

		if (unit.plannedPath == null)
			unit.plannedPath = path;
		else
			for (int n = 1; n < path.Count; n++)
				unit.plannedPath.Add(path[n]);
		unit.CheckPath();
	}

	//public void GeneratePathUsingAStar(int x, int y, int z)
	//{
	//	if (gm.playResults)
	//		return;

	//	List<Node> path = new List<Node>();
	//	Node source = null, target = graph[x, y, z];

	//	UnitBasics unit = selectedUnit.GetComponent<UnitBasics>();

	//	if (unit.plannedPath == null)
	//		source = graph[
	//				unit.tileX,
	//				unit.tileY,
	//				unit.tileZ
	//				];

	//	else if (unit.plannedPath.Count <= 1)
	//	{
	//		source = graph[
	//				unit.tileX,
	//				unit.tileY,
	//				unit.tileZ
	//				];
	//		unit.plannedPath = new List<Node>();
	//	}
	//	else
	//		source = unit.plannedPath[unit.plannedPath.Count - 1];

	//	List<Node> open = new List<Node> { source };
	//	List<Node> closed = new List<Node>();

	//	for (int mapX = 0; mapX < mapSizeX; mapX++)
	//		for (int mapY = 0; mapY < mapSizeX; mapY++)
	//			for (int mapZ = 0; mapZ < mapSizeX; mapZ++)
	//			{
	//				Node node = graph[mapX, mapY, mapZ];
	//				node.gCost = int.MaxValue;
	//				node.CalculateF();
	//				node.previous = null;
	//			}

	//	source.gCost = 0;
	//	source.DistanceTo(target);
	//	source.CalculateF();

	//	while (open.Count > 0)
	//	{
	//		Node node = GetLowestF(open);
	//		if (node == target)
	//			break;

	//		open.Remove(node);
	//		closed.Add(node);

	//		foreach (Node neighbor in node.neighbors)
	//		{
	//			if (closed.Contains(neighbor))
	//				continue;

	//			int tentativeG = node.gCost + node.CalculateDistanceCost(neighbor);
	//			if (tentativeG<neighbor.gCost)
	//			{
	//				neighbor.previous = node;
	//				neighbor.gCost = tentativeG;
	//				neighbor.hCost = neighbor.CalculateDistanceCost(target);
	//				neighbor.CalculateF();

	//				if (!open.Contains(neighbor))
	//					open.Add(neighbor);
	//			}
	//		}
	//	}

	//	path.Add(target);
	//	Node currentNode = target;
	//	while (currentNode.previous != null)
	//	{
	//		path.Add(currentNode.previous);
	//		currentNode = currentNode.previous;
	//	}

	//	path.Reverse();

	//	if (unit.plannedPath == null)
	//		unit.plannedPath = path;
	//	else
	//		for (int n = 1; n < path.Count; n++)
	//			unit.plannedPath.Add(path[n]);
	//	unit.CheckPath();
	//}

	Node GetLowestF(List<Node> nodes)
	{
		Node lowest = nodes[0];
		for (int i = 1; i < nodes.Count; i++)
			if (nodes[i].fCost < lowest.fCost)
				lowest = nodes[i];

		return lowest;
	}

	float CostToEnter(int currentX, int currentY, int currentZ, int newX, int newY, int newZ)
	{
		if (CanEnter(currentX, currentY, currentZ, newX, newY, newZ))
		{
			TileType tt = tileTypes[tiles[newX, newY, newZ]];
			float cost = tt.movementcost;
			if (newX != currentX)
				cost += .001f;
			if (newY != currentY)
				cost += .001f;
			if (newZ != currentZ)
				cost += .001f;
			return cost;
		}
		else if (selectedUnit.tag == "Shot")
		{
			float cost = 1;
			if (newX != currentX)
				cost += .001f;
			if (newY != currentY)
				cost += .001f;
			if (newZ != currentZ)
				cost += .001f;
			return cost;
		}
		else
			return 99f;
	}

	public bool CanEnter(int currentX, int currentY, int currentZ, int newX, int newY, int newZ)
	{
		GameObject next = transform.Find("(" + newX.ToString() + ", "
										+ newY.ToString() + ", "
										+ newZ.ToString() + ") ").gameObject;

		GameObject current = transform.Find("(" + currentX.ToString() + ", "
											+ currentY.ToString() + ", "
											+ currentZ.ToString() + ") ").gameObject;

		TileScript nextData = next.GetComponent<TileScript>();
		TileScript currentData = current.GetComponent<TileScript>();

		//if no floor fall straight down
		if (!currentData.floor)
			if (newX == currentX && newY < currentY && newZ == currentZ)
				return true;
			else
				return false;

		if (newY == currentY && nextData.floor
			|| newY < currentY && !nextData.ceiling
			|| newY > currentY && !currentData.ceiling)
		{
			if (newX == currentX)
			{
				if (newZ > currentZ && currentData.northViable )
					//&& currentData.defendNorth < 3 && nextData.defendSouth < 3)
					return true;
				if (newZ < currentZ && currentData.southViable )
					//&& currentData.defendSouth < 3 && nextData.defendNorth < 3)
					return true;
			}

			if (newZ == currentZ)
			{
				if (newX < currentX && currentData.westViable )
					//&& currentData.defendWest < 3 && nextData.defendEast < 3)
					return true;
				if (newX > currentX && currentData.eastViable )
					//&& currentData.defendEast < 3 && nextData.defendWest < 3)
					return true;
			}

			if (newX < currentX && newZ > currentZ && currentData.northViable && currentData.westViable)
				//&& nextData.southViable && nextData.eastViable && currentData.defendNorth < 3
				//&& currentData.defendWest < 3 && nextData.defendSouth < 3 && nextData.defendEast < 3)
				return true;
			if (newX > currentX && newZ > currentZ && currentData.northViable && currentData.eastViable)
				//&& nextData.southViable && nextData.westViable && currentData.defendNorth < 3
				//&& currentData.defendEast < 3 && nextData.defendSouth < 3 && nextData.defendWest < 3)
				return true;
			if (newX < currentX && newZ < currentZ && currentData.southViable && currentData.westViable)
				//&& nextData.northViable && nextData.eastViable && currentData.defendSouth < 3
				//&& currentData.defendWest < 3 && nextData.defendNorth < 3 && nextData.defendEast < 3)
				return true;
			if (newX > currentX && newZ < currentZ && currentData.southViable && currentData.eastViable)
				//&& nextData.northViable && nextData.westViable && currentData.defendSouth < 3
				//&& currentData.defendEast < 3 && nextData.defendNorth < 3 && nextData.defendWest < 3)
				return true;
		}
		return false;
	}

	public GameObject SpawnPoint()
	{
		int num = Random.Range(0, viableSpawns.Count);

		GameObject spawnPoint = viableSpawns[num];
		SpawnLimit(viableSpawns[num].GetComponent<TileScript>());
		return spawnPoint;
	}

	void SpawnLimit(TileScript spawnTile)
	{
		int x = spawnTile.tileX - minSpawnDist;
		int xMax = spawnTile.tileX + minSpawnDist;

		while (x <= xMax)
		{
			if (x >= 0 && x < mapSizeX)
			{
				int y = spawnTile.tileY - 2;
				int yMax = spawnTile.tileY + 2;
				while (y <= yMax)
				{
					if (y >= 0 && y < mapSizeY)
					{
						int z = spawnTile.tileZ - minSpawnDist;
						int zMax = spawnTile.tileZ + minSpawnDist;
						while (z <= zMax)
						{
							if (z >= 0 && z < mapSizeZ)
							{
								GameObject possibleSpawn = GetTileFromNode(graph[x, y, z]).gameObject;
								if (viableSpawns.Contains(possibleSpawn))
									viableSpawns.Remove(possibleSpawn);
							}
							z++;
						}
					}
					y++;
				}
			}
			x++;
		}

		//Debug.Log(viableSpawns.Count);
	}

	public void Select(GameObject go)
	{
		selectedUnit = go;
	}

	public void GetAttackPaths()
	{
		ShotScript[] shotCodes = FindObjectsOfType<ShotScript>();
		foreach (ShotScript shot in shotCodes)
			attackPaths.Add(shot);
	}

	void RemovePastIntersect(CharacterStats character, Node point)
	{
		List<Node> nodes = character.basics.plannedPath;
		int p = nodes.IndexOf(point);
		while (nodes.Count > p)
			nodes.RemoveAt(p);
		character.basics.CheckPath();
	}

	public Node TextToNode(string text)
	{
		string[] info = text.Split(',');
		return graph[int.Parse(info[0]), int.Parse(info[1]), int.Parse(info[2])];
	}

	public string NodeToText(Node node)
	{
		return node.x.ToString() + "," + node.y.ToString() + "," + node.z.ToString();
	}

	public class Node
	{
		public int x, y, z;
		public Node previous;
		public List<Node> neighbors;
		public int gCost, hCost, fCost;

		public Node()
		{
			neighbors = new List<Node>();
		}

		public void CalculateF()
		{
			fCost = gCost + hCost;
		}
		public float DistanceTo(Node newNode)
		{
			return Vector3.Distance(new Vector3(x, y, z), new Vector3(newNode.x, newNode.y, newNode.z));
		}

		public int CalculateDistanceCost(Node newNode)
		{
			int xDistance = Mathf.Abs(x - newNode.x);
			int yDistance = Mathf.Abs(y - newNode.y);
			int zDistance = Mathf.Abs(z - newNode.z);

			int remainingXY = xDistance - yDistance;
			int remainingXZ = xDistance - zDistance;
			int remainingYZ = yDistance - zDistance;

			if (Mathf.Min(xDistance, yDistance, zDistance) == xDistance)
				return 17 * xDistance + 14 * Mathf.Min(yDistance, zDistance) + 10 * remainingYZ;
			else if (Mathf.Min(xDistance, yDistance, zDistance) == yDistance)
				return 17 * yDistance + 14 * Mathf.Min(xDistance, zDistance) + 10 * remainingXZ;
			else
				return 17 * zDistance + 14 * Mathf.Min(xDistance, yDistance) + 10 * remainingXY;
		}
	}
}
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class MapMaker : MonoBehaviour {

    int[,,] tiles;
    public Node[,,] graph;
	int focusedHeight = 0;
	public string mapName = null;

    int mapSizeX = 10, mapSizeY = 1, mapSizeZ = 10;

	public TileType[] tileTypes;

    public GameObject selectedUnit;
    public List<GameObject> units;
    public List<GameObject> viableSpawns;

	public List<ShotScript> attackPaths;
	public List<CharacterStats> characterPaths;

	public float turnMovementTime = 5;

	void Start() {
        CreateMap();
		GeneratePathfindingGraph();
        GenerateMapVisual();
        AdjustPathfindingGraph();
	}

	void CreateMap() {
		switch (mapName)
		{

			default:
				mapSizeX = 10;
				mapSizeY = 2;
				mapSizeZ = 10;

				GenerateMapData();

				tiles[3, 0, 5] = 5;
				tiles[5, 1, 5] = 0;
				tiles[4, 0, 5] = 2;
				tiles[5, 0, 5] = 2;
				tiles[3, 0, 4] = 3;
				tiles[3, 0, 3] = 3;
				tiles[3, 0, 1] = 3;
				break;
		}
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
					tiles[x, y, z] = 1;
	}

	void GeneratePathfindingGraph() {
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
        for (int x=0; x < mapSizeX; x++) 
			for(int y=0; y < mapSizeY; y++) 
                for(int z = 0; z < mapSizeZ; z++)
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

	void GenerateMapVisual() {
		for (int x = 0; x < mapSizeX; x++)
			for (int y = 0; y < mapSizeY; y++)
				for (int z = 0; z < mapSizeZ; z++)
				{
					TileType tt = tileTypes[tiles[x, y, z]];
					GameObject go = Instantiate(tt.tileVisualPrefab, new Vector3(x, y, z), Quaternion.identity);
					go.transform.parent = gameObject.transform;
					go.transform.localRotation = Quaternion.identity;
					go.name = "(" + x.ToString() + ", " + y.ToString() + ", " + z.ToString() + ") ";
					TileScript ts = go.GetComponent<TileScript>();
					ts.tt = tt;
					ts.tileX = x;
					ts.tileY = y;
					ts.tileZ = z;
					ts.map = this;
					if (ts.floor)
						viableSpawns.Add(go);
				}
	}

    void AdjustPathfindingGraph()
    {
        for (int x = 0; x < mapSizeX; x++)
            for (int y = 0; y < mapSizeY; y++)
                for (int z = 0; z < mapSizeZ; z++)
                {
                    TileScript ct= transform.Find("(" + x.ToString() + ", " 
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

    public Vector3 TileCoordToWorldCoord(int x, int y, int z) {
		return new Vector3(x, y, z);
	}

	public void GeneratePathTo(int x, int y, int z) {
		if (selectedUnit.name == "Strong" || selectedUnit.name == "Composite")
			if (!selectedUnit.GetComponent<UnitBasics>().CheckSlope(x, y, z))
				return;

		Node source = null;
		List<Node> path = new List<Node>();

		UnitBasics unit = selectedUnit.GetComponent<UnitBasics>();

		if (!unit.vector || unit.plannedPath == null)
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
			if (!selectedUnit.GetComponent<UnitBasics>().full)
				selectedUnit.GetComponent<UnitBasics>().keyPoints.Add(target);
		}

		dist[source] = 0;
		prev[source] = null;

		// Initialize everything to have INFINITY distance, since
		// we don't know any better right now. Also, it's possible
		// that some nodes CAN'T be reached from the source,
		// which would make INFINITY a reasonable value
		foreach(Node v in graph) {
			if(v != source) {
				dist[v] = Mathf.Infinity;
				prev[v] = null;
			}

			unvisited.Add(v);
		}

		while(unvisited.Count > 0) {
			// "nxt" is going to be the unvisited node with the smallest distance.
			Node nxt = null;

			foreach(Node potentialNext in unvisited) {
				if(nxt == null || dist[potentialNext] < dist[nxt]) {
					nxt = potentialNext;
				}
			}

			if(nxt == target) {
				break;	// Exit the while loop!
			}

			unvisited.Remove(nxt);

			foreach(Node v in nxt.neighbors) {
                //float alt = dist[nxt] + nxt.DistanceTo(v);

                float alt = dist[nxt] + nxt.DistanceTo(v) + CostToEnter(v.x, v.y, v.z, nxt.x, nxt.y, nxt.z);

                if ( alt < dist[v] ) {
					dist[v] = alt;
					prev[v] = nxt;
				}
			}
		}

		// If we get there, the either we found the shortest route
		// to our target, or there is no route at ALL to our target.

		if(prev[target] == null) {
			// No route between our target and the source
			return;
		}

		Node currentStep = target;

		// Step through the "prev" chain and add it to our path
		while(currentStep != null) {
			path.Add(currentStep);
			currentStep = prev[currentStep];
		}

		// Right now, currentPath describes a route from out target to our source
		// So we need to invert it!

		path.Reverse();

		if (!unit.vector || unit.plannedPath == null)
			unit.plannedPath = path;
		else
			for (int n = 1; n < path.Count; n++)
				unit.plannedPath.Add(path[n]);
		unit.CheckPath();
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
                cost += .003f;
            if (newZ != currentZ)
                cost += .001f;
            return cost;
        }
		else if (selectedUnit.tag=="Shot")
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

		if (newY == currentY && nextData.floor
			|| newY < currentY && !nextData.ceiling && nextData.defendCeiling < 3
			|| newY > currentY && !currentData.ceiling && currentData.floor && currentData.defendCeiling < 3
			|| newY > currentY && !currentData.ceiling && currentData.defendFloor >= 3 && currentData.defendCeiling < 3)
		{
			if (newX == currentX)
			{
				if (newZ > currentZ && currentData.northViable && currentData.defendNorth < 3 && nextData.defendSouth < 3)
					return true;
				if (newZ < currentZ && currentData.southViable && currentData.defendSouth < 3 && nextData.defendNorth < 3)
					return true;
			}

			if (newZ == currentZ)
			{
				if (newX < currentX && currentData.westViable && currentData.defendWest < 3 && nextData.defendEast < 3)
					return true;
				if (newX > currentX && currentData.eastViable && currentData.defendEast < 3 && nextData.defendWest < 3)
					return true;
			}

			if (newX < currentX && newZ > currentZ && currentData.northViable && currentData.westViable
				&& nextData.southViable && nextData.eastViable && currentData.defendNorth < 3
				&& currentData.defendWest < 3 && nextData.defendSouth < 3 && nextData.defendEast < 3)
				return true;
			if (newX > currentX && newZ > currentZ && currentData.northViable && currentData.eastViable
				&& nextData.southViable && nextData.westViable && currentData.defendNorth < 3
				&& currentData.defendEast < 3 && nextData.defendSouth < 3 && nextData.defendWest < 3)
				return true;
			if (newX < currentX && newZ < currentZ && currentData.southViable && currentData.westViable
				&& nextData.northViable && nextData.eastViable && currentData.defendSouth < 3
				&& currentData.defendWest < 3 && nextData.defendNorth < 3 && nextData.defendEast < 3)
				return true;
			if (newX > currentX && newZ < currentZ && currentData.southViable && currentData.eastViable
				&& nextData.northViable && nextData.westViable && currentData.defendSouth < 3
				&& currentData.defendEast < 3 && nextData.defendNorth < 3 && nextData.defendWest < 3)
				return true;
		}
		return false;
    }

    public GameObject SpawnPoint()
    {
		int num = Random.Range(0, viableSpawns.Count);

		GameObject spawnPoint = viableSpawns[num];
		viableSpawns.Remove(viableSpawns[num]);
		return spawnPoint;
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

	public void CheckAllPaths()
	{
		// Check if players get in eachother's way

		foreach (CharacterStats character in characterPaths)
			CheckIfCrossed(character);

		Node currentPoint = null;
		float currentTiming =99f;
		CharacterStats currentHit=null;

		foreach (ShotScript atk in attackPaths)
		{
			foreach (Node point in atk.basics.shortPath)
				foreach (CharacterStats character in characterPaths)
				{
					if (character.basics.shortPath.Contains(point)&&atk.owner.transform.parent!=character.transform.parent)
					{
						float timing = character.basics.shortPath.IndexOf(point) / character.basics.speed;
						if (timing < currentTiming)
						{
							currentHit = character;
							currentTiming = timing;
							currentPoint = point;
						}
					}
					break;
				}

			//Damage breakdown
			if (currentHit != null)
			{
				if (currentHit.basics.shortPath.Count == 1)
					atk.Impact(currentHit, 1, false);
				else
				{
					float damagePercent = currentHit.basics.shortPath.IndexOf(currentPoint) / currentHit.basics.shortPath.Count - 1;
					atk.Impact(currentHit, damagePercent, true);
				}
			}
		}

		attackPaths.Clear();
		characterPaths.Clear();
	}

	void CheckIfCrossed(CharacterStats character)
	{
		foreach (Node step in character.basics.plannedPath)
			foreach (CharacterStats other in characterPaths)
				if (other.basics.plannedPath.Contains(step) && other != character)
				{
					if (character.basics.plannedPath.IndexOf(step) / character.basics.speed == other.basics.plannedPath.IndexOf(step) / other.basics.speed)
					{
						RemovePastIntersect(character, step);
						RemovePastIntersect(other, step);
					}
					else if (character.basics.plannedPath.IndexOf(step) / character.basics.speed > other.basics.plannedPath.IndexOf(step) / other.basics.speed
								&& character.basics.plannedPath.IndexOf(step) - 1 / character.basics.speed < other.basics.plannedPath.IndexOf(step) / other.basics.speed)
						RemovePastIntersect(character, step);
					else if (other.basics.plannedPath.IndexOf(step)==other.basics.plannedPath.Count-1)
						RemovePastIntersect(character, step);
					return;
				}
	}

	void RemovePastIntersect(CharacterStats character, Node point)
	{
		List<Node> nodes = character.basics.plannedPath;
		int p = nodes.IndexOf(point);
		while (nodes.Count > p)
			nodes.RemoveAt(p);
		character.basics.CheckPath();
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

    public class Node
    {
        public int x, y, z;
        public List<Node> neighbors;

        public Node()
        {
            neighbors = new List<Node>();
        }

        public float DistanceTo(Node n)
        {
            return Vector3.Distance(new Vector3(x, y, z), new Vector3(n.x, n.y, n.z));
        }
    }
}

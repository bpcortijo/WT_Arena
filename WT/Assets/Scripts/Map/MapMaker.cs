using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class MapMaker : MonoBehaviour {

    int[,,] tiles;
    Node[,,] graph;

    public int mapSizeX = 10;
    public int mapSizeY = 1;
    public int mapSizeZ = 10;

	public TileType[] tileTypes;

    public GameObject selectedUnit;
    public List<GameObject> units;
    public List<GameObject> viableSpawns;

    void Start() {
        GenerateMapData();
		GeneratePathfindingGraph();
        GenerateMapVisual();
        AdjustPathfindingGraph();
	}

	void GenerateMapData() {
        // Allocate our map tiles
        tiles = new int[mapSizeX, mapSizeY, mapSizeZ];
		
		int x,y,z;

        // Initialize our map tiles to be Temp
        for (x = 0; x < mapSizeX; x++)
            for (y = 0; y < mapSizeY; y++)
                for (z = 0; z < mapSizeZ; z++)
                    tiles[x, y, z] = 1;

        //CustomizeMap()

        tiles[3, 0, 5] = 4;
        tiles[4, 0, 5] = 2;
        tiles[5, 0, 5] = 2;
        tiles[3, 0, 4] = 3;
        tiles[3, 0, 3] = 3;
        tiles[3, 0, 1] = 3;
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
                    if (tiles[x, y, z] != 0) {
                        TileType tt = tileTypes[tiles[x, y, z]];
                        GameObject go = Instantiate(tt.tileVisualPrefab, new Vector3(x, y, z), Quaternion.identity);
                        go.transform.parent = gameObject.transform;
                        go.name = "(" + x.ToString() + ", " + y.ToString() + ", " + z.ToString() + ") ";
                        TileScript ts = go.GetComponent<TileScript>();
                        ts.tileX = x;
                        ts.tileY = y;
                        ts.tileZ = z;
                        ts.map = this;
                        if (ts.floor)
                            viableSpawns.Add(go);
                    }
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

	void AdjustNeighbors(TileScript ct, int x, int y, int z)
    {
        TileScript n;

        if (x > 0)
            if (!ct.westViable)
            {
                n = transform.Find("(" + (x - 1).ToString() + ", "
                                    + y.ToString() + ", "
                                    + z.ToString() + ") ").gameObject.GetComponent<TileScript>();
                n.eastViable = false;
            }

        if (x < mapSizeX - 1)
            if (!ct.eastViable)
            {
                n = transform.Find("(" + (x + 1).ToString() + ", "
                                    + y.ToString() + ", "
                                    + z.ToString() + ") ").gameObject.GetComponent<TileScript>();
                n.westViable = false;
            }

        if (y > 0)
            if (!ct.floor)
            {
                n = transform.Find("(" + x.ToString() + ", "
                                    + (y - 1).ToString() + ", "
                                    + z.ToString() + ") ").gameObject.GetComponent<TileScript>();
                n.ceiling = false;
            }

        if (y < mapSizeY - 1)
            if (!ct.ceiling)
            {
                n = transform.Find("(" + x.ToString() + ", "
                                    + (y + 1).ToString() + ", "
                                    + z.ToString() + ") ").gameObject.GetComponent<TileScript>();
                n.floor = false;
            }

        if (z > 0)
            if (!ct.southViable)
            {
				n = transform.Find("(" + x.ToString() + ", "
									+ y.ToString() + ", "
									+ (z - 1).ToString() + ") ").gameObject.GetComponent<TileScript>();
                n.northViable = false;
            }

        if (z < mapSizeZ - 1)
            if (!ct.northViable)
            {
				n = transform.Find("(" + x.ToString() + ", "
									+ y.ToString() + ", "
									+ (z + 1).ToString() + ") ").gameObject.GetComponent<TileScript>();
                n.southViable = false;
            }
    }

    public Vector3 TileCoordToWorldCoord(int x, int y, int z) {
		return new Vector3(x, y, z);
	}

	public void GeneratePathTo(int x, int y, int z) {
		// Clear out our unit's old path.
		Node source = null;
		List<Node> currentPath = new List<Node>();

		UnitBasics unit = selectedUnit.GetComponent<UnitBasics>();

		if (!unit.vector || unit.currentPath == null)
			source = graph[
					unit.tileX,
					unit.tileY,
					unit.tileZ
					];
		else
			source = unit.currentPath[unit.currentPath.Count - 1];

		Dictionary<Node, float> dist = new Dictionary<Node, float>();
		Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

		// Setup the "Q" -- the list of nodes we haven't checked yet.
		List<Node> unvisited = new List<Node>();

        Node target = graph[x, y, z];
		
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
			currentPath.Add(currentStep);
			currentStep = prev[currentStep];
		}

		// Right now, currentPath describes a route from out target to our source
		// So we need to invert it!

		currentPath.Reverse();
		if (!unit.vector || unit.currentPath == null)
			unit.currentPath = currentPath;
		else
			for (int n = 1; n < currentPath.Count; n++)
				unit.currentPath.Add(currentPath[n]);
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
                cost += .001f;
            if (newZ != currentZ)
                cost += .001f;
            return cost;
        }
        else
            return 99f;
    }

    bool CanEnter(int currentX, int currentY, int currentZ, int newX, int newY, int newZ)
    {
        GameObject next = transform.Find("(" + newX.ToString() + ", "
                                        + newY.ToString() + ", "
                                        + newZ.ToString() + ") ").gameObject;

        GameObject current = transform.Find("(" + currentX.ToString() + ", "
                                            + currentY.ToString() + ", "
                                            + currentZ.ToString() + ") ").gameObject;

        TileScript nextData = next.GetComponent<TileScript>();
        TileScript currentData = current.GetComponent<TileScript>();

		if (newY == currentY)
		{
			if (newX == currentX)
			{
				if (newZ > currentZ && currentData.northViable)
					return true;
				if (newZ < currentZ && currentData.southViable)
					return true;
			}

			if (newZ == currentZ)
			{
				if (newX < currentX && currentData.westViable)
					return true;
				if (newX > currentX && currentData.eastViable)
					return true;
			}

			//if (newX < currentX && newZ > currentZ) //NW
			//{
			//	if (currentData.northViable)
			//		if (nextData.eastViable)
			//			return true;
			//	if (currentData.westViable)
			//		if (nextData.southViable)
			//			return true;
			//}
			//if (newX > currentX && newZ < currentZ) //NE
			//{
			//	if (currentData.northViable)
			//		if (nextData.westViable)
			//			return true;
			//	if (currentData.eastViable)
			//		if (nextData.southViable)
			//			return true;
			//}
			//if (newX > currentX && newZ < currentZ) //SW
			//{
			//	if (currentData.southViable)
			//		if (nextData.eastViable)
			//			return true;
			//	if (currentData.westViable)
			//		if (nextData.northViable)
			//			return true;
			//}
			//if (newX > currentX && newZ < currentZ) //SE
			//{
			//	if (currentData.southViable)
			//		if (nextData.westViable)
			//			return true;
			//	if (currentData.eastViable)
			//		if (nextData.northViable)
			//			return true;
			//}
			if (newX < currentX && newZ > currentZ && currentData.northViable && currentData.westViable
				&& nextData.southViable && nextData.eastViable)
				return true;
			if (newX > currentX && newZ > currentZ && currentData.northViable && currentData.eastViable
				&& nextData.southViable && nextData.westViable)
				return true;
			if (newX < currentX && newZ < currentZ && currentData.southViable && currentData.westViable
				&& nextData.northViable && nextData.eastViable)
				return true;
			if (newX > currentX && newZ < currentZ && currentData.southViable && currentData.eastViable
				&& nextData.northViable && nextData.westViable)
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

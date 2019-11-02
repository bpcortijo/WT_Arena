using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualPathScript : MonoBehaviour
{
	int step = 1;
	float stepTime = 1;

	public int speed;
	public MapMaker map;
	LineRenderer lineRenderer;
	float timeStep = 0f, pointToPoint;
	public float timeToAnimate, totalLineDistance=0, size=.1f;
	public List<MapMaker.Node> nodePath = new List<MapMaker.Node>();

	void Start()
    {
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.positionCount = nodePath.Count;
		lineRenderer.widthMultiplier = size;

		stepTime = timeToAnimate / speed;

		int i = 0;
		while (i < nodePath.Count)
		{
			lineRenderer.SetPosition(i, map.TileCoordToWorldCoord(nodePath[i].x, nodePath[i].y, nodePath[i].z, false));
			if (i > 1)
				totalLineDistance += Vector3.Distance(map.TileCoordToWorldCoord(nodePath[i].x, nodePath[i].y, nodePath[i].z, false),
													map.TileCoordToWorldCoord(nodePath[i - 1].x, nodePath[i - 1].y, nodePath[i - 1].z, false));
			i++;
		}
	}
}

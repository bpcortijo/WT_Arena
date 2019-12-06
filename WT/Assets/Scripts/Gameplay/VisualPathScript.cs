using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualPathScript : MonoBehaviour
{
	int step = 1;
	float stepTime = 1;
	public UnitBasics represents;

	Transform localPlayer;

	public int speed;
	public MapMaker map;
	LineRenderer lineRenderer;
	float timeStep = 0f, pointToPoint;
	public float timeToAnimate, totalLineDistance=0, size=.1f;
	public List<MapMaker.Node> nodePath = new List<MapMaker.Node>();
	public Dictionary<MapMaker.Node, bool> visibleNodePath = new Dictionary<MapMaker.Node, bool>();

	void Start()
    {
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.positionCount = nodePath.Count;
		lineRenderer.widthMultiplier = size;

//		stepTime = timeToAnimate / speed;

		int i = 0;
		while (i < nodePath.Count)
		{
			lineRenderer.SetPosition(i, map.TileCoordToWorldCoord(nodePath[i].x, nodePath[i].y, nodePath[i].z, false));
			if (i > 1)
				totalLineDistance += Vector3.Distance(map.TileCoordToWorldCoord(nodePath[i].x, nodePath[i].y, nodePath[i].z, false),
													map.TileCoordToWorldCoord(nodePath[i - 1].x, nodePath[i - 1].y, nodePath[i - 1].z, false));
			i++;
		}

		PlayerScript[] users = FindObjectsOfType<PlayerScript>();
		foreach (PlayerScript user in users)
			if (user.isLocalPlayer)
				localPlayer = user.transform;

		foreach (MapMaker.Node node in nodePath)
		{
			if (represents.GetComponent<ShotScript>() != null)
			{
				if (represents.GetComponent<ShotScript>().owner.transform.parent = localPlayer)
					visibleNodePath.Add(node, true);
			}
			else if (represents.transform.parent = localPlayer)
				visibleNodePath.Add(node, true);
			else
				visibleNodePath.Add(node, false);
		}
	}
}

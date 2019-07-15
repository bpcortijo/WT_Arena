using UnityEngine;
using System.Collections.Generic;

public class UnitBasics : MonoBehaviour {
	public int speed, bonus;
	public float PieceMovementTime = 3, tempTime;

	public MapMaker map;
	public float unitHeight;
    public int tileX, tileY, tileZ, turns;
	public List<MapMaker.Node> currentPath = null;
	bool lerpNow;

	public void CheckPath()
	{
		if (gameObject.tag == "Player")
			while (currentPath.Count > speed * 2 + 1 + bonus)
				currentPath.RemoveAt(currentPath.Count - 1);
		if (gameObject.tag == "Attack")
			while (currentPath.Count > speed * 2 * turns + 1)
				currentPath.RemoveAt(currentPath.Count - 1);
	}

	private void Update()
    {
        if (currentPath != null)
        {
            for (int currentStep = 0; currentStep < currentPath.Count - 1; currentStep++)
            {
                Vector3 start = map.TileCoordToWorldCoord(currentPath[currentStep].x,
                                                            currentPath[currentStep].y,
                                                            currentPath[currentStep].z);
                Vector3 end = map.TileCoordToWorldCoord(currentPath[currentStep + 1].x,
                                                            currentPath[currentStep + 1].y,
                                                            currentPath[currentStep + 1].z);
                start.y += unitHeight;
                end.y += unitHeight;
                Debug.DrawLine(start, end, Color.red);
            }
        }
    }

	public void Move()
	{
		for (int s = 0; s < speed; s++)
		{
			if (currentPath == null)
				return;
			currentPath.RemoveAt(0);

			transform.position = map.TileCoordToWorldCoord(currentPath[0].x,
															currentPath[0].y,
															currentPath[0].z);

			if (currentPath.Count == 1)
			{
				tileX = currentPath[0].x;
				tileY = currentPath[0].y;
				tileZ = currentPath[0].z;
				currentPath = null;
			}
		}
	}
}

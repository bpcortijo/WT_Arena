using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selection : MonoBehaviour {
    public MapMaker map;

    private void Start()
    {
        map = transform.parent.GetComponent<UnitBasics>().map;
        transform.parent.GetComponent<UnitBasics>().unitHeight = transform.localPosition.y;
    }

    private void OnMouseUp()
	{
		if (map.selectedUnit != null)
			if (map.selectedUnit.name == "Hunter")
				if (map.selectedUnit.GetComponent<ShotScript>().target == null)
				{
					map.selectedUnit.GetComponent<ShotScript>().target = gameObject.transform.parent.gameObject;
					UnitBasics target = gameObject.transform.parent.GetComponent<UnitBasics>();
					map.GeneratePathTo(target.tileX, target.tileY, target.tileZ);
					map.selectedUnit = map.selectedUnit.GetComponent<ShotScript>().owner;
					return;
				}
		map.Select(gameObject.transform.parent.gameObject);
    }
}

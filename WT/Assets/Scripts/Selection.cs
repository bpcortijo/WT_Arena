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
		map.Select(gameObject.transform.parent.gameObject);
    }
}

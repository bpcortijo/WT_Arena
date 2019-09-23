using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DefenceUI : MonoBehaviour
{
	int speed = 2;
	MapMaker mapCode;
	public bool on = false;
	public int northPos = 1;
	public TileScript selectedTile;
	public GameObject north, west, south, east, background;
	Vector3 frontLeft = new Vector3(-35, 0, 0), frontRight = new Vector3(35, 0, 0),
			backLeft = new Vector3(-90, 120, 0), backRight = new Vector3(90, 120, 0), 
			backgroundOn = new Vector3(250, 0, 0), backgroundOff = new Vector3(550, 0, 0);
    void Start()
    {
		east.GetComponent<RectTransform>().anchoredPosition = frontRight;
		south.GetComponent<RectTransform>().anchoredPosition = frontLeft;
		west.GetComponent<RectTransform>().anchoredPosition = backLeft;
		north.GetComponent<RectTransform>().anchoredPosition = backRight;
		background.GetComponent<RectTransform>().anchoredPosition = backgroundOff;
		mapCode = GameObject.FindObjectOfType<MapMaker>().GetComponent<MapMaker>();
	}

	void Update()
    {
		if (mapCode.selectedUnit.GetComponent<CharacterStats>() == null)
			background.GetComponent<RectTransform>().anchoredPosition =
				Vector3.Lerp(background.GetComponent<RectTransform>().anchoredPosition, backgroundOff, Time.deltaTime * speed);
		else if (!mapCode.selectedUnit.GetComponent<CharacterStats>().defending)
			background.GetComponent<RectTransform>().anchoredPosition =
				Vector3.Lerp(background.GetComponent<RectTransform>().anchoredPosition, backgroundOff, Time.deltaTime * speed);
		else
			background.GetComponent<RectTransform>().anchoredPosition =
				Vector3.Lerp(background.GetComponent<RectTransform>().anchoredPosition, backgroundOn, Time.deltaTime * speed);

		switch (northPos)
		{
			case 1:
				north.GetComponent<RectTransform>().anchoredPosition =
					Vector3.Lerp(north.GetComponent<RectTransform>().anchoredPosition, backRight, Time.deltaTime * speed);
				east.GetComponent<RectTransform>().anchoredPosition = 
					Vector3.Lerp(east.GetComponent<RectTransform>().anchoredPosition, frontRight, Time.deltaTime * speed);
				south.GetComponent<RectTransform>().anchoredPosition = 
					Vector3.Lerp(south.GetComponent<RectTransform>().anchoredPosition, frontLeft, Time.deltaTime * speed);
				west.GetComponent<RectTransform>().anchoredPosition =
					Vector3.Lerp(west.GetComponent<RectTransform>().anchoredPosition, backLeft, Time.deltaTime * speed);
				break;
			case 2:
				north.GetComponent<RectTransform>().anchoredPosition =
					Vector3.Lerp(north.GetComponent<RectTransform>().anchoredPosition, frontRight, Time.deltaTime * speed);
				east.GetComponent<RectTransform>().anchoredPosition =
					Vector3.Lerp(east.GetComponent<RectTransform>().anchoredPosition, frontLeft, Time.deltaTime * speed);
				south.GetComponent<RectTransform>().anchoredPosition =
					Vector3.Lerp(south.GetComponent<RectTransform>().anchoredPosition, backLeft, Time.deltaTime * speed);
				west.GetComponent<RectTransform>().anchoredPosition =
					Vector3.Lerp(west.GetComponent<RectTransform>().anchoredPosition, backRight, Time.deltaTime * speed);
				break;
			case 3:
				north.GetComponent<RectTransform>().anchoredPosition =
					Vector3.Lerp(north.GetComponent<RectTransform>().anchoredPosition, frontLeft, Time.deltaTime * speed);
				east.GetComponent<RectTransform>().anchoredPosition =
					Vector3.Lerp(east.GetComponent<RectTransform>().anchoredPosition, backLeft, Time.deltaTime * speed);
				south.GetComponent<RectTransform>().anchoredPosition =
					Vector3.Lerp(south.GetComponent<RectTransform>().anchoredPosition, backRight, Time.deltaTime * speed);
				west.GetComponent<RectTransform>().anchoredPosition =
					Vector3.Lerp(west.GetComponent<RectTransform>().anchoredPosition, frontRight, Time.deltaTime * speed);
				break;
			case 4:
				north.GetComponent<RectTransform>().anchoredPosition =
					Vector3.Lerp(north.GetComponent<RectTransform>().anchoredPosition, backLeft, Time.deltaTime * speed);
				east.GetComponent<RectTransform>().anchoredPosition =
					Vector3.Lerp(east.GetComponent<RectTransform>().anchoredPosition, backRight, Time.deltaTime * speed);
				south.GetComponent<RectTransform>().anchoredPosition =
					Vector3.Lerp(south.GetComponent<RectTransform>().anchoredPosition, frontRight, Time.deltaTime * speed);
				west.GetComponent<RectTransform>().anchoredPosition =
					Vector3.Lerp(west.GetComponent<RectTransform>().anchoredPosition, frontLeft, Time.deltaTime * speed);
				break;
			case 5:
				northPos = 1;
				break;
			case 0:
				northPos = 4;
				break;
			default:
				break;
		}
    }

	public void PressBackRight()
	{
		string dir = null;
		switch (northPos)
		{
			case 1:
				dir = "North";
				break;
			case 2:
				dir = "West";
				break;
			case 3:
				dir = "South";
				break;
			case 4:
				dir = "East";
				break;
			default:
				break;
		}
		mapCode.selectedUnit.GetComponent<CharacterStats>().BlockTile(dir);
	}

	public void PressFrontRight()
	{
		string dir = null;
		switch (northPos)
		{
			case 1:
				dir = "East";
				break;
			case 2:
				dir = "North";
				break;
			case 3:
				dir = "West";
				break;
			case 4:
				dir = "South";
				break;
			default:
				break;
		}
		mapCode.selectedUnit.GetComponent<CharacterStats>().BlockTile(dir);
	}

	public void PressFrontLeft()
	{
		string dir = null;
		switch (northPos)
		{
			case 1:
				dir = "South";
				break;
			case 2:
				dir = "East";
				break;
			case 3:
				dir = "North";
				break;
			case 4:
				dir = "West";
				break;
			default:
				break;
		}
		mapCode.selectedUnit.GetComponent<CharacterStats>().BlockTile(dir);
	}

	public void PressBackLeft()
	{
		string dir = null;
		switch (northPos)
		{
			case 1:
				dir = "West";
				break;
			case 2:
				dir = "South";
				break;
			case 3:
				dir = "East";
				break;
			case 4:
				dir = "North";
				break;
			default:
				break;
		}
		mapCode.selectedUnit.GetComponent<CharacterStats>().BlockTile(dir);
	}

	public void PressTop()
	{
		mapCode.selectedUnit.GetComponent<CharacterStats>().BlockTile("Up");
	}

	public void PressBottom()
	{
		mapCode.selectedUnit.GetComponent<CharacterStats>().BlockTile("Down");
	}
}

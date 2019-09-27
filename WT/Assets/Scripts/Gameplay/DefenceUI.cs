using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DefenceUI : MonoBehaviour
{
	MapMaker mapCode;
	float rotateTime = 2;
	bool rotating = false;
	int uiSpeed = 3, letterSpeed = 2;
	Vector3 backgroundOn = new Vector3(250, 0, 0), backgroundOff = new Vector3(550, 0, 0),
			brPos = new Vector3(60, 180, 0), blPos = new Vector3(-60, 180, 0);

	public bool on = false;
	public int northPos = 1;
	public TileScript selectedTile;
	public GameObject north, west, south, east, background;
	public GameObject backRight, backLeft, frontRight, frontLeft;

    void Start()
    {
		east.transform.parent= frontRight.transform;
		south.transform.parent = frontLeft.transform;
		west.transform.parent = backLeft.transform;
		north.transform.parent = backRight.transform;
		north.GetComponent<RectTransform>().anchoredPosition = brPos;
		west.GetComponent<RectTransform>().anchoredPosition = blPos;
		south.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
		east.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

		background.GetComponent<RectTransform>().anchoredPosition = backgroundOff;
		mapCode = FindObjectOfType<MapMaker>().GetComponent<MapMaker>();
	}

	void Update()
	{
		if (mapCode.selectedUnit == null)
		{
			background.GetComponent<RectTransform>().anchoredPosition =
				Vector3.Lerp(background.GetComponent<RectTransform>().anchoredPosition, backgroundOff, Time.deltaTime * uiSpeed);
			on = false;
		}

		else if (mapCode.selectedUnit.GetComponent<CharacterStats>() == null)
		{
			background.GetComponent<RectTransform>().anchoredPosition =
				Vector3.Lerp(background.GetComponent<RectTransform>().anchoredPosition, backgroundOff, Time.deltaTime * uiSpeed);
			on = false;
		}

		else if (!mapCode.selectedUnit.GetComponent<CharacterStats>().defending)
		{
			background.GetComponent<RectTransform>().anchoredPosition =
				Vector3.Lerp(background.GetComponent<RectTransform>().anchoredPosition, backgroundOff, Time.deltaTime * uiSpeed);
			on = false;
		}

		else if (mapCode.selectedUnit.GetComponent<CharacterStats>().selectedTile == null)
		{
			background.GetComponent<RectTransform>().anchoredPosition =
				Vector3.Lerp(background.GetComponent<RectTransform>().anchoredPosition, backgroundOff, Time.deltaTime * uiSpeed);
			on = false;
		}

		else
		{
			background.GetComponent<RectTransform>().anchoredPosition =
				  Vector3.Lerp(background.GetComponent<RectTransform>().anchoredPosition, backgroundOn, Time.deltaTime * uiSpeed);
			on = true;
		}

		if (rotating)
			if (on)
			{
				switch (northPos)
				{
					case 1:
						north.transform.parent = backRight.transform;
						east.transform.parent = frontRight.transform;
						south.transform.parent = frontLeft.transform;
						west.transform.parent = backLeft.transform;

						north.GetComponent<RectTransform>().anchoredPosition =
							Vector3.Lerp(north.GetComponent<RectTransform>().anchoredPosition, brPos, Time.deltaTime * letterSpeed);
						east.GetComponent<RectTransform>().anchoredPosition =
							Vector3.Lerp(east.GetComponent<RectTransform>().anchoredPosition, Vector3.zero, Time.deltaTime * letterSpeed);
						south.GetComponent<RectTransform>().anchoredPosition =
							Vector3.Lerp(south.GetComponent<RectTransform>().anchoredPosition, Vector3.zero, Time.deltaTime * letterSpeed);
						west.GetComponent<RectTransform>().anchoredPosition =
							Vector3.Lerp(west.GetComponent<RectTransform>().anchoredPosition, blPos, Time.deltaTime * letterSpeed);
						rotateTime -= Time.deltaTime;
						break;
					case 2:
						north.transform.parent = frontRight.transform;
						east.transform.parent = frontLeft.transform;
						south.transform.parent = backLeft.transform;
						west.transform.parent = backRight.transform;

						north.GetComponent<RectTransform>().anchoredPosition =
							Vector3.Lerp(north.GetComponent<RectTransform>().anchoredPosition, Vector3.zero, Time.deltaTime * letterSpeed);
						east.GetComponent<RectTransform>().anchoredPosition =
							Vector3.Lerp(east.GetComponent<RectTransform>().anchoredPosition, Vector3.zero, Time.deltaTime * letterSpeed);
						south.GetComponent<RectTransform>().anchoredPosition =
							Vector3.Lerp(south.GetComponent<RectTransform>().anchoredPosition, blPos, Time.deltaTime * letterSpeed);
						west.GetComponent<RectTransform>().anchoredPosition =
							Vector3.Lerp(west.GetComponent<RectTransform>().anchoredPosition, brPos, Time.deltaTime * letterSpeed);
						rotateTime -= Time.deltaTime;
						break;
					case 3:
						north.transform.parent = frontLeft.transform;
						east.transform.parent = backLeft.transform;
						south.transform.parent = backRight.transform;
						west.transform.parent = frontRight.transform;

						north.GetComponent<RectTransform>().anchoredPosition =
							Vector3.Lerp(north.GetComponent<RectTransform>().anchoredPosition, Vector3.zero, Time.deltaTime * letterSpeed);
						east.GetComponent<RectTransform>().anchoredPosition =
							Vector3.Lerp(east.GetComponent<RectTransform>().anchoredPosition, blPos, Time.deltaTime * letterSpeed);
						south.GetComponent<RectTransform>().anchoredPosition =
							Vector3.Lerp(south.GetComponent<RectTransform>().anchoredPosition, brPos, Time.deltaTime * letterSpeed);
						west.GetComponent<RectTransform>().anchoredPosition =
							Vector3.Lerp(west.GetComponent<RectTransform>().anchoredPosition, Vector3.zero, Time.deltaTime * letterSpeed);
						rotateTime -= Time.deltaTime;
						break;
					case 4:
						north.transform.parent = backLeft.transform;
						east.transform.parent = backRight.transform;
						south.transform.parent = frontRight.transform;
						west.transform.parent = frontLeft.transform;

						north.GetComponent<RectTransform>().anchoredPosition =
							Vector3.Lerp(north.GetComponent<RectTransform>().anchoredPosition, blPos, Time.deltaTime * letterSpeed);
						east.GetComponent<RectTransform>().anchoredPosition =
							Vector3.Lerp(east.GetComponent<RectTransform>().anchoredPosition, brPos, Time.deltaTime * letterSpeed);
						south.GetComponent<RectTransform>().anchoredPosition =
							Vector3.Lerp(south.GetComponent<RectTransform>().anchoredPosition, Vector3.zero, Time.deltaTime * letterSpeed);
						west.GetComponent<RectTransform>().anchoredPosition =
							Vector3.Lerp(west.GetComponent<RectTransform>().anchoredPosition, Vector3.zero, Time.deltaTime * letterSpeed);
						rotateTime -= Time.deltaTime;
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
				if (rotateTime <= 0)
				{
					rotating = false;
					rotateTime = 3f;
				}
			}
			else
			{
				switch (northPos)
				{
					case 1:
						north.transform.parent = backRight.transform;
						east.transform.parent = frontRight.transform;
						south.transform.parent = frontLeft.transform;
						west.transform.parent = backLeft.transform;

						north.GetComponent<RectTransform>().anchoredPosition = brPos;
						east.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
						south.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
						west.GetComponent<RectTransform>().anchoredPosition = blPos;
						break;
					case 2:
						north.transform.parent = frontRight.transform;
						east.transform.parent = frontLeft.transform;
						south.transform.parent = backLeft.transform;
						west.transform.parent = backRight.transform;

						north.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
						east.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
						south.GetComponent<RectTransform>().anchoredPosition = blPos;
						west.GetComponent<RectTransform>().anchoredPosition = brPos;
						break;
					case 3:
						north.transform.parent = frontLeft.transform;
						east.transform.parent = backLeft.transform;
						south.transform.parent = backRight.transform;
						west.transform.parent = frontRight.transform;

						north.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
						east.GetComponent<RectTransform>().anchoredPosition = blPos;
						south.GetComponent<RectTransform>().anchoredPosition = brPos;
						west.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
						break;
					case 4:
						north.transform.parent = backLeft.transform;
						east.transform.parent = backRight.transform;
						south.transform.parent = frontRight.transform;
						west.transform.parent = frontLeft.transform;

						north.GetComponent<RectTransform>().anchoredPosition = blPos;
						east.GetComponent<RectTransform>().anchoredPosition = brPos;
						south.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
						west.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
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
				rotating = false;
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

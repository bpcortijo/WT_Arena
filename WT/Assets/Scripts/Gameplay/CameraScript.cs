using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
	bool turning;
	int cameraLocation = 1, speed = 2;
	public Vector3 sePos, swPos, nwPos, nePos;
	public Quaternion seRot, swRot, nwRot, neRot;

	public void CameraSpawn(string mapName)
    {
		//Put camera at south-east position and rotation
		switch (mapName)
		{
			default:
				sePos = new Vector3(-1.5f, 4, -1.5f);
				swPos = new Vector3(10.5f, 4, -1.5f);
				nwPos = new Vector3(10.5f, 4, 10.5f);
				nePos = new Vector3(-1.5f, 4, 10.5f);
				seRot.eulerAngles = new Vector3(40, 45, 0);
				swRot.eulerAngles = new Vector3(40, 315, 0);
				nwRot.eulerAngles = new Vector3(40, 225, 0);
				neRot.eulerAngles = new Vector3(40, 135, 0);
				break;
		}
		transform.position = sePos;
	}

	void Update()
	{
		if (Input.GetKeyUp(KeyCode.LeftArrow))
			cameraLocation--;
		if (Input.GetKeyUp(KeyCode.RightArrow))
			cameraLocation++;
		
		switch (cameraLocation)
		{
			case 1:
				transform.position = Vector3.Lerp(transform.position, sePos, Time.deltaTime * speed);
				transform.rotation = Quaternion.Lerp(transform.rotation, seRot, Time.deltaTime * speed);
				break;
			case 2:
				transform.position = Vector3.Lerp(transform.position, swPos, Time.deltaTime * speed);
				transform.rotation = Quaternion.Lerp(transform.rotation, swRot, Time.deltaTime * speed);
				break;
			case 3:
				transform.position = Vector3.Lerp(transform.position, nwPos, Time.deltaTime * speed);
				transform.rotation = Quaternion.Lerp(transform.rotation, nwRot, Time.deltaTime * speed);
				break;
			case 4:
				transform.position = Vector3.Lerp(transform.position, nePos, Time.deltaTime * speed);
				transform.rotation = Quaternion.Lerp(transform.rotation, neRot, Time.deltaTime * speed);
				break;
			case 5:
				cameraLocation = 1;
				break;
			case 0:
				cameraLocation = 4;
				break;
			default:
				break;
		}
	}
}

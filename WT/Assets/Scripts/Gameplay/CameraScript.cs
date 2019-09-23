using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
	public MapMaker mapCode;
	int cameraLocation = 1, speed = 2;
	public Vector3 sePos, swPos, nwPos, nePos;
	public Quaternion seRot, swRot, nwRot, neRot;

	void Start()
    {
		//Put camera at south-east position and rotation
		switch (mapCode.mapName)
		{
			default:
				break;
		}
	}

	void Update()
	{
		/*switch (cameraLocation)
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
		}*/
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
	bool turning;
	int cameraLocation = 1, speed = 2;
	public Vector3 swPos, sePos, nePos, nwPos;
	public Quaternion swRot, seRot, neRot, nwRot;

	public void SetPositions(int x, int y, int z)
	{
		float west = -2f, south = -2f, east = x + 1f, north = z + 1f, height = y + 3f;
		swPos = new Vector3(west, height, south);
		sePos = new Vector3(east, height, south);
		nePos = new Vector3(east, height, north);
		nwPos = new Vector3(west, height, north);

		transform.position = swPos;
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
				transform.position = Vector3.Lerp(transform.position, swPos, Time.deltaTime * speed);
				transform.rotation = Quaternion.Lerp(transform.rotation, swRot, Time.deltaTime * speed);
				break;
			case 2:
				transform.position = Vector3.Lerp(transform.position, sePos, Time.deltaTime * speed);
				transform.rotation = Quaternion.Lerp(transform.rotation, seRot, Time.deltaTime * speed);
				break;
			case 3:
				transform.position = Vector3.Lerp(transform.position, nePos, Time.deltaTime * speed);
				transform.rotation = Quaternion.Lerp(transform.rotation, neRot, Time.deltaTime * speed);
				break;
			case 4:
				transform.position = Vector3.Lerp(transform.position, nwPos, Time.deltaTime * speed);
				transform.rotation = Quaternion.Lerp(transform.rotation, nwRot, Time.deltaTime * speed);
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

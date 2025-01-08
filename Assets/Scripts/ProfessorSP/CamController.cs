using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CamController : MonoBehaviour
{

	[Header("Settings")]
	public float distanceAbove = 200;
	public float currentSpeed = 0.05f;

	[Header("References")]
	public Camera cam;
	public TerrainGeneration.TerrainHeightSettings heightSettings;

	[Header("Controls")]
	public bool canMove = true;

	Transform target;

	void Start()
	{
		InitView();
	}

	public void InitView()
	{
		target = cam.transform;
		target.position = new Vector3(0, 0, -1 * (heightSettings.worldRadius + distanceAbove));
	}


	void Update()
	{
		HandleInput();
		HandleMovement();
	}

	void HandleInput()
	{
		// TODO: Implement input handling
	}
	void HandleMovement()
	{
		if (!canMove)
		{
			return;
		}
		UpdatePosition(currentSpeed);
		UpdateRotation(currentSpeed);
	}


	void UpdatePosition(float forwardSpeed)
	{
		// transform.position += transform.forward * forwardSpeed * Time.deltaTime;
	}

	void UpdateRotation(float turnAmount)
	{
		transform.Rotate(Vector3.up, turnAmount);
	}


}
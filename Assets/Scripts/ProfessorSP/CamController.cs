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

	public float moveDuration = 2f;
	Transform target;
	MoveDoneCallback moveDoneCallback;
	string targetCountry;
	Vector3 targetPosition;
	Vector3 camPosition;

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
		// HandleInput();
		// HandleMovement();
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

	public delegate void MoveDoneCallback(string str);
// moveDoneCallback(countryName);
	public void MoveTo(string countryName, Vector3 center, MoveDoneCallback callback = null)
	{
		targetPosition = center.normalized * heightSettings.worldRadius;
		camPosition = center.normalized * (heightSettings.worldRadius + distanceAbove);
		Debug.Log("Moving to " + countryName + " at " + targetPosition + " and " + camPosition);
		// cam.transform.position = camPosition;
		// cam.transform.LookAt(targetPosition);
		targetCountry = countryName;
		moveDoneCallback = callback;
		StartCoroutine(MoveCamera());
	}

	IEnumerator MoveCamera()
	{
		float elapsedTime = 0;
		Vector3 startPos = cam.transform.position;
		Vector3 endPos = camPosition;
		Quaternion startRot = cam.transform.rotation;
		Quaternion endRot = Quaternion.LookRotation(Vector3.zero - targetPosition);
		while (elapsedTime < moveDuration)
		{
			cam.transform.position = Vector3.Lerp(startPos, endPos, (elapsedTime / moveDuration));
			cam.transform.rotation = Quaternion.Slerp(startRot, endRot, (elapsedTime / moveDuration));
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		moveDoneCallback(targetCountry);
	}

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeographicalAdventures.ProfessorSP;


public class CamController : MonoBehaviour
{

	[Header("Settings")]
	public float distanceAbove = 200;
	public float distanceAboveHigh = 150;
	public float distanceAboveMedium = 120;
	public float distanceAboveLow = 50;
	public float currentSpeed = 0.05f;

	[Header("References")]
	public Camera cam;
	public TerrainGeneration.TerrainHeightSettings heightSettings;

	[Header("Controls")]
	public bool canMove = true;

	public float moveDuration = 5f;
	Transform target;
	MoveDoneCallback moveDoneCallback;
	HlInfo targetCountry;
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
	}

	public void ResetView()
	{
		targetPosition = new Vector3(0, 0, -1).normalized * heightSettings.worldRadius;
		camPosition = new Vector3(0, 0, -1).normalized * (heightSettings.worldRadius + distanceAbove);
		StartCoroutine(MoveCamera());
	}

	public delegate void MoveDoneCallback(string str);
// moveDoneCallback(countryName);
	public void MoveTo(HlInfo country, MoveDoneCallback callback = null)
	{
		string countryName = country.Name;
		Vector3 center = country.Center;
		uint population = country.Population;
		targetPosition = center.normalized * heightSettings.worldRadius;
		camPosition = center.normalized * (heightSettings.worldRadius + GetDistanceByPopulation(population));
		Debug.Log("Moving to " + countryName + " at " + targetPosition + " and " + camPosition + " with population " + population);	
		// cam.transform.position = camPosition;
		// cam.transform.LookAt(targetPosition);
		targetCountry = country;
		StartCoroutine(MoveCamera(callback));
	}

	float GetDistanceByPopulation(uint population)
	{
		if (population > 200_000_000)
		{
			return distanceAbove;
		}
		else if (population > 100_000_000)
		{
			return distanceAboveHigh;
		}
		else if (population > 10_000_000)
		{
			return distanceAboveMedium;
		}
		else
		{
			return distanceAboveLow;
		}
	}

	IEnumerator MoveCamera(MoveDoneCallback callback = null)
	{
		float elapsedTime = 0;
		Vector3 startPos = cam.transform.position;
		Vector3 endPos = camPosition;
		Quaternion startRot = cam.transform.rotation;
		Quaternion endRot = Quaternion.LookRotation(Vector3.zero - targetPosition);
		while (elapsedTime < moveDuration)
		{
			cam.transform.position = Vector3.Slerp(startPos, endPos, (elapsedTime / moveDuration));
			cam.transform.rotation = Quaternion.Lerp(startRot, endRot, (elapsedTime / moveDuration));
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		if (callback != null)
		{
			callback(targetCountry.Name);
		}
	}

}
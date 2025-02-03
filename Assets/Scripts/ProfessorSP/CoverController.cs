using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MongoDB.Bson;
using TMPro;
using GeographicalAdventures.ProfessorSP;

public class CoverController : MonoBehaviour
{
	[Header("References")]
	public CamController camController;
	public CoverMapLoader mapLoader;
	public DetailsDisplay detailsDisplay;
	public TextMeshProUGUI pauseText;

	[Header("Settings")]
	public int displaySeconds = 20;

	GameObject oceanObject;
	string[] countryNames;
	Material[] countryMaterials;
	float[] countryHighlightStates;
	BsonDocument impactContent;
	List<HlInfo> highlightCountries;
	int countryIndex = -1;
	bool paused = false;

	static Color[] hightColors = { Color.red, Color.green, Color.yellow, Color.cyan, Color.magenta };
	static Color greyColor = Color.grey;

	static readonly Dictionary<string, string> countryNameMap = new Dictionary<string, string>() {
		{ "USA", "United States" },
		{ "US", "United States" },
	};

	void Awake()
	{
	}

	void Start()
	{
		if (mapLoader.hasLoaded)
		{
			for (int i = 0; i < hightColors.Length; i++)
			{
				hightColors[i].a = 0.7f;
			}
			greyColor.a = 0.7f;
			highlightCountries = new List<HlInfo>();
			oceanObject = mapLoader.oceanObject;
			oceanObject.SetActive(false);
			int numCountries = mapLoader.countryObjects.Length;
			countryNames = new string[numCountries];
			countryMaterials = new Material[numCountries];
			countryHighlightStates = new float[numCountries];
			for (int i = 0; i < numCountries; i++)
			{
				MeshRenderer renderer = mapLoader.countryObjects[i].renderer;
				countryMaterials[i] = renderer.sharedMaterial;
				countryNames[i] = renderer.gameObject.name;
				renderer.gameObject.SetActive(false);
			}
		}
		else
		{
			Debug.LogError("Map loader has not yet loaded map");
		}
	}


	void Update()
	{
		if (impactContent != null)
		{
			if (Input.GetMouseButtonDown(1))
			{
				paused = !paused;
				pauseText.gameObject.SetActive(paused);
			}
			else if (Input.GetKeyDown(KeyCode.Space))
			{
				paused = !paused;
				pauseText.gameObject.SetActive(paused);
			}
			if (paused)
			{
				return;
			}
			if (Time.time % displaySeconds < Time.deltaTime || countryIndex == -1)
			{
				countryIndex = (countryIndex + 1) % highlightCountries.Count;
				DisplayDetail(countryIndex);
			}
		}
	}

	void DisplayDetail(int index)
	{
		var country = highlightCountries[index];
		detailsDisplay.HideAll();
		camController.MoveTo(country, MoveDoneCallback);
	}

	public void MoveDoneCallback(string name)
	{
		detailsDisplay.ShowContent(name);
	}

	public void UpdateCover(ref BsonDocument content)
	{
		ClearHighlights();
		paused = false;
		pauseText.gameObject.SetActive(paused);
		if (content == null)
		{
			detailsDisplay.UpdateDetails(null);
			countryIndex = -1;
			impactContent = null;
			camController.ResetView();
			return;
		}
		ParseImpactContent(ref content);
		StartCoroutine(UpdateUI(content));
	}

	IEnumerator UpdateUI(BsonDocument content)
	{
		HighlightCountries();
		detailsDisplay.UpdateDetails(content);
		yield return null;
		impactContent = content;
	}

	void ParseImpactContent(ref BsonDocument content)
	{
		UpdateHighlightCountries(ref content);
	}

	void UpdateHighlightCountries(ref BsonDocument content)
	{
		highlightCountries.Clear();
		List<string> names = new List<string>();
		foreach (var element in content)
		{
			names.Add(element.Name);
		}
		foreach (var countryName in names)
		{
			// var newName = countryName;
			var newName = countryName.Replace('_', ' ');
			if (newName != countryName)
			{
				content.Add(newName, content.GetElement(countryName).Value);
			}
			if (countryNameMap.ContainsKey(newName))
			{
				content.Add(countryNameMap[newName], content.GetElement(newName).Value);
				newName = countryNameMap[countryName];
			}
			if (System.Array.IndexOf(countryNames, newName) == -1)
			{
				if (newName == "Global")
				{
					List<string> subNames = new List<string>(countryNames);
					highlightCountries.Add(new HlInfo(newName, subNames));
				}
				else
				{
					BsonElement countriesElement;
					bool tryResult = content.GetElement(newName).Value.AsBsonDocument.TryGetElement("Countries", out countriesElement);
					if (tryResult)
					{
						List<string> subNames = new List<string>();
						var countriesList = countriesElement.Value.AsBsonArray.ToList();
						countriesList.ForEach(country =>
						{
							var subName = country.AsString;
							if (System.Array.IndexOf(countryNames, subName) == -1)
							{
								Debug.LogWarning($"Country {subName} not found in loaded map countries");
							}
							else
							{
								subNames.Add(subName);
							}
						});
						if (subNames.Count > 0)
						{
							highlightCountries.Add(new HlInfo(newName, subNames));
						}
					}
				}
			}
			else
			{
				highlightCountries.Add(new HlInfo(newName));
			}
		}
	}

	void ClearHighlights()
	{
		countryIndex = -1;
		for (int i = 0; i < countryHighlightStates.Length; i++)
		{
			if (countryHighlightStates[i] == 1)
			{
				countryHighlightStates[i] = 0;
				countryMaterials[i].color = Color.clear;// mapLoader.countryColours[i].colour;
				mapLoader.countryObjects[i].gameObject.SetActive(false);
			}
		}
	}

	void HighlightCountries()
	{
		for (int i = 0; i < highlightCountries.Count; i++)
		{
			HlInfo info = highlightCountries[i];
			if (info.SubNames != null)
			{
				Vector3 center = Vector3.zero;
				uint population = 0;
				foreach (var subName in info.SubNames)
				{
					for (int j = 0; j < countryNames.Length; j++)
					{
						if (subName == countryNames[j])
						{
							countryHighlightStates[j] = 1;
							if (System.Array.Exists(hightColors, color => color == countryMaterials[j].color))
							{
								// Already highlighted
							}
							else if (info.Name == "Global")
							{
								countryMaterials[j].color = greyColor;
							}
							else
							{
								countryMaterials[j].color = hightColors[i % hightColors.Length];
							}
							var countryObject = mapLoader.countryObjects[j].gameObject;
							countryObject.SetActive(true);
							MeshFilter meshFilter = countryObject.GetComponent<MeshFilter>();
							if (meshFilter != null)
							{
								Mesh mesh = meshFilter.mesh;
								Bounds bounds = mesh.bounds;
								Vector3 boundsCenter = countryObject.transform.TransformPoint(bounds.center);
								if (center == Vector3.zero)
								{
									center = boundsCenter;
								}
								else
								{
									center += boundsCenter;
									center /= 2;
								}
								population += (uint)detailsDisplay.GetCountryPopulation(subName);
							}
						}
					}
				}
				highlightCountries[i] = new HlInfo(info.Name, info.SubNames, center, population);
			}
			else
			{
				for (int j = 0; j < countryNames.Length; j++)
				{
					if (info.Name == countryNames[j])
					{
						countryHighlightStates[j] = 1;
						countryMaterials[j].color = hightColors[i % hightColors.Length];
						var countryObject = mapLoader.countryObjects[j].gameObject;
						countryObject.SetActive(true);
						MeshFilter meshFilter = countryObject.GetComponent<MeshFilter>();
						if (meshFilter != null)
						{
							Mesh mesh = meshFilter.mesh;
							Bounds bounds = mesh.bounds;
							Vector3 boundsCenter = countryObject.transform.TransformPoint(bounds.center);
							highlightCountries[i] = new HlInfo(info.Name, null, boundsCenter, (uint)detailsDisplay.GetCountryPopulation(info.Name));
						}
					}
				}
			}
		}
		// countryIndex = 0;
	}

}
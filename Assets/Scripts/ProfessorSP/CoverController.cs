using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MongoDB.Bson;

public class CoverController : MonoBehaviour
{
	[Header("References")]
	public CamController camController;
	public CoverMapLoader mapLoader;
	public DetailsDisplay detailsDisplay;

	GameObject oceanObject;
	string[] countryNames;
	Material[] countryMaterials;
	float[] countryHighlightStates;
	PlayerAction playerActions;
	BsonDocument impactContent;
	List<HlCountryInfo> highlightCountries;
	int countryIndex = -1;
	struct HlCountryInfo
	{
		public string Name;
		public Vector3 Center;

		public HlCountryInfo(string name, Vector3? center = null)
		{
			Name = name;
			Center = center ?? Vector3.zero;
		}
	}

	static readonly Color[] hightColors = { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.cyan };

	static readonly Dictionary<string, string> countryNameMap = new Dictionary<string, string>() {
		{ "USA", "United States" },
		{ "US", "United States" },
	};

	void Awake()
	{
		playerActions = new PlayerAction();
	}

	void Start()
	{
		if (mapLoader.hasLoaded)
		{
			highlightCountries = new List<HlCountryInfo>();
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
			if (Time.time % 10 < Time.deltaTime && countryIndex != -1)
			{
				DisplayDetail(countryIndex);
				countryIndex = (countryIndex + 1) % highlightCountries.Count;
			}
		}
	}

	void DisplayDetail(int index)
	{
		var country = highlightCountries[index];
		detailsDisplay.HideAll();
		camController.MoveTo(country.Name, country.Center, MoveDoneCallback);
	}

	public void MoveDoneCallback(string name)
	{
		detailsDisplay.ShowContent(name);
	}

	public void Open()
	{
		playerActions.MapControls.Enable();
	}

	public void Close()
	{
		playerActions.MapControls.Disable();
		ClearHighlights();
	}

	public void UpdateCover(ref BsonDocument content)
	{
		ClearHighlights();
		if (content == null)
		{
			detailsDisplay.UpdateDetails(null);
			countryIndex = -1;
			impactContent = null;
			return;
		}
		ParseImpactContent(ref content);
		HighlightCountries();
		detailsDisplay.UpdateDetails(content);
		impactContent = content;
	}

	void ParseImpactContent(ref BsonDocument content)
	{
		var impacts = content.ToJson();
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
			Debug.Log($"Country: {countryName}");
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
				Debug.LogWarning($"Country {newName} not found in loaded map countries");
			}
			else
			{
				Debug.Log($"Country {newName} found in loaded map countries");
				highlightCountries.Add(new HlCountryInfo(newName));
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
				countryMaterials[i].color = mapLoader.countryColours[i].colour;
				mapLoader.countryObjects[i].gameObject.SetActive(false);
			}
		}
	}

	void HighlightCountries()
	{
		for (int i = 0; i < highlightCountries.Count; i++)
		{
			for (int j = 0; j < countryNames.Length; j++)
			{
				HlCountryInfo info = highlightCountries[i];
				if (info.Name == countryNames[j])
				{
					Debug.Log("Highlighting country: " + countryNames[j]);
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
						Debug.Log($"Highlighting country: {countryNames[j]} Bounds center: {boundsCenter}");
						highlightCountries[i] = new HlCountryInfo(info.Name, boundsCenter);
					}
				}
			}
		}
		countryIndex = 0;
	}

}
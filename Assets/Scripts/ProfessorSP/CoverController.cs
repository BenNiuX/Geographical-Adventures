using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MongoDB.Bson;

public class CoverController : MonoBehaviour
{
	public Camera cam;
	public float camDst = -55;

	public LayerMask globeMask;
	public float poleAngleLimit = 15;
	public float rotateSensitivity;
	public float zoomSensitivity = 4;
	public float fadeSpeed = 2;

	public Vector2 zoomMinMax;
	public float cursorHighlightRadius;

	[Header("References")]
	public Transform globe;
	public CoverMapLoader mapLoader;
	public UnityEngine.UI.CanvasScaler canvasScaler;
	public TerrainGeneration.TerrainHeightSettings heightSettings;

	// Private stuff
	float angleX;
	float angleY;

	GameObject lastHighlightedCountry;
	GameObject oceanObject;

	string[] countryNames;
	Material[] countryMaterials;
	float[] countryHighlightStates;

	Dictionary<GameObject, int> countryIndexLookup;

	bool overrideTextDisplay;
	string overridenText;
	bool isZoomed;
	float targetZoom;
	float smoothZoomV;

	PlayerAction playerActions;
	BsonDocument impactContent;
	List<string> highlightCountryNames;

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

			countryIndexLookup = new Dictionary<GameObject, int>();
			highlightCountryNames = new List<string>();
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
				countryIndexLookup.Add(renderer.gameObject, i);
				// mapLoader.countryObjects[i].gameObject.SetActive(false);
				renderer.gameObject.SetActive(false);
			}

			targetZoom = zoomMinMax.x;
			// cam.fieldOfView = targetZoom;
		}
		else
		{
			Debug.LogError("Map loader has not yet loaded map");
		}
	}


	void Update()
	{
	}

	public void Open()
	{
		playerActions.MapControls.Enable();
	}

	public void Close()
	{
		playerActions.MapControls.Disable();
		for (int i = 0; i < countryHighlightStates.Length; i++)
		{
			countryHighlightStates[i] = 0;
		}
	}

	public void UpdateCover(BsonDocument content)
	{
		impactContent = content;
		ClearHighlights();
		if (content == null)
		{
			return;
		}
		ParseImpactContent(content);
		HighlightCountries();
	}

	void ParseImpactContent(BsonDocument content)
	{
		var impacts = content.ToJson();
		UpdateHighlightCountryNames(content);
	}
	void UpdateHighlightCountryNames(BsonDocument content)
	{
		highlightCountryNames.Clear();
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
				highlightCountryNames.Add(newName);
			}
		}
	}

	// static void TraverseBsonDocument(BsonDocument bsonDocument)
	//     {
	//         foreach (var element in bsonDocument)
	//         {
	//             Console.WriteLine($"Key: {element.Name}, Value: {element.Value}");

	//             if (element.Value.BsonType == BsonType.Document)
	//             {
	//                 TraverseBsonDocument(element.Value.AsBsonDocument);
	//             }
	//         }
	//     }

	void ClearHighlights()
	{
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
		for (int i = 0; i < highlightCountryNames.Count; i++)
		{
			for (int j = 0; j < countryNames.Length; j++)
			{
				if (highlightCountryNames[i] == countryNames[j])
				{
					Debug.Log("Highlighting country: " + countryNames[j]);
					countryHighlightStates[j] = 1;
					countryMaterials[j].color = hightColors[i % hightColors.Length];
					var countryObject = mapLoader.countryObjects[j].gameObject;
					countryObject.SetActive(true);
				}
			}
		}
	}

}
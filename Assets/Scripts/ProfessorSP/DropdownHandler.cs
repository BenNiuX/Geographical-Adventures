using System.Collections.Generic;
using TMPro;
using UnityEngine;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using GeographicalAdventures.ProfessorSP;

public class DropdownHandler : MonoBehaviour
{
	[Header("References")]
	public TextMeshProUGUI menuNote;
	public CoverController coverController;
	[Header("Settings")]
	public int maxWidth = 1000;
	public int minWidth = 500;
	public float updateInterval = 60f;

	private const string MONGO_URI = "mongodb+srv://21678145:21678145@cluster0.zemxy3i.mongodb.net";
	private const string DATABASE_NAME = "ProfessorSP";
	private MongoClient client;
	private IMongoDatabase db;
	private List<ForecastData> forecastData;
	private int screenWidth;
	private int screenHeight;
	private float timer = 0f;
	private DateTime latestUpdate = DateTime.MinValue;

	private void Start()
	{
		var dropdown = transform.GetComponent<TMP_Dropdown>();
		dropdown.options.Clear();
		dropdown.onValueChanged.AddListener(delegate { DropdownItemSelected(dropdown); });
		client = new MongoClient(MONGO_URI);
		db = client.GetDatabase(DATABASE_NAME);
		forecastData = new List<ForecastData>();
		CheckDataUpdated();
	}

	void Update()
	{
		if (screenWidth != Screen.width || screenHeight != Screen.height)
		{
			screenWidth = Screen.width;
			screenHeight = Screen.height;
			UpdatePosSize();
		}
		timer += Time.deltaTime;
		if (timer >= updateInterval)
		{
			CheckDataUpdated();
			timer = 0f;
		}
	}

	void CheckDataUpdated()
	{
		var colForecast = db.GetCollection<ForecastData>("Forecast");
		var sortBuilder = Builders<ForecastData>.Sort;
		var sort = sortBuilder.Descending(f => f.updatedAt);
		var filterBuilder = Builders<ForecastData>.Filter;
		var filter = filterBuilder.Ne(f => f.settings.impactPrompt, null);
		var data = colForecast.Find(filter).Sort(sort).Limit(1).ToList();
		if (data[0].updatedAt > latestUpdate)
		{
			UpdateDropdownItems();
			latestUpdate = data[0].updatedAt;
		}
	}

	void UpdatePosSize()
	{
		var width = Mathf.Clamp(screenWidth / 3, minWidth, maxWidth);
		var dropdown = transform.GetComponent<TMP_Dropdown>();
		var rectTransform = dropdown.GetComponent<RectTransform>();
		var posX = -1 * screenWidth / 2 + width / 2 + 10;
		var posY = screenHeight / 2 - rectTransform.sizeDelta.y / 2 - 10;
		// rectTransform.position = new Vector3(posX, posY, 0);
		rectTransform.sizeDelta = new Vector2(width, rectTransform.sizeDelta.y);
		rectTransform.anchoredPosition = new Vector2(posX, posY);
		rectTransform = menuNote.GetComponent<RectTransform>();
		posX = posX + width / 2 + 80;
		rectTransform.anchoredPosition = new Vector2(posX, posY);
	}

	private void UpdateDropdownItems()
	{
		var dropdown = transform.GetComponent<TMP_Dropdown>();
		dropdown.options.Clear();
		forecastData.Clear();
		var colForecast = db.GetCollection<ForecastData>("Forecast");
		var sortBuilder = Builders<ForecastData>.Sort;
		var sort = sortBuilder.Descending(f => f.updatedAt);
		var filterBuilder = Builders<ForecastData>.Filter;
		var filter = filterBuilder.Ne(f => f.settings.impactPrompt, null);
		// filter = Builders<ForecastData>.Filter.Empty;
		var findOptions = new FindOptions<ForecastData> { BatchSize = 10 };
		var data = colForecast.Find(filter).Sort(sort).Limit(10).ToList();
		// var pipeline = new EmptyPipelineDefinition<ForecastData>().Sort(sort).Match(filter);
		// var items = colForecast.Aggregate(pipeline).ToList();
		int maxIndex = data.Count; // Get the total count of items
		dropdown.options.Add(new TMP_Dropdown.OptionData("--- Select an item ---"));
		for (int i = 0; i < data.Count; i++)
		{
			var item = data[i];
			dropdown.options.Add(new TMP_Dropdown.OptionData($"{i + 1}. {item.messages[0].content}"));
			forecastData.Add(item);
		}
		dropdown.value = 1;
		DropdownItemSelected(dropdown);
	}

	private void DropdownItemSelected(TMP_Dropdown dropdown)
	{
		int index = dropdown.value;
		if (index == 0)
		{
			BsonDocument a = null;
			coverController.UpdateCover(ref a);
			return;
		}
		// index == 0 is the guideline item in the dropdown list
		var item = forecastData[index - 1];
		var impactContent = GetImpactContent(GetImpactId(item));
		coverController.UpdateCover(ref impactContent);
	}

	private string GetImpactId(ForecastData item)
	{
		var id = GetImpactIdFromContent(item.messages[1].content);
		return id;
	}

	private string GetImpactIdFromContent(string content)
	{
		var start = content.IndexOf("<impacts>") + "<impacts>".Length;
		var end = content.IndexOf("</impacts>", start);
		var id = content.Substring(start, end - start);
		return id;
	}

	private BsonDocument GetImpactContent(string id)
	{
		var colImpact = db.GetCollection<ImpactData>("impacts");
		var filter = Builders<ImpactData>.Filter.Eq(f => f._id, id);
		var impact = colImpact.Find(filter).FirstOrDefault();
		return impact.content;
	}
}

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using MongoDB.Bson;
using System;
using GeographicalAdventures.ProfessorSP;
using UnityEngine.UI;

public class DropdownHandler : MonoBehaviour
{
	[Header("References")]
	public TextMeshProUGUI menuNote;
    public TextMeshProUGUI pauseText;
	public Button goWebButton;
    public CoverController coverController;
	[Header("Settings")]
	public int maxWidth = 2000;
	public int minWidth = 500;
	public float updateInterval = 30f;

	private DatabaseService dbService;
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
		dbService = new DatabaseService();
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
		dbService.GetLatestData(this, CallbackLatestData);
	}

	void CallbackLatestData(List<ForecastData> data)
	{
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
        rectTransform = pauseText.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0, posY);
		rectTransform = goWebButton.GetComponent<RectTransform>();
        Vector2 a = rectTransform.sizeDelta;
        rectTransform.anchoredPosition = new Vector2(screenWidth / 2 - a.x, posY);
    }

	private void UpdateDropdownItems()
	{
		dbService.GetForecastData(this, 10, CallbackData);
	}

	void CallbackData(List<ForecastData> data)
	{
		var dropdown = transform.GetComponent<TMP_Dropdown>();
		dropdown.options.Clear();
		forecastData.Clear();
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
		GetImpactContent(GetImpactId(item));
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

	private void GetImpactContent(string id)
	{
		dbService.GetImpactContent(this, id, CallbackImpactContent);
	}

	private void CallbackImpactContent(BsonDocument content)
	{
		coverController.UpdateCover(ref content);
	}

	public string GetForecastId()
	{
		var dropdown = transform.GetComponent<TMP_Dropdown>();
		int index = dropdown.value;
		if (index == 0 || forecastData == null || forecastData.Count == 0)
		{
			return null;
		}
		var item = forecastData[index - 1];
		return item._id ?? null;
	}
}

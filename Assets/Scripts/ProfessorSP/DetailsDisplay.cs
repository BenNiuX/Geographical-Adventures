using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MongoDB.Bson;
using TMPro;

public class DetailsDisplay : MonoBehaviour
{
	public Canvas canvas;
	List<TextMeshProUGUI> textList;
	List<Sprite> spriteList;
	List<Image> imageList;
	Dictionary<string, int> countryMap;
	BsonDocument impactContent;
	private int screenWidth;
	private int screenHeight;
	void Start()
	{
		textList = new List<TextMeshProUGUI>();
		spriteList = new List<Sprite>();
		imageList = new List<Image>();
		countryMap = new Dictionary<string, int>();
	}

	// Update is called once per frame
	void Update()
	{
		if (screenWidth != Screen.width || screenHeight != Screen.height)
		{
			screenWidth = Screen.width;
			screenHeight = Screen.height;
		}
	}

	public void UpdateDetails(BsonDocument content)
	{
		impactContent = content;
		ClearContent();
		if (content == null)
		{
			return;
		}
		GenerateContent();
	}

	void ClearContent()
	{
		foreach (TextMeshProUGUI text in textList)
		{
			Destroy(text.gameObject);
		}
		foreach (Sprite sprite in spriteList)
		{
			Destroy(sprite);
		}
		foreach (Image image in imageList)
		{
			Destroy(image.gameObject);
		}
		textList.Clear();
		spriteList.Clear();
		imageList.Clear();
		countryMap.Clear();
	}

	void GenerateContent()
	{
		if (impactContent == null)
		{
			return;
		}
		foreach (var element in impactContent)
		{
			var countryName = element.Name;
			var countryData = element.Value;
			foreach (var aspectElement in countryData.AsBsonDocument)
			{
				var aspectName = aspectElement.Name;
				var aspectValue = aspectElement.Value;

				if (!countryMap.ContainsKey(countryName))
				{
					countryMap[countryName] = 0;
				}
				countryMap[countryName] = countryMap[countryName] + 1;
				foreach (var dataElement in aspectValue.AsBsonDocument)
				{
					var dataName = dataElement.Name;
					var dataValue = dataElement.Value;
					if (dataName == "text")
					{
						GameObject textObj = new GameObject($"Text_{countryName}_{aspectName}");
						textObj.transform.SetParent(transform);
						TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
						text.text = $"{countryName}: {aspectName} - {dataValue}";
						RectTransform rectTransform = text.GetComponent<RectTransform>();
						rectTransform.sizeDelta = new Vector2(100, 100); // Set the width and height of the text zone
						text.fontSize = 25;
						text.fontStyle = FontStyles.Bold;
						textList.Add(text);
					}
					else if (dataName == "img")
					{
						var sprite = GenerateSprite(dataValue.ToString());
						spriteList.Add(sprite);
						GameObject imageObj = new GameObject($"Image_{countryName}_{aspectName}");
						imageObj.transform.SetParent(transform);
						Image image = imageObj.AddComponent<Image>();
						RectTransform rectTransform = image.GetComponent<RectTransform>();
						rectTransform.anchoredPosition = Vector2.zero;
						rectTransform.sizeDelta = new Vector2(100, 100);
						image.sprite = sprite;
						imageList.Add(image);
					}
				}
			}
		}
		HideAll();
	}

	public void HideAll()
	{
		foreach (TextMeshProUGUI text in textList)
		{
			text.gameObject.SetActive(false);
		}
		foreach (Image image in imageList)
		{
			image.gameObject.SetActive(false);
		}
	}

	public void ShowContent(string countryName)
	{
		HideAll();
		var num = countryMap[countryName];
		var gap = 10;
		var contentWidth = (screenWidth - 60) / 7;
		float x = -0.5f * num * (contentWidth + gap) + contentWidth / 2;
		float y = screenHeight / 2 - contentWidth;
		foreach (TextMeshProUGUI text in textList)
		{
			if (text.gameObject.name.Contains(countryName))
			{
				text.gameObject.SetActive(true);
				RectTransform rectTransform = text.GetComponent<RectTransform>();
				// rectTransform.position = new Vector3(x, y, 0);
				rectTransform.anchoredPosition = new Vector2(x, y);
				rectTransform.sizeDelta = new Vector2(contentWidth, contentWidth);
				x += (contentWidth + gap);
			}
		}
		y -= contentWidth;
		x = -0.5f * num * (contentWidth + gap) + contentWidth / 2;
		foreach (Image image in imageList)
		{
			if (image.gameObject.name.Contains(countryName))
			{
				image.gameObject.SetActive(true);
				RectTransform rectTransform = image.GetComponent<RectTransform>();
				// rectTransform.position = new Vector3(x, y, 0);
				rectTransform.anchoredPosition = new Vector2(x, y);
				rectTransform.sizeDelta = new Vector2(contentWidth, contentWidth);
				x += (contentWidth + gap);
			}
		}
	}

	Sprite GenerateSprite(string base64String)
	{
		byte[] imageBytes = Convert.FromBase64String(base64String);
		Texture2D tex = new Texture2D(2, 2);
		tex.LoadImage(imageBytes);
		Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
		return sprite;
	}
}

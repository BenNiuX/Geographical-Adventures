using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MongoDB.Bson;
using TMPro;

public class DetailsDisplay : MonoBehaviour
{
	[Header("References")]
	public Locations locations;
	public GameObject panelLeft;
	public GameObject panelRight;
	public GameObject panelCenter;
	List<TextMeshProUGUI> textList;
	List<Sprite> spriteList;
	List<Image> imageList;
	TextMeshProUGUI countryText;
	Sprite countrySprite;
	Image countryFlag;
	BsonDocument impactContent;
	private int screenWidth;
	private int screenHeight;
	private GameObject[] panels;
	private Location[] locationsArray;
	void Start()
	{
		textList = new List<TextMeshProUGUI>();
		spriteList = new List<Sprite>();
		imageList = new List<Image>();
		panels = new GameObject[] { panelLeft, panelRight };
		locationsArray = locations.CreateLocations();
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
		textList.ForEach(text => Destroy(text.gameObject));
		spriteList.ForEach(Destroy);
		imageList.ForEach(image => Destroy(image.gameObject));
		// Only hide theses objects
		// Destroy(countryText?.gameObject);
		// countryText = null;
		// Destroy(countryFlag?.gameObject);
		// countryFlag = null;
		textList.Clear();
		spriteList.Clear();
		imageList.Clear();
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
				if (aspectName == "Countries")
				{
					continue;
				}
				var aspectValue = aspectElement.Value;

				foreach (var dataElement in aspectValue.AsBsonDocument)
				{
					var dataName = dataElement.Name;
					var dataValue = dataElement.Value;
					if (dataName == "text")
					{
						GameObject textObj = new GameObject($"Text_{countryName}_{aspectName}");
						// textObj.transform.SetParent(transform);
						TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
						text.text = $"{aspectName} - {dataValue}";
						RectTransform rectTransform = text.GetComponent<RectTransform>();
						Debug.Log($"{rectTransform.sizeDelta}");
						// rectTransform.sizeDelta = new Vector2(100, 100); // Set the width and height of the text zone
						text.fontSize = 25;
						text.fontStyle = FontStyles.Bold;
						text.enableAutoSizing = true;
                        textList.Add(text);
					}
					else if (dataName == "img")
					{
						var sprite = GenerateSprite(dataValue.ToString());
						spriteList.Add(sprite);
						GameObject imageObj = new GameObject($"Image_{countryName}_{aspectName}");
						// imageObj.transform.SetParent(transform);
						Image image = imageObj.AddComponent<Image>();
						image.preserveAspect = true;
						RectTransform rectTransform = image.GetComponent<RectTransform>();
						Debug.Log($"Image: {sprite.texture.width}x{sprite.texture.height}");
						Debug.Log($"{rectTransform.sizeDelta}");
						// rectTransform.anchoredPosition = Vector2.zero;
						rectTransform.sizeDelta = new Vector2(sprite.texture.width, sprite.texture.height);
						image.sprite = sprite;
						imageList.Add(image);
					}
				}
			}
		}
		HideAll();
	}

	void updateObj(GameObject obj, bool active, Transform parent)
	{
		if (obj != null)
		{
			obj.SetActive(active);
			obj.transform.SetParent(parent);
		}
	}

	public void HideAll()
	{
		textList.ForEach(text => updateObj(text.gameObject, false, null));
		imageList.ForEach(image => updateObj(image.gameObject, false, null));
		updateObj(countryText?.gameObject, false, null);
		updateObj(countryFlag?.gameObject, false, null);
	}

	public void ShowContent(string countryName)
	{
		HideAll();
		int panelIndex = 0;
		foreach (TextMeshProUGUI text in textList)
		{
			if (text.gameObject.name.Contains(countryName))
			{
				updateObj(text.gameObject, true, panels[panelIndex].transform);
				var suffix = text.gameObject.name.Replace("Text_", "");
				foreach (Image image in imageList)
				{
					if (image.gameObject.name.Contains(suffix))
					{
						updateObj(image.gameObject, true, panels[panelIndex].transform);
					}
				}
				panelIndex = (panelIndex + 1) % panels.Length;
			}
        }
		if (textList.Count > 0)
		{
			if (countryText == null)
			{
				GameObject textObj = new GameObject("CountryName");
				TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
				countryText = text;
			}
			countryText.text = countryName;
			countryText.enableWordWrapping = false;
			countryText.alignment = TextAlignmentOptions.Center;
			countryText.enableAutoSizing = true;
			countryText.fontStyle = FontStyles.Bold;
			RectTransform rectTransform = countryText.GetComponent<RectTransform>();
			rectTransform.sizeDelta = new Vector2(500, 150);
			updateObj(countryText.gameObject, true, panelCenter.transform);
			foreach (var location in locationsArray)
			{
				if (CountyNameMatch(location.country, countryName))
				{
					if (countryFlag == null)
					{
						GameObject flagObj = new GameObject("CountryFlag");
						Image flagImage = flagObj.AddComponent<Image>();
						countryFlag = flagImage;
					}
					countryFlag.sprite = Texture2SpriteWithBorder(location.flag);
					countryFlag.preserveAspect = true;
					countrySprite = countryFlag.sprite;
					RectTransform rectT = countryFlag.GetComponent<RectTransform>();
					rectT.sizeDelta = new Vector2(150, 150);
					updateObj(countryFlag.gameObject, true, panelCenter.transform);
					break;
				}
			}
		}
	}

	bool CountyNameMatch(Country country, string countryName)
	{
		if (country != null)
		{
			return country.name == countryName
				 || country.name_long == countryName
				 || country.name_sort == countryName
				 || country.nameOfficial == countryName;
		}
		return false;
	}

	public int GetCountryPopulation(string countryName)
	{
		foreach (var location in locationsArray)
		{
			if (CountyNameMatch(location.country, countryName))
			{
				return location.country.population;
			}
		}
		return 0;
	}

	Sprite GenerateSprite(string base64String)
	{
		byte[] imageBytes = Convert.FromBase64String(base64String);
		Texture2D tex = new Texture2D(2, 2);
		tex.LoadImage(imageBytes);
		return Texture2Sprite(tex);
	}

	Sprite Texture2Sprite(Texture2D tex)
	{
		return Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
	}

	Sprite Texture2SpriteWithBorder(Texture2D tex, int borderSize = 5)
	{
		Texture2D borderTex = new Texture2D(tex.width + 2 * borderSize, tex.height + 2 * borderSize);
		for (int x = 0; x < borderTex.width; x++) {
			for (int y = 0; y < borderTex.height; y++) {
				if (x < borderSize || x > borderTex.width - borderSize || y < borderSize || y > borderTex.height - borderSize) {
					borderTex.SetPixel(x, y, Color.white);
				} else {
					borderTex.SetPixel(x, y, tex.GetPixel(x - borderSize, y - borderSize));
				}
			}
		}
		borderTex.Apply();
		Sprite sprite = Sprite.Create(borderTex, new Rect(0.0f, 0.0f, borderTex.width, borderTex.height), new Vector2(0.5f, 0.5f));
		return sprite;
	}
}

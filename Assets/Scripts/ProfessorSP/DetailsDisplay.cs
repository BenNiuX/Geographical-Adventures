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
	public GameObject impactPrefab;
	List<GameObject> prefabImpactList;
	List<Sprite> spriteList;
	TextMeshProUGUI countryText;
	Sprite countrySprite;
	Image countryFlag;
	BsonDocument impactContent;
	private int screenWidth;
	private int screenHeight;
	private GameObject[] panels;
	private Location[] locationsArray;
	private int paddingTopBottomBak = -1;
	private bool isCountryNameColorWhite = true;
	private bool isCountryNameCapitalized = true;
	private bool isCountryNameNewRoman = true;
	void Start()
	{
		spriteList = new List<Sprite>();
		prefabImpactList = new List<GameObject>();
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
		if (Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.LeftAlt))
		{
			if (Input.GetKeyDown(KeyCode.C))
			{
				isCountryNameCapitalized = !isCountryNameCapitalized;
			}
			if (Input.GetKeyDown(KeyCode.B))
			{
				isCountryNameColorWhite = !isCountryNameColorWhite;
			}
			if (Input.GetKeyDown(KeyCode.N))
			{
				isCountryNameNewRoman = !isCountryNameNewRoman;
			}
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
		spriteList.ForEach(Destroy);
		prefabImpactList.ForEach(Destroy);
		if (countrySprite != null)
		{
			Destroy(countrySprite);
			countrySprite = null;
		}
		if (countryText != null)
		{
			Destroy(countryText.gameObject);
			countryText = null;
		}
		if (countryFlag != null)
		{
			Destroy(countryFlag.gameObject);
			countryFlag = null;
		}
		spriteList.Clear();
		prefabImpactList.Clear();
		Resources.UnloadUnusedAssets();
	}

	void GenerateContent()
	{
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
				GameObject impactInstance = Instantiate(impactPrefab);
				impactInstance.name = $"{countryName}_{aspectName}";
				// impactInstance.transform.SetParent(transform);
				impactInstance.SetActive(false);
				prefabImpactList.Add(impactInstance);
				foreach (var dataElement in aspectValue.AsBsonDocument)
				{
					var dataName = dataElement.Name;
					var dataValue = dataElement.Value;
					if (dataName == "text")
					{
						TextMeshProUGUI[] prefabTexts = impactInstance.GetComponentsInChildren<TextMeshProUGUI>();
						prefabTexts[0].text = char.ToUpper(aspectName[0]) + aspectName.Substring(1);
						prefabTexts[1].text = dataValue.ToString();
					}
					else if (dataName == "img")
					{
						Image[] prefabImages = impactInstance.GetComponentsInChildren<Image>();
						Image prefabImage = prefabImages[prefabImages.Length - 1];
						var sprite = GenerateSprite(dataValue.ToString());
						spriteList.Add(sprite);
						prefabImage.preserveAspect = true;
						prefabImage.sprite = sprite;
					}
				}
			}
		}
		HideAll();
	}

	void UpdateObj(GameObject obj, bool active, Transform parent)
	{
		if (obj != null)
		{
			obj.SetActive(active);
			obj.transform.SetParent(parent);
		}
	}

	public void HideAll()
	{
		prefabImpactList.ForEach(impact => UpdateObj(impact, false, null));
		UpdateObj(countryText?.gameObject, false, null);
		UpdateObj(countryFlag?.gameObject, false, null);
	}

	public void ShowContent(string countryName)
	{
		HideAll();
		int panelIndex = 0;
		int[] panelCount = new int[panels.Length];
		foreach (GameObject impactPrefab in prefabImpactList)
		{
			if (impactPrefab.name.StartsWith(countryName + "_"))
			{
				UpdateObj(impactPrefab, true, panels[panelIndex].transform);
				panelCount[panelIndex]++;
				panelIndex = (panelIndex + 1) % panels.Length;
			}
		}
		for (int i = 0; i < panels.Length; i++)
		{
			RectOffset oldPadding = panels[i].GetComponent<VerticalLayoutGroup>().padding;
			if (paddingTopBottomBak > 0)
			{
				oldPadding.top = oldPadding.bottom = paddingTopBottomBak;
				panels[i].GetComponent<VerticalLayoutGroup>().padding = oldPadding;
			}
			if (panelCount[i] == 1)
			{
                paddingTopBottomBak = oldPadding.top;
				oldPadding.top = oldPadding.bottom = (screenHeight - paddingTopBottomBak - paddingTopBottomBak) / 4 + paddingTopBottomBak;
				panels[i].GetComponent<VerticalLayoutGroup>().padding = oldPadding;
			}
		}
		if (prefabImpactList.Count > 0)
		{
			if (countryText == null)
			{
				GameObject textObj = new GameObject("CountryName");
				TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
				countryText = text;
			}
			countryText.text = isCountryNameCapitalized ? countryName.ToUpper() : countryName;
			countryText.enableWordWrapping = false;
			countryText.alignment = TextAlignmentOptions.Center;
			countryText.enableAutoSizing = true;
			countryText.fontSizeMin = 30;
			countryText.fontSizeMax = 200;
			countryText.fontStyle = FontStyles.Bold;
			countryText.color = isCountryNameColorWhite ? Color.white : Color.black;
			countryText.font = Resources.Load<TMP_FontAsset>(
				isCountryNameNewRoman ?
				"Fonts & Materials/Times New Roman SDF"
				: "Fonts & Materials/LiberationSans SDF");
			countryText.enableWordWrapping = true;
			RectTransform rectTransform = countryText.GetComponent<RectTransform>();
			rectTransform.sizeDelta = new Vector2(screenWidth / 6, screenHeight / 6);
			UpdateObj(countryText.gameObject, true, panelCenter.transform);
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
					rectT.sizeDelta = new Vector2(screenHeight / 6, screenHeight / 6);
					UpdateObj(countryFlag.gameObject, true, panelCenter.transform);
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
		for (int x = 0; x < borderTex.width; x++)
		{
			for (int y = 0; y < borderTex.height; y++)
			{
				if (x < borderSize || x >= borderTex.width - borderSize || y < borderSize || y >= borderTex.height - borderSize)
				{
					borderTex.SetPixel(x, y, Color.black);
				}
				else
				{
					borderTex.SetPixel(x, y, tex.GetPixel(x - borderSize, y - borderSize));
				}
			}
		}

		borderTex.Apply();
		Sprite sprite = Sprite.Create(borderTex, new Rect(0.0f, 0.0f, borderTex.width, borderTex.height), new Vector2(0.5f, 0.5f));
		return sprite;
	}
}

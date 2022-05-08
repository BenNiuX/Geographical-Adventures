using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringLocalizer : MonoBehaviour
{
	public string id;
	public TMPro.TMP_Text textElement;

	public string currentValue { get; private set; }
	public bool controlRectTransformWidth;
	public float padding = 50;

	void Start()
	{
		Localize();
		LocalizeManager.onLanguageChanged += Localize;
	}

	void Localize()
	{
		currentValue = LocalizeManager.Localize(id);
		textElement.text = currentValue;

		if (controlRectTransformWidth)
		{
			textElement.ForceMeshUpdate();
			RectTransform rectTransform = GetComponent<RectTransform>();
			if (rectTransform != null)
			{
				GetComponent<RectTransform>().sizeDelta = new Vector2(textElement.bounds.size.x + padding, rectTransform.sizeDelta.y);
			}
		}
	}

#if UNITY_EDITOR
	void OnValidate()
	{
		if (textElement == null)
		{
			textElement = GetComponent<TMPro.TMP_Text>();
		}
	}
#endif
}

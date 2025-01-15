using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
    // Start is called before the first frame update
    public DropdownHandler dropdownHandler;
    void Start()
    {
        Button btn = gameObject.GetComponent<Button>();
        btn.onClick.AddListener(delegate() { OpenWebOnClick(); });
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OpenWebOnClick()
    {
        string curId = dropdownHandler.GetForecastId();
        string query = "";
        if (curId != null)
        {
            query = "?id=" + curId;
        }
        Application.OpenURL("https://superhuman-fg-b4eeaadpajgaf0e2.australiacentral-01.azurewebsites.net/" + query);
    }
}

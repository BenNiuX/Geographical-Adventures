using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
    // Start is called before the first frame update
    public DropdownHandler dropdownHandler;

    [DllImport("user32")]
    private static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int Width, int Height, uint flags);

    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_SHOWWINDOW = 0x0040;

    [DllImport("user32")]
    private static extern long GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32")]
    private static extern long SetWindowLong(IntPtr hWnd, int nIndex, long dwNewLong);

    private const int GWL_STYLE = -16;
    private const int WS_CAPTION = 0x00C00000;

    private static long GetWindowHandle()
    {
        long windowHandle = GetWindowLong(new IntPtr(GetWindowLong(IntPtr.Zero, GWL_STYLE)), GWL_STYLE);
        SetWindowLong(new IntPtr(GetWindowLong(IntPtr.Zero, GWL_STYLE)), GWL_STYLE, windowHandle & ~WS_CAPTION);
        return windowHandle;
    }

    void Start()
    {
        Button btn = gameObject.GetComponent<Button>();
        btn.onClick.AddListener(delegate() { OpenWebOnClick(); });
        #if UNITY_STANDALONE_WIN
        StartCoroutine(ReorgniseWindow());
        #endif
    }

    IEnumerator ReorgniseWindow()
    {
        yield return null;
        yield return new WaitForSeconds(10f);
        System.Diagnostics.Process[] processList = System.Diagnostics.Process.GetProcesses();
        var resolutionMax = Screen.resolutions[Screen.resolutions.Length - 1];

        foreach (System.Diagnostics.Process proc in processList)
        {
            long windowState = GetWindowLong(proc.MainWindowHandle, GWL_STYLE);
            // if (windowState != 0)
            {//2880x1800 2160 2138x1789
                if (proc.ProcessName.Contains("ProfessorSP"))
                {
                    Debug.Log("Found ProfessorSP");
                    SetWindowPos(proc.MainWindowHandle, IntPtr.Zero,
                        resolutionMax.width - (int)(resolutionMax.width * 0.75f) - 7, 0,
                        (int)(resolutionMax.width * 0.75f) + 22, resolutionMax.height + 1,
                        SWP_NOZORDER | SWP_SHOWWINDOW);
                }
                else if (proc.ProcessName.Contains("chrome") || proc.ProcessName.Contains("firefox"))
                {
                    Debug.Log("Found browser");
                    SetWindowPos(proc.MainWindowHandle, IntPtr.Zero,
                        0 - 7, 0,
                        (int)(resolutionMax.width * 0.25f) + 7, resolutionMax.height + 1,
                        SWP_NOZORDER | SWP_SHOWWINDOW);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OpenWebOnClick()
    {
        string curId = dropdownHandler.GetForecastId();
        string query = "";
        string url = "http://170.64.255.98:3000/";
        string customUrl = Environment.GetEnvironmentVariable("ProfessorSP_Web");
        if (customUrl != null)
        {
            url = customUrl;
        }
        if (curId != null)
        {
            query = "?id=" + curId;
        }
        Application.OpenURL(url + query);
    }
}
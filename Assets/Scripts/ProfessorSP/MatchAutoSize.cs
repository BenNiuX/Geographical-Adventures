using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MatchAutoSize : MonoBehaviour
{
    public TMP_Text a;
    public TMP_Text b;

    public string atext;
    public string btext;

    private void Start()
    {
        StartCoroutine(updateafteroneframe());
    }

    private IEnumerator updateafteroneframe()
    {

        a.text = atext;
        b.text = btext;

        yield return null; // wait one frame

        float size = Mathf.Min(a.fontSize, b.fontSize);
        a.enableAutoSizing = false;
        b.enableAutoSizing = false;
        a.fontSize = size;
        b.fontSize = size;
    }

    // Update is called once per frame
    void Update()
    {
       

        
    }
}

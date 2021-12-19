using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIResponse : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var trans = GetComponent<RectTransform>();
        var child = trans.Find("Test");
        var btn = child.GetComponent<Button>();
        btn.onClick.AddListener(Test);
    }

    void Test()
	{
        Debug.Log("Buttoned.");
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}

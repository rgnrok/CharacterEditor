using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FpsCounter : MonoBehaviour
{
    private Text text;
    private float deltaTime;

    void Start()
    {
        text = GetComponent<Text>();
        StartCoroutine(CalFPS());
        deltaTime = 0;
    }
	
	// Update is called once per frame
    void Update()
    {
        deltaTime += Time.deltaTime;
    }

    IEnumerator CalFPS()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(0.5f);
            text.text = (1 / deltaTime * 30).ToString();
            deltaTime = 0;
        }

    }
}

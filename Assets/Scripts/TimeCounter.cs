using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TimeCounter : MonoBehaviour
{
    public Text timeUI;
    // public Text timeRecordUI;

    float startTime;
    public float ellapsedTime;
    public float recordTime;
    bool startCounter;

    int minute;
    int second;
    // Start is called before the first frame update
    void Start()
    {
        timeUI = GetComponent<Text>();
        var record = PlayerPrefs.GetFloat("Time Record");
        Debug.Log(record);
        if (record == 0 || record == Mathf.Infinity)
        {
            // timeRecordUI.text = "Unknown";
        } else
        {
            var minute = (int)record / 60;
            var second = (int)record % 60;
            // timeRecordUI.text = string.Format("{0:00}:{1:00}", minute, second);
        }
        startCounter = true;
    }

    public void StartTimeCounter()
    {
        startTime = Time.time;
        startCounter = true;
    }

    public void StopTimeCounter()
    {
        startCounter = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (startCounter)
        {
            ellapsedTime = Time.time - startTime;
            minute = (int)ellapsedTime/60;
            second = (int)ellapsedTime%60;

            timeUI.text = GetEllaspedTime();
        }
    }
    public void UpdateRecordUI()
    {
        // timeRecordUI.text = GetEllaspedTime();
    }
    public string GetEllaspedTime()
    {
        return string.Format("{0:00}:{1:00}", minute, second);
    }
}

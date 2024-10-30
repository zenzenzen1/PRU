using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndSceneManager : MonoBehaviour
{
    // Start is called before the first frame update
    public TextMeshProUGUI finalScoreText;
    public string finalScore;
    public float time;
    void Start()
    {
        finalScore = PlayerPrefs.GetInt(Setting.score).ToString();
        time = PlayerPrefs.GetFloat(Setting.time);
        Debug.Log("Final Score: " + PlayerPrefs.GetInt(Setting.score) + " Time: " + PlayerPrefs.GetFloat(Setting.time));
        finalScoreText.text = Setting.developer + "\n" + "Final Score: " + finalScore + "\n" + "Time: " + GetEllaspedTime(time);
    }
    public string GetEllaspedTime(float ellapsedTime)
    {
        int minute = (int)ellapsedTime/60;
        int second = (int)ellapsedTime%60;
        return string.Format("{0:00}:{1:00}", minute, second);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DisplayScoreText : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    // Start is called before the first frame update
    void Awake()
    {
        scoreText.fontSize = 14;
    }

    // Update is called once per frame
    void Update()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        sceneName = sceneName == "End" ? "" : $"Scene{sceneName.Substring(sceneName.Length - 2, 2)}\n";
        scoreText.text = $"{sceneName}{Setting.groupName}\n" + "Score: " + PlayerPrefs.GetInt("score", 0);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameScore : MonoBehaviour
{
    public Text scoreTextUI;
    // public Text highScoreText;
    int score = 0;
    int hiScoreCount = 0;
    public int Score
    {
        get
        {
            return this.score;
        }
        set
        {
            this.score = value;
            UpdateScoreTextUI(score);
        }
    }
    public int HiScore
    {
        get
        {
            return this.hiScoreCount;
        }
        set
        {
            this.hiScoreCount = value;
            UpdateScoreTextUI(score);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //Get the TextUI component of this game object
        scoreTextUI = GetComponent<Text>();
    }
    void Update()
    {

    }
    //Function to update the score text UI 
    void UpdateScoreTextUI(int scores)
    {

        // string hiScoreStr = string.Format("{0:000000}", hiScoreCount);
        // highScoreText.text = hiScoreStr.ToString();

        string scoreStr = string.Format("{0:000000}", scores);
        scoreTextUI.text = scoreStr.ToString();

    }

}

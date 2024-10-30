using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public GameObject playButton;
    public GameObject quitButton;
    public GameObject player;
    public GameObject GameOverGO;
    public GameObject scoreUITextGO;
    // public GameObject hiScoreUITextGO;
    public GameObject TimeCounterGO;
    // public GameObject TimeRecordGO;
    public GameObject FinalScoreGO;
    public GameObject FinalTimeGO;
    public GameObject FinalTimeText;
    public GameObject FinalScoreText;
    public GameObject VictoryGO;
    public enum GameManagerState
    {
        GamePlay,
        GameOver,
        Victory,
        Opening,
    }
    GameManagerState state;
    // Start is called before the first frame update
    void Start()
    {
        //state = GameManagerState.Opening;
        StartGameplay();
    }

    // Update is called once per frame
    void UpdateGameManagerState()
    {
        switch (state)
        {
            case GameManagerState.GamePlay:
                //hide the play button
                playButton.SetActive(false);

                //hide the quit button
                quitButton.SetActive(false);

                //hide gameover
                GameOverGO.SetActive(false);
                VictoryGO.SetActive(false);
                FinalScoreText.SetActive(false);
                FinalTimeText.SetActive(false);

                //Set player firerate
                //player.GetComponent<PlayerControl>().FireRate = 2f;

                //Reset the score 
                scoreUITextGO.GetComponent<GameScore>().Score = 0;


                //Get hight score
                scoreUITextGO.GetComponent<GameScore>().HiScore =  PlayerPrefs.GetInt("High Score");


                //set the player visible (active) and init the player lives
                //player.GetComponent<ShipMovement>().Init();

                //start the time counter 
                TimeCounterGO.GetComponent<TimeCounter>().StartTimeCounter();

                break;
            case GameManagerState.GameOver:


                //Stop the time counter
                TimeCounterGO.GetComponent<TimeCounter>().StopTimeCounter();

                //Display game over 
                GameOverGO.SetActive(true);

                PlayerPrefs.SetFloat(Setting.time, TimeCounterGO.GetComponent<TimeCounter>().ellapsedTime);
                PlayerPrefs.SetInt(Setting.score, scoreUITextGO.GetComponent<GameScore>().Score);
                //Change game manager state 
                SceneManager.LoadScene(Setting.endScene, LoadSceneMode.Single);
                // Invoke("ChangeToOpeningState", 4f);


                break;
            case GameManagerState.Victory:
                if (scoreUITextGO.GetComponent<GameScore>().HiScore < scoreUITextGO.GetComponent<GameScore>().Score)
                {
                    scoreUITextGO.GetComponent<GameScore>().HiScore = scoreUITextGO.GetComponent<GameScore>().Score;
                }

                PlayerPrefs.SetInt("High Score", scoreUITextGO.GetComponent<GameScore>().HiScore);
                var time = PlayerPrefs.GetFloat("Time Record");
                if (time == 0f) time = Mathf.Infinity;
                if (TimeCounterGO.GetComponent<TimeCounter>().ellapsedTime < time)
                {
                    Debug.Log("Set new record");
                    TimeCounterGO.GetComponent<TimeCounter>().UpdateRecordUI();
                    PlayerPrefs.SetFloat("Time Record", TimeCounterGO.GetComponent<TimeCounter>().ellapsedTime);
                }

                //Stop the time counter
                TimeCounterGO.GetComponent<TimeCounter>().StopTimeCounter();

                //Display game over 

                VictoryGO.SetActive(true);
                FinalScoreText.SetActive(true);
                FinalTimeText.SetActive(true);

                FinalTimeGO.GetComponent<Text>().text = TimeCounterGO.GetComponent<TimeCounter>().GetEllaspedTime();
                // PlayerPrefs.SetString(Setting.time, TimeCounterGO.GetComponent<TimeCounter>().GetEllaspedTime());
                FinalScoreGO.GetComponent<Text>().text = scoreUITextGO.GetComponent<GameScore>().Score + "";

                //Change game manager state 
                // Invoke("ChangeToOpeningState", 4f);
                PlayerPrefs.SetFloat(Setting.time, TimeCounterGO.GetComponent<TimeCounter>().ellapsedTime);
                PlayerPrefs.SetInt(Setting.score, scoreUITextGO.GetComponent<GameScore>().Score);
                SceneManager.LoadScene(Setting.endScene, LoadSceneMode.Single);
                break;
            case GameManagerState.Opening:


                scoreUITextGO.GetComponent<GameScore>().HiScore =  PlayerPrefs.GetInt("High Score");

                scoreUITextGO.GetComponent<GameScore>().Score = 0;

                //Hide game over 
                GameOverGO.SetActive(false);

                //set play button visible (active)
                playButton.SetActive(true);

                //set exit button
                quitButton.SetActive(true);
                break;
        }
    }
    //Function to set the game manager state 
    public void SetGameManagerState(GameManagerState states)
    {
        state = states;
        UpdateGameManagerState();
    }
    public void StartGameplay()
    {
        state = GameManagerState.GamePlay;
        UpdateGameManagerState();
    }
    public void ChangeToOpeningState()
    {
        SetGameManagerState(GameManagerState.Opening);
    }
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }
    public void ExitGame()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        UnityEngine.Debug.Log("Quit success");
    }
}

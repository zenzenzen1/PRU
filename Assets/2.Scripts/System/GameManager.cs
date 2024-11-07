using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    public delegate void PlayerDieEventHandler();
    public event PlayerDieEventHandler PlayerDieEvent = null;

    public enum GameState
    {
        Title,
        Play,
        MenuOpen
    }
    public GameState currentGameState = GameState.Play;

    [HideInInspector] public bool firstStart = true;
    [HideInInspector] public Vector2 playerStartPos;
    [HideInInspector] public float playerStartlocalScaleX;
    
    [HideInInspector] public int playerCurrentHealth;
    [HideInInspector] public int playerCurrentDrivingForce;

    [HideInInspector] public string resurrectionScene;
    [HideInInspector] public Vector2 playerResurrectionPos;

    int gameDataNum = 1;

    int _prevPlayTime;
    float _startTime;

    bool _isStarted = true;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        Cursor.visible = false;

        EnemyDataManager.Init();
        SceneManager.sceneLoaded += OnSceneLoaded;

        OptionsData.Init();

#if UNITY_EDITOR
        if (currentGameState == GameState.Play)
        {
            GameLoad(1, false);
        }
#endif
    }

    public bool IsStarted()
    {
        if (_isStarted)
        {
            _startTime = (float)DateTime.Now.TimeOfDay.TotalSeconds;
            _isStarted = false;
            return true;
        }
        return false;
    }

    public void SetGameState(GameState newGameState)
    {
        switch(newGameState)
        {
            case GameState.Play:
                ScreenEffect.instance.TimeStopCancle();
                break;
            case GameState.MenuOpen:
                ScreenEffect.instance.TimeStopStart();
                break;
            case GameState.Title:
                break;
        }

        currentGameState = newGameState;
    }

    public string IntToTimeText(int time)
    {
        int sec = (time % 60);
        int min = ((time / 60) % 60);
        int hour = (time / 3600);

        string secToStr = sec < 10 ? "0" + sec.ToString() : sec.ToString();
        string minToStr = min < 10 ? "0" + min.ToString() : min.ToString();
        string hourToStr = hour.ToString();

        return string.Format("{0}:{1}:{2}", hourToStr, minToStr, secToStr);
    }

    public void HandlePlayerDeath()
    {
        playerStartPos = playerResurrectionPos;
        DeadEnemyManager.ClearDeadEnemies();
        PlayerDieEvent?.Invoke();
    }

    public void GameSave()
    {
        string sceneName = resurrectionScene;
        List<string> seenTutorials = TutorialManager.GetSeenTutorialsToString();
        List<string> deadBosses = DeadEnemyManager.GetDeadBosses();
        List<Vector2> foundMaps = MapManager.GetDiscoveredMaps();
        int playTime = _prevPlayTime + Mathf.CeilToInt((float)DateTime.Now.TimeOfDay.TotalSeconds - _startTime);

        var saveData = new GameSaveData
        (
            playTime,
            sceneName,
            playerResurrectionPos,
            PlayerLearnedSkills.hasLearnedClimbingWall,
            PlayerLearnedSkills.hasLearnedDoubleJump,
            foundMaps,
            deadBosses,
            seenTutorials
        );
        SaveSystem.GameSave(saveData, gameDataNum);
    }

    public void GameLoad(int gameDataNum, bool sceneLoaded = true)
    {
        var saveData = SaveSystem.GameLoad(gameDataNum);
        this.gameDataNum = gameDataNum;

        if(saveData == null)
        {
            SceneTransition.instance.LoadScene("OldMachineRoom_A");
            return;
        }
        _prevPlayTime = saveData.playTime;

        resurrectionScene = saveData.sceneName;
        playerResurrectionPos = saveData.playerPos;
        
        MapManager.AddDiscoveredMaps(saveData.foundMaps);
        PlayerLearnedSkills.hasLearnedClimbingWall = saveData.hasLearnedClimbingWall;
        PlayerLearnedSkills.hasLearnedDoubleJump = saveData.hasLearnedDoubleJump;

        for(int i = 0; i < saveData.deadBosses.Count; i++)
        {
            DeadEnemyManager.AddDeadBoss(saveData.deadBosses[i]);
        }

        for (int i = 0; i < saveData.seenTutorials.Count; i++)
        {
            TutorialManager.AddSeenTutorial(saveData.seenTutorials[i]);
        }

        firstStart = false;

        if (sceneLoaded)
        {
            playerStartPos = saveData.playerPos;
            SceneTransition.instance.LoadScene(saveData.sceneName);
        }
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(currentGameState == GameState.Title)
        {
            gameDataNum = 1;

            firstStart = true;

            playerStartPos = Vector2.zero;
            playerStartlocalScaleX = 0;

            playerCurrentHealth = 0;
            playerCurrentDrivingForce = 0;

            resurrectionScene = string.Empty;
            playerResurrectionPos = Vector2.zero;

            _prevPlayTime = 0;
            _startTime = 0;

            _isStarted = true;
        }
        PlayerDieEvent = null;
    }

    void OnApplicationQuit()
    {
        if (instance.currentGameState != GameState.Title)
        {
            instance.GameSave();
        }
        OptionsData.OptionsSave();
        PlayerPrefs.SetInt(Setting.score, 0);
    }
}

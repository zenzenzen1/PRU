using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

[System.Serializable]
public class OptionsSaveData
{
    public bool fullScreenMode = true;
    public Resolution? resolution = null;
    public bool vSync = true;

    public List<int> keyMapping = new List<int>();
    public List<int> buttonMapping = new List<int>();

    public float masterVolume = 1.0f;
    public float musicVolume = 1.0f;
    public float effectsVolume = 1.0f;

    public float gamepadVibration = 1.0f;
    public bool screenShake = true;
    public bool screenFlashes = true;

    public int language = (int)LanguageManager.Language.Last;
}

[System.Serializable]
public class GameSaveData
{
    public int playTime;
    public string sceneName;
    public Vector2 playerPos;
    public bool hasLearnedClimbingWall;
    public bool hasLearnedDoubleJump;
    public List<Vector2> foundMaps = new List<Vector2>();
    public List<string> deadBosses = new List<string>();
    public List<string> seenTutorials = new List<string>();

    public GameSaveData(int playTime,
                        string sceneName,
                        Vector2 playerPos,
                        bool hasLearnedClimbingWall,
                        bool hasLearnedDoubleJump,
                        List<Vector2> foundMaps,
                        List<string> deadBosses,
                        List<string> seenTutorials)
    {
        this.playTime = playTime;
        this.sceneName = sceneName;
        this.playerPos = playerPos;
        this.hasLearnedClimbingWall = hasLearnedClimbingWall;
        this.hasLearnedDoubleJump = hasLearnedDoubleJump;
        this.foundMaps = foundMaps;
        this.deadBosses = deadBosses;
        this.seenTutorials = seenTutorials;
    }
}

public static class SaveSystem
{
    private static readonly string privateKey = "EKDe$BMqvxvVf6z^ovKrhuf%JIJUg01CgCnadXYcOeGT%Iu5kS0xrj^09%^N";

    public static void OptionSave(OptionsSaveData optionsSaveData)
    {
        string jsonString = JsonUtility.ToJson(optionsSaveData);
        string encryptString = Encrypt(jsonString);

        using (FileStream fs = new FileStream(OptionsGetPath(), FileMode.Create, FileAccess.Write))
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(encryptString);
            fs.Write(bytes, 0, bytes.Length);
        }
#if UNITY_EDITOR
        Debug.Log("Save Success: " + OptionsGetPath());
#endif
    }

    public static OptionsSaveData OptionLoad()
    {
        if (!File.Exists(OptionsGetPath()))
        {
#if UNITY_EDITOR
            Debug.Log("저장 된 파일이 존재하지 않습니다.");
#endif
            return null;
        }

        string encryptData;
        using (FileStream fs = new FileStream(OptionsGetPath(), FileMode.Open, FileAccess.Read))
        {
            byte[] bytes = new byte[(int)fs.Length];
            fs.Read(bytes, 0, (int)fs.Length);
            encryptData = System.Text.Encoding.UTF8.GetString(bytes);
        }

        string decryptData = Decrypt(encryptData);
        OptionsSaveData saveData = JsonUtility.FromJson<OptionsSaveData>(decryptData);
        return saveData;
    }

    public static void GameSave(GameSaveData saveData, int dataNum)
    {
        string jsonString = JsonUtility.ToJson(saveData);
        string encryptString = Encrypt(jsonString);

        using (FileStream fs = new FileStream(GetPath(dataNum), FileMode.Create, FileAccess.Write))
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(encryptString);
            fs.Write(bytes, 0, bytes.Length);
        }

#if UNITY_EDITOR
		    Debug.Log("저장 성공: " + GetPath(dataNum));
#endif
	}

	public static GameSaveData GameLoad(int dataNum)
	{
        if (!File.Exists(GetPath(dataNum)))
        {
#if UNITY_EDITOR
            Debug.Log(dataNum + "번 저장 파일이 존재하지 않음.");
#endif
            return null;
        }

        string encryptData;
        using (FileStream fs = new FileStream(GetPath(dataNum), FileMode.Open, FileAccess.Read))
        {
            byte[] bytes = new byte[(int)fs.Length];
            fs.Read(bytes, 0, (int)fs.Length);
            encryptData = System.Text.Encoding.UTF8.GetString(bytes);
        }

        string decryptData = Decrypt(encryptData);
#if UNITY_EDITOR
        Debug.Log(decryptData);
#endif
        GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(decryptData);
        return saveData;
    }

    public static void GameDelete(int dataNum)
    {
        if (!File.Exists(GetPath(dataNum)))
        {
#if UNITY_EDITOR
            Debug.Log("해당 저장 파일은 존재하지 않습니다.");
#endif
            return;
        }
        File.Delete(GetPath(dataNum));
    }

    static string GetPath(int dataNum) => Path.Combine(Application.persistentDataPath + @"/save0" + dataNum + ".save");

    static string OptionsGetPath() => Path.Combine(Application.persistentDataPath + @"/options.save");

    static string Encrypt(string data)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
        RijndaelManaged rm = CreateRijndaelManaged();
        ICryptoTransform ct = rm.CreateEncryptor();
        byte[] results = ct.TransformFinalBlock(bytes, 0, bytes.Length);
        return System.Convert.ToBase64String(results, 0, results.Length);

    }

    static string Decrypt(string data)
    {
        byte[] bytes = System.Convert.FromBase64String(data);
        RijndaelManaged rm = CreateRijndaelManaged();
        ICryptoTransform ct = rm.CreateDecryptor();
        byte[] resultArray = ct.TransformFinalBlock(bytes, 0, bytes.Length);
        return System.Text.Encoding.UTF8.GetString(resultArray);
    }

    static RijndaelManaged CreateRijndaelManaged()
    {
        byte[] keyArray = System.Text.Encoding.UTF8.GetBytes(privateKey);
        RijndaelManaged result = new RijndaelManaged();

        byte[] newKeysArray = new byte[16];
        System.Array.Copy(keyArray, 0, newKeysArray, 0, 16);

        result.Key = newKeysArray;
        result.Mode = CipherMode.ECB;
        result.Padding = PaddingMode.PKCS7;
        return result;
    }
}

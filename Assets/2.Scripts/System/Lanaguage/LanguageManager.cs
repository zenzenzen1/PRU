using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;


public static class LanguageManager
{
    
    public enum Language
    {
        English,
        Korean,  
        Last     
    }
    public static Language currentLanguage;

    static Dictionary<string, LanguageData> languageData = new Dictionary<string, LanguageData>();

   
    public static void Init()
    {
        if (OptionsData.optionsSaveData.language != (int)Language.Last)
        {
            
            currentLanguage = (Language)OptionsData.optionsSaveData.language;
        }
        else
        {
           
            switch (CultureInfo.CurrentCulture.Name)
            {
                case "ko-KR":
                   
                    currentLanguage = Language.Korean;
                    break;
                default:
                  
                    currentLanguage = Language.English;
                    break;
            }
        }

       
        List<Dictionary<string, object>> getUILanguageData = CSVReader.Read("LanguageData/UILanguageData");
        for (int i = 0; i < getUILanguageData.Count; i++)
        {
            var newLanguageData = (LanguageData)ScriptableObject.CreateInstance(typeof(LanguageData));
            string keyName = getUILanguageData[i]["Key"].ToString();              
            newLanguageData.english = getUILanguageData[i]["English"].ToString();   
            newLanguageData.korean = getUILanguageData[i]["Korean"].ToString();     
            languageData.Add(keyName, newLanguageData);
        }
        OptionsData.optionsSaveData.language = (int)currentLanguage;    
    }

  
  
    /// <param name="key"></param>
    
    public static string GetText(string key)
    {
        string text;
        switch(currentLanguage)
        {
            case Language.English:
                text = languageData[key].english;
                break;
            case Language.Korean:
                text = languageData[key].korean;
                break;
            default:
                text = "ERROR!!";
                break;
        }
        return text;
    }

   
    /// <param name="right"></param>
    public static void SetLanguage(bool right)
    {
        if (right)
        {
            currentLanguage++;
            if (currentLanguage >= Language.Last)
            {
                currentLanguage = 0;
            }
        }
        else
        {
            currentLanguage--;
            if (currentLanguage < 0)
            {
                currentLanguage = Language.Last - 1;
            }
        }
        OptionsData.optionsSaveData.language = (int)currentLanguage;
    }

  
    public static string GetCurrentLanguageToText()
    {
        string languageToString;
        switch(currentLanguage)
        {
            case Language.English:
                languageToString = "ENGLISH";
                break;
            case Language.Korean:
                languageToString = "한국어";
                break;
            default:
                languageToString = "ERROR!!";
                break;
        }

        return languageToString;
    }
}
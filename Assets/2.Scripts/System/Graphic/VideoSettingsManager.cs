using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public static class VideoSettingsManager
{
    static List<Resolution> resolutions = new List<Resolution>();   
    static bool fullScreen = Screen.fullScreen; 
    static int prevResolutionIndex;             
    static int currentResolutionIndex;         

    public static bool vsync;          

  
    public static void VideoOptionsInit()
    {
       
        if (resolutions.Count > 0) return;

       
        resolutions = Screen.resolutions.ToList();
        for (int i = resolutions.Count - 1; i > 0; i--)
        {
          
            int prevResolutionWidth = resolutions[i - 1].width;
            int prevResolutionHeight = resolutions[i - 1].height;
            int currentResolutionWidth = resolutions[i].width;
            int currentResolutionHeight = resolutions[i].height;

            if (prevResolutionWidth == currentResolutionWidth && prevResolutionHeight == currentResolutionHeight)
            {
                resolutions.RemoveAt(i);
            }
        }

     
        Screen.fullScreen = fullScreen = OptionsData.optionsSaveData.fullScreenMode;
        vsync = OptionsData.optionsSaveData.vSync;
        QualitySettings.vSyncCount = vsync ? 1 : 0;

        if (OptionsData.optionsSaveData.resolution == null)
        {
        
            SetResolutionToScreenSize();
        }
        else
        {
            bool isFound = false;
            Resolution optionResolution = (Resolution)OptionsData.optionsSaveData.resolution;
            for (int i = 0; i < resolutions.Count; i++)
            {
              
                if (optionResolution.width == resolutions[i].width && optionResolution.height == resolutions[i].height)
                {
                 
                    prevResolutionIndex = currentResolutionIndex = i;
                    isFound = true;
                    break;
                }
            }
          
            if (!isFound)
            {
                SetResolutionToScreenSize();
            }
        }

   
        int newWidth = resolutions[currentResolutionIndex].width;
        int newHeight = resolutions[currentResolutionIndex].height;
        Screen.SetResolution(newWidth, newHeight, fullScreen);
    }

    
    static void SetResolutionToScreenSize()
    {
        int width = Screen.width;
        int height = Screen.height;

     
        for (int i = 0; i < resolutions.Count; i++)
        {
          
            if (width == resolutions[i].width && height == resolutions[i].height)
            {
             
                prevResolutionIndex = currentResolutionIndex = i;
                break;
            }
        }

      
        OptionsData.optionsSaveData.resolution = resolutions[currentResolutionIndex];
    }

  
    public static void SetFullScreen()
    {
        fullScreen = !fullScreen;
        Screen.fullScreen = fullScreen;
        OptionsData.optionsSaveData.fullScreenMode = fullScreen;
    }

    
    public static string GetFullScreenStatusText() => fullScreen ? LanguageManager.GetText("Enabled") : LanguageManager.GetText("Disabled");

   
    /// <param name="increase"></param>
    public static void SetResolution(bool increase)
    {
        if (increase)
        {
           
            currentResolutionIndex++;
            if (currentResolutionIndex >= resolutions.Count)
            {
                currentResolutionIndex = 0;
            }
        }
        else
        {
           
            currentResolutionIndex--;
            if (currentResolutionIndex < 0)
            {
                currentResolutionIndex = resolutions.Count - 1;
            }
        }
    }

   
    public static string GetCurrentResolutionText()
    {
        int width = resolutions[currentResolutionIndex].width;
        int height = resolutions[currentResolutionIndex].height;
        string resolutionToString = width + " x " + height;
        return resolutionToString;
    }

    
    public static void ResolutionIndexReturn() => currentResolutionIndex = prevResolutionIndex;

   
    public static void NewResolutionAccept()
    {
       
        if (prevResolutionIndex == currentResolutionIndex) return;

      
        int width = resolutions[currentResolutionIndex].width;
        int height = resolutions[currentResolutionIndex].height;
        Screen.SetResolution(width, height, fullScreen);

        prevResolutionIndex = currentResolutionIndex;  

        OptionsData.optionsSaveData.resolution = resolutions[currentResolutionIndex];  
    }

   
    public static void SetVSync()
    {
        vsync = !vsync;
        QualitySettings.vSyncCount = vsync ? 1 : 0;
        OptionsData.optionsSaveData.vSync = vsync;  
    }

   
    public static string GetVSyncStatusText() => vsync ? LanguageManager.GetText("Enabled") : LanguageManager.GetText("Disabled");
}
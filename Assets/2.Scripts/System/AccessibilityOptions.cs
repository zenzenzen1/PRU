using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class AccessibilitySettingsManager
{
    public static float gamepadVibration = 1.0f;
    public static bool screenShake = true;
    public static bool screenFlashes = true;

    public static void Init()
    {
        gamepadVibration = OptionsData.optionsSaveData.gamepadVibration;
        screenShake = OptionsData.optionsSaveData.screenShake;
        screenFlashes = OptionsData.optionsSaveData.screenFlashes;
    }

    public static void SetGamepadVibration(bool increase)
    {
        gamepadVibration += increase ? 0.2f : -0.2f;
        gamepadVibration = Mathf.Clamp(gamepadVibration, 0f, 1f);
        GamepadVibrationManager.instance.GamepadRumbleStart(gamepadVibration, 0.05f);
        OptionsData.optionsSaveData.gamepadVibration = gamepadVibration;
    }

    public static string GetGamepadVibrationToUI()
    {
        int vibration = Mathf.RoundToInt(gamepadVibration * 5);
        StringBuilder vibrationToText = new StringBuilder();
        for (int i = 0; i < 5; i++)
        {
            if (i < vibration)
            {
                vibrationToText.Append("■");
            }
            else
            {
                vibrationToText.Append("□");
            }
        }
        return vibrationToText.ToString();
    }

    public static void SetScreenShake()
    {
        screenShake = !screenShake;
        OptionsData.optionsSaveData.screenShake = screenShake;
    }

    public static string GetScreenShakeToggle()
    {
        return screenShake ? LanguageManager.GetText("Enabled") : LanguageManager.GetText("Disabled");
    }

    public static void SetScreenFlashes()
    {
        screenFlashes = !screenFlashes;
        OptionsData.optionsSaveData.screenFlashes = screenFlashes;
    }

    public static string GetScreenFlashesToggle()
    {
        return screenFlashes == true ? LanguageManager.GetText("Enabled") : LanguageManager.GetText("Disabled");
    }
}
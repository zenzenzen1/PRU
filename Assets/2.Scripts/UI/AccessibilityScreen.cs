using System.Collections;
using System.Collections.Generic;
using TMPro;
using MenuUI;
using UnityEngine;

public class AccessibilityScreen : MonoBehaviour
{
    public GameObject optionsMenuScreen;
    public TextMeshProUGUI accessibilityText;
    public TextMeshProUGUI manualText;
    public Menu[] menu;
    int _currentMenuIndex;

    bool _rightInput;
    bool _leftInput;

    void OnEnable()
    {
        AccessibilityOptionsRefresh();
        MenuUIController.MenuRefresh(menu, ref _currentMenuIndex, manualText);
    }

    void Update()
    {
        bool upInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Up);
        bool downInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Down);
        _rightInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Right);
        _leftInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Left);
        bool backInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Cancle);

        if (upInput)
        {
            _currentMenuIndex--;
            MenuUIController.MenuRefresh(menu, ref _currentMenuIndex, manualText);
        }
        else if (downInput)
        {
            _currentMenuIndex++;
            MenuUIController.MenuRefresh(menu, ref _currentMenuIndex, manualText);
        }

        if (_rightInput || _leftInput)
        {
            menu[_currentMenuIndex].menuSelectEvent.Invoke();
        }

        if (backInput)
        {
            _currentMenuIndex = 0;
            MenuUIController.MenuRefresh(menu, ref _currentMenuIndex, manualText);
            ReturnToOptionsMenuScreen();
        }
    }

    public void SetGamepadVibration()
    {
        bool increase = _leftInput ? false : true;
        AccessibilitySettingsManager.SetGamepadVibration(increase);
        AccessibilityOptionsRefresh();
    }

    public void SetScreenShake()
    {
        AccessibilitySettingsManager.SetScreenShake();
        AccessibilityOptionsRefresh();
    }

    public void SetScreenFlashes()
    {
        AccessibilitySettingsManager.SetScreenFlashes();
        AccessibilityOptionsRefresh();
    }

    void AccessibilityOptionsRefresh()
    {
        accessibilityText.text = LanguageManager.GetText("Accessibility");
        for (int i = 0; i < menu.Length; i++)
        {
            switch (menu[i].text[0].name)
            {
                case "ControllerVibrationText":
                    menu[i].text[0].text = LanguageManager.GetText("ControllerVibration");
                    menu[i].text[1].text = AccessibilitySettingsManager.GetGamepadVibrationToUI();
                    break;
                case "ScreenShakeText":
                    menu[i].text[0].text = LanguageManager.GetText("ScreenShake");
                    menu[i].text[1].text = AccessibilitySettingsManager.GetScreenShakeToggle();
                    break;
                case "ScreenFlashesText":
                    menu[i].text[0].text = LanguageManager.GetText("ScreenFlashes");
                    menu[i].text[1].text = AccessibilitySettingsManager.GetScreenFlashesToggle();
                    break;
            }
        }
    }

    void ReturnToOptionsMenuScreen()
    {
        gameObject.SetActive(false);
        optionsMenuScreen.SetActive(true);
    }
}

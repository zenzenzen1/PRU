using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Controls;
using UnityEngine;
using System;

public static class GameInputManager
{
   
    public enum PlayerActions
    {
        MoveLeft,
        MoveRight,
        MoveUp,
        MoveDown,
        Jump,
        Attack,
        SpecialAttack,
        Dodge,
        Pause,
        Map,
        Count
    }

  
    public enum MenuControl
    {
        Left,
        Right,
        Up,
        Down,
        Select,
        Cancle,
        Delete,
        Count
    }

    static public bool usingController; 

   
    static Key[] keyboardActionsDefaults = new Key[]
    {
        Key.LeftArrow,
        Key.RightArrow,
        Key.UpArrow,
        Key.DownArrow,
        Key.Z,
        Key.X,
        Key.A,
        Key.C,
        Key.Escape,
        Key.Tab
    };

    static GamepadButton[] gamepadActionsDefaults = new GamepadButton[]
    {
        GamepadButton.DpadLeft,
        GamepadButton.DpadRight,
        GamepadButton.DpadUp,
        GamepadButton.DpadDown,
        GamepadButton.A,
        GamepadButton.X,
        GamepadButton.Y,
        GamepadButton.RightTrigger,
        GamepadButton.Start,
        GamepadButton.Select,
    };

  
    static Key[] keyboardMenuDefaults = new Key[]
    {
        Key.LeftArrow,
        Key.RightArrow,
        Key.UpArrow,
        Key.DownArrow,
        Key.Space,
        Key.Escape,
        Key.Delete
    };

   
    static GamepadButton[] gamepadMenuDefaults = new GamepadButton[]
    {
        GamepadButton.DpadLeft,
        GamepadButton.DpadRight,
        GamepadButton.DpadUp,
        GamepadButton.DpadDown,
        GamepadButton.A,
        GamepadButton.B,
        GamepadButton.X
    };

    static Dictionary<PlayerActions, Key> keyboardActionsMap;          
    static Dictionary<MenuControl, Key> keyboardMenuMap;               
    static Dictionary<PlayerActions, GamepadButton> gamepadActionsMap; 
    static Dictionary<MenuControl, GamepadButton> gamepadMenuMap;       

    static bool hasInit; 

    static GameInputManager()
    {
        Init();
    }

    public static void Init()
    {
        if(!hasInit)
        {
            hasInit = true;

            InitializeActionsDictionary();
            InitializeMenuControlDictionary();

            usingController = Gamepad.all.Count > 0;
        }
    }

    
    static void InitializeActionsDictionary()
    {
        keyboardActionsMap = new Dictionary<PlayerActions, Key>();
        gamepadActionsMap = new Dictionary<PlayerActions, GamepadButton>();

        if (OptionsData.optionsSaveData.keyMapping.Count > 0)
        {
            LoadCustomMappings();
        }
        else
        {
            LoadDefaultMappings();
        }
    }

    private static void LoadCustomMappings()
    {
        List<int> keyMapping = OptionsData.optionsSaveData.keyMapping;
        List<int> buttonMapping = OptionsData.optionsSaveData.buttonMapping;

        for (int i = 0; i < keyMapping.Count; i++)
        {
            keyboardActionsMap.Add((PlayerActions)i, (Key)keyMapping[i]);
        }
        for (int i = 0; i < buttonMapping.Count; i++)
        {
            gamepadActionsMap.Add((PlayerActions)i, (GamepadButton)buttonMapping[i]);
        }
    }

    private static void LoadDefaultMappings()
    {
        for (int i = 0; i < (int)PlayerActions.Count; i++)
        {
            keyboardActionsMap.Add((PlayerActions)i, keyboardActionsDefaults[i]);
            gamepadActionsMap.Add((PlayerActions)i, gamepadActionsDefaults[i]);

            OptionsData.optionsSaveData.keyMapping.Add((int)keyboardActionsDefaults[i]);
            OptionsData.optionsSaveData.buttonMapping.Add((int)gamepadActionsDefaults[i]);
        }
    }

   
    static void InitializeMenuControlDictionary()
    {
        keyboardMenuMap = new Dictionary<MenuControl, Key>();
        gamepadMenuMap = new Dictionary<MenuControl, GamepadButton>();

        for (int i = 0; i < (int)MenuControl.Count; i++)
        {
            keyboardMenuMap.Add((MenuControl)i, keyboardMenuDefaults[i]);
            gamepadMenuMap.Add((MenuControl)i, gamepadMenuDefaults[i]);
        }
    }

    public static bool PlayerInput(PlayerActions action) => KeyboardInput(keyboardActionsMap[action]) || (Gamepad.all.Count > 0 && GamepadInput(gamepadActionsMap[action]));
    public static bool PlayerInputDown(PlayerActions action) => KeyboardInputDown(keyboardActionsMap[action]) || (Gamepad.all.Count > 0 && GamepadInputDown(gamepadActionsMap[action]));

    /// <param name="menuControl"></param>
   
    public static bool MenuInput(MenuControl menuControl) => KeyboardInput(keyboardMenuMap[menuControl]) || (Gamepad.all.Count > 0 && GamepadInput(gamepadMenuMap[menuControl]));

  
    /// <param name="menuControl"></param>
  
    public static bool MenuInputDown(MenuControl menuControl) => KeyboardInputDown(keyboardMenuMap[menuControl]) || (Gamepad.all.Count > 0 && GamepadInputDown(gamepadMenuMap[menuControl]));

   
    /// <param name="key"></param>
  
    static bool KeyboardInput(Key key)
    {
        if (Keyboard.current[key].isPressed)
        {
            usingController = false;
            return true;
        }
        return false;
    }

   
    /// <param name="key"></param>
   
    static bool KeyboardInputDown(Key key)
    {
        if (Keyboard.current[key].wasPressedThisFrame)
        {
            usingController = false;
            return true;
        }
        return false;
    }

   
    /// <param name="button"></param>
   
    static bool GamepadInput(GamepadButton button)
    {
        if (Gamepad.current[button].isPressed)
        {
            usingController = true;
            return true;
        }
        else
        {
          
            ButtonControl stick = DpadToStickInput(button);
            if (stick != null && GamepadStickInput(stick))
            {
                usingController = true;
                return true;
            }
            return false;
        }
    }

    
    /// <param name="button"></param>
   
    static bool GamepadInputDown(GamepadButton button)
    {
        if (Gamepad.current[button].wasPressedThisFrame)
        {
            usingController = true;
            return true;
        }
        else
        {
           
            ButtonControl stick = DpadToStickInput(button);
            if(stick != null && GamepadStickInputDown(stick))
            {
                usingController = true;
                return true;
            }
        }
        return false;
    }

    
    /// <param name="button"></param>
   
    static ButtonControl DpadToStickInput(GamepadButton button)
    {
        switch (button)
        {
            case GamepadButton.DpadUp:
                return Gamepad.current.leftStick.up;
            case GamepadButton.DpadDown:
                return Gamepad.current.leftStick.down;
            case GamepadButton.DpadLeft:
                return Gamepad.current.leftStick.left;
            case GamepadButton.DpadRight:
                return Gamepad.current.leftStick.right;
            default:
                return null;
        }
    }

  
    /// <param name="stick"></param>
   
    static bool GamepadStickInput(ButtonControl stick)
    {
        if (stick.isPressed)
        {
            usingController = true;
            return true;
        }
        return false;
    }

   
    /// <param name="stick"></param>
  
    static bool GamepadStickInputDown(ButtonControl stick)
    {
        if (stick.wasPressedThisFrame)
        {
            usingController = true;
            return true;
        }
        return false;
    }

    /// <param name="action"></param>
    /// <param name="key"></param>
    public static void KeyboardMapping(PlayerActions action, Key key)
    {
        foreach (KeyValuePair<PlayerActions, Key> pair in keyboardActionsMap)
        {
            if (pair.Value == key)
            {
                keyboardActionsMap[pair.Key] = keyboardActionsMap[action];
                OptionsData.optionsSaveData.keyMapping[(int)pair.Key] = (int)keyboardActionsMap[action];
                break;
            }
        }
        keyboardActionsMap[action] = key;
        OptionsData.optionsSaveData.keyMapping[(int)action] = (int)key;
    }

   
    /// <param name="action"></param>
    /// <param name="button"></param>
    public static void GamepadMapping(PlayerActions action, GamepadButton button)
    {
        foreach(KeyValuePair<PlayerActions, GamepadButton> pair in gamepadActionsMap)
        {
            if(pair.Value == button)
            {
                gamepadActionsMap[pair.Key] = gamepadActionsMap[action];
                OptionsData.optionsSaveData.buttonMapping[(int)pair.Key] = (int)gamepadActionsMap[action];
                break;
            }
        }
        gamepadActionsMap[action] = button;
        OptionsData.optionsSaveData.buttonMapping[(int)action] = (int)button;
    }

   
    public static Key GetCurrentInputKey()
    {
        Key inputKey = Key.None;

        for(int i = 1; i <= (int)Key.F12; i++)
        {
            if (Keyboard.current[(Key)i].wasPressedThisFrame)
            {
             
                switch ((Key)i)
                {
                    case Key.Escape:
                    case Key.Tab:
                    case Key.LeftMeta:
                    case Key.RightMeta:
                    case Key.ContextMenu:
                    case Key.CapsLock:
                    case Key.NumLock:
                    case Key.PrintScreen:
                    case Key.ScrollLock:
                    case Key.Pause:
                        break;
                }
                inputKey = (Key)i;
            }
        }

        return inputKey;
    }

   
    public static GamepadButton GetCurrentInputButton()
    {
        GamepadButton inputButton = GamepadButton.Start;

        for (int i = 0; i <= (int)GamepadButton.RightShoulder; i++)
        {
            if (Gamepad.current[(GamepadButton)i].wasPressedThisFrame)
            {
                inputButton = (GamepadButton)i;
            }
        }

        
        if(Gamepad.current[GamepadButton.LeftTrigger].wasPressedThisFrame)
        {
            inputButton = GamepadButton.LeftTrigger;
        }
        else if(Gamepad.current[GamepadButton.RightTrigger].wasPressedThisFrame)
        {
            inputButton = GamepadButton.RightTrigger;
        }

       
        if(Gamepad.current.leftStick.up.wasPressedThisFrame)
        {
            inputButton = GamepadButton.DpadUp;
        }
        else if (Gamepad.current.leftStick.down.wasPressedThisFrame)
        {
            inputButton = GamepadButton.DpadDown;
        }
        else if (Gamepad.current.leftStick.left.wasPressedThisFrame)
        {
            inputButton = GamepadButton.DpadLeft;
        }
        else if (Gamepad.current.leftStick.right.wasPressedThisFrame)
        {
            inputButton = GamepadButton.DpadRight;
        }

        return inputButton;
    }

    public static void KeyboardInputSetDefaults()
    {
        SetDefaultInputMap(keyboardActionsMap, OptionsData.optionsSaveData.keyMapping, keyboardActionsDefaults);
    }
    public static void ControllerInputSetDefaults()
    {
        SetDefaultInputMap(gamepadActionsMap, OptionsData.optionsSaveData.buttonMapping, gamepadActionsDefaults);
    }
    private static void SetDefaultInputMap<T>(Dictionary<PlayerActions, T> inputMap, List<int> mappingList, T[] defaults)
    {
        inputMap.Clear();

        for (int i = 0; i < mappingList.Count; i++)
        {
            inputMap.Add((PlayerActions)i, defaults[i]);
            mappingList[i] = Convert.ToInt32(defaults[i]);
        }
    }

    public static string ActionToKeyText(PlayerActions action) => KeyToText(keyboardActionsMap[action]);
    public static string ActionToButtonText(PlayerActions action) => ButtonToText(gamepadActionsMap[action]);
    public static string MenuControlToKeyText(MenuControl menuControl) => KeyToText(keyboardMenuMap[menuControl]);
    public static string MenuControlToButtonText(MenuControl menuControl) => ButtonToText(gamepadMenuMap[menuControl]);
    static string KeyToText(Key key)
    {
        string text;
        switch (key)
        {
            case Key.Space:
                text = "Spcae";
                break;
            case Key.Enter:
                text = "Enter";
                break;
            case Key.Tab:
                text = "Tab";
                break;
            case Key.Backquote:
                text = "`";
                break;
            case Key.Quote:
                text = "'";
                break;
            case Key.Semicolon:
                text = ";";
                break;
            case Key.Comma:
                text = ",";
                break;
            case Key.Period:
                text = ".";
                break;
            case Key.Slash:
                text = "/";
                break;
            case Key.Backslash:
                text = "|";
                break;
            case Key.LeftBracket:
                text = "[";
                break;
            case Key.RightBracket:
                text = "]";
                break;
            case Key.Minus:
                text = "-";
                break;
            case Key.Equals:
                text = "=";
                break;
            case Key.A:
            case Key.B:
            case Key.C:
            case Key.D:
            case Key.E:
            case Key.F:
            case Key.G:
            case Key.H:
            case Key.I:
            case Key.J:
            case Key.K:
            case Key.L:
            case Key.M:
            case Key.N:
            case Key.O:
            case Key.P:
            case Key.Q:
            case Key.R:
            case Key.S:
            case Key.T:
            case Key.U:
            case Key.V:
            case Key.W:
            case Key.X:
            case Key.Y:
            case Key.Z:
                text = key.ToString();
                break;
            case Key.Digit1:
                text = "!";
                break;
            case Key.Digit2:
                text = "@";
                break;
            case Key.Digit3:
                text = "#";
                break;
            case Key.Digit4:
                text = "$";
                break;
            case Key.Digit5:
                text = "%";
                break;
            case Key.Digit6:
                text = "^";
                break;
            case Key.Digit7:
                text = "&";
                break;
            case Key.Digit8:
                text = "*";
                break;
            case Key.Digit9:
                text = "(";
                break;
            case Key.Digit0:
                text = ")";
                break;
            case Key.LeftShift:
                text = "Left Shift";
                break;
            case Key.RightShift:
                text = "RIght Shift";
                break;
            case Key.LeftAlt:
                text = "Left Alt";
                break;
            case Key.RightAlt:
                text = "Right Alt";
                break;
            case Key.LeftCtrl:
                text = "Left Ctrl";
                break;
            case Key.RightCtrl:
                text = "Right Ctrl";
                break;
            case Key.Escape:
                text = "ESC";
                break;
            case Key.LeftArrow:
                text = "←";
                break;
            case Key.RightArrow:
                text = "→";
                break;
            case Key.UpArrow:
                text = "↑";
                break;
            case Key.DownArrow:
                text = "↓";
                break;
            case Key.Backspace:
                text = "Backspace";
                break;
            case Key.PageDown:
                text = "Page Down";
                break;
            case Key.PageUp:
                text = "Page Up";
                break;
            case Key.Home:
                text = "Home";
                break;
            case Key.End:
                text = "End";
                break;
            case Key.Insert:
                text = "Insert";
                break;
            case Key.Delete:
                text = "Delete";
                break;
            case Key.NumpadEnter:
                text = "Num Enter";
                break;
            case Key.NumpadDivide:
                text = "Num /";
                break;
            case Key.NumpadMultiply:
                text = "Num *";
                break;
            case Key.NumpadPlus:
                text = "Num +";
                break;
            case Key.NumpadMinus:
                text = "Num -";
                break;
            case Key.NumpadPeriod:
                text = "Num .";
                break;
            case Key.NumpadEquals:
                text = "Num =";
                break;
            case Key.Numpad0:
                text = "Num 0";
                break;
            case Key.Numpad1:
                text = "Num 1";
                break;
            case Key.Numpad2:
                text = "Num 2";
                break;
            case Key.Numpad3:
                text = "Num 3";
                break;
            case Key.Numpad4:
                text = "Num 4";
                break;
            case Key.Numpad5:
                text = "Num 5";
                break;
            case Key.Numpad6:
                text = "Num 6";
                break;
            case Key.Numpad7:
                text = "Num 7";
                break;
            case Key.Numpad8:
                text = "Num 8";
                break;
            case Key.Numpad9:
                text = "Num 9";
                break;
            case Key.F1:
                text = "F1";
                break;
            case Key.F2:
                text = "F2";
                break;
            case Key.F3:
                text = "F3";
                break;
            case Key.F4:
                text = "F4";
                break;
            case Key.F5:
                text = "F5";
                break;
            case Key.F6:
                text = "F6";
                break;
            case Key.F7:
                text = "F7";
                break;
            case Key.F8:
                text = "F8";
                break;
            case Key.F9:
                text = "F9";
                break;
            case Key.F10:
                text = "F10";
                break;
            case Key.F11:
                text = "F11";
                break;
            case Key.F12:
                text = "F12";
                break;
            default:
                text = "ERROR!!";
                break;
        }
        return text;
    }

    static string ButtonToText(GamepadButton button)
    {
        string text;
        switch (button)
        {
            case GamepadButton.DpadUp:
                text = "↑";
                break;
            case GamepadButton.DpadDown:
                text = "↓";
                break;
            case GamepadButton.DpadLeft:
                text = "←";
                break;
            case GamepadButton.DpadRight:
                text = "→";
                break;
            case GamepadButton.Y:
                text = "Y";
                break;
            case GamepadButton.B:
                text = "B";
                break;
            case GamepadButton.A:
                text = "A";
                break;
            case GamepadButton.X:
                text = "X";
                break;
            case GamepadButton.LeftStick:
                text = "L Stick Button";
                break;
            case GamepadButton.RightStick:
                text = "R Stick Button";
                break;
            case GamepadButton.LeftShoulder:
                text = "LB";
                break;
            case GamepadButton.RightShoulder:
                text = "RB";
                break;
            case GamepadButton.LeftTrigger:
                text = "LT";
                break;
            case GamepadButton.RightTrigger:
                text = "RT";
                break;
            case GamepadButton.Start:
                text = "Start";
                break;
            case GamepadButton.Select:
                text = "Select";
                break;
            default:
                text = "ERROR!!";
                break;
        }
        return text;
    }
}

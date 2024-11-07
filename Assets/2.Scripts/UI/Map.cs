using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class MapManager
{
    static List<Vector2> discoveredMapPositions = new List<Vector2>();

    public static void ClearDiscoveredMaps()
    {
        discoveredMapPositions.Clear();
    }

    public static void AddDiscoveredMap(Image mapImage)
    {
        discoveredMapPositions.Add(mapImage.rectTransform.anchoredPosition);
    }

    public static void AddDiscoveredMaps(List<Vector2> mapPositions)
    {
        discoveredMapPositions.AddRange(mapPositions);
    }

    public static List<Vector2> GetDiscoveredMaps() => discoveredMapPositions;
}

[System.Serializable]
public class MenuControlAndKey
{
    public GameInputManager.MenuControl[] menuControl;
    public string menuKey;
}

public class Map : MonoBehaviour
{
    const int MapTileSpacing = 8;
    const int MaxZoomLevel = 3;
    const int MinZoomLevel = 1;

    [SerializeField] TextMeshProUGUI _mapTitleText;
    [SerializeField] TextMeshProUGUI _manualText;
    [SerializeField] RectTransform _mapTileContainer;
    [SerializeField] RectTransform _playerIcon;
    [SerializeField] List<Image> _mapTiles;
    [SerializeField] MenuControlAndKey[] _menuControlAndKey;

    int _mapZoomLevel = 1;

    Vector2 _mapOriginPos = Vector2.zero;
    Camera _camera;
    Transform _cameraTransform;

    void Awake()
    {
        transform.GetComponent<PauseScreen>().MapOpend += OnMapOpend;

        _camera = Camera.main;
        _cameraTransform = _camera.GetComponent<Transform>();

        foreach (var mapTile in _mapTiles)
        {
            mapTile.gameObject.SetActive(false);
        }

        foreach (Vector2 mapPos in MapManager.GetDiscoveredMaps())
        {
            Image mapImage = _mapTiles.Find(x => x.rectTransform.anchoredPosition == mapPos);
            mapImage?.gameObject.SetActive(true);
        }
        _mapOriginPos = _mapTileContainer.anchoredPosition;
    }

    void Update()
    {
        UpdatePlayerIcon();
        DiscoverMap();
        MoveMap();
        ZoomMap();
    }

    void UpdatePlayerIcon()
    {
        float cameraHalfSizeY = _camera.orthographicSize;
        float cameraHalfSizeX = cameraHalfSizeY * _camera.aspect;

        float xPos = (_cameraTransform.position.x + cameraHalfSizeX) / (cameraHalfSizeX * 2);
        float yPos = (_cameraTransform.position.y + cameraHalfSizeY) / (cameraHalfSizeY * 2);

        int xPosIndex = Mathf.FloorToInt(xPos) * MapTileSpacing;
        int yPosIndex = Mathf.FloorToInt(yPos) * MapTileSpacing;

        _playerIcon.anchoredPosition = new Vector2(xPosIndex, yPosIndex);
    }

    void DiscoverMap()
    {
        Image mapImage = _mapTiles.Find(x => x.rectTransform.anchoredPosition == _playerIcon.anchoredPosition);
        if (!mapImage.gameObject.activeSelf)
        {
            mapImage.gameObject.SetActive(true);
            MapManager.AddDiscoveredMap(mapImage);
        }
    }

    void MoveMap()
    {
        float yMove = GameInputManager.MenuInput(GameInputManager.MenuControl.Up) ? 1 :
                      GameInputManager.MenuInput(GameInputManager.MenuControl.Down) ? -1 :
                      0;
        float xMove = GameInputManager.MenuInput(GameInputManager.MenuControl.Right) ? 1 :
                      GameInputManager.MenuInput(GameInputManager.MenuControl.Left) ? -1 :
                      0;

        _mapTileContainer.Translate(new Vector2(xMove, yMove) * 10f * Time.unscaledDeltaTime);

        _mapTileContainer.anchoredPosition = new Vector2(Mathf.Clamp(_mapTileContainer.anchoredPosition.x, -50f, 20f), 
                                                         Mathf.Clamp(_mapTileContainer.anchoredPosition.y, -20f, 20f));
    }

    void ZoomMap()
    {
        if (GameInputManager.MenuInputDown(GameInputManager.MenuControl.Select))
        {
            if (_mapZoomLevel < MaxZoomLevel)
            {
                _mapTileContainer.localScale += Vector3.one;
                _mapZoomLevel++;
            }
            else
            {
                _mapTileContainer.localScale = Vector3.one;
                _mapZoomLevel = MinZoomLevel;
            }
        }
    }

    public void OnMapOpend()
    {
        _mapTitleText.text = LanguageManager.GetText("Map");
        SetManualText();

        _mapZoomLevel = 1;
        _mapTileContainer.localScale = Vector3.one;
        
        _mapTileContainer.anchoredPosition = _mapOriginPos - _playerIcon.anchoredPosition;
    }

    void SetManualText()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < _menuControlAndKey.Length; i++)
        {

            for (int j = 0; j < _menuControlAndKey[i].menuControl.Length; j++)
            {
                string menuControlToText;
                if (GameInputManager.usingController)
                {
                    menuControlToText = GameInputManager.MenuControlToButtonText(_menuControlAndKey[i].menuControl[j]);
                }
                else
                {
                    menuControlToText = GameInputManager.MenuControlToKeyText(_menuControlAndKey[i].menuControl[j]);
                }
                sb.AppendFormat("[ <color=#ffaa5e>{0}</color> ] ", menuControlToText);
            }
            string keyToName = LanguageManager.GetText(_menuControlAndKey[i].menuKey);
            sb.Append(keyToName);

            if (i < _menuControlAndKey.Length - 1)
            {
                sb.Append("  ");
            }
        }
        _manualText.text = sb.ToString();
    }
}
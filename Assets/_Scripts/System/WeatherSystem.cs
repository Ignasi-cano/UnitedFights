// Assets/_Scripts/System/WeatherSystem.cs
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WeatherSystem : Singleton<WeatherSystem>
{
    [SerializeField] private string apiKey = "TU_API_KEY";
    [SerializeField] private string city = "Barcelona";
   
    [Header("Backgrounds por clima")]
    [SerializeField] private Sprite clearBackground;
    [SerializeField] private Sprite cloudyBackground;
    [SerializeField] private Sprite rainyBackground;
    [SerializeField] private Sprite snowyBackground;
    [SerializeField] private Sprite defaultBackground;
   
    [Header("Referencias")]
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private bool useFixedScale = true;
    [SerializeField] private Vector2 globalBackgroundScale = new Vector2(21f, 21f);
    
    private float lastScreenWidth;
    private float lastScreenHeight;
   
    public string CurrentWeather { get; private set; }
   
    private void Start()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        StartCoroutine(FetchWeather());
    }

    private void Update()
    {
        // Detect resolution changes
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            
            if (backgroundRenderer != null && backgroundRenderer.sprite != null)
            {
                FitBackgroundToScreen(backgroundRenderer.sprite);
            }
        }
    }
   
    private IEnumerator FetchWeather()
    {
        string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}";
       
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
           
            if (request.result == UnityWebRequest.Result.Success)
            {
                ProcessWeatherData(request.downloadHandler.text);
            }
            else
            {
                Debug.LogWarning($"Weather API error: {request.error}");
                SetBackground(defaultBackground);
            }
        }
    }
   
    private void ProcessWeatherData(string json)
    {
        WeatherResponse response = JsonUtility.FromJson<WeatherResponse>(json);
       
        if (response.weather != null && response.weather.Length > 0)
        {
            CurrentWeather = response.weather[0].main;
            UpdateBackground(CurrentWeather);
        }
    }
   
    private void UpdateBackground(string weatherCondition)
    {
        Sprite newBackground = weatherCondition.ToLower() switch
        {
            "clear" => clearBackground,
            "clouds" => cloudyBackground,
            "rain" or "drizzle" or "thunderstorm" => rainyBackground,
            "snow" => snowyBackground,
            _ => defaultBackground
        };
       
        SetBackground(newBackground);
    }
   
    private void SetBackground(Sprite sprite)
    {
        if (backgroundRenderer != null && sprite != null)
        {
            backgroundRenderer.sprite = sprite;
            FitBackgroundToScreen(sprite);
        }
    }

    private void FitBackgroundToScreen(Sprite sprite)
    {
        Camera mainCam = Camera.main;
        if (mainCam == null || sprite == null) return;

        // 1. Get screen dimensions in world units
        float screenHeight = mainCam.orthographicSize * 2f;
        float screenWidth = screenHeight * mainCam.aspect;

        // 2. Get sprite original dimensions in world units (ignoring current transform scale)
        float spriteWorldWidth = sprite.rect.width / sprite.pixelsPerUnit;
        float spriteWorldHeight = sprite.rect.height / sprite.pixelsPerUnit;

        if (spriteWorldWidth == 0 || spriteWorldHeight == 0) return;

        // 3. Calculate scales
        float scaleX = screenWidth / spriteWorldWidth;
        float scaleY = screenHeight / spriteWorldHeight;

        // NEW: Apply global scale override ONLY if enabled
        if (useFixedScale && globalBackgroundScale != Vector2.zero)
        {
            scaleX = globalBackgroundScale.x;
            scaleY = globalBackgroundScale.y;
            Debug.Log($"[WeatherSystem] Applying Global Scale Override: {scaleX}, {scaleY}");
        }
        else
        {
            // Maintain aspect ratio while covering the whole screen (using "Cover" logic)
            float maxScale = Mathf.Max(scaleX, scaleY);
            scaleX = maxScale;
            scaleY = maxScale;
        }

        // 4. Set the scale
        backgroundRenderer.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        
        // 5. Center it EXACTLY at the camera position, accounting for Sprite Pivot
        // Calculate the offset from the visual center to the pivot in world units
        float pivotOffsetX = (0.5f - sprite.pivot.x / sprite.rect.width) * spriteWorldWidth * scaleX;
        float pivotOffsetY = (0.5f - sprite.pivot.y / sprite.rect.height) * spriteWorldHeight * scaleY;

        float currentZ = backgroundRenderer.transform.position.z;
        backgroundRenderer.transform.position = new Vector3(
            mainCam.transform.position.x + pivotOffsetX, 
            mainCam.transform.position.y + pivotOffsetY, 
            currentZ
        );
        
        Debug.Log($"[WeatherSystem] Background scaled to {scaleX}, {scaleY} and centered at {backgroundRenderer.transform.position} with pivot offset {pivotOffsetX}, {pivotOffsetY}");
    }
}

// Clases para deserializar el JSON
[Serializable]
public class WeatherResponse
{
    public WeatherInfo[] weather;
}

[Serializable]
public class WeatherInfo
{
    public string main;
    public string description;
}
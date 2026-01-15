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
   
    public string CurrentWeather { get; private set; }
   
    private void Start()
    {
        StartCoroutine(FetchWeather());
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
        }
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
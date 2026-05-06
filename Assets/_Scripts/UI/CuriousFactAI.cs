using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class CuriousFactAI : MonoBehaviour
{
    [SerializeField] private GameObject chatBubble;
    [SerializeField] private TMP_Text chatText;

    [Header("Google Gemini")]
    [SerializeField] private string apiKey = "TU_API_KEY";
    [SerializeField] private string model = "gemini-2.0-flash";

    private bool isGenerating = false;

    public void OnCuriousFactButton()
    {
        if (isGenerating) return;
        StartCoroutine(GetCuriousFact());
    }

    private IEnumerator GetCuriousFact()
    {
        isGenerating = true;

        chatBubble.SetActive(true);
        chatText.text = "Pensando...";

        string url =
            "https://generativelanguage.googleapis.com/v1beta/models/"
            + model
            + ":generateContent?key="
            + apiKey;

        string jsonBody = @"
        {
            ""contents"": [
                {
                    ""parts"": [
                        {
                            ""text"": ""Responde en español con un dato curioso breve sobre un Pokémon aleatorio. No uses más de dos frases.""
                        }
                    ]
                }
            ]
        }";

        using UnityWebRequest request = new UnityWebRequest(url, "POST");

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        isGenerating = false;

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("ERROR: " + request.error);
            Debug.LogError("CODE: " + request.responseCode);
            Debug.LogError("BODY: " + request.downloadHandler.text);

            chatText.text = "Ha fallado: " + request.responseCode;
            yield break;
        }

        GeminiResponse response =
            JsonUtility.FromJson<GeminiResponse>(request.downloadHandler.text);

        if (response != null &&
            response.candidates != null &&
            response.candidates.Length > 0 &&
            response.candidates[0].content != null &&
            response.candidates[0].content.parts != null &&
            response.candidates[0].content.parts.Length > 0)
        {
            chatText.text = response.candidates[0].content.parts[0].text;
        }
        else
        {
            chatText.text = "La IA no ha devuelto texto.";
            Debug.LogWarning("Respuesta inesperada: " + request.downloadHandler.text);
        }
    }
}

[System.Serializable]
public class GeminiResponse
{
    public GeminiCandidate[] candidates;
}

[System.Serializable]
public class GeminiCandidate
{
    public GeminiContent content;
}

[System.Serializable]
public class GeminiContent
{
    public GeminiPart[] parts;
}

[System.Serializable]
public class GeminiPart
{
    public string text;
}
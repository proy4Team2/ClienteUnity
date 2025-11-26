using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;

public class ApiClient : MonoBehaviour {
    public static ApiClient Instance { get; private set; }
    [Header("Server Configuration")]
    public string baseUrl = "http://localhost:3000"; 

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public IEnumerator UploadAudioSession(byte[] audioData, string token, string language, System.Action<AnalysisResponse> onSuccess, System.Action<string> onError) {
        if (string.IsNullOrEmpty(token)) {
            onError?.Invoke("Authorization token is missing. Please log in first.");
            yield break;
        }

        string endpoint = $"{baseUrl}/api/sessions";
        WWWForm form = new WWWForm();
        form.AddField("language", language);
        form.AddBinaryData("audio", audioData, "recording.m4a", "audio/mp4");

        using (UnityWebRequest request = UnityWebRequest.Post(endpoint, form)) {
            request.SetRequestHeader("Authorization", $"Bearer {token}");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success) {
                string errorMessage = $"Server Error: {request.error} \nResponse: {request.downloadHandler.text}";
                onError?.Invoke(errorMessage);
            } else ProcessResponse(request.downloadHandler.text, onSuccess, onError);
        }
    }

    private void ProcessResponse(string jsonResponse, System.Action<AnalysisResponse> onSuccess, System.Action<string> onError) {
        try {
            var responseObj = JsonConvert.DeserializeObject<AnalysisResponse>(jsonResponse);
            onSuccess?.Invoke(responseObj);
        } catch (System.Exception ex) { onError?.Invoke($"JSON Parsing Error: {ex.Message}"); }
    }
}
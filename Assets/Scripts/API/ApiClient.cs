using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;
using System.Text;

public class ApiClient : MonoBehaviour
{
    public static ApiClient Instance { get; private set; }

    [Header("Server Configuration")]
    public string baseUrl = "http://localhost:3000";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    // ==========================================
    // AUTENTICACIÓN Y USUARIOS
    // ==========================================

    public IEnumerator RegisterUser(string email, string password, string name, System.Action<AuthServerResponse> onSuccess, System.Action<string> onError)
    {
        string jsonBody = JsonConvert.SerializeObject(new { email = email, password = password, name = name });
        yield return SendJsonRequest($"{baseUrl}/api/auth/register", "POST", jsonBody, null, onSuccess, onError);
    }

    public IEnumerator LoginUser(string email, string password, System.Action<AuthServerResponse> onSuccess, System.Action<string> onError)
    {
        string jsonBody = JsonConvert.SerializeObject(new { email = email, password = password });
        yield return SendJsonRequest($"{baseUrl}/api/auth/login", "POST", jsonBody, null, onSuccess, onError);
    }

    public IEnumerator GetProfile(string token, System.Action<UserProfileResponse> onSuccess, System.Action<string> onError)
    {
        yield return SendJsonRequest($"{baseUrl}/api/auth/profile", "GET", null, token, onSuccess, onError);
    }

    public IEnumerator UpdateProfile(string name, string token, System.Action<GenericServerResponse> onSuccess, System.Action<string> onError)
    {
        string jsonBody = JsonConvert.SerializeObject(new { name = name });
        yield return SendJsonRequest($"{baseUrl}/api/auth/profile", "PUT", jsonBody, token, onSuccess, onError);
    }

    // ==========================================
    // SESIONES Y ANÁLISIS
    // ==========================================

    public IEnumerator UploadAudioSession(byte[] audioData, string token, string language, System.Action<AnalysisResponse> onSuccess, System.Action<string> onError)
    {
        if (string.IsNullOrEmpty(token)) { onError?.Invoke("Falta el token."); yield break; }

        WWWForm form = new WWWForm();
        form.AddField("language", language);
        form.AddBinaryData("audio", audioData, "recording.wav", "audio/wav");

        using (UnityWebRequest request = UnityWebRequest.Post($"{baseUrl}/api/sessions", form))
        {
            request.SetRequestHeader("Authorization", $"Bearer {token}");
            yield return request.SendWebRequest();
            HandleResponse(request, onSuccess, onError);
        }
    }

    public IEnumerator ListSessions(int limit, string token, System.Action<SessionListResponse> onSuccess, System.Action<string> onError)
    {
        yield return SendJsonRequest($"{baseUrl}/api/sessions?limit={limit}", "GET", null, token, onSuccess, onError);
    }

    public IEnumerator GetSessionDetails(string sessionId, string token, System.Action<AnalysisResponse> onSuccess, System.Action<string> onError)
    {
        yield return SendJsonRequest($"{baseUrl}/api/sessions/{sessionId}", "GET", null, token, onSuccess, onError);
    }

    public IEnumerator DeleteSession(string sessionId, string token, System.Action<GenericServerResponse> onSuccess, System.Action<string> onError)
    {
        yield return SendJsonRequest($"{baseUrl}/api/sessions/{sessionId}", "DELETE", null, token, onSuccess, onError);
    }

    // ==========================================
    // MÉTODOS DE ENVÍO Y RESPUESTA
    // ==========================================

    private IEnumerator SendJsonRequest<T>(string url, string method, string jsonBody, string token, System.Action<T> onSuccess, System.Action<string> onError)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, method))
        {
            if (!string.IsNullOrEmpty(jsonBody))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.SetRequestHeader("Content-Type", "application/json");
            }

            request.downloadHandler = new DownloadHandlerBuffer();

            if (!string.IsNullOrEmpty(token))
            {
                request.SetRequestHeader("Authorization", $"Bearer {token}");
            }

            yield return request.SendWebRequest();
            HandleResponse(request, onSuccess, onError);
        }
    }

    private void HandleResponse<T>(UnityWebRequest request, System.Action<T> onSuccess, System.Action<string> onError)
    {
        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke($"Error servidor ({request.responseCode}): {request.downloadHandler.text}");
        }
        else
        {
            try
            {
                var responseObj = JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
                onSuccess?.Invoke(responseObj);
            }
            catch (System.Exception ex)
            {
                onError?.Invoke($"Error parseando JSON: {ex.Message}");
            }
        }
    }
}
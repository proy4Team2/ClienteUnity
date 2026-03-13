using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Singleton que gestiona el token de Firebase.
/// Referenciado por AppController, ConversationAudioRecorder y AutoLogin.
/// </summary>
public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    [Header("Firebase Web API Key")]
    public string firebaseWebApiKey = "AIzaSyBpRKS_2G4L_r6YwWsLB357DMGHqjpYQiE";

    /// <summary>Token activo tras el login. Vacío si no hay sesión.</summary>
    public string CurrentIdToken { get; private set; }
    public bool   IsLoggedIn     => !string.IsNullOrEmpty(CurrentIdToken);

    private const string SignInUrl =
        "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Autentica al usuario con email y contraseña.
    /// Callback: (bool success, string errorMessageOrToken)
    /// </summary>
    public void AuthenticateUser(string email, string password,
                                 Action<bool, string> callback)
    {
        StartCoroutine(LoginCoroutine(email, password, callback));
    }

    private IEnumerator LoginCoroutine(string email, string password,
                                       Action<bool, string> callback)
    {
        string json = $"{{\"email\":\"{email}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";

        using var req = new UnityWebRequest(SignInUrl + firebaseWebApiKey, "POST");
        req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var res = JsonUtility.FromJson<LoginResponse>(req.downloadHandler.text);
            CurrentIdToken = res.idToken;
            Debug.Log($"[AuthManager] ✅ Login OK — uid: {res.localId}");
            callback?.Invoke(true, res.idToken);
        }
        else
        {
            string error = ExtractError(req.downloadHandler.text);
            Debug.LogError($"[AuthManager] ❌ Login fallido: {error}");
            callback?.Invoke(false, error);
        }
    }

    private static string ExtractError(string json)
    {
        int idx = json.IndexOf("\"message\":", StringComparison.Ordinal);
        if (idx < 0) return json;
        int start = json.IndexOf('"', idx + 10) + 1;
        int end   = json.IndexOf('"', start);
        return json.Substring(start, end - start);
    }

    [Serializable] private class LoginResponse
    {
        public string idToken;
        public string localId;
    }
}
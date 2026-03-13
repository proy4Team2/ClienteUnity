using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Singleton que gestiona la autenticación con Firebase y la comunicación con el servidor local.
/// Asume que ya tienes el archivo de audio guardado en disco.
/// 
/// SETUP:
/// 1. Añade este script a un GameObject vacío (ej: "ApiManager").
/// 2. Rellena serverBaseUrl y firebaseWebApiKey en el Inspector.
/// 3. Llama a Login() primero, luego a CreateSession() con la ruta del archivo.
/// </summary>
public class ApiManager : MonoBehaviour
{
    public static ApiManager Instance { get; private set; }

    [Header("Servidor local")]
    public string serverBaseUrl = "http://localhost:3000";

    [Header("Firebase")]
    public string firebaseWebApiKey = "TU_WEB_API_KEY_AQUI";

    public string IdToken   { get; private set; }
    public bool   IsLoggedIn => !string.IsNullOrEmpty(IdToken);

    private const string FirebaseSignInUrl =
        "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=";

    // ──────────────────────────────────────────────
    //  Lifecycle
    // ──────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ──────────────────────────────────────────────
    //  Login
    // ──────────────────────────────────────────────

    /// <summary>
    /// Inicia sesión con email y contraseña.
    /// onSuccess devuelve el idToken. onError devuelve el mensaje de error.
    /// </summary>
    public void Login(string email, string password,
                      Action<string> onSuccess, Action<string> onError)
    {
        StartCoroutine(LoginCoroutine(email, password, onSuccess, onError));
    }

    private IEnumerator LoginCoroutine(string email, string password,
                                       Action<string> onSuccess, Action<string> onError)
    {
        string json = $"{{\"email\":\"{email}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";
        string url  = FirebaseSignInUrl + firebaseWebApiKey;

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var res = JsonUtility.FromJson<LoginResponse>(req.downloadHandler.text);
            IdToken = res.idToken;
            Debug.Log($"[ApiManager] Login OK — uid: {res.localId}");
            onSuccess?.Invoke(res.idToken);
        }
        else
        {
            string msg = ExtractFirebaseError(req.downloadHandler.text);
            Debug.LogError($"[ApiManager] Login FAILED — {msg}");
            onError?.Invoke(msg);
        }
    }

    // ──────────────────────────────────────────────
    //  Crear sesión — enviar audio ya guardado
    // ──────────────────────────────────────────────

    /// <summary>
    /// Envía el archivo de audio al servidor para transcripción y análisis.
    /// </summary>
    /// <param name="audioFilePath">Ruta local completa del archivo (ej: .wav, .m4a, .mp3)</param>
    /// <param name="language">"es" o "en"</param>
    /// <param name="onSuccess">Callback con la respuesta del servidor</param>
    /// <param name="onError">Callback con el mensaje de error</param>
    public void CreateSession(string audioFilePath, string language,
                              Action<SessionResponse> onSuccess, Action<string> onError)
    {
        if (!IsLoggedIn)
        {
            onError?.Invoke("No autenticado. Llama a Login() primero.");
            return;
        }
        StartCoroutine(CreateSessionCoroutine(audioFilePath, language, onSuccess, onError));
    }

    private IEnumerator CreateSessionCoroutine(string audioFilePath, string language,
                                               Action<SessionResponse> onSuccess,
                                               Action<string> onError)
    {
        // Leer el archivo de disco
        byte[] audioBytes;
        try
        {
            audioBytes = System.IO.File.ReadAllBytes(audioFilePath);
        }
        catch (Exception ex)
        {
            onError?.Invoke($"No se pudo leer el archivo: {ex.Message}");
            yield break;
        }

        string fileName = System.IO.Path.GetFileName(audioFilePath);
        string mimeType = GetMimeType(audioFilePath);
        string boundary = "----UnityBoundary" + Guid.NewGuid().ToString("N");

        // Construir el body multipart/form-data
        byte[] body = BuildMultipartBody(boundary, audioBytes, fileName, mimeType, language);

        using var req = new UnityWebRequest($"{serverBaseUrl}/api/sessions", "POST");
        req.uploadHandler   = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", $"multipart/form-data; boundary={boundary}");
        req.SetRequestHeader("Authorization", $"Bearer {IdToken}");

        Debug.Log($"[ApiManager] Enviando '{fileName}' ({audioBytes.Length / 1024} KB) — lang: {language}");
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var res = JsonUtility.FromJson<SessionResponse>(req.downloadHandler.text);
            Debug.Log($"[ApiManager] Sesión creada — id: {res.sessionId}");
            onSuccess?.Invoke(res);
        }
        else
        {
            Debug.LogError($"[ApiManager] Error servidor — {req.downloadHandler.text}");
            onError?.Invoke(req.downloadHandler.text);
        }
    }

    // ──────────────────────────────────────────────
    //  Otros endpoints (opcionales)
    // ──────────────────────────────────────────────

    /// <summary>Devuelve el JSON crudo con el historial de sesiones del usuario.</summary>
    public void ListSessions(int limit, Action<string> onSuccess, Action<string> onError)
    {
        if (!IsLoggedIn) { onError?.Invoke("No autenticado."); return; }
        StartCoroutine(GetCoroutine($"{serverBaseUrl}/api/sessions?limit={limit}", onSuccess, onError));
    }

    /// <summary>Devuelve el JSON crudo con el detalle de una sesión.</summary>
    public void GetSessionDetails(string sessionId, Action<string> onSuccess, Action<string> onError)
    {
        if (!IsLoggedIn) { onError?.Invoke("No autenticado."); return; }
        StartCoroutine(GetCoroutine($"{serverBaseUrl}/api/sessions/{sessionId}", onSuccess, onError));
    }

    /// <summary>Borra una sesión por ID.</summary>
    public void DeleteSession(string sessionId, Action onSuccess, Action<string> onError)
    {
        if (!IsLoggedIn) { onError?.Invoke("No autenticado."); return; }
        StartCoroutine(DeleteCoroutine($"{serverBaseUrl}/api/sessions/{sessionId}", onSuccess, onError));
    }

    // ──────────────────────────────────────────────
    //  Helpers internos
    // ──────────────────────────────────────────────

    private IEnumerator GetCoroutine(string url, Action<string> onSuccess, Action<string> onError)
    {
        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", $"Bearer {IdToken}");
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success) onSuccess?.Invoke(req.downloadHandler.text);
        else onError?.Invoke(req.downloadHandler.text);
    }

    private IEnumerator DeleteCoroutine(string url, Action onSuccess, Action<string> onError)
    {
        using var req = new UnityWebRequest(url, "DELETE");
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Authorization", $"Bearer {IdToken}");
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success) onSuccess?.Invoke();
        else onError?.Invoke(req.downloadHandler.text);
    }

    // Construye el body multipart/form-data con el campo language y el archivo de audio
    private static byte[] BuildMultipartBody(string boundary, byte[] audioBytes,
                                             string fileName, string mimeType, string language)
    {
        var ms = new System.IO.MemoryStream();

        // Campo: language
        Write(ms, $"--{boundary}\r\n"
                + $"Content-Disposition: form-data; name=\"language\"\r\n\r\n"
                + $"{language}\r\n");

        // Campo: audio (binario)
        Write(ms, $"--{boundary}\r\n"
                + $"Content-Disposition: form-data; name=\"audio\"; filename=\"{fileName}\"\r\n"
                + $"Content-Type: {mimeType}\r\n\r\n");
        ms.Write(audioBytes, 0, audioBytes.Length);
        Write(ms, "\r\n");

        // Cierre
        Write(ms, $"--{boundary}--\r\n");

        return ms.ToArray();
    }

    private static void Write(System.IO.MemoryStream ms, string text)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        ms.Write(bytes, 0, bytes.Length);
    }

    private static string GetMimeType(string path)
    {
        return System.IO.Path.GetExtension(path).ToLower() switch
        {
            ".m4a"  => "audio/mp4",
            ".mp4"  => "audio/mp4",
            ".mp3"  => "audio/mpeg",
            ".wav"  => "audio/wav",
            ".ogg"  => "audio/ogg",
            ".flac" => "audio/flac",
            ".webm" => "audio/webm",
            ".aac"  => "audio/aac",
            _       => "audio/octet-stream"
        };
    }

    private static string ExtractFirebaseError(string json)
    {
        int idx = json.IndexOf("\"message\":", StringComparison.Ordinal);
        if (idx < 0) return json;
        int start = json.IndexOf('"', idx + 10) + 1;
        int end   = json.IndexOf('"', start);
        return json.Substring(start, end - start);
    }

    // ──────────────────────────────────────────────
    //  Modelos de datos
    // ──────────────────────────────────────────────

    [Serializable] private class LoginResponse
    {
        public string idToken;
        public string localId;
    }
}

// ── Modelos públicos ──────────────────────────────

[Serializable]
public class SessionResponse
{
    public bool        success;
    public string      sessionId;
    public SessionData data;
}

[Serializable]
public class SessionData
{
    public string transcript;
    // El objeto "feedback" viene anidado; para parsearlo completo
    // usa Newtonsoft.Json (disponible en el Package Manager de Unity)
    // o crea clases adicionales que espejeen la estructura del servidor.
}
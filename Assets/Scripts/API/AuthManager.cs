using System;
using UnityEngine;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    public string CurrentIdToken { get; private set; }
    public string CurrentUserName { get; private set; }
    public bool IsLoggedIn => !string.IsNullOrEmpty(CurrentIdToken);

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── LOGIN ──
    public void AuthenticateUser(string email, string password, Action<bool, string> callback)
    {
        StartCoroutine(ApiClient.Instance.LoginUser(email, password,
            onSuccess: (response) =>
            {
                if (response.success)
                {
                    CurrentIdToken = response.token;
                    CurrentUserName = response.name;
                    Debug.Log($"[AuthManager] Login OK — Bienvenido: {response.name}");
                    callback?.Invoke(true, response.token);
                }
                else
                {
                    callback?.Invoke(false, "Login fallido.");
                }
            },
            onError: (errorMsg) =>
            {
                Debug.LogError($"[AuthManager] {errorMsg}");
                callback?.Invoke(false, errorMsg);
            }
        ));
    }

    // ── REGISTRO ──
    public void RegisterUser(string email, string password, string name, Action<bool, string> callback)
    {
        StartCoroutine(ApiClient.Instance.RegisterUser(email, password, name,
            onSuccess: (response) =>
            {
                if (response.success)
                {
                    CurrentIdToken = response.token;
                    CurrentUserName = response.name;
                    Debug.Log($"[AuthManager] Registro OK — Bienvenido: {response.name}");
                    callback?.Invoke(true, response.token);
                }
                else
                {
                    callback?.Invoke(false, "Registro fallido.");
                }
            },
            onError: (errorMsg) =>
            {
                Debug.LogError($"[AuthManager] {errorMsg}");
                callback?.Invoke(false, errorMsg);
            }
        ));
    }

    // ── CERRAR SESIÓN ──
    public void Logout()
    {
        CurrentIdToken = null;
        CurrentUserName = null;
        Debug.Log("[AuthManager] Sesión cerrada.");
    }
}
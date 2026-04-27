using UnityEngine;

/// <summary>
/// Hace login automáticamente al cargar la escena para que
/// ConversationAudioRecorder tenga token cuando termine la grabación.
///
/// SOLO PARA PRUEBAS. En producción el login debe ocurrir
/// en una escena anterior antes de llegar a la entrevista.
/// </summary>
public class AutoLogin : MonoBehaviour
{
    [Header("Credenciales de prueba")]
    public string email;
    public string password;

    private void Start()
    {
        // Si ya estamos logueados (porque venimos del menú), no hace falta re-loguear
        if (AuthManager.Instance.IsLoggedIn)
        {
            Debug.Log("[AutoLogin] Usuario ya autenticado. Saltando login automático.");
            return;
        }

        // Intentar cargar credenciales guardadas en el menú
        string savedEmail = PlayerPrefs.GetString("SavedEmail", email);
        string savedPassword = PlayerPrefs.GetString("SavedPassword", password);

        Debug.Log($"[AutoLogin] Iniciando sesión automática para: {savedEmail}...");

        AuthManager.Instance.AuthenticateUser(savedEmail, savedPassword, (success, message) =>
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (success)
                    Debug.Log("[AutoLogin] Login OK — token listo para el upload");
                else
                    Debug.LogError($"[AutoLogin] Login fallido: {message}");
            });
        });
    }
}
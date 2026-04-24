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
    public string email = "alvaro.vazquez.1716@gmail.com";
    public string password = "password123";

    private void Start()
    {
        Debug.Log("[AutoLogin] Iniciando sesión automática...");

        AuthManager.Instance.AuthenticateUser(email, password, (success, message) =>
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (success)
                    Debug.Log("[AutoLogin] ✅ Login OK — token listo para el upload");
                else
                    Debug.LogError($"[AutoLogin] ❌ Login fallido: {message}");
            });
        });
    }
}
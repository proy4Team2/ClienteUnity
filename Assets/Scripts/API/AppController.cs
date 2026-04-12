using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;

public class AppController : MonoBehaviour
{
    // Singleton para que ConversationAudioRecorder pueda encontrarlo
    public static AppController Instance { get; private set; }

    [Header("UI References")]
    public TMP_Text statusText;
    public TMP_Text feedbackText;
    public Button loginButton;
    public Button sendButton;

    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    public string testAudioFile = "sampleES.m4a"; 

    // Inicializamos el Singleton
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        statusText.text = "Ready to start...";
        feedbackText.text = "";
        sendButton.interactable = false;
        
        loginButton.onClick.AddListener(OnLoginClicked);
        sendButton.onClick.AddListener(OnSendAudioClicked);
    }

    private void OnLoginClicked()
    {
        // 1. Validamos que no estén vacíos
        if (string.IsNullOrEmpty(emailInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            UpdateStatus("Por favor, rellena todos los campos", Color.red);
            return;
        }

        UpdateStatus("Autenticando...", Color.yellow);
        loginButton.interactable = false;

        // 2. Usamos el texto que el usuario escribió en la UI
        string email = emailInput.text;
        string password = passwordInput.text;

        AuthManager.Instance.AuthenticateUser(email, password, (success, message) =>
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (success)
                {
                    UpdateStatus($"Bienvenido: {AuthManager.Instance.CurrentUserName}", Color.green);
                    sendButton.interactable = true;
                    // Opcional: Ocultar el panel de login aquí
                }
                else
                {
                    UpdateStatus($"Error: {message}", Color.red);
                    loginButton.interactable = true;
                }
            });
        });
    }

    // Enviar archivo de audio local
    private void OnSendAudioClicked()
    {
        UpdateStatus("Reading audio file...", Color.yellow);
        sendButton.interactable = false;

        byte[] audioData = LoadAudioFile();
        if (audioData == null) return;

        string token = AuthManager.Instance.CurrentIdToken;

        UpdateStatus("Uploading to server...", Color.yellow);
        
        StartCoroutine(ApiClient.Instance.UploadAudioSession(audioData, token, "es", 
            (response) => 
            {
                ProcessServerResponse(response);
            }, 
            (error) => 
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    UpdateStatus($"Upload Failed: {error}", Color.red);
                    sendButton.interactable = true;
                });
            }
        ));
    }

    // Método público que llama ConversationAudioRecorder (VR Mode)
    public void ProcessServerResponse(AnalysisResponse response)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            DisplayResults(response);
            UpdateStatus("Analysis Complete", Color.green);
            sendButton.interactable = true;
        });
    }

    private byte[] LoadAudioFile()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, testAudioFile);
        
        if (!File.Exists(filePath))
        {
            UpdateStatus($"File not found: {filePath}", Color.red);
            sendButton.interactable = true;
            return null;
        }

        return File.ReadAllBytes(filePath);
    }

    private void UpdateStatus(string message, Color color)
    {
        statusText.text = message;
        statusText.color = color;
    }

    private void DisplayResults(AnalysisResponse response)
    {
        string display = "";

        // Transcript
        display += $"<size=120%><b>Transcript:</b></size>\n<i>\"{response.data.transcript}\"</i>\n\n";
        
        // Metrics
        display += "<size=120%><b>Metrics:</b></size>\n";
        display += $"• Speed: {response.data.quality.speakingRateWPM} WPM\n";
        display += $"• Fillers: {response.data.quality.fillerPercentage}%\n";
        display += $"• Confidence: {(response.data.quality.avgConfidence * 100):F0}%\n";
        display += $"• Duration: {response.data.quality.duration:F1}s\n\n";

        // Feedback
        if (response.data.feedback.positivePoints?.Count > 0)
        {
            display += "<size=120%><color=#4CAF50><b>Good Points:</b></color></size>\n";
            foreach(var item in response.data.feedback.positivePoints)
            {
                display += $"• {item.message}\n";
            }
            display += "\n";
        }

        if (response.data.feedback.improvementAreas?.Count > 0)
        {
            display += "<size=120%><color=#FF5252><b>To Improve:</b></color></size>\n";
            foreach(var item in response.data.feedback.improvementAreas)
            {
                display += $"• {item.message}\n   <i>Tip: {item.suggestion}</i>\n";
            }
        }

        feedbackText.text = display;
    }
}
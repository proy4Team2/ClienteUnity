using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;

public class AppController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text statusText;
    public TMP_Text feedbackText;
    public Button loginButton;
    public Button sendButton;

    [Header("Test Configuration")]
    public string testEmail = "alvaro.vazquez.1716@gmail.com";
    public string testPassword = "password123";
    public string testAudioFile = "sampleES.m4a"; 

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        statusText.text = "Ready to start...";
        feedbackText.text = "";
        sendButton.interactable = false;
        
        // Add listeners
        loginButton.onClick.AddListener(OnLoginClicked);
        sendButton.onClick.AddListener(OnSendAudioClicked);
    }

    private void OnLoginClicked()
    {
        UpdateStatus("Authenticating...", Color.yellow);
        loginButton.interactable = false;

        AuthManager.Instance.AuthenticateUser(testEmail, testPassword, (success, message) =>
        {
            // Switch back to main thread
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (success)
                {
                    UpdateStatus("Login Successful!", Color.green);
                    sendButton.interactable = true;
                }
                else
                {
                    UpdateStatus($"Login Failed: {message}", Color.red);
                    loginButton.interactable = true;
                }
            });
        });
    }

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
                DisplayResults(response);
                UpdateStatus("Analysis Complete", Color.green);
                sendButton.interactable = true;
            }, 
            (error) => 
            {
                UpdateStatus($"Upload Failed: {error}", Color.red);
                sendButton.interactable = true;
            }
        ));
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
        display += $"• Confidence: {(response.data.quality.avgConfidence * 100):F0}%\n\n";

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
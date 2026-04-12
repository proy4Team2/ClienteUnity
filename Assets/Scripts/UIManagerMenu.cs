using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManagerMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject loginPanel;
    public GameObject registerPanel;

    [Header("Login Fields")]
    public TMP_InputField loginEmail;
    public TMP_InputField loginPassword;
    public Button loginBtn;

    [Header("Register Fields")]
    public TMP_InputField regName;
    public TMP_InputField regEmail;
    public TMP_InputField regPassword;
    public Button regBtn;

    [Header("Feedback")]
    public TMP_Text statusText;

    private void Start()
    {
        loginBtn.onClick.AddListener(HandleLogin);
        regBtn.onClick.AddListener(HandleRegister);
        ShowLogin();
    }

    public void ShowLogin() { loginPanel.SetActive(true); registerPanel.SetActive(false); }
    public void ShowRegister() { loginPanel.SetActive(false); registerPanel.SetActive(true); }

    private void HandleLogin()
    {
        statusText.text = "Iniciando sesión...";
        AuthManager.Instance.AuthenticateUser(loginEmail.text, loginPassword.text, (success, msg) => {
            if (success) {
                statusText.text = "¡Bienvenido!";
                UnityEngine.SceneManagement.SceneManager.LoadScene("VR_Interview_Scene");
            } else {
                statusText.text = "Error: " + msg;
            }
        });
    }

    private void HandleRegister()
    {
        statusText.text = "Creando cuenta...";
        AuthManager.Instance.RegisterUser(regEmail.text, regPassword.text, regName.text, (success, msg) => {
            if (success) {
                statusText.text = "Cuenta creada con éxito.";
                UnityEngine.SceneManagement.SceneManager.LoadScene("VR_Interview_Scene");
            } else {
                statusText.text = "Error: " + msg;
            }
        });
    }
}
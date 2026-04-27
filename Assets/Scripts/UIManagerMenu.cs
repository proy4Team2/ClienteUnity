using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManagerMenu : MonoBehaviour
{
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
    public TMP_Text statusTextLogin;
    public TMP_Text statusTextRegister;

    [Header("CAMPOS DE UI QUE EXISTEN POR UN MOTIVO **NO BORRAR**")]
    public GameObject UILogin;
    public GameObject UIRegister;
    public GameObject UIMainMenu;
    public GameObject UIDetailsMenu;
    public TMP_Text DetsTitle, DetsPeople, DetsType, DetsDescript;
    public Button DetsStart;
    public TMP_Dropdown DifSelector;
    public CameraFadeManager FadeManager;
    private string _sceneName, _sceneDif;

    private void Start()
    {
        loginBtn.onClick.AddListener(HandleLogin);
        regBtn.onClick.AddListener(HandleRegister);
        UILogin.SetActive(true);
        UIRegister.SetActive(false);
        UIMainMenu.SetActive(false);
        UIDetailsMenu.SetActive(false);
        _sceneName = "";
        _sceneDif = "FACIL";
    }

    public void HandleLogin()
    {
        statusTextLogin.text = "Iniciando sesión...";
        string email = loginEmail.text;
        string password = loginPassword.text;

        AuthManager.Instance.AuthenticateUser(email, password, (success, msg) => {
            if (success) {
                // Guardar para el autologin de la siguiente escena
                PlayerPrefs.SetString("SavedEmail", email);
                PlayerPrefs.SetString("SavedPassword", password);
                PlayerPrefs.Save();

                statusTextLogin.text = "¡Bienvenido!";
                UILogin.SetActive(false);
                UIMainMenu.SetActive(true);
            } else {
                statusTextLogin.text = "Error: " + msg;
            }
        });
    }

    public void HandleRegister()
    {
        statusTextRegister.text = "Creando cuenta...";
        string email = regEmail.text;
        string password = regPassword.text;
        string name = regName.text;

        AuthManager.Instance.RegisterUser(email, password, name, (success, msg) => {
            if (success) {
                // Guardar para el autologin de la siguiente escena
                PlayerPrefs.SetString("SavedEmail", email);
                PlayerPrefs.SetString("SavedPassword", password);
                PlayerPrefs.Save();

                statusTextRegister.text = "Cuenta creada con éxito.";
                UIRegister.SetActive(false);
                UIMainMenu.SetActive(true);
            } else {
                statusTextRegister.text = "Error: " + msg;
            }
        });
    }

    public void DetailsToMain()
    {
        UIDetailsMenu.SetActive(false);
        _sceneName = "";
        _sceneDif = "FACIL";
        UIMainMenu.SetActive(true);
    }

    public void LoginToRegister()
    {
        UILogin.SetActive(false);
        UIRegister.SetActive(true);
    }

    public void RegisterToLogin()
    {
        UIRegister.SetActive(false);
        UILogin.SetActive(true);
    }

    public void clickedButtonInterview()
    {
        UIDetailsMenu.SetActive(true);
        DetailsSetup(DetailsData.Instance.Entrevista());
        UIMainMenu.SetActive(false);
    }
    public void clickedButtonReunion()
    {
        UIDetailsMenu.SetActive(true);
        DetailsSetup(DetailsData.Instance.Reunion());
        UIMainMenu.SetActive(false);
    }

    public void DetailsSetup(List<string> data)
    {
        DetsTitle.text = data[0];
        DetsPeople.text = data[1];
        DetsType.text = data[2];
        DetsDescript.text = data[3];
        _sceneName = data[4];
    }

    public void StartExperienceButton()
    {
        _sceneDif = DifSelector.options[DifSelector.value].text;
        if (_sceneName == "") { Debug.Log("ERROR: Escena invalida seleccionada"); return; }
        Debug.Log(_sceneName + " " + _sceneDif);
        StartCoroutine(TPFadeOut());
    }

    private IEnumerator TPFadeOut()
    {
        yield return FadeManager.fadeOut();
        doTP();
    }
    private void doTP()
    {
        SceneManager.LoadScene(_sceneName + " " + _sceneDif);
    }
}
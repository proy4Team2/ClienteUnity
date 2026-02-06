using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject UILogin, UIMainMenu, UIDetailsMenu;
    [SerializeField] TMP_Text DetsTitle, DetsPeople, DetsType, DetsDescript;
    [SerializeField] Button DetsStart;
    [SerializeField] TMP_Dropdown DifSelector;
    private string _sceneName, _sceneDif;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UILogin.SetActive(true);
        UIMainMenu.SetActive(false);
        UIDetailsMenu.SetActive(false);
        _sceneName = "";
        _sceneDif = "FACIL";
    }

    public void LoginToMain()
    {
        UILogin.SetActive(false);
        UIMainMenu.SetActive(true);
    }

    public void DetailsToMain()
    {
        UIDetailsMenu.SetActive(false);
        _sceneName = "";
        _sceneDif = "FACIL";
        UIMainMenu.SetActive(true);
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
        if (_sceneName == "") { Debug.Log("ERROR: Escena inválida seleccionada"); return;}
        Debug.Log(_sceneName + " " + _sceneDif);
        SceneManager.LoadScene(_sceneName + " " + _sceneDif);
    }
}

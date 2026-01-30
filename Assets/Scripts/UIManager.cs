using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject UILogin, UIMainMenu, UIDetailsMenu;
    [SerializeField] TMP_Text DetsTitle, DetsPeople, DetsType, DetsDescript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UILogin.SetActive(true);
        UIMainMenu.SetActive(false);
        UIDetailsMenu.SetActive(false);
    }

    public void LoginToMain()
    {
        UILogin.SetActive(false);
        UIMainMenu.SetActive(true);
    }

    public void DetailsToMain()
    {
        UIDetailsMenu.SetActive(false);
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
    }
}

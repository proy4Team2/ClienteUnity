using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject UILogin, UIMainMenu, UIDetailsMenu;

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

    public void MainToDetails()
    {
        UIDetailsMenu.SetActive(true);
        UIMainMenu.SetActive(false);
    }
    public void DetailsToMain()
    {
        UIDetailsMenu.SetActive(false);
        UIMainMenu.SetActive(true);
    }
}

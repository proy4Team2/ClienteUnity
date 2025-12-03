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

    public void LoginClicked()
    {
        UILogin.SetActive(false);
        UIMainMenu.SetActive(true);
    }
}

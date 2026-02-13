using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManagerResult : MonoBehaviour
{
    [SerializeField] GameObject UIRes1, UIRes2, UIMejora;
    [SerializeField] CameraFadeManager FadeManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UIRes1.SetActive(true);
        UIRes2.SetActive(false);
        UIMejora.SetActive(false);
    }

    public void MoveRight()
    {
        if (UIRes1.activeSelf)
        {
            UIRes1.SetActive(false);
            UIRes2.SetActive(true);
        }
        else if (UIRes2.activeSelf) 
        {
            UIRes2.SetActive(false);
            UIMejora.SetActive(true);
        }
        else
        {
            UIMejora.SetActive(false);
            UIRes1.SetActive(true);
        }
    }

    public void MoveLeft()
    {
        if (UIRes1.activeSelf)
        {
            UIRes1.SetActive(false);
            UIMejora.SetActive(true);
        }
        else if (UIRes2.activeSelf) 
        {
            UIRes2.SetActive(false);
            UIRes1.SetActive(true);
        }
        else
        {
            UIMejora.SetActive(false);
            UIRes2.SetActive(true);
        }
    }

    //public void StartExperienceButton()
    //{
    //    _sceneDif = DifSelector.options[DifSelector.value].text;
    //    if (_sceneName == "") { Debug.Log("ERROR: Escena inválida seleccionada"); return;}
    //    Debug.Log(_sceneName + " " + _sceneDif);
    //    StartCoroutine(TPFadeOut());
    //}

    //private IEnumerator TPFadeOut()
    //{
    //    yield return FadeManager.fadeOut();
    //    doTP();
    //}
    //private void doTP()
    //{
    //    SceneManager.LoadScene(_sceneName + " " + _sceneDif);
    //}
}

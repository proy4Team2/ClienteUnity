@ -1,97 +1,125 @@
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.XR.CoreUtils;
using Unity.VisualScripting;


public class UIManagerResult : MonoBehaviour
{
    [SerializeField] GameObject UIRes1, UIRes2, UIMejora;
    [SerializeField] CameraFadeManager FadeManager;
    private ResultsClass results;


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
        if (response == null)
        {
            UIRes2.SetActive(false);
            UIMejora.SetActive(true);
            Debug.LogError("[UIManagerResult] La respuesta es null.");
            return;
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

    public void getJsonContents(string jsonString)
    {
        results = JsonUtility.FromJson<ResultsClass>(jsonString);
        UIRes1.transform.Find("Resumen").gameObject.GetComponent<TMP_Text>().text = "RESUMEN:" + results.summary;
        UIRes1.transform.Find("PuntosF").gameObject.GetComponent<TMP_Text>().text = "PUNTOS FUERTES\n" + results.strengths;
        UIRes1.transform.Find("PuntosD").gameObject.GetComponent<TMP_Text>().text = "PUNTOS D�BILES\n" + results.weaknesses;
        UIRes1.transform.Find("Puntuaci�n").gameObject.GetComponent<TMP_Text>().text = "PUNTUACI�N: " + results.score.ToString();

        UIRes2.transform.Find("Veredicto").gameObject.GetComponent<TMP_Text>().text = "VEREDICTO: " + (results.passed ? "Aprobado" : "No aprobado");
        UIRes2.transform.Find("Justificacion").gameObject.GetComponent<TMP_Text>().text = "JUSTIFICACI�N: " + results.decision_rationale;
        UIRes2.transform.Find("Soft Skills").gameObject.GetComponent<TMP_Text>().text = "SOFT SKILLS\n" + results.soft_skills;
        UIRes2.transform.Find("Red Flags").gameObject.GetComponent<TMP_Text>().text = "RED FLAGS\n" + results.red_flags;
        UIRes2.transform.Find("STAR").gameObject.GetComponent<TMP_Text>().text = "M�TODO STAR: " + results.star_method_check;

        UIMejora.transform.Find("CortoPText").gameObject.GetComponent<TMP_Text>().text = results.immediate_action;
        UIMejora.transform.Find("LargoPtext").gameObject.GetComponent<TMP_Text>().text = results.long_term_advice;
    }

    //public void StartExperienceButton()
    //{
    //    _sceneDif = DifSelector.options[DifSelector.value].text;
    //    if (_sceneName == "") { Debug.Log("ERROR: Escena inv�lida seleccionada"); return;}
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
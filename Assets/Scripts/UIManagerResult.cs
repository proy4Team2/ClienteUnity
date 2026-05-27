using UnityEngine;
using TMPro;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement; // Importante usar Newtonsoft aquí

public class UIManagerResult : MonoBehaviour
{
    [Header("Pantallas")]
    [SerializeField] GameObject UIResumen;
    [SerializeField] GameObject UIRes1;
    [SerializeField] GameObject UIRes2;
    [SerializeField] GameObject UIMejora;

    [Header("Otros elementos")]
    [SerializeField] GameObject ErrorMSG;
    [SerializeField] Button sideButtonL, sideButtonR;
    [SerializeField] CameraFadeManager FadeManager;

    private int _currentScreen;

    // Cambiamos ResultsClass por el AnalysisResponse de DataModels.cs
    private AnalysisResponse analysisResult; 

    void Start()
    {

        if (AppController.Instance != null)
        {            
            analysisResult = AppController.Instance.resp;
            Debug.Log(analysisResult.ToString());

            _currentScreen = 0;

            UIResumen.GetComponent<RectTransform>().localScale = Vector3.one;
            UIResumen.GetComponent<RectTransform>().localScale = Vector3.zero;
            UIRes1.GetComponent<RectTransform>().localScale = Vector3.zero;
            UIRes2.GetComponent<RectTransform>().localScale = Vector3.zero;
            UIMejora.GetComponent<RectTransform>().localScale = Vector3.zero;
            ErrorMSG.SetActive(false);

            getResults();
        } else {
            UIResumen.GetComponent<RectTransform>().localScale = Vector3.zero;
            UIRes1.GetComponent<RectTransform>().localScale = Vector3.zero;
            UIRes2.GetComponent<RectTransform>().localScale = Vector3.zero;
            UIMejora.GetComponent<RectTransform>().localScale = Vector3.zero;
            ErrorMSG.SetActive(true);

            sideButtonL.enabled = false;
            sideButtonR.enabled = false;
        }
    }

    public void MoveRight()
    {
        switch (_currentScreen)
        {
            case 0:
                UIResumen.transform.localScale = Vector3.zero;
                UIRes1.transform.localScale = Vector3.one;
                break;
            case 1:
                UIRes1.transform.localScale = Vector3.zero;
                UIRes2.transform.localScale = Vector3.one;
                break;
            case 2:
                UIRes2.transform.localScale = Vector3.zero;
                UIMejora.transform.localScale = Vector3.one;
                break;
            case 3:
                UIMejora.transform.localScale = Vector3.zero;
                UIResumen.transform.localScale = Vector3.one;
                break;
        }

        _currentScreen++;
        if (_currentScreen > 3) 
            _currentScreen = 0;
    }

    public void MoveLeft()
    {
        switch (_currentScreen)
        {
            case 0:
                UIResumen.GetComponent<RectTransform>().localScale = Vector3.zero;
                UIResumen.transform.localScale = Vector3.zero;
                UIMejora.transform.localScale = Vector3.one;
                break;
            case 1:
                UIRes1.transform.localScale = Vector3.zero;
                UIResumen.transform.localScale = Vector3.one;
                break;
            case 2:
                UIRes2.transform.localScale = Vector3.zero;
                UIRes1.transform.localScale = Vector3.one;
                break;
            case 3:
                UIMejora.transform.localScale = Vector3.zero;
                UIRes2.transform.localScale = Vector3.one;
                break;
        }

        _currentScreen++;
        if (_currentScreen < 0)
            _currentScreen = 3;
    }


    //public void MoveRight() { /* Tu lógica de UI actual se mantiene igual */ }
    //public void MoveLeft() { /* Tu lógica de UI actual se mantiene igual */ }
    
    public void getResults()
    {
        // Atajos para leer más fácil
        var quality = analysisResult.data.quality;
        var oratory = analysisResult.data.feedback.oratory_expert;
        var recruiter = analysisResult.data.feedback.recruiter_verdict;
        var plan = analysisResult.data.feedback.improvement_plan;

        Debug.Log("CHECK: " + UIResumen.transform.Find("Confidence").GetComponent<TMP_Text>().text);
        Debug.Log("CHECK: " + UIRes1.transform.Find("Puntuacion").GetComponent<TMP_Text>().text);
        Debug.Log("CHECK: " + (oratory != null ? oratory.score.ToString() : "N/A"));

        //UI 0: Resumen de calidad
        UIResumen.transform.Find("PercentFiller").GetComponent<TMP_Text>().text = "Porcentaje de \"palabras relleno\": " + quality.fillerPercentage.ToString();
        UIResumen.transform.Find("PercentPause").GetComponent<TMP_Text>().text = "Porcentaje de pausas: " + quality.pausePercentage.ToString();
        UIResumen.transform.Find("Confidence").GetComponent<TMP_Text>().text = "Puntuación de confianza: " + quality.avgConfidence.ToString();
        UIResumen.transform.Find("Duration").GetComponent<TMP_Text>().text = "Duración de la conversación:" + quality.duration.ToString();

        // UI 1: Oratoria
        Debug.Log("CHECK " + UIRes1.activeSelf);
        UIRes1.transform.Find("Puntuacion").GetComponent<TMP_Text>().text = "PUNTUACIÓN: " + (oratory != null ? oratory.score.ToString() + "/100" : "N/A");
        UIRes1.transform.Find("Resumen").GetComponent<TMP_Text>().text = "RESUMEN:\n" + (oratory != null ? oratory.summary : "N/A");
        UIRes1.transform.Find("PuntosF").GetComponent<TMP_Text>().text = "PUNTOS FUERTES\n- " + (oratory != null ? string.Join("\n- ", oratory.strengths) : "N/A");
        UIRes1.transform.Find("PuntosD").GetComponent<TMP_Text>().text = "PUNTOS DÉBILES\n- " + (oratory != null ? string.Join("\n- ", oratory.weaknesses) : "N/A");
        UIRes1.transform.Find("WPM").GetComponent<TMP_Text>().text = "Palabras por minuto (WPM): " + quality.speakingRateWPM.ToString();

        // UI 2: Reclutador
        UIRes2.transform.Find("Veredicto").GetComponent<TMP_Text>().text = "VEREDICTO: " + (recruiter != null ? (recruiter.passed ? "Aprobado" : "No aprobado") : "N/A");
        UIRes2.transform.Find("Justificacion").GetComponent<TMP_Text>().text = "JUSTIFICACIÓN:\n" + (recruiter != null ? recruiter.decision_rationale : "N/A");
        UIRes2.transform.Find("Soft Skills").GetComponent<TMP_Text>().text = "SOFT SKILLS\n- " + (recruiter != null ? string.Join("\n- ", recruiter.soft_skills) : "N/A");
        UIRes2.transform.Find("Red Flags").GetComponent<TMP_Text>().text = "RED FLAGS\n- " + (recruiter != null ? string.Join("\n- ", recruiter.red_flags) : "N/A");
        UIRes2.transform.Find("STAR").GetComponent<TMP_Text>().text = "MÉTODO STAR:\n" + (recruiter != null ? recruiter.star_method_check : "N/A");

        // UI 3: Mejoras
        UIMejora.transform.Find("CortoPText").GetComponent<TMP_Text>().text = (plan != null ? plan.immediate_action : "N/A");
        UIMejora.transform.Find("LargoPText").GetComponent<TMP_Text>().text = (plan != null ? plan.long_term_advice : "N/A");
    }

    public void returnToMenu()
    {
        //Cambiar de escena una vez hecho esto
        StartCoroutine(TPFadeOut());
    }
    private IEnumerator TPFadeOut()
    {
        yield return FadeManager.fadeOut();
        SceneManager.LoadScene("StartMenu");
    }
}
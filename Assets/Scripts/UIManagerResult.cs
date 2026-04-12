using UnityEngine;
using TMPro;
using Newtonsoft.Json; // Importante usar Newtonsoft aquí

public class UIManagerResult : MonoBehaviour
{
    [SerializeField] GameObject UIRes1, UIRes2, UIMejora;
    [SerializeField] CameraFadeManager FadeManager;
    
    // Cambiamos ResultsClass por el AnalysisResponse de DataModels.cs
    private AnalysisResponse analysisResult; 

    void Start()
    {
        UIRes1.SetActive(true);
        UIRes2.SetActive(false);
        UIMejora.SetActive(false);
    }

    public void MoveRight() { /* Tu lógica de UI actual se mantiene igual */ }
    public void MoveLeft() { /* Tu lógica de UI actual se mantiene igual */ }

    // Cambiamos esto para que reciba el objeto ya parseado desde el recorder o lo parsee con Newtonsoft
    public void getJsonContents(string jsonString)
    {
        // Usar Newtonsoft en lugar de JsonUtility porque tienes JSON anidados
        analysisResult = JsonConvert.DeserializeObject<AnalysisResponse>(jsonString);
        
        // Atajos para leer más fácil
        var oratory = analysisResult.data.feedback.oratory_expert;
        var recruiter = analysisResult.data.feedback.recruiter_verdict;
        var plan = analysisResult.data.feedback.improvement_plan;

        // UI 1: Oratoria
        UIRes1.transform.Find("Resumen").GetComponent<TMP_Text>().text = "RESUMEN:\n" + oratory.summary;
        UIRes1.transform.Find("PuntosF").GetComponent<TMP_Text>().text = "PUNTOS FUERTES\n- " + string.Join("\n- ", oratory.strengths);
        UIRes1.transform.Find("PuntosD").GetComponent<TMP_Text>().text = "PUNTOS DÉBILES\n- " + string.Join("\n- ", oratory.weaknesses);
        UIRes1.transform.Find("Puntuación").GetComponent<TMP_Text>().text = "PUNTUACIÓN: " + oratory.score.ToString() + "/100";

        // UI 2: Reclutador
        UIRes2.transform.Find("Veredicto").GetComponent<TMP_Text>().text = "VEREDICTO: " + (recruiter.passed ? "Aprobado" : "No aprobado");
        UIRes2.transform.Find("Justificacion").GetComponent<TMP_Text>().text = "JUSTIFICACIÓN:\n" + recruiter.decision_rationale;
        UIRes2.transform.Find("Soft Skills").GetComponent<TMP_Text>().text = "SOFT SKILLS\n- " + string.Join("\n- ", recruiter.soft_skills);
        UIRes2.transform.Find("Red Flags").GetComponent<TMP_Text>().text = "RED FLAGS\n- " + string.Join("\n- ", recruiter.red_flags);
        UIRes2.transform.Find("STAR").GetComponent<TMP_Text>().text = "MÉTODO STAR:\n" + recruiter.star_method_check;

        // UI 3: Mejoras
        UIMejora.transform.Find("CortoPText").GetComponent<TMP_Text>().text = plan.immediate_action;
        UIMejora.transform.Find("LargoPtext").GetComponent<TMP_Text>().text = plan.long_term_advice;
    }
}
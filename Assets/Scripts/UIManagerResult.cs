using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Gestiona la pantalla de resultados tras recibir la respuesta del servidor.
/// Asigna los campos en el Inspector y llama a DisplayResults(response) desde
/// Conversationaudiorecorder o AppController.
/// </summary>
public class UIManagerResult : MonoBehaviour
{
    // ── Panel raíz (opcional: para activar/desactivar la pantalla) ──
    [Header("Panel")]
    public GameObject resultsPanel;

    // ── Transcript ──────────────────────────────────────────────────
    [Header("Transcript")]
    public TMP_Text transcriptText;

    // ── Oratory Expert ──────────────────────────────────────────────
    [Header("Oratory Expert")]
    public TMP_Text oratoryScoreText;
    public TMP_Text oratorySummaryText;
    public TMP_Text oratoryStrengthsText;
    public TMP_Text oratoryWeaknessesText;
    public TMP_Text oratoryPacingText;

    // ── Recruiter Verdict ───────────────────────────────────────────
    [Header("Recruiter Verdict")]
    public TMP_Text recruiterPassedText;
    public TMP_Text recruiterRationaleText;
    public TMP_Text recruiterStarText;
    public TMP_Text recruiterSoftSkillsText;
    public TMP_Text recruiterRedFlagsText;

    // ── Improvement Plan ────────────────────────────────────────────
    [Header("Improvement Plan")]
    public TMP_Text improvementImmediateText;
    public TMP_Text improvementLongTermText;

    // ── Session meta ─────────────────────────────────────────────────
    [Header("Session Info")]
    public TMP_Text sessionIdText;

    // ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Punto de entrada principal. Llama a este método con la respuesta del servidor.
    /// </summary>
    public void DisplayResults(AnalysisResponse response)
    {
        if (response == null)
        {
            Debug.LogError("[UIManagerResult] La respuesta es null.");
            return;
        }

        Debug.Log("[UIManagerResult] Respuesta completa:\n" +
                  Newtonsoft.Json.JsonConvert.SerializeObject(response,
                      Newtonsoft.Json.Formatting.Indented));

        if (resultsPanel != null)
            resultsPanel.SetActive(true);

        SetText(sessionIdText, $"Session ID: {response.sessionId}");

        var data = response.data;
        if (data == null) { Debug.LogWarning("[UIManagerResult] data es null."); return; }

        // ── Transcript ──
        SetText(transcriptText, data.transcript);

        // ── Oratory Expert ──
        var oe = data.feedback?.oratory_expert;
        if (oe != null)
        {
            SetText(oratoryScoreText,    $"{oe.score} / 100");
            SetText(oratorySummaryText,  oe.summary);
            SetText(oratoryStrengthsText,  BuildBulletList(oe.strengths));
            SetText(oratoryWeaknessesText, BuildBulletList(oe.weaknesses));
            SetText(oratoryPacingText,   oe.pacing_feedback);
        }

        // ── Recruiter Verdict ──
        var rv = data.feedback?.recruiter_verdict;
        if (rv != null)
        {
            bool passed = rv.passed;
            if (recruiterPassedText != null)
            {
                recruiterPassedText.text  = passed ? "✅ APTO" : "❌ NO APTO";
                recruiterPassedText.color = passed ? Color.green : Color.red;
            }
            SetText(recruiterRationaleText,  rv.decision_rationale);
            SetText(recruiterStarText,       rv.star_method_check);
            SetText(recruiterSoftSkillsText, BuildBulletList(rv.soft_skills));
            SetText(recruiterRedFlagsText,   BuildBulletList(rv.red_flags));
        }

        // ── Improvement Plan ──
        var ip = data.feedback?.improvement_plan;
        if (ip != null)
        {
            SetText(improvementImmediateText, ip.immediate_action);
            SetText(improvementLongTermText,  ip.long_term_advice);
        }
    }

    // ── Helpers ─────────────────────────────────────────────────────

    private static void SetText(TMP_Text field, string value)
    {
        if (field != null)
            field.text = string.IsNullOrEmpty(value) ? "—" : value;
    }

    private static string BuildBulletList(System.Collections.Generic.List<string> items)
    {
        if (items == null || items.Count == 0) return "—";
        var sb = new System.Text.StringBuilder();
        foreach (var item in items)
            sb.AppendLine($"• {item}");
        return sb.ToString().TrimEnd();
    }
}
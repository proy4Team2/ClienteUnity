using System;
using System.Collections.Generic;
using Newtonsoft.Json;

// ── Respuesta raíz ────────────────────────────────────────────────
[Serializable]
public class AnalysisResponse
{
    [JsonProperty("success")]   public bool         success;
    [JsonProperty("sessionId")] public string       sessionId;
    [JsonProperty("data")]      public AnalysisData data;
}

// ── data ──────────────────────────────────────────────────────────
[Serializable]
public class AnalysisData
{
    [JsonProperty("transcript")] public string          transcript;
    [JsonProperty("feedback")]   public ServerFeedback  feedback;

    // AppController usa response.data.quality — lo construimos
    // desde las métricas que devuelve el servidor dentro del feedback
    public QualityMetrics quality => new QualityMetrics
    {
        speakingRateWPM  = feedback?.oratory_expert?.score ?? 0,
        fillerPercentage = 0f,
        pausePercentage  = 0f,
        avgConfidence    = feedback?.oratory_expert?.score / 100f ?? 0f,
        duration         = 0f
    };
}

// ── Estructura real del servidor ──────────────────────────────────
[Serializable]
public class ServerFeedback
{
    [JsonProperty("oratory_expert")]    public OratoryExpert    oratory_expert;
    [JsonProperty("recruiter_verdict")] public RecruiterVerdict recruiter_verdict;
    [JsonProperty("improvement_plan")]  public ImprovementPlan  improvement_plan;

    // ── Alias que usa AppController.DisplayResults() ──────────────

    public List<FeedbackItem> positivePoints
    {
        get
        {
            var list = new List<FeedbackItem>();
            if (oratory_expert?.strengths == null) return list;
            foreach (var s in oratory_expert.strengths)
                list.Add(new FeedbackItem { area = "Oratoria", message = s, suggestion = "" });
            return list;
        }
    }

    public List<FeedbackItem> improvementAreas
    {
        get
        {
            var list = new List<FeedbackItem>();
            if (oratory_expert?.weaknesses == null) return list;
            string tip = improvement_plan?.immediate_action ?? "";
            foreach (var w in oratory_expert.weaknesses)
                list.Add(new FeedbackItem { area = "Mejora", message = w, suggestion = tip });
            return list;
        }
    }
}

[Serializable]
public class OratoryExpert
{
    [JsonProperty("score")]           public int          score;
    [JsonProperty("summary")]         public string       summary;
    [JsonProperty("strengths")]       public List<string> strengths;
    [JsonProperty("weaknesses")]      public List<string> weaknesses;
    [JsonProperty("pacing_feedback")] public string       pacing_feedback;
}

[Serializable]
public class RecruiterVerdict
{
    [JsonProperty("passed")]             public bool         passed;
    [JsonProperty("decision_rationale")] public string       decision_rationale;
    [JsonProperty("star_method_check")]  public string       star_method_check;
    [JsonProperty("soft_skills")]        public List<string> soft_skills;
    [JsonProperty("red_flags")]          public List<string> red_flags;
}

[Serializable]
public class ImprovementPlan
{
    [JsonProperty("immediate_action")] public string immediate_action;
    [JsonProperty("long_term_advice")] public string long_term_advice;
}

// ── Modelos que ya usaba AppController (se mantienen igual) ───────
[Serializable]
public class QualityMetrics
{
    public float speakingRateWPM;
    public float fillerPercentage;
    public float pausePercentage;
    public float avgConfidence;
    public float duration;
}

[Serializable]
public class FeedbackItem
{
    public string area;
    public string message;
    public string suggestion;
}
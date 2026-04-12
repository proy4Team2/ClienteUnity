using System;
using System.Collections.Generic;
using Newtonsoft.Json;

// ==================================================================
// MODELOS DE ANÁLISIS DE ENTREVISTA
// ==================================================================

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
    [JsonProperty("metrics")]    public TechnicalMetrics metrics; 
    [JsonProperty("feedback")]   public ServerFeedback  feedback;

    public QualityMetrics quality => new QualityMetrics
    {
        speakingRateWPM  = metrics?.wpm ?? 0,
        fillerPercentage = 0f,
        pausePercentage  = metrics?.pause_percentage ?? 0f,
        avgConfidence    = (float)(metrics?.average_confidence ?? 0f),
        duration         = metrics?.duration_seconds ?? 0f
    };
}

[Serializable]
public class TechnicalMetrics
{
    [JsonProperty("duration_seconds")]  public float duration_seconds;
    [JsonProperty("word_count")]       public int   word_count;
    [JsonProperty("wpm")]              public float wpm;
    [JsonProperty("pause_percentage")] public float pause_percentage;
    [JsonProperty("average_confidence")] public float average_confidence;
}

// ── Estructura del servidor ──────────────────────────────────
[Serializable]
public class ServerFeedback
{
    [JsonProperty("oratory_expert")]    public OratoryExpert    oratory_expert;
    [JsonProperty("recruiter_verdict")] public RecruiterVerdict recruiter_verdict;
    [JsonProperty("improvement_plan")]  public ImprovementPlan  improvement_plan;

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


// ==================================================================
// MODELS (AUTENTICACIÓN, PERFIL Y SESIONES GENERALES)
// ==================================================================

[Serializable]
public class AuthServerResponse 
{
    [JsonProperty("success")] public bool success;
    [JsonProperty("uid")]     public string uid;
    [JsonProperty("email")]   public string email;
    [JsonProperty("name")]    public string name;
    [JsonProperty("token")]   public string token;
    [JsonProperty("stats")]   public DashboardStats stats; 
}

[Serializable]
public class DashboardStats
{
    [JsonProperty("totalSessions")]  public int totalSessions;
    [JsonProperty("averageScore")]   public float averageScore;
    [JsonProperty("sessionsPassed")] public int sessionsPassed;
    [JsonProperty("averageWpm")]     public float averageWpm;
    [JsonProperty("lastSessionDate")]public string lastSessionDate;
}

[Serializable]
public class UserProfileResponse
{
    [JsonProperty("success")] public bool success;
    [JsonProperty("data")]    public UserProfileData data;
}

[Serializable]
public class UserProfileData
{
    [JsonProperty("uid")]   public string uid;
    [JsonProperty("email")] public string email;
    [JsonProperty("name")]  public string name;
    [JsonProperty("dashboardStats")] public DashboardStats dashboardStats;
}

[Serializable]
public class SessionListResponse
{
    [JsonProperty("success")] public bool success;
    [JsonProperty("data")]    public List<AnalysisData> data; 
}

[Serializable]
public class GenericServerResponse
{
    [JsonProperty("success")] public bool success;
    [JsonProperty("message")] public string message;
}
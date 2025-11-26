using System;
using System.Collections.Generic;

[Serializable]
public class AnalysisResponse
{
    public bool success;
    public string sessionId;
    public AnalysisData data;
}

[Serializable]
public class AnalysisData
{
    public string transcript;
    public QualityMetrics quality;
    public Feedback feedback;
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
public class Feedback
{
    public List<FeedbackItem> positivePoints;
    public List<FeedbackItem> improvementAreas;
}

[Serializable]
public class FeedbackItem
{
    public string area;
    public string message;
    public string suggestion;
}
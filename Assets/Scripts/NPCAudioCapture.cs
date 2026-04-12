using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class NpcAudioCapture : MonoBehaviour
{
    private ConversationAudioRecorder _recorder;
    private int _outputSampleRate;

    private void Start()
    {
        _recorder = FindObjectOfType<ConversationAudioRecorder>();
        _outputSampleRate = AudioSettings.outputSampleRate;

        if (_recorder == null)
        {
            Debug.LogWarning("[NpcAudioCapture] No se encontró ConversationAudioRecorder en la escena.");
        }
        else
        {
            Debug.Log($"[NpcAudioCapture] Conectado a ConversationAudioRecorder. SampleRate: {_outputSampleRate} Hz");
        }
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (_recorder == null || !_recorder.IsRecording) return;

        // Si el audio es estéreo, lo mezclamos a mono antes de enviarlo
        float[] mono = channels == 1 ? data : StereoToMono(data);

        _recorder.WriteNpcSamples(mono, _outputSampleRate);
    }

    private static float[] StereoToMono(float[] stereo)
    {
        int monoLength = stereo.Length / 2;
        float[] mono = new float[monoLength];
        for (int i = 0; i < monoLength; i++)
        {
            mono[i] = (stereo[i * 2] + stereo[i * 2 + 1]) * 0.5f;
        }
        return mono;
    }
}
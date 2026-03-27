using UnityEngine;

/// <summary>
/// Adjunta este componente al mismo GameObject que tiene el AudioSource del NPC de Convai.
/// Intercepta el audio en tiempo real con OnAudioFilterRead y lo reenvía al
/// Conversationaudiorecorder activo en la escena.
///
/// SETUP:
/// 1. Busca el GameObject del NPC que tiene el componente AudioSource.
/// 2. Añádele este script (Add Component → NpcAudioCapture).
/// 3. Listo. Se autoconecta a Conversationaudiorecorder al iniciar.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class NpcAudioCapture : MonoBehaviour
{
    // El recorder al que se enviarán las muestras del NPC
    private Conversationaudiorecorder _recorder;
    private int _outputSampleRate;

    private void Start()
    {
        _recorder = FindObjectOfType<Conversationaudiorecorder>();
        _outputSampleRate = AudioSettings.outputSampleRate;

        if (_recorder == null)
            Debug.LogWarning("[NpcAudioCapture] No se encontró Conversationaudiorecorder en la escena.");
        else
            Debug.Log($"[NpcAudioCapture] Conectado a Conversationaudiorecorder. SampleRate: {_outputSampleRate} Hz");
    }

    /// <summary>
    /// Unity llama a este método automáticamente en el hilo de audio cada vez que
    /// el AudioSource reproduce muestras. Operamos en modo pass-through (no modificamos
    /// el audio) y enviamos una copia al recorder.
    /// </summary>
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
            mono[i] = (stereo[i * 2] + stereo[i * 2 + 1]) * 0.5f;
        return mono;
    }
}
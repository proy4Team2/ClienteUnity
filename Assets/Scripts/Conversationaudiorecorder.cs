using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Convai.Scripts.Runtime.Core;

public class Conversationaudiorecorder : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Frecuencia de muestreo del WAV final (Hz). 44100 es estándar.")]
    public int sampleRate = 44100;

    [Tooltip("Duración máxima de la grabación en segundos.")]
    public int maxDurationSec = 3600; // 1 hora

    private float[] _masterBuffer;
    private int _bufferWriteIndex = 0;
    private float _startTime;
    private bool _isRecording = false;
    private string _exportPath;
    
    [Header("Automatización")]
    [Tooltip("¿Subir automáticamente al servidor al detener la grabación?")]
    public bool autoUploadOnStop = true;

    private void Start()
    {
        StartRecording();
    }

    private void Update()
    {
        // ATAJO DE TECLADO: Pulsa la tecla 'U' para subir manualmente
        if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log("[ConversationRecorder] Tecla 'U' detectada. Forzando subida...");
            StopAndUploadInternal();
        }
    }

    private void OnEnable()
    {
        ConvaiGRPCAPI.OnPlayerAudioCaptured += HandlePlayerAudio;
        ConvaiGRPCAPI.OnNPCAudioCaptured += HandleNpcAudio;
    }

    private void OnDisable()
    {
        ConvaiGRPCAPI.OnPlayerAudioCaptured -= HandlePlayerAudio;
        ConvaiGRPCAPI.OnNPCAudioCaptured -= HandleNpcAudio;
    }

    public void StartRecording()
    {
        if (_isRecording) return;

        Debug.Log("[ConversationRecorder] ▶ Iniciando grabación de conversación...");
        _masterBuffer = new float[sampleRate * maxDurationSec];
        _bufferWriteIndex = 0;
        _startTime = Time.time;
        _isRecording = true;
    }

    private void HandlePlayerAudio(float[] samples, int rate)
    {
        if (!_isRecording) return;
        if (samples != null && samples.Length > 0)
        {
            // Debug.Log($"[ConversationRecorder] Capturado audio del jugador: {samples.Length} muestras.");
            WriteToBuffer(Resample(samples, rate, sampleRate));
        }
    }

    private void HandleNpcAudio(float[] samples, int rate)
    {
        if (!_isRecording) return;
        WriteToBuffer(Resample(samples, rate, sampleRate));
    }

    private void WriteToBuffer(float[] samples)
    {
        // Calculamos la posición basada en el tiempo real para mantener el ritmo
        int targetIndex = (int)((Time.time - _startTime) * sampleRate);
        
        if (targetIndex < _bufferWriteIndex) targetIndex = _bufferWriteIndex;

        for (int i = 0; i < samples.Length; i++)
        {
            int idx = targetIndex + i;
            if (idx < _masterBuffer.Length)
            {
                _masterBuffer[idx] = Mathf.Clamp(_masterBuffer[idx] + samples[i], -1f, 1f);
                if (idx > _bufferWriteIndex) _bufferWriteIndex = idx;
            }
        }
    }

    [Header("Configuración de Servidor")]
    [Tooltip("Idioma de la conversación para el análisis.")]
    public string language = "es";

    public void StopAndUploadButton()
    {
        StopAndUploadInternal();
    }

    private void StopAndUploadInternal()
    {
        string path = StopAndSave();
        if (string.IsNullOrEmpty(path)) return;

        byte[] wavData = File.ReadAllBytes(path);
        string token = AuthManager.Instance.CurrentIdToken;

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("[ConversationRecorder] ❌ No hay token de sesión. Debes estar logueado.");
            return;
        }

        Debug.Log("[ConversationRecorder] 📤 Subiendo audio al servidor...");
        // Usamos la instancia de ApiClient para que la corrutina persista aunque este objeto se destruya
        ApiClient.Instance.StartCoroutine(ApiClient.Instance.UploadAudioSession(
            wavData, 
            token, 
            language, 
            (response) => {
                Debug.Log("[ConversationRecorder] ✅ Subida completada con éxito. ID Sesión: " + response.sessionId);
            },
            (error) => {
                Debug.LogError("[ConversationRecorder] ❌ Error en la subida: " + error);
            }
        ));
    }

    public string StopAndSave()
    {
        if (!_isRecording) 
        {
            Debug.LogWarning("[ConversationRecorder] ⚠️ StopAndSave llamado pero no se estaba grabando.");
            return null;
        }
        _isRecording = false;

        if (_bufferWriteIndex <= 0)
        {
            Debug.LogError("[ConversationRecorder] ❌ No se capturó NADA de audio. ¿Pulsaste 'Start' y hablaste con el NPC?");
            return null;
        }

        // Truncar al tamaño real
        float[] finalSamples = new float[_bufferWriteIndex + 1];
        Array.Copy(_masterBuffer, finalSamples, _bufferWriteIndex + 1);

        byte[] wavData = EncodeWav(finalSamples, sampleRate, 1);
        string fileName = $"Convai_Conversation_{DateTime.Now:yyyyMMdd_HHmmss}.wav";
        _exportPath = Path.Combine(Application.persistentDataPath, fileName);

        try
        {
            File.WriteAllBytes(_exportPath, wavData);
            Debug.Log($"[ConversationRecorder] ✅ WAV guardado: {_exportPath} ({wavData.Length} bytes, {finalSamples.Length / (float)sampleRate:F1}s)");
        }
        catch (Exception e)
        {
            Debug.LogError($"[ConversationRecorder] Error al guardar WAV: {e.Message}");
            return null;
        }

        return _exportPath;
    }

    private void OnDestroy()
    {
        if (_isRecording) 
        {
            if (autoUploadOnStop) StopAndUploadInternal();
            else StopAndSave();
        }
    }

    private float[] Resample(float[] input, int fromRate, int toRate)
    {
        if (fromRate == toRate || input.Length == 0) return input;
        int outLen = Mathf.RoundToInt((float)input.Length * toRate / fromRate);
        float[] output = new float[outLen];
        float ratio = (float)(input.Length - 1) / Mathf.Max(outLen - 1, 1);
        for (int i = 0; i < outLen; i++)
        {
            float pos = i * ratio;
            int lo = (int)pos;
            float t = pos - lo;
            float a = input[lo];
            float b = (lo + 1 < input.Length) ? input[lo + 1] : a;
            output[i] = a + t * (b - a);
        }
        return output;
    }

    private byte[] EncodeWav(float[] samples, int rate, int channels)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.UTF8))
            {
                int bitsPerSample = 16;
                int bytesPerSample = bitsPerSample / 8;
                int dataSize = samples.Length * bytesPerSample;

                // 1. Chunk ID: "RIFF"
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                // 2. Chunk Size: 36 + dataSize
                writer.Write(36 + dataSize);
                // 3. Format: "WAVE"
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

                // 4. Sub-chunk 1 ID: "fmt "
                writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                // 5. Sub-chunk 1 Size: 16 (for PCM)
                writer.Write(16);
                // 6. Audio Format: 1 (PCM)
                writer.Write((short)1);
                // 7. Num Channels: 1 (Mono)
                writer.Write((short)channels);
                // 8. Sample Rate
                writer.Write(rate);
                // 9. Byte Rate: rate * channels * bytesPerSample
                writer.Write(rate * channels * bytesPerSample);
                // 10. Block Align: channels * bytesPerSample
                writer.Write((short)(channels * bytesPerSample));
                // 11. Bits Per Sample
                writer.Write((short)bitsPerSample);

                // 12. Sub-chunk 2 ID: "data"
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                // 13. Sub-chunk 2 Size
                writer.Write(dataSize);

                // 14. Audio Data
                for (int i = 0; i < samples.Length; i++)
                {
                    writer.Write((short)(Mathf.Clamp(samples[i], -1f, 1f) * short.MaxValue));
                }
                
                writer.Flush();
                return stream.ToArray();
            }
        }
    }
}
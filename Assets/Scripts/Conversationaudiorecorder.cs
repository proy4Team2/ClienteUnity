using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Convai.Scripts.Runtime.Core;
using Newtonsoft.Json;

public class ConversationAudioRecorder : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Frecuencia de muestreo del WAV final (Hz). 44100 es estándar.")]
    public int sampleRate = 44100;

    [Tooltip("Duración máxima de la grabación en segundos.")]
    public int maxDurationSec = 600; // 1 hora

    [Header("Automatización y Servidor")]
    public bool autoUploadOnStop = true;
    public string language = "es";

    // ── Variables de Buffer y Sincronización ──
    private float[] _playerBuffer;
    private float[] _npcBuffer;
    private int _playerWriteIndex = 0;
    private int _npcWriteIndex = 0;
    private System.Diagnostics.Stopwatch _recordingStopwatch = new System.Diagnostics.Stopwatch();
    
    private bool _isRecording = false;
    private readonly object _lock = new object();
    
    public bool IsRecording => _isRecording;

    // ── Lifecycle ─────────────────────────────────────────────────────
    private void OnEnable()
    {
        ConvaiGRPCAPI.OnPlayerAudioCaptured += HandlePlayerAudio;
    }

    private void OnDisable()
    {
        ConvaiGRPCAPI.OnPlayerAudioCaptured -= HandlePlayerAudio;
    }

    private void Update()
    {
        // ATAJO DE TECLADO: Pulsa 'U' para detener y subir manualmente
        if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log("[ConversationRecorder] Tecla 'U' detectada. Forzando subida...");
            StopAndUpload();
        }
    }

    private void OnDestroy()
    {
        // Si se destruye el objeto mientras graba, salvamos y subimos automáticamente
        if (_isRecording && autoUploadOnStop)
        {
            StopAndUpload();
        }
    }

    // ── Control de Grabación ──────────────────────────────────────────
    public void StartRecording()
    {
        if (_isRecording) return;
        
        Debug.Log("[ConversationRecorder] ▶ Iniciando grabación en estéreo (Jugador + NPC)...");
        _playerBuffer = new float[sampleRate * maxDurationSec];
        _npcBuffer = new float[sampleRate * maxDurationSec];
        
        _playerWriteIndex = 0;
        _npcWriteIndex = 0;
        
        _recordingStopwatch.Restart();
        _isRecording = true;
    }

    public void StopAndUpload()
    {
        if (!_isRecording) return;
        _isRecording = false;
        _recordingStopwatch.Stop();

        // 1. Procesar los buffers y crear el WAV Estéreo
        byte[] wavData = CreateStereoWav();
        
        if (wavData == null) return;

        // 2. Obtener Token del AuthManager
        string token = AuthManager.Instance.CurrentIdToken;
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("[ConversationRecorder] ❌ No hay token de sesión. Debes estar logueado.");
            return;
        }

        Debug.Log($"[ConversationRecorder] 📤 Subiendo audio al servidor... ({wavData.Length} bytes)");

        // 3. Subir al servidor usando la corrutina del ApiClient (para que no se corte al cambiar de escena)
        ApiClient.Instance.StartCoroutine(ApiClient.Instance.UploadAudioSession(
            wavData, 
            token, 
            language,
            (response) =>
            {
                Debug.Log($"[ConversationRecorder] ✅ Análisis completado. ID Sesión: {response.sessionId}");
                
                // Si AppController existe en la escena, le pasamos los datos
                if (AppController.Instance != null) 
                {
                    AppController.Instance.ProcessServerResponse(response);
                } 
                else 
                {
                    // Si no existe, al menos imprimimos el resultado en la consola como hacías antes
                    Debug.Log("[ConversationRecorder] Resultados obtenidos:\n" + JsonConvert.SerializeObject(response, Formatting.Indented));
                }
            },
            (error) => Debug.LogError($"[ConversationRecorder] ❌ Error en la subida: {error}")
        ));
    }

    // ── Callbacks de Audio ────────────────────────────────────────────

    private void HandlePlayerAudio(float[] samples, int rate)
    {
        if (!_isRecording || samples == null || samples.Length == 0) return;
        WriteToBuffer(Resample(samples, rate, sampleRate), true);
    }

    /// <summary>Llamado por NpcAudioCapture desde el hilo de audio de Unity.</summary>
    public void WriteNpcSamples(float[] samples, int rate)
    {
        if (!_isRecording || samples == null || samples.Length == 0) return;
        WriteToBuffer(Resample(samples, rate, sampleRate), false);
    }

    // ── Lógica Core: Buffers Sincronizados ─────────────────────────────

    private void WriteToBuffer(float[] samples, bool isPlayer)
    {
        double elapsedSeconds = _recordingStopwatch.Elapsed.TotalSeconds;
        int targetIndex = (int)(elapsedSeconds * sampleRate);
        
        lock (_lock)
        {
            if (isPlayer)
            {
                if (targetIndex < _playerWriteIndex) targetIndex = _playerWriteIndex;
                for (int i = 0; i < samples.Length; i++)
                {
                    int idx = targetIndex + i;
                    if (idx < _playerBuffer.Length)
                    {
                        _playerBuffer[idx] = Mathf.Clamp(_playerBuffer[idx] + samples[i], -1f, 1f);
                        if (idx > _playerWriteIndex) _playerWriteIndex = idx;
                    }
                }
            }
            else
            {
                if (targetIndex < _npcWriteIndex) targetIndex = _npcWriteIndex;
                for (int i = 0; i < samples.Length; i++)
                {
                    int idx = targetIndex + i;
                    if (idx < _npcBuffer.Length)
                    {
                        _npcBuffer[idx] = Mathf.Clamp(_npcBuffer[idx] + samples[i], -1f, 1f);
                        if (idx > _npcWriteIndex) _npcWriteIndex = idx;
                    }
                }
            }
        }
    }

    // ── Lógica Core: Entrelazado y WAV ─────────────────────────────────

    private byte[] CreateStereoWav()
    {
        int maxIndex = Mathf.Max(_playerWriteIndex, _npcWriteIndex);
        if (maxIndex <= 0)
        {
            Debug.LogError("[ConversationRecorder] ❌ No se capturó NADA de audio.");
            return null;
        }

        // Archivo estéreo (2 canales). Canal 0: Player, Canal 1: NPC.
        float[] interleaved = new float[(maxIndex + 1) * 2];
        for (int i = 0; i <= maxIndex; i++)
        {
            interleaved[i * 2] = _playerBuffer[i];       // Canal 0
            interleaved[i * 2 + 1] = _npcBuffer[i];      // Canal 1
        }

        return EncodeWav(interleaved, sampleRate, 2);
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
        using (BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.UTF8))
        {
            int bitsPerSample = 16;
            int bytesPerSample = bitsPerSample / 8;
            int dataSize = samples.Length * bytesPerSample;

            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + dataSize);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

            writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)channels);
            writer.Write(rate);
            writer.Write(rate * channels * bytesPerSample);
            writer.Write((short)(channels * bytesPerSample));
            writer.Write((short)bitsPerSample);

            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(dataSize);
            for (int i = 0; i < samples.Length; i++)
                writer.Write((short)(Mathf.Clamp(samples[i], -1f, 1f) * short.MaxValue));
            
            writer.Flush();
            return stream.ToArray();
        }
    }
}
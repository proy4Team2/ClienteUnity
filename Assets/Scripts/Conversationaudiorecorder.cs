using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conversationaudiorecorder : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("AudioSource del GameObject del NPC de Convai")]
    public AudioSource npcAudioSource;

    [Header("Configuración de audio")]
    [Tooltip("Frecuencia de muestreo del WAV final (Hz). 16000 es suficiente para voz.")]
    public int sampleRate     = 16000;

    [Tooltip("Duración máxima de la grabación en segundos.")]
    public int maxDurationSec = 600;

    // ── Estado interno ────────────────────────────────────────────────────────

    private List<float>     _masterBuffer;
    private int             _npcWritePos;

    private AudioClip       _micClip;
    private int             _micReadPos;

    private RingBuffer      _npcRing;
    private NPCAudioCapture _npcCapture;

    private bool            _isRecording;
    private bool            _alreadySaved;  // evita doble guardado si el usuario
                                            // llama StopAndSave() y luego OnDestroy()

    // ── Ciclo de vida ─────────────────────────────────────────────────────────

    private void Start()
    {
        StartRecording();
    }

    private void Update()
    {
        if (!_isRecording) return;
        DrainMic();
        DrainNpcRing();
    }

    private void OnDestroy()
    {
        // Guardar automáticamente si aún no se ha hecho (ej. cambio de escena)
        if (_isRecording && !_alreadySaved)
            StopAndSave();
        else
            StopMicrophone(); // limpieza mínima si ya se guardó
    }

    // ── API pública ───────────────────────────────────────────────────────────

    /// <summary>
    /// Detiene la grabación y guarda el WAV en disco.
    /// Se llama automáticamente en OnDestroy; también puedes llamarla
    /// manualmente desde un botón UI si quieres terminar antes de salir.
    /// Devuelve la ruta completa del fichero, o null si hubo error.
    /// </summary>
    public string StopAndSave()
    {
        if (!_isRecording) { Debug.LogWarning("[AudioRecorder] No estaba grabando."); return null; }
        if (_alreadySaved)  { Debug.LogWarning("[AudioRecorder] Ya se guardó esta sesión."); return null; }

        _isRecording  = false;
        _alreadySaved = true;

        // Vaciar lo que quede pendiente en ambas fuentes
        DrainMic();
        DrainNpcRing();

        StopMicrophone();
        if (_npcCapture != null) _npcCapture.Deactivate();

        if (_masterBuffer == null || _masterBuffer.Count == 0)
        {
            Debug.LogWarning("[AudioRecorder] Buffer vacío, no se genera WAV.");
            return null;
        }

        float  durationSecs = (float)_masterBuffer.Count / sampleRate;
        byte[] wav          = EncodeWav(_masterBuffer.ToArray(), sampleRate, channels: 1);
        string fileName     = $"interview_{DateTime.Now:yyyyMMdd_HHmmss}.wav";
        string path         = Path.Combine(Application.persistentDataPath, fileName);

        File.WriteAllBytes(path, wav);
        Debug.Log($"[AudioRecorder] ✅ WAV guardado  {durationSecs:F1}s  {wav.Length / 1024} KB  →  {path}");
        return path;
    }

    // ── Inicio de grabación (privado, llamado desde Start) ────────────────────

    private void StartRecording()
    {
        if (_isRecording) { Debug.LogWarning("[AudioRecorder] Ya está grabando."); return; }

        if (npcAudioSource == null)
        {
            Debug.LogError("[AudioRecorder] npcAudioSource no está asignado en el Inspector.");
            return;
        }

        // Inicializar buffers
        _masterBuffer = new List<float>(sampleRate * maxDurationSec);
        _npcWritePos  = 0;
        _alreadySaved = false;
        _npcRing      = new RingBuffer(sampleRate * 2); // 2 s de margen DSP

        // Conectar capturador al NPC
        _npcCapture = npcAudioSource.gameObject.GetComponent<NPCAudioCapture>()
                   ?? npcAudioSource.gameObject.AddComponent<NPCAudioCapture>();
        _npcCapture.Initialize(_npcRing, AudioSettings.outputSampleRate, sampleRate);

        // Iniciar micrófono
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("[AudioRecorder] No se detectó ningún micrófono.");
            return;
        }
        string mic  = Microphone.devices[0];
        _micClip    = Microphone.Start(mic, /*loop*/ true, maxDurationSec, sampleRate);
        _micReadPos = 0;

        StartCoroutine(WaitForMicThenEnable(mic));
    }

    private IEnumerator WaitForMicThenEnable(string mic)
    {
        // El driver de micrófono tarda unos frames en arrancar
        while (Microphone.GetPosition(mic) <= 0)
            yield return null;

        _isRecording = true;
        Debug.Log($"[AudioRecorder] ▶ Grabando  mic:'{mic}'  {sampleRate} Hz  max {maxDurationSec}s");
    }

    // ── Drenado del micrófono ─────────────────────────────────────────────────

    private void DrainMic()
    {
        if (_micClip == null) return;

        int writePos  = Microphone.GetPosition(null);
        if (writePos < 0) return;

        int available = (writePos >= _micReadPos)
            ? writePos - _micReadPos
            : (_micClip.samples - _micReadPos) + writePos; // wrap-around

        if (available <= 0) return;

        float[] raw   = new float[available * _micClip.channels];
        _micClip.GetData(raw, _micReadPos);
        _micReadPos   = writePos;

        float[] mono  = ToMono(raw, _micClip.channels);
        float[] final = Resample(mono, _micClip.frequency, sampleRate);

        _masterBuffer.AddRange(final);
    }

    // ── Drenado del ring buffer del NPC ───────────────────────────────────────

    private void DrainNpcRing()
    {
        int available = _npcRing.AvailableRead;
        if (available <= 0) return;

        float[] npc = new float[available];
        _npcRing.Read(npc, available);

        for (int i = 0; i < npc.Length; i++)
        {
            int idx = _npcWritePos + i;
            if (idx < _masterBuffer.Count)
                _masterBuffer[idx] = SoftClip(_masterBuffer[idx] + npc[i]);
            else
                _masterBuffer.Add(SoftClip(npc[i]));
        }
        _npcWritePos += npc.Length;
    }

    // ── DSP Helpers ───────────────────────────────────────────────────────────

    private static float[] ToMono(float[] data, int channels)
    {
        if (channels <= 1) return data;
        float[] mono = new float[data.Length / channels];
        for (int i = 0; i < mono.Length; i++)
        {
            float sum = 0f;
            for (int c = 0; c < channels; c++) sum += data[i * channels + c];
            mono[i] = sum / channels;
        }
        return mono;
    }

    private static float[] Resample(float[] input, int fromRate, int toRate)
    {
        if (fromRate == toRate || input.Length == 0) return input;
        int     outLen = Mathf.RoundToInt((float)input.Length * toRate / fromRate);
        float[] output = new float[outLen];
        float   ratio  = (float)(input.Length - 1) / Mathf.Max(outLen - 1, 1);
        for (int i = 0; i < outLen; i++)
        {
            float pos = i * ratio;
            int   lo  = (int)pos;
            float t   = pos - lo;
            float a   = input[lo];
            float b   = (lo + 1 < input.Length) ? input[lo + 1] : a;
            output[i] = a + t * (b - a);
        }
        return output;
    }

    private static float SoftClip(float v) => v > 1f ? 1f : v < -1f ? -1f : v;

    private void StopMicrophone()
    {
        if (Microphone.IsRecording(null)) Microphone.End(null);
        _micClip = null;
    }

    // ── Codificador WAV PCM 16-bit ─────────────────────────────────────────────

    private static byte[] EncodeWav(float[] samples, int rate, int channels)
    {
        int dataSize = samples.Length * 2;
        using var ms = new MemoryStream(44 + dataSize);
        using var bw = new BinaryWriter(ms);

        bw.Write(new[] { (byte)'R',(byte)'I',(byte)'F',(byte)'F' });
        bw.Write(36 + dataSize);
        bw.Write(new[] { (byte)'W',(byte)'A',(byte)'V',(byte)'E' });
        bw.Write(new[] { (byte)'f',(byte)'m',(byte)'t',(byte)' ' });
        bw.Write(16);
        bw.Write((short)1);
        bw.Write((short)channels);
        bw.Write(rate);
        bw.Write(rate * channels * 2);
        bw.Write((short)(channels * 2));
        bw.Write((short)16);
        bw.Write(new[] { (byte)'d',(byte)'a',(byte)'t',(byte)'a' });
        bw.Write(dataSize);
        foreach (float s in samples)
            bw.Write((short)(SoftClip(s) * short.MaxValue));

        return ms.ToArray();
    }
}

// ═════════════════════════════════════════════════════════════════════════════
//  RingBuffer — SPSC lock-free (Single Producer / Single Consumer)
//  Producer : hilo DSP  (OnAudioFilterRead)
//  Consumer : hilo principal  (Update → DrainNpcRing)
// ═════════════════════════════════════════════════════════════════════════════
public class RingBuffer
{
    private readonly float[] _buf;
    private readonly int     _mask;
    private volatile int     _writePos;
    private volatile int     _readPos;

    public RingBuffer(int minCapacity)
    {
        int cap = NextPow2(minCapacity);
        _buf    = new float[cap];
        _mask   = cap - 1;
    }

    public int AvailableRead
    {
        get
        {
            int diff = _writePos - _readPos;
            return diff >= 0 ? diff : diff + (_mask + 1);
        }
    }

    public void Write(float[] data, int count)
    {
        int w = _writePos;
        for (int i = 0; i < count; i++)
            _buf[(w + i) & _mask] = data[i];
        _writePos = w + count;
    }

    public void Read(float[] dest, int count)
    {
        int r = _readPos;
        for (int i = 0; i < count; i++)
            dest[i] = _buf[(r + i) & _mask];
        _readPos = r + count;
    }

    private static int NextPow2(int v)
    {
        v--;
        v |= v >> 1; v |= v >> 2; v |= v >> 4; v |= v >> 8; v |= v >> 16;
        return v + 1;
    }
}

// ═════════════════════════════════════════════════════════════════════════════
//  NPCAudioCapture — se añade automáticamente al GameObject del NPC.
//  Intercepta el audio en el hilo DSP y lo empuja al RingBuffer sin bloquear.
//  No modifica 'data' → el NPC se sigue escuchando con normalidad.
// ═════════════════════════════════════════════════════════════════════════════
[RequireComponent(typeof(AudioSource))]
public class NPCAudioCapture : MonoBehaviour
{
    private RingBuffer _ring;
    private int        _srcRate;
    private int        _dstRate;
    private bool       _active;

    internal void Initialize(RingBuffer ring, int systemSampleRate, int targetSampleRate)
    {
        _ring    = ring;
        _srcRate = systemSampleRate;
        _dstRate = targetSampleRate;
        _active  = true;
    }

    internal void Deactivate() => _active = false;

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (!_active || _ring == null) return;

        // Convertir a mono
        float[] mono;
        if (channels == 1)
        {
            mono = new float[data.Length];
            Array.Copy(data, mono, data.Length);
        }
        else
        {
            mono = new float[data.Length / channels];
            for (int i = 0; i < mono.Length; i++)
            {
                float sum = 0f;
                for (int c = 0; c < channels; c++) sum += data[i * channels + c];
                mono[i] = sum / channels;
            }
        }

        // Remuestrear si el sistema corre a frecuencia distinta (ej. 48000 → 16000)
        if (_srcRate != _dstRate)
        {
            int    outLen    = (int)((long)mono.Length * _dstRate / _srcRate);
            float[] resampled = new float[outLen];
            float  ratio      = (float)(mono.Length - 1) / Mathf.Max(outLen - 1, 1);
            for (int i = 0; i < outLen; i++)
            {
                float pos = i * ratio;
                int   lo  = (int)pos;
                float t   = pos - lo;
                float a   = mono[lo];
                float b   = (lo + 1 < mono.Length) ? mono[lo + 1] : a;
                resampled[i] = a + t * (b - a);
            }
            mono = resampled;
        }

        _ring.Write(mono, mono.Length);
    }
}

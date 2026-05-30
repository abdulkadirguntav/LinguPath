using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json.Linq;

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

public class MicrophoneInputManager : MonoBehaviour
{
    [Header("References")]
    public GeminiManager geminiManager;
    public Button micButton;
    public TMP_Text statusText;

    [Header("Recording")]
    public int maxRecordSeconds = 10;
    public int sampleRate = 16000;

    [Header("Visual Feedback")]
    public Color micActiveColor = new Color(1f, 0.2f, 0.2f);
    public Color micIdleColor   = Color.white;

    private bool isRecording = false;
    private AudioClip recordedClip;
    private string micDevice;
    private Image micImage;

    void Start()
    {
        if (micButton != null)
        {
            micImage = micButton.GetComponent<Image>();
            micButton.onClick.AddListener(ToggleMic);
        }

        if (Microphone.devices.Length == 0)
        {
            if (micButton != null) micButton.gameObject.SetActive(false);
            return;
        }

        micDevice = Microphone.devices[0];
    }

    public void ToggleMic()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            var cb = new PermissionCallbacks();
            cb.PermissionGranted += _ => StartRecording();
            Permission.RequestUserPermission(Permission.Microphone, cb);
            return;
        }
#endif
        if (isRecording) StopAndTranscribe();
        else             StartRecording();
    }

    void StartRecording()
    {
        recordedClip = Microphone.Start(micDevice, false, maxRecordSeconds, sampleRate);
        isRecording = true;
        SetStatus("Listening...");
        SetButtonColor(micActiveColor);
        StartCoroutine(AutoStop());
    }

    IEnumerator AutoStop()
    {
        yield return new WaitForSeconds(maxRecordSeconds);
        if (isRecording) StopAndTranscribe();
    }

    void StopAndTranscribe()
    {
        int pos = Microphone.GetPosition(micDevice);
        Microphone.End(micDevice);
        isRecording = false;
        SetButtonColor(micIdleColor);

        if (pos <= 0 || recordedClip == null)
        {
            SetStatus("No audio recorded.");
            Invoke(nameof(ClearStatus), 2f);
            return;
        }

        float[] samples = new float[pos * recordedClip.channels];
        recordedClip.GetData(samples, 0);
        byte[] wav = BuildWav(samples, recordedClip.channels, sampleRate);

        SetStatus("Processing...");
        StartCoroutine(Transcribe(wav));
    }

    IEnumerator Transcribe(byte[] wav)
    {
        string apiKey = geminiManager != null ? geminiManager.ApiKey : "";
        if (string.IsNullOrEmpty(apiKey))
        {
            SetStatus("API key missing.");
            Invoke(nameof(ClearStatus), 2f);
            yield break;
        }

        string url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key=" + apiKey;

        JObject payload = new JObject
        {
            ["contents"] = new JArray
            {
                new JObject
                {
                    ["parts"] = new JArray
                    {
                        new JObject
                        {
                            ["inlineData"] = new JObject
                            {
                                ["mimeType"] = "audio/wav",
                                ["data"]     = Convert.ToBase64String(wav)
                            }
                        },
                        new JObject { ["text"] = "Transcribe the audio. Return only the spoken words, nothing else." }
                    }
                }
            }
        };

        byte[] body = System.Text.Encoding.UTF8.GetBytes(payload.ToString());

        using UnityWebRequest req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.timeout = 30;

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[Mic] Transcription error: {req.error}");
            SetStatus("Error. Try again.");
            Invoke(nameof(ClearStatus), 2f);
            yield break;
        }

        try
        {
            JObject data = JObject.Parse(req.downloadHandler.text);
            string text  = data["candidates"][0]["content"]["parts"][0]["text"].ToString().Trim();
            ClearStatus();
            geminiManager?.SendVoiceMessage(text);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Mic] Parse error: {e.Message}");
            SetStatus("Couldn't understand.");
            Invoke(nameof(ClearStatus), 2f);
        }
    }

    static byte[] BuildWav(float[] samples, int channels, int hz)
    {
        byte[] pcm = new byte[samples.Length * 2];
        for (int i = 0; i < samples.Length; i++)
        {
            short s = (short)Mathf.Clamp(samples[i] * 32767f, -32768f, 32767f);
            pcm[i * 2]     = (byte)(s & 0xFF);
            pcm[i * 2 + 1] = (byte)((s >> 8) & 0xFF);
        }

        byte[] h = new byte[44];
        int byteRate = hz * channels * 2;
        WStr(h, 0,  "RIFF"); WI32(h, 4,  36 + pcm.Length);
        WStr(h, 8,  "WAVE"); WStr(h, 12, "fmt ");
        WI32(h, 16, 16);     WI16(h, 20, 1);
        WI16(h, 22, (short)channels);
        WI32(h, 24, hz);     WI32(h, 28, byteRate);
        WI16(h, 32, (short)(channels * 2)); WI16(h, 34, 16);
        WStr(h, 36, "data"); WI32(h, 40, pcm.Length);

        byte[] wav = new byte[44 + pcm.Length];
        Buffer.BlockCopy(h,   0, wav, 0,  44);
        Buffer.BlockCopy(pcm, 0, wav, 44, pcm.Length);
        return wav;
    }

    static void WStr(byte[] b, int p, string s) { foreach (char c in s) b[p++] = (byte)c; }
    static void WI32(byte[] b, int p, int v)
    {
        b[p]   = (byte)(v         & 0xFF); b[p+1] = (byte)((v >> 8)  & 0xFF);
        b[p+2] = (byte)((v >> 16) & 0xFF); b[p+3] = (byte)((v >> 24) & 0xFF);
    }
    static void WI16(byte[] b, int p, short v) { b[p] = (byte)(v & 0xFF); b[p+1] = (byte)((v >> 8) & 0xFF); }

    void SetStatus(string msg)    { if (statusText  != null) statusText.text    = msg; }
    void ClearStatus()            => SetStatus("");
    void SetButtonColor(Color c)  { if (micImage    != null) micImage.color     = c; }

    void OnDestroy()
    {
        if (Microphone.IsRecording(micDevice)) Microphone.End(micDevice);
    }
}

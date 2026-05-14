using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Speech-to-text input for the AI conversation panel.
// Uses Windows DictationRecognizer (built-in, no extra API key needed).
// On non-Windows builds, the mic button is hidden automatically.
//
// Inspector setup:
//   geminiManager  → the GeminiManager on this scene
//   micButton      → the Button the player holds/taps to speak
//   statusText     → a TMP_Text that shows "Listening..." / "Processing..."
//   micActiveColor → color of the button while recording (e.g. red)
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using UnityEngine.Windows.Speech;
#endif

public class MicrophoneInputManager : MonoBehaviour
{
    [Header("References")]
    public GeminiManager geminiManager;
    public Button micButton;
    public TMP_Text statusText;

    [Header("Visual Feedback")]
    public Color micActiveColor = new Color(1f, 0.2f, 0.2f);
    public Color micIdleColor = Color.white;

    private bool isListening = false;
    private Image micButtonImage;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    private DictationRecognizer dictation;
#endif

    void Start()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        try
        {
            dictation = new DictationRecognizer();
            dictation.DictationResult += OnResult;
            dictation.DictationError += OnError;
            dictation.DictationComplete += OnComplete;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Mic] Konuşma tanıma başlatılamadı: {e.Message}\nWindows Ayarları > Gizlilik > Konuşma bölümünü kontrol et.");
            if (micButton != null) micButton.gameObject.SetActive(false);
            return;
        }
#else
        if (micButton != null) micButton.gameObject.SetActive(false);
        return;
#endif

        if (micButton != null)
        {
            micButtonImage = micButton.GetComponent<Image>();
            micButton.onClick.AddListener(ToggleMic);
        }

        SetStatus("");
    }

    public void ToggleMic()
    {
        if (isListening) StopListening();
        else StartListening();
    }

    void StartListening()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (dictation == null || dictation.Status == SpeechSystemStatus.Running) return;

        dictation.Start();
        isListening = true;
        SetStatus("Listening...");
        SetButtonColor(micActiveColor);
#endif
    }

    void StopListening()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (dictation == null || dictation.Status != SpeechSystemStatus.Running) return;

        dictation.Stop();
        // Result arrives via OnResult callback
#endif
    }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    private void OnResult(string text, ConfidenceLevel confidence)
    {
        isListening = false;
        SetButtonColor(micIdleColor);

        if (string.IsNullOrWhiteSpace(text))
        {
            SetStatus("Didn't catch that. Try again.");
            Invoke(nameof(ClearStatus), 2f);
            return;
        }

        SetStatus("Sending...");
        geminiManager?.SendVoiceMessage(text);
        Invoke(nameof(ClearStatus), 1f);
    }

    private void OnComplete(DictationCompletionCause cause)
    {
        if (cause != DictationCompletionCause.Complete)
        {
            isListening = false;
            SetButtonColor(micIdleColor);
            SetStatus("Mic stopped: " + cause);
            Invoke(nameof(ClearStatus), 2f);
        }
    }

    private void OnError(string error, int hresult)
    {
        isListening = false;
        SetButtonColor(micIdleColor);
        Debug.LogError($"Dictation error: {error} (0x{hresult:X})");
        SetStatus("Mic error. Try again.");
        Invoke(nameof(ClearStatus), 2f);
    }
#endif

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }

    private void ClearStatus() => SetStatus("");

    private void SetButtonColor(Color c)
    {
        if (micButtonImage != null) micButtonImage.color = c;
    }

    void OnDestroy()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (dictation != null)
        {
            dictation.DictationResult -= OnResult;
            dictation.DictationError -= OnError;
            dictation.DictationComplete -= OnComplete;
            dictation.Dispose();
        }
#endif
    }
}

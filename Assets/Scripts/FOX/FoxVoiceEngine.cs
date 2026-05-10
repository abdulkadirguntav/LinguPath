using System.Collections;
using UnityEngine;

// Placeholder for fox TTS/audio output.
// Wire a TTS AudioSource + OVRLipSyncContextCanned here if you add text-to-speech later.
// Currently handles the "fox is speaking" timing so other systems know when to react.
public class FoxVoiceEngine : MonoBehaviour
{
    [Tooltip("Estimated spoken characters per second (for timing without real TTS)")]
    public float charsPerSecond = 18f;

    public bool IsSpeaking { get; private set; }

    private Coroutine _talkRoutine;

    public void SpeakFromAI(string text)
    {
        if (_talkRoutine != null) StopCoroutine(_talkRoutine);
        _talkRoutine = StartCoroutine(SpeakingRoutine(text.Length / charsPerSecond));
    }

    public void StopSpeaking()
    {
        if (_talkRoutine != null) StopCoroutine(_talkRoutine);
        IsSpeaking = false;
    }

    private IEnumerator SpeakingRoutine(float duration)
    {
        IsSpeaking = true;
        yield return new WaitForSeconds(duration);
        IsSpeaking = false;
    }
}

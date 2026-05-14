using System;
using System.Collections.Generic;
using Unity.InferenceEngine;
using UnityEngine;

public class RunJets : MonoBehaviour
{
    public ModelAsset modelAsset;
    public TextAsset phonemeAsset;

    readonly string[] phonemes =
    {
        "<blank>", "<unk>", "AH0", "N", "T", "D", "S", "R", "L", "DH", "K", "Z", "IH1",
        "IH0", "M", "EH1", "W", "P", "AE1", "AH1", "V", "ER0", "F", ",", "AA1", "B",
        "HH", "IY1", "UW1", "IY0", "AO1", "EY1", "AY1", ".", "OW1", "SH", "NG", "G",
        "ER1", "CH", "JH", "Y", "AW1", "TH", "UH1", "EH2", "OW0", "EY2", "AO0", "IH2",
        "AE2", "AY2", "AA2", "UW0", "EH0", "OY1", "EY0", "AO2", "ZH", "OW2", "AE0", "UW2",
        "AH2", "AY0", "IY2", "AW2", "AA0", "\"", "ER2", "UH2", "?", "OY2", "!", "AW0",
        "UH0", "OY0", "..", "<sos/eos>"
    };

    const int samplerate = 22050;

    Dictionary<string, string> dict = new();
    Worker worker;
    AudioSource audioSource;
    bool modelReady = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            Debug.LogError("[RunJets] AudioSource bulunamadı! Aynı GameObject'e bir AudioSource ekle.");

        // Sözlük küçük (3.6MB), sahne açılışında yüklenir.
        ReadDictionary();
        // 127MB model, ilk konuşmada yüklenir — sahneyi yavaşlatmaz.
    }

    void LoadModel()
    {
        var model = ModelLoader.Load(modelAsset);
        worker = new Worker(model, BackendType.GPUCompute);
        modelReady = true;
    }

    void ReadDictionary()
    {
        string[] lines = phonemeAsset.text.Split("\n");
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] parts = line.Split();
            if (parts[0] == ";;;") continue;
            string key = parts[0];
            dict.TryAdd(key, line.Substring(key.Length + 2));
        }
        dict.TryAdd(",", ",");
        dict.TryAdd(".", ".");
        dict.TryAdd("!", "!");
        dict.TryAdd("?", "?");
        dict.TryAdd("\"", "\"");
    }

    string ExpandNumbers(string text) => text
        .Replace("0", " ZERO ").Replace("1", " ONE ").Replace("2", " TWO ")
        .Replace("3", " THREE ").Replace("4", " FOUR ").Replace("5", " FIVE ")
        .Replace("6", " SIX ").Replace("7", " SEVEN ").Replace("8", " EIGHT ")
        .Replace("9", " NINE ");

    string TextToPhonemes(string text)
    {
        string output = "";
        foreach (string word in ExpandNumbers(text).ToUpper().Split())
            output += DecodeWord(word);
        return output;
    }

    string DecodeWord(string word)
    {
        string output = "";
        int start = 0;
        for (int end = word.Length; end >= 0 && start < word.Length; end--)
        {
            if (end <= start) { start++; end = word.Length + 1; continue; }
            if (dict.TryGetValue(word.Substring(start, end - start), out string value))
            {
                output += value + " ";
                start = end;
                end = word.Length + 1;
            }
        }
        return output;
    }

    int[] GetTokens(string ptext)
    {
        string[] p = ptext.Split();
        int[] tokens = new int[p.Length];
        for (int i = 0; i < tokens.Length; i++)
            tokens[i] = Mathf.Max(0, Array.IndexOf(phonemes, p[i]));
        return tokens;
    }

    void DoInference(string ptext)
    {
        int[] tokens = GetTokens(ptext);
        using var input = new Tensor<int>(new TensorShape(tokens.Length), tokens);
        worker.Schedule(input);

        using var samplesTensor = (worker.PeekOutput("wav") as Tensor<float>).ReadbackAndClone();
        var samples = samplesTensor.AsReadOnlySpan();

        AudioClip clip = AudioClip.Create("fox_voice", samples.Length, 1, samplerate, false);
        clip.SetData(samples, 0);

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    // GeminiManager bu metodu çağırır
    public void SpeakFromAI(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        if (!modelReady)
            LoadModel(); // İlk çağrıda model yüklenir (127MB, tek seferlik)

        DoInference(TextToPhonemes(text));
    }

    [ContextMenu("Test Ses")]
    void TestSes() => SpeakFromAI("Hello! I am the fox. Can you hear me speaking?");

    void OnDestroy() => worker?.Dispose();
}

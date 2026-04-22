using System.Collections.Generic;
using UnityEngine;
using Unity.InferenceEngine; // Hatanı önlemek için güncellendi
using System.Text;
using Unity.Collections;
using Newtonsoft.Json;
using TMPro;

public class RunWhisper : MonoBehaviour
{
    Worker decoder1, decoder2, encoder, spectrogram;
    Worker argmax;

    private AudioClip audioClip;
    private string micName;
    private bool isRecording = false;

    const int maxTokens = 100;
    const int END_OF_TEXT = 50257;
    const int START_OF_TRANSCRIPT = 50258;
    const int ENGLISH = 50259;
    const int TRANSCRIBE = 50359; 
    const int NO_TIME_STAMPS = 50363;

    int numSamples;
    string[] tokens;
    int tokenCount = 0;
    NativeArray<int> outputTokens;
    int[] whiteSpaceCharacters = new int[256];
    Tensor<float> encodedAudio;
    bool transcribe = false;
    string outputString = "";

    const int maxSamples = 30 * 16000; // Maksimum 30 saniye

    public ModelAsset audioDecoder1, audioDecoder2;
    public ModelAsset audioEncoder;
    public ModelAsset logMelSpectro;
    public TextAsset vocabAsset;
    public GeminiManager gemini;
    public TextMeshProUGUI chatText;

    Awaitable m_Awaitable;
    NativeArray<int> lastToken;
    Tensor<int> lastTokenTensor;
    Tensor<int> tokensTensor;
    Tensor<float> audioInput;

    public void Start()
    {
        // 1. MİKROFONU BUL
        if (Microphone.devices.Length > 0)
        {
            micName = Microphone.devices[0];
            Debug.Log("🎤 Mikrofon bulundu: " + micName);
        }

        // 2. YAPAY ZEKA MODELLERİNİ YÜKLE (Sadece 1 kere yapılır)
        SetupWhiteSpaceShifts();
        GetTokens();

        decoder1 = new Worker(ModelLoader.Load(audioDecoder1), BackendType.GPUCompute);
        decoder2 = new Worker(ModelLoader.Load(audioDecoder2), BackendType.GPUCompute);

        FunctionalGraph graph = new FunctionalGraph();
        var input = graph.AddInput(DataType.Float, new DynamicTensorShape(1, 1, 51865));
        var amax = Functional.ArgMax(input, -1, false);
        var selectTokenModel = graph.Compile(amax);
        argmax = new Worker(selectTokenModel, BackendType.GPUCompute);

        encoder = new Worker(ModelLoader.Load(audioEncoder), BackendType.GPUCompute);
        spectrogram = new Worker(ModelLoader.Load(logMelSpectro), BackendType.GPUCompute);

        outputTokens = new NativeArray<int>(maxTokens, Allocator.Persistent);
        tokensTensor = new Tensor<int>(new TensorShape(1, maxTokens));
        ComputeTensorData.Pin(tokensTensor);

        lastToken = new NativeArray<int>(1, Allocator.Persistent); 
        lastToken[0] = NO_TIME_STAMPS;
        lastTokenTensor = new Tensor<int>(new TensorShape(1, 1), new[] { NO_TIME_STAMPS });
        
        Debug.Log("🧠 Sentis Yapay Zeka Modelleri Yüklendi ve Bekliyor!");
    }

    // --- OYUNCU MİKROFON BUTONUNA BASILI TUTTUĞUNDA ÇALIŞACAK ---
    public void StartRecording()
    {
        if (micName == null) return;
        isRecording = true;
        
        // Whisper ZORUNLU OLARAK 16000 Hz ister!
        audioClip = Microphone.Start(micName, false, 30, 16000); 
        Debug.Log("🔴 Kayıt Başladı... Konuş!");
    }

    // --- OYUNCU BUTONDAN ELİNİ ÇEKTİĞİNDE ÇALIŞACAK ---
    public async void StopRecording()
    {
        if (!isRecording) return;
        Microphone.End(micName);
        isRecording = false;
        Debug.Log("⏹️ Kayıt Bitti. Sentis Sesi Metne Çeviriyor (Offline)...");

        // Çeviri için değişkenleri sıfırla
        outputString = "";
        tokenCount = 3;
        outputTokens[0] = START_OF_TRANSCRIPT;
        outputTokens[1] = ENGLISH;
        outputTokens[2] = TRANSCRIBE;

        tokensTensor.Reshape(new TensorShape(1, tokenCount));
        tokensTensor.dataOnBackend.Upload<int>(outputTokens, tokenCount);
        lastToken[0] = NO_TIME_STAMPS;
        lastTokenTensor.dataOnBackend.Upload<int>(lastToken, 1);

        // İşlemi başlat
        LoadAudio();
        EncodeAudio();
        transcribe = true;

        while (true)
        {
            if (!transcribe || tokenCount >= (outputTokens.Length - 1))
            {
                Debug.Log("✅ OYUNCU ŞUNU SÖYLEDİ: " + outputString);
                // SONRAKİ AŞAMA: Bu string'i alıp ChatGPT'ye / Mentör sisteme atacağız!
                // Kulağın duyduğu metni, beynin işlemesi için Gemini'ye yolluyoruz!
                if(gemini != null)
                {
                    if(chatText != null)
                    {
                        chatText.text += $"\n\n<color=#00FF00><b>Sen:</b></color> {outputString}";
                    }
                    gemini.AskGemini(outputString);
                }

                return;
            }
            m_Awaitable = InferenceStep();
            await m_Awaitable;
        }
    }

    void LoadAudio()
    {
        numSamples = audioClip.samples;
        var data = new float[maxSamples];

        numSamples = Mathf.Min(numSamples, maxSamples);
        audioClip.GetData(data, 0);

        numSamples = maxSamples;
        audioInput = new Tensor<float>(new TensorShape(1, numSamples), data);
    }

    void EncodeAudio()
    {
        spectrogram.Schedule(audioInput);
        var logmel = spectrogram.PeekOutput() as Tensor<float>;
        encoder.Schedule(logmel);
        encodedAudio = encoder.PeekOutput() as Tensor<float>;
    }

    async Awaitable InferenceStep()
    {
        decoder1.SetInput("input_ids", tokensTensor);
        decoder1.SetInput("encoder_hidden_states", encodedAudio);
        decoder1.Schedule();

        var past_key_values_0_decoder_key = decoder1.PeekOutput("present.0.decoder.key") as Tensor<float>;
        var past_key_values_0_decoder_value = decoder1.PeekOutput("present.0.decoder.value") as Tensor<float>;
        var past_key_values_1_decoder_key = decoder1.PeekOutput("present.1.decoder.key") as Tensor<float>;
        var past_key_values_1_decoder_value = decoder1.PeekOutput("present.1.decoder.value") as Tensor<float>;
        var past_key_values_2_decoder_key = decoder1.PeekOutput("present.2.decoder.key") as Tensor<float>;
        var past_key_values_2_decoder_value = decoder1.PeekOutput("present.2.decoder.value") as Tensor<float>;
        var past_key_values_3_decoder_key = decoder1.PeekOutput("present.3.decoder.key") as Tensor<float>;
        var past_key_values_3_decoder_value = decoder1.PeekOutput("present.3.decoder.value") as Tensor<float>;

        var past_key_values_0_encoder_key = decoder1.PeekOutput("present.0.encoder.key") as Tensor<float>;
        var past_key_values_0_encoder_value = decoder1.PeekOutput("present.0.encoder.value") as Tensor<float>;
        var past_key_values_1_encoder_key = decoder1.PeekOutput("present.1.encoder.key") as Tensor<float>;
        var past_key_values_1_encoder_value = decoder1.PeekOutput("present.1.encoder.value") as Tensor<float>;
        var past_key_values_2_encoder_key = decoder1.PeekOutput("present.2.encoder.key") as Tensor<float>;
        var past_key_values_2_encoder_value = decoder1.PeekOutput("present.2.encoder.value") as Tensor<float>;
        var past_key_values_3_encoder_key = decoder1.PeekOutput("present.3.encoder.key") as Tensor<float>;
        var past_key_values_3_encoder_value = decoder1.PeekOutput("present.3.encoder.value") as Tensor<float>;

        decoder2.SetInput("input_ids", lastTokenTensor);
        decoder2.SetInput("past_key_values.0.decoder.key", past_key_values_0_decoder_key);
        decoder2.SetInput("past_key_values.0.decoder.value", past_key_values_0_decoder_value);
        decoder2.SetInput("past_key_values.1.decoder.key", past_key_values_1_decoder_key);
        decoder2.SetInput("past_key_values.1.decoder.value", past_key_values_1_decoder_value);
        decoder2.SetInput("past_key_values.2.decoder.key", past_key_values_2_decoder_key);
        decoder2.SetInput("past_key_values.2.decoder.value", past_key_values_2_decoder_value);
        decoder2.SetInput("past_key_values.3.decoder.key", past_key_values_3_decoder_key);
        decoder2.SetInput("past_key_values.3.decoder.value", past_key_values_3_decoder_value);

        decoder2.SetInput("past_key_values.0.encoder.key", past_key_values_0_encoder_key);
        decoder2.SetInput("past_key_values.0.encoder.value", past_key_values_0_encoder_value);
        decoder2.SetInput("past_key_values.1.encoder.key", past_key_values_1_encoder_key);
        decoder2.SetInput("past_key_values.1.encoder.value", past_key_values_1_encoder_value);
        decoder2.SetInput("past_key_values.2.encoder.key", past_key_values_2_encoder_key);
        decoder2.SetInput("past_key_values.2.encoder.value", past_key_values_2_encoder_value);
        decoder2.SetInput("past_key_values.3.encoder.key", past_key_values_3_encoder_key);
        decoder2.SetInput("past_key_values.3.encoder.value", past_key_values_3_encoder_value);

        decoder2.Schedule();

        var logits = decoder2.PeekOutput("logits") as Tensor<float>;
        argmax.Schedule(logits);
        using var t_Token = await argmax.PeekOutput().ReadbackAndCloneAsync() as Tensor<int>;
        int index = t_Token[0];

        outputTokens[tokenCount] = lastToken[0];
        lastToken[0] = index;
        tokenCount++;
        tokensTensor.Reshape(new TensorShape(1, tokenCount));
        tokensTensor.dataOnBackend.Upload<int>(outputTokens, tokenCount);
        lastTokenTensor.dataOnBackend.Upload<int>(lastToken, 1);

        if (index == END_OF_TEXT)
        {
            transcribe = false;
        }
        else if (index < tokens.Length)
        {
            outputString += GetUnicodeText(tokens[index]);
        }
    }

    void GetTokens()
    {
        var vocab = JsonConvert.DeserializeObject<Dictionary<string, int>>(vocabAsset.text);
        tokens = new string[vocab.Count];
        foreach (var item in vocab)
        {
            tokens[item.Value] = item.Key;
        }
    }

    string GetUnicodeText(string text)
    {
        var bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(ShiftCharacterDown(text));
        return Encoding.UTF8.GetString(bytes);
    }

    string ShiftCharacterDown(string text)
    {
        string outText = "";
        foreach (char letter in text)
        {
            outText += ((int)letter <= 256) ? letter : (char)whiteSpaceCharacters[(int)(letter - 256)];
        }
        return outText;
    }

    void SetupWhiteSpaceShifts()
    {
        for (int i = 0, n = 0; i < 256; i++)
        {
            if (IsWhiteSpace((char)i)) whiteSpaceCharacters[n++] = i;
        }
    }

    bool IsWhiteSpace(char c)
    {
        return !(('!' <= c && c <= '~') || ('¡' <= c && c <= 'ÿ') || ('œ' <= c && c <= 'œ'));
    }

    private void OnDestroy()
    {
        decoder1?.Dispose();
        decoder2?.Dispose();
        encoder?.Dispose();
        spectrogram?.Dispose();
        argmax?.Dispose();
        audioInput?.Dispose();
        lastTokenTensor?.Dispose();
        tokensTensor?.Dispose();
    }
}
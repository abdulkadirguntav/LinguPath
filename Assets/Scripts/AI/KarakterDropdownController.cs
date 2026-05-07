using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KarakterDropdownController : MonoBehaviour
{
    [Header("Bağlantılar")]
    public TMP_Dropdown dropdown;
    public GeminiManager geminiManager;

    private void Start()
    {
        if (dropdown == null || geminiManager == null) return;

        dropdown.ClearOptions();

        var options = new List<string>();
        foreach (var karakter in geminiManager.karakterler)
            options.Add(karakter.karakterAdi);

        dropdown.AddOptions(options);

        dropdown.value = geminiManager.aktifKarakterIndex;
        dropdown.RefreshShownValue();

        dropdown.onValueChanged.AddListener(OnKarakterSecildi);
    }

    private void OnKarakterSecildi(int index)
    {
        geminiManager.KarakterSec(index);
    }
}

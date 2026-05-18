using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor.Rendering.LookDev;

public class CharacterDropdownController : MonoBehaviour
{
    [Header("References")]
    public TMP_Dropdown dropdown;
    public GeminiManager geminiManager;

    private void Start()
    {
        if (dropdown == null || geminiManager == null) return;

        dropdown.ClearOptions();

        var options = new List<string>();
        foreach (var character in geminiManager.characters)
            options.Add(character.characterName);

        dropdown.AddOptions(options);
        dropdown.value = geminiManager.activeCharacterIndex;
        dropdown.RefreshShownValue();

        dropdown.onValueChanged.AddListener(OnCharacterSelected);
    }

    private void OnCharacterSelected(int index)
    {
        geminiManager.SelectCharacter(index);
    }
}

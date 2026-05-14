using UnityEngine;
using UnityEngine.UI;

public class CharacterPortraitRenderer : MonoBehaviour
{
    public static CharacterPortraitRenderer instance;

    [System.Serializable]
    public class PortraitEntry
    {
        public Camera cam;
        public PortraitCharacterApplier character;
        [HideInInspector] public RenderTexture renderTexture;
    }

    [Header("3 Portrait Slot (kamera + karakter)")]
    public PortraitEntry[] portraits = new PortraitEntry[3];

    [Header("Çözünürlük")]
    public int resolution = 256;

    void Awake()
    {
        instance = this;

        foreach (PortraitEntry entry in portraits)
        {
            if (entry.cam == null) continue;
            entry.renderTexture = new RenderTexture(resolution, resolution, 16);
            entry.cam.targetTexture = entry.renderTexture;
        }
    }

    public void SetupPortrait(int index, CharacterSaveData data)
    {
        if (index >= portraits.Length) return;
        PortraitEntry entry = portraits[index];

        bool hasCharacter = data != null && data.isCreated;
        entry.cam.gameObject.SetActive(hasCharacter);

        if (hasCharacter)
            entry.character.Apply(data);
        else
            ClearTexture(entry.renderTexture);
    }

    private void ClearTexture(RenderTexture rt)
    {
        if (rt == null) return;
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = prev;
    }

    public RenderTexture GetTexture(int index)
    {
        if (index >= portraits.Length) return null;
        return portraits[index].renderTexture;
    }
}

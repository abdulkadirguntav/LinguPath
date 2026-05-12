using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SlotSelectionUI : MonoBehaviour
{
    [Header("3 Slot — Inspector'dan sıraya göre bağla")]
    public Button[] slotButtons = new Button[3];
    public TMP_Text[] slotNameTexts = new TMP_Text[3];
    public TMP_Text[] slotInfoTexts = new TMP_Text[3];
    public Button[] deleteButtons = new Button[3];

    [Header("Panel Referansları")]
    public GameObject slotSelectionPanel;
    public GameObject characterCreatorPanel;
    public GameObject mainMenuPanel;
    public GameObject deleteConfirmPanel;

    [Header("Silme Onayı")]
    public TMP_Text deleteConfirmText;
    public Button confirmDeleteButton;

    private int pendingDeleteIndex = -1;

    void OnEnable()
    {
        RefreshSlots();
    }

    public void RefreshSlots()
    {
        if (SaveSlotManager.instance == null) return;

        for (int i = 0; i < 3; i++)
        {
            SlotSaveData slot = SaveSlotManager.instance.GetSlot(i);
            int captured = i;

            slotButtons[i].onClick.RemoveAllListeners();

            if (slot.isEmpty)
            {
                if (slotNameTexts[i] != null) slotNameTexts[i].text = "Boş Slot";
                if (slotInfoTexts[i] != null) slotInfoTexts[i].text = "Yeni Oyun Başlat";
                slotButtons[i].onClick.AddListener(() => OnEmptySlotClicked(captured));

                if (deleteButtons[i] != null) deleteButtons[i].gameObject.SetActive(false);
            }
            else
            {
                int done = SaveSlotManager.instance.GetCompletedMissionCount(i);
                if (slotNameTexts[i] != null) slotNameTexts[i].text = slot.playerName;
                if (slotInfoTexts[i] != null) slotInfoTexts[i].text = $"{done} görev tamamlandı";
                slotButtons[i].onClick.AddListener(() => OnFilledSlotClicked(captured));

                if (deleteButtons[i] != null)
                {
                    deleteButtons[i].gameObject.SetActive(true);
                    deleteButtons[i].onClick.RemoveAllListeners();
                    deleteButtons[i].onClick.AddListener(() => ShowDeleteConfirm(captured));
                }
            }
        }
    }

    private void OnFilledSlotClicked(int index)
    {
        SaveSlotManager.instance.SelectSlot(index);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void OnEmptySlotClicked(int index)
    {
        SaveSlotManager.instance.CreateSlot(index, "Oyuncu " + (index + 1));
        if (slotSelectionPanel != null) slotSelectionPanel.SetActive(false);
        if (characterCreatorPanel != null) characterCreatorPanel.SetActive(true);
    }

    public void BackToMainMenu()
    {
        if (slotSelectionPanel != null) slotSelectionPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void ShowDeleteConfirm(int index)
    {
        pendingDeleteIndex = index;
        if (deleteConfirmText != null)
            deleteConfirmText.text = $"\"{SaveSlotManager.instance.GetSlot(index).playerName}\" kaydını sil?";

        if (deleteConfirmPanel != null) deleteConfirmPanel.SetActive(true);

        if (confirmDeleteButton != null)
        {
            confirmDeleteButton.onClick.RemoveAllListeners();
            confirmDeleteButton.onClick.AddListener(ConfirmDelete);
        }
    }

    public void CancelDelete()
    {
        pendingDeleteIndex = -1;
        if (deleteConfirmPanel != null) deleteConfirmPanel.SetActive(false);
    }

    private void ConfirmDelete()
    {
        if (pendingDeleteIndex >= 0)
            SaveSlotManager.instance.DeleteSlot(pendingDeleteIndex);

        pendingDeleteIndex = -1;
        if (deleteConfirmPanel != null) deleteConfirmPanel.SetActive(false);
        RefreshSlots();
    }
}

using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CartItemUI : MonoBehaviour
{
    [Header("Arayüz Bağlantıları")]
    public TextMeshProUGUI itemNameText; // Ürünün adı
    public Image itemIcon;               // Ürünün resmi
    public Button removeButton;          // Çarpı (Sil) butonu

    private string myItemName;
    private MarketManager myManager;

    // MarketManager bu objeyi oluşturduğunda içini dolduracak
    public void SetupCartItem(string itemName, Sprite icon, MarketManager manager)
    {
        myItemName = itemName;
        myManager = manager;

        if (itemNameText != null) itemNameText.text = myItemName;
        if (icon != null && itemIcon != null) itemIcon.sprite = icon;

        // Sil butonuna basıldığında MarketManager'a haber ver
        if (removeButton != null)
        {
            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener(OnRemoveClicked);
        }
    }

    private void OnRemoveClicked()
    {
        // Sepetten çıkar ve kendini yok et
        myManager.RemoveItemFromCart(myItemName);
        Destroy(gameObject); 
    }
}
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
    public void SetupCartItem(string itemName, MarketManager manager)
    {
        myItemName = itemName;
        myManager = manager;
        
        if (itemNameText != null) itemNameText.text = myItemName;
        
        // Resources klasöründen resmi anlık olarak çekip gösteriyoruz
        Sprite loadedIcon = Resources.Load<Sprite>("Icons/" + myItemName);
        if(loadedIcon != null && itemIcon != null) 
        {
            itemIcon.sprite = loadedIcon;
        }

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
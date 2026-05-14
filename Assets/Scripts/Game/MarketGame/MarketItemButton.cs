using UnityEngine;
using UnityEngine.UI;

public class MarketItemButton : MonoBehaviour
{
    [Header("Bileşenler")]
    public Image itemImage; // Butonun kendi resim (Image) bileşeni
    
    // Arka planda tutulacak kimlik
    private string myItemName; 
    private MarketManager myManager;

    // Bu fonksiyonu CSV okuyucu kodumuz çağıracak ve butonu dolduracak
    public void SetupItem(string itemName, Sprite icon, MarketManager manager)
    {
        myItemName = itemName;
        myManager = manager;
        
        if(icon != null)
        {
            itemImage.sprite = icon;
        }

        // SİHİRLİ KISIM: Butona tıklandığında senin yazdığın sepet kodunu tetikle!
        Button btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClickItem);
    }

    private void OnClickItem()
    {
        myManager.AddItemToCart(myItemName, itemImage != null ? itemImage.sprite : null);
    }
}
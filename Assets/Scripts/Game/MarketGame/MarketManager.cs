using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class MarketManager : MonoBehaviour
{
    [Header("Item's the NPC wants")]
    public List<string> npcList = new List<string>();

    [Header("Player's Items")]
    public List<string> currentItems = new List<string>();

    [Header("UI Connections")]
    public GameObject marketPanel;
    public GameObject mainGameUI;

    [Header("Cart UI")]
    public GameObject cartPanel;
    public Transform cartContentParent;
    public GameObject cartItemPrefab;

    void Start()
    {
        currentItems.Clear();
    }

    // sepete ekleme fonksiyonu şu şekilde çalışacak = parametre olarak string alacak ve public olacak. parametre olarak gelen ürün mevcut sepete eklenmeli.

    public void AddItemToCart(string itemName)
    {
        currentItems.Add(itemName);
        Debug.Log(itemName + "Added to Cart" + currentItems.Count);
    }

    public void Checked()
    {
        // Sepetteki Ürün Sayısı Eşleşiyor mu ? 
        if(currentItems.Count != npcList.Count)
        {
            Debug.Log("Wrong Order! Order Count mismatch. Cart emptying.");
            currentItems.Clear();
            return;
        }

        // Sepetteki Ürünler Eşleşiyor mu ?

        List<string> tempCart = new List<string>(npcList); // NPC'nin istediği ürünleri geçici bir listeye kopyala
        bool isCorrect = true;

        foreach (string item in currentItems)
        {
            if(tempCart.Contains(item))
            {
                tempCart.Remove(item);
            }
            else
            {
                isCorrect = false;
                break;
            }
        }

        if(isCorrect)
        {
            Debug.Log(" Mission Complete ");
            GameEventSystem.LogAnswer("Market Alışverişi", true);
            GameEventSystem.LogGameEnd("Market Oyunu", true, npcList.Count * 25);
            currentItems.Clear();
            ExitMiniGame();

            if(NPC.ActiveNPC != null)
            {
                NPC.ActiveNPC.FinishMission(true);
            }
        }
        else
        {
            Debug.Log(" Mission Failed ");
            GameEventSystem.LogAnswer("Market Alışverişi", false, "Yanlış ürün seçildi");
            GameEventSystem.LogGameEnd("Market Oyunu", false, 0);
            currentItems.Clear();

            // NPC'ye kaybettiğimizi bildir (lose diyaloğu çalışır, tekrar denenebilir)
            if(NPC.ActiveNPC != null)
            {
                ExitMiniGame();
                NPC.ActiveNPC.FinishMission(false);
            }
        }
    }

    public void ExitMiniGame()
    {
        currentItems.Clear();

        if(marketPanel != null) marketPanel.SetActive(false);
        if(mainGameUI != null) mainGameUI.SetActive(true);

        Debug.Log("Marketten çıkıldı, kasabaya dönüldü");
    }

    public void OpenCartUI()
    {
        if(cartPanel != null) cartPanel.SetActive(true);
        RefreshCartUI();
    }

    public void RefreshCartUI()
    {
        foreach(Transform child in cartContentParent)
        {
            Destroy(child.gameObject);
        }

        foreach(string item in currentItems)
        {
            GameObject newCartObj = Instantiate(cartItemPrefab, cartContentParent);
            CartItemUI cartUI = newCartObj.GetComponent<CartItemUI>();
            if(cartUI!= null)
            {
                cartUI.SetupCartItem(item, this);
            }
        }
    }

    public void RemoveItemFromCart(string itemName)
    {
        currentItems.Remove(itemName);
        Debug.Log(itemName + "sepetten çıkarıldı. Kalan " + currentItems.Count);

        RefreshCartUI();
    }
}

using UnityEngine;
using System.Collections.Generic;

public class MarketManager : MonoBehaviour
{
    [Header("Item's the NPC wants")]
    public List<string> npcList = new List<string>();

    [System.Serializable]
    public struct CartEntry
    {
        public string itemName;
        public Sprite itemIcon;
    }

    [Header("Player's Items")]
    public List<CartEntry> currentItems = new List<CartEntry>();

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

    public void AddItemToCart(string itemName, Sprite icon = null)
    {
        currentItems.Add(new CartEntry { itemName = itemName, itemIcon = icon });
        Debug.Log(itemName + " added to cart. Count: " + currentItems.Count);
    }

    public void Checked()
    {
        if (currentItems.Count != npcList.Count)
        {
            Debug.Log("Wrong order! Item count mismatch. Clearing cart.");
            currentItems.Clear();
            return;
        }

        List<string> tempCart = new List<string>(npcList);
        bool isCorrect = true;

        foreach (CartEntry entry in currentItems)
        {
            string item = entry.itemName;
            if (tempCart.Contains(item))
                tempCart.Remove(item);
            else
            {
                isCorrect = false;
                break;
            }
        }

        if (isCorrect)
        {
            Debug.Log("Mission Complete!");
            currentItems.Clear();
            ExitMiniGame();

            if (NPC.ActiveNPC != null)
                NPC.ActiveNPC.FinishMission(true);
        }
        else
        {
            Debug.Log("Mission Failed!");
            currentItems.Clear();

            if (NPC.ActiveNPC != null)
            {
                ExitMiniGame();
                NPC.ActiveNPC.FinishMission(false);
            }
        }
    }

    public void ExitMiniGame()
    {
        currentItems.Clear();
        if (marketPanel != null) marketPanel.SetActive(false);
        if (mainGameUI != null) mainGameUI.SetActive(true);
        Debug.Log("Exited market, returned to town.");
    }

    public void OpenCartUI()
    {
        if (cartPanel != null) cartPanel.SetActive(true);
        RefreshCartUI();
    }

    public void RefreshCartUI()
    {
        foreach (Transform child in cartContentParent)
            Destroy(child.gameObject);

        foreach (CartEntry entry in currentItems)
        {
            GameObject newCartObj = Instantiate(cartItemPrefab, cartContentParent);
            CartItemUI cartUI = newCartObj.GetComponent<CartItemUI>();
            if (cartUI != null)
                cartUI.SetupCartItem(entry.itemName, entry.itemIcon, this);
        }
    }

    public void RemoveItemFromCart(string itemName)
    {
        currentItems.RemoveAll(e => e.itemName == itemName);
        Debug.Log(itemName + " removed from cart. Remaining: " + currentItems.Count);
        RefreshCartUI();
    }
}

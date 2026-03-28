using UnityEngine;
using System.Collections.Generic;

public class MarketManager : MonoBehaviour
{
    [Header("Item's the NPC wants")]
    public List<string> npcList = new List<string>();

    [Header("Player's Items")]
    public List<string> currentItems = new List<string>();

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
            currentItems.Clear();
        }
        else
        {
            Debug.Log(" Mission Failed ");
            currentItems.Clear();
        }
    }
}

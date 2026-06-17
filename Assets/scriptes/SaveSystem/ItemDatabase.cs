using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<Item_ScriptableObject> allItems;

    private static ItemDatabase instance;

    public static ItemDatabase Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.Load<ItemDatabase>("ItemDatabase");
            return instance;
        }
    }

    public Item_ScriptableObject GetItemByID(string id)
    {
        foreach (var item in allItems)
        {
            if (item.itemID == id)
                return item;
        }
        Debug.LogWarning($"Item ID '{id}' introuvable dans la database.");
        return null;
    }
}
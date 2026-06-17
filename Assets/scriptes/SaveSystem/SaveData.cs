using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public string sceneName;
    public float posX, posY, posZ;
    public List<string> inventoryItemIDs = new List<string>();

    public bool isEmpty = true;
}
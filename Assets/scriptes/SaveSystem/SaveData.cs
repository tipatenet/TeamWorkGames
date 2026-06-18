using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public string sceneName;
    public float posX, posY, posZ;
    public List<string> inventoryItemIDs = new List<string>();
    public List<string> inventoryUniqueIDs = new List<string>(); // Nouveau

    public List<ScenePickupData> pickedUpItemsByScene = new List<ScenePickupData>();

    public bool isEmpty = true;
}

[Serializable]
public class ScenePickupData
{
    public string sceneName;
    public List<string> pickedUpUniqueIDs = new List<string>();
}
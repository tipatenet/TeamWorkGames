using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int currentSlot = -1;
    public SaveData currentSaveData;

    [HideInInspector] public bool isSceneTransition = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartNewGame(int slot, string startingSceneName)
    {
        currentSlot = slot;
        currentSaveData = new SaveData
        {
            sceneName = startingSceneName,
            posX = 0,
            posY = 0,
            posZ = 0,
            isEmpty = true
        };
        SaveManager.Save(slot, currentSaveData);
        SceneTransitionManager.Instance.GoToScene(startingSceneName, savePlayerState: false);
    }

    public void LoadGame(int slot)
    {
        currentSlot = slot;
        currentSaveData = SaveManager.Load(slot);
        SceneTransitionManager.Instance.GoToScene(currentSaveData.sceneName, savePlayerState: false);
    }

    public void ReturnToMainMenu()
    {
        SceneTransitionManager.Instance.GoToScene("MainMenu", savePlayerState: true);
        currentSlot = -1;
    }

    public void SaveCurrentGame(string overrideSceneName = null)
    {
        if (currentSlot < 0) return;

        isSceneTransition = overrideSceneName != null;


        InventorySystem inv = FindObjectOfType<InventorySystem>();
        if (inv == null)
        {
            Debug.LogError("[SAVE] InventorySystem introuvable dans la scène !");
            return;
        }
        GameObject player = inv.gameObject;
        Debug.Log($"[SAVE] InventorySystem trouvé sur : {player.name}");

        Debug.Log($"[SAVE] Inventaire : {inv.inventory.Count} items, IDs : {string.Join(", ", inv.inventoryUniqueIDs)}");

        currentSaveData.inventoryItemIDs.Clear();
        currentSaveData.inventoryUniqueIDs.Clear();

        foreach (var item in inv.inventory)
            currentSaveData.inventoryItemIDs.Add(item.itemID);

        foreach (var uid in inv.inventoryUniqueIDs)
            currentSaveData.inventoryUniqueIDs.Add(uid);

        Vector3 pos = player.transform.position;
        currentSaveData.posX = pos.x;
        currentSaveData.posY = pos.y;
        currentSaveData.posZ = pos.z;
        currentSaveData.sceneName = overrideSceneName ?? SceneManager.GetActiveScene().name;

        SaveManager.Save(currentSlot, currentSaveData);
        Debug.Log($"[SAVE] Sauvegarde écrite — scène : {currentSaveData.sceneName}, items : {currentSaveData.inventoryItemIDs.Count}");
    }
    public void RegisterPickedUpItem(string uniqueID)
    {
        string currentScene = SceneManager.GetActiveScene().name;

        ScenePickupData sceneData = currentSaveData.pickedUpItemsByScene
            .Find(s => s.sceneName == currentScene);

        if (sceneData == null)
        {
            sceneData = new ScenePickupData { sceneName = currentScene };
            currentSaveData.pickedUpItemsByScene.Add(sceneData);
        }

        if (!sceneData.pickedUpUniqueIDs.Contains(uniqueID))
        {
            sceneData.pickedUpUniqueIDs.Add(uniqueID);
        }
    }

    public void UnregisterPickedUpItem(string uniqueID)
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Cherche dans TOUTES les scènes, pas seulement l'active, au cas où l'item viendrait d'ailleurs
        foreach (var sceneData in currentSaveData.pickedUpItemsByScene)
        {
            if (sceneData.pickedUpUniqueIDs.Contains(uniqueID))
            {
                sceneData.pickedUpUniqueIDs.Remove(uniqueID);
                return;
            }
        }
    }
}
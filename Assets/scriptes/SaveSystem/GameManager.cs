using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int currentSlot = -1;
    public SaveData currentSaveData;

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

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player introuvable via tag 'Player' !");
            return;
        }

        Vector3 pos = player.transform.position;
        currentSaveData.posX = pos.x;
        currentSaveData.posY = pos.y;
        currentSaveData.posZ = pos.z;

        // Si on transite vers une nouvelle scène, on enregistre la cible plutôt que la scène active actuelle
        currentSaveData.sceneName = overrideSceneName ?? SceneManager.GetActiveScene().name;

        InventorySystem inv = player.GetComponent<InventorySystem>();
        if (inv != null)
        {
            currentSaveData.inventoryItemIDs.Clear();
            currentSaveData.inventoryUniqueIDs.Clear();

            foreach (var item in inv.inventory)
            {
                currentSaveData.inventoryItemIDs.Add(item.itemID);
            }

            foreach (var uid in inv.inventoryUniqueIDs)
            {
                currentSaveData.inventoryUniqueIDs.Add(uid);
            }
        }

        SaveManager.Save(currentSlot, currentSaveData);
        currentSaveData.isEmpty = false;
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
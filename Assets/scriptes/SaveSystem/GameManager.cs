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
            isEmpty = false
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

        GameObject player = GameObject.FindGameObjectWithTag("Playere");
        if (player == null)
        {
            Debug.LogError("Player introuvable via tag 'Playere' !");
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
            foreach (var item in inv.inventory)
            {
                currentSaveData.inventoryItemIDs.Add(item.itemID);
            }
        }

        SaveManager.Save(currentSlot, currentSaveData);
    }
}
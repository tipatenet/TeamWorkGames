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
        SceneManager.LoadScene(startingSceneName);
    }

    public void LoadGame(int slot)
    {
        currentSlot = slot;
        currentSaveData = SaveManager.Load(slot);
        SceneManager.LoadScene(currentSaveData.sceneName);
    }

    public void SaveCurrentGame()
    {
        if (currentSlot < 0) return;

        GameObject player = GameObject.FindGameObjectWithTag("Playere");
        if (player != null)
        {
            Vector3 pos = player.transform.position;
            currentSaveData.posX = pos.x;
            currentSaveData.posY = pos.y;
            currentSaveData.posZ = pos.z;
        }

        currentSaveData.sceneName = SceneManager.GetActiveScene().name;

        InventorySystem inv = player.GetComponent<InventorySystem>();
        if (inv != null)
        {
            currentSaveData.inventoryItemIDs.Clear();
            foreach (var item in inv.inventory)
            {
                currentSaveData.inventoryItemIDs.Add(item.itemID); // À adapter selon ton champ réel
            }
        }

        SaveManager.Save(currentSlot, currentSaveData);
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSaveLoader : MonoBehaviour
{
    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyLoadedData();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyLoadedData();
    }

    void ApplyLoadedData()
    {
        if (GameManager.Instance == null || GameManager.Instance.currentSaveData == null)
            return;

        SaveData data = GameManager.Instance.currentSaveData;

        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        Debug.Log($"[SPAWN] data.isEmpty = {data.isEmpty}, data.sceneName = '{data.sceneName}', currentSceneName = '{currentSceneName}'");

        Vector3 targetPosition;

        bool useSpawnPoint = data.isEmpty || data.sceneName != currentSceneName;

        Debug.Log($"[SPAWN] useSpawnPoint = {useSpawnPoint}");

        if (useSpawnPoint)
        {
            targetPosition = GetSpawnPointPosition();
            Debug.Log($"[SPAWN] Position du SpawnPoint utilisée : {targetPosition}");

            data.isEmpty = false;

            if (GameManager.Instance.currentSlot >= 0)
            {
                SaveManager.Save(GameManager.Instance.currentSlot, data);
            }
        }
        else
        {
            targetPosition = new Vector3(data.posX, data.posY, data.posZ);
            Debug.Log($"[SPAWN] Position sauvegardée utilisée : {targetPosition}");
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.position = targetPosition;
            rb.transform.position = targetPosition;
            Debug.Log($"[SPAWN] Position appliquée via Rigidbody : {rb.position}");
        }
        else
        {
            transform.position = targetPosition;
            Debug.Log($"[SPAWN] Position appliquée via transform : {transform.position}");
        }

        InventorySystem inv = GetComponent<InventorySystem>();
        if (inv != null)
        {
            inv.inventory.Clear();
            inv.inventoryUniqueIDs.Clear();

            foreach (string id in data.inventoryItemIDs)
            {
                Item_ScriptableObject itemAsset = ItemDatabase.Instance.GetItemByID(id);
                if (itemAsset != null)
                    inv.inventory.Add(itemAsset);
            }

            foreach (string uid in data.inventoryUniqueIDs)
            {
                inv.inventoryUniqueIDs.Add(uid);
            }

            inv.currentInventorySize = inv.inventory.Count;
            inv.selectedIndex = Mathf.Clamp(inv.selectedIndex, 0, Mathf.Max(0, inv.currentInventorySize - 1));
            inv.UpdateUI();

            HandAnimation handAnim = GameObject.FindGameObjectWithTag("Hand")?.GetComponent<HandAnimation>();
            if (handAnim != null)
            {
                handAnim.HoldAnimation();
            }
        }
    }

    private Vector3 GetSpawnPointPosition()
    {
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");

        if (spawnPoint != null)
        {
            return spawnPoint.transform.position;
        }

        Debug.LogWarning("Aucun SpawnPoint trouvé dans cette scène, position par défaut (0,0,0) utilisée.");
        return Vector3.zero;
    }
}
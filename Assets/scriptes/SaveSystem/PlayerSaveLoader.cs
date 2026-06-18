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
        Debug.Log("ApplyLoadedData appelé");

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance est NULL");
            return;
        }

        if (GameManager.Instance.currentSaveData == null)
        {
            Debug.LogError("currentSaveData est NULL");
            return;
        }

        SaveData data = GameManager.Instance.currentSaveData;
        Debug.Log($"Position à charger : {data.posX}, {data.posY}, {data.posZ}, scène : {data.sceneName}, items : {data.inventoryItemIDs.Count}");

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; // reset la vélocité accumulée
            rb.position = new Vector3(data.posX, data.posY, data.posZ);
            rb.transform.position = new Vector3(data.posX, data.posY, data.posZ); // double sécurité
        }
        else
        {
            transform.position = new Vector3(data.posX, data.posY, data.posZ);
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

            Debug.Log($"[LOAD] currentInventorySize = {inv.currentInventorySize}, selectedIndex = {inv.selectedIndex}");

            HandAnimation handAnim = GameObject.FindGameObjectWithTag("Hand")?.GetComponent<HandAnimation>();

            if (handAnim == null)
            {
                Debug.LogError("[LOAD] HandAnimation introuvable sur l'objet taggé 'Hand' !");
            }
            else
            {
                Debug.Log("[LOAD] Appel de HoldAnimation()");
                handAnim.HoldAnimation();
                Debug.Log("[LOAD] HoldAnimation() terminé");
            }
        }
        else
        {
            Debug.LogError("[LOAD] InventorySystem introuvable !");
        }
    }
}
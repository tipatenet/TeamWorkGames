using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePickupLoader : MonoBehaviour
{
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RemoveAlreadyPickedUpItems();
    }

    private void RemoveAlreadyPickedUpItems()
    {
        if (GameManager.Instance == null || GameManager.Instance.currentSaveData == null)
            return;

        string currentScene = SceneManager.GetActiveScene().name;

        ScenePickupData sceneData = GameManager.Instance.currentSaveData.pickedUpItemsByScene
            .Find(s => s.sceneName == currentScene);

        if (sceneData == null) return;

        Item[] allItemsInScene = FindObjectsOfType<Item>();

        foreach (Item item in allItemsInScene)
        {
            if (sceneData.pickedUpUniqueIDs.Contains(item.uniqueID))
            {
                Destroy(item.gameObject);
            }
        }
    }
}
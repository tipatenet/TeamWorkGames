using System.Collections.Generic;
using UnityEngine;

public class PianoSheetSlot : MonoBehaviour
{
    [Header("Configuration")]
    public int slotIndex;
    public Transform displayPoint;
    public string requiredItemID = "IndiceEnint";

    [Header("Références")]
    public PianoInteractable piano;
    public InventorySystem inventory;

    [Header("Interaction")]
    public GameObject placePromptUI;   // (optionnel) UI "P : Poser"

    [Header("Affichage")]
    public Vector3 sheetScale = new Vector3(0.3f, 0.3f, 0.3f); // Taille de la feuille posée
    public Vector3 sheetRotationOffset = Vector3.zero;          // Rotation supplémentaire si besoin

    [Header("Main du joueur")]
    public Transform handItemHolder;   // Glisser le HoldPosition / BrasJoueur ici

    [HideInInspector] public bool isFilled = false;
    private GameObject displayedSheet;

    void Start()
    {
        if (displayPoint == null)
            displayPoint = transform;
    }

    public bool TryPlaceSheet()
    {
        if (isFilled) return false;
        if (inventory == null || inventory.currentInventorySize == 0) return false;

        Item_ScriptableObject currentItem = inventory.inventory[inventory.selectedIndex];
        if (currentItem == null) return false;

        if (!string.IsNullOrEmpty(requiredItemID)
            && currentItem.itemID != requiredItemID
            && currentItem.name != requiredItemID)
        {
            Debug.Log($"[PianoSlot] Mauvais item : {currentItem.name} (attendu : {requiredItemID})");
            return false;
        }

        // Instancier la feuille sur le slot avec une petite taille fixe
        Quaternion rot = displayPoint.rotation * Quaternion.Euler(sheetRotationOffset);
        displayedSheet = Instantiate(currentItem.goItem, displayPoint.position, rot);
        displayedSheet.transform.SetParent(displayPoint);
        displayedSheet.transform.localScale = sheetScale;

        // Désactiver physics et interactions sur l'objet posé
        foreach (var rb in displayedSheet.GetComponentsInChildren<Rigidbody>())
            rb.isKinematic = true;
        foreach (var col in displayedSheet.GetComponentsInChildren<Collider>())
            col.enabled = false;
        foreach (var item in displayedSheet.GetComponentsInChildren<Item>())
            Destroy(item);

        // Vider la main : détruire tous les enfants de HoldPosition
        Transform holdTransform = handItemHolder;
        if (holdTransform == null)
        {
            GameObject holdObj = GameObject.Find("HoldPosition");
            if (holdObj != null) holdTransform = holdObj.transform;
        }
        if (holdTransform != null)
        {
            List<GameObject> toDestroy = new List<GameObject>();
            foreach (Transform child in holdTransform)
                toDestroy.Add(child.gameObject);
            foreach (GameObject obj in toDestroy)
                Destroy(obj);
        }

        inventory.inventory.RemoveAt(inventory.selectedIndex);
        inventory.inventoryUniqueIDs.RemoveAt(inventory.selectedIndex);
        inventory.currentInventorySize--;
        inventory.selectedIndex = Mathf.Clamp(inventory.selectedIndex, 0, Mathf.Max(0, inventory.currentInventorySize - 1));
        inventory.UpdateUI();

        isFilled = true;

        if (placePromptUI != null) placePromptUI.SetActive(false);

        if (piano != null) piano.OnSheetPlaced(slotIndex);

        Debug.Log($"[PianoSlot] Partition posée sur l'emplacement {slotIndex}.");
        return true;
    }

    public void RemoveSheet()
    {
        if (displayedSheet != null) Destroy(displayedSheet);
        isFilled = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isFilled ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.2f);
    }
}
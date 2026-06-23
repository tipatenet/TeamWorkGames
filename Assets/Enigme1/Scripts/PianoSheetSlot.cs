using UnityEngine;

public class PianoSheetSlot : MonoBehaviour
{
    [Header("Configuration")]
    public int slotIndex;
    public Transform displayPoint;
    public string requiredItemID = "partition";

    [Header("Références")]
    public PianoInteractable piano;
    public InventorySystem inventory;

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

        if (!string.IsNullOrEmpty(requiredItemID) && currentItem.itemID != requiredItemID)
            return false;

        displayedSheet = Instantiate(currentItem.goItem, displayPoint.position, displayPoint.rotation);
        displayedSheet.transform.SetParent(displayPoint);
        displayedSheet.transform.localScale = Vector3.one * currentItem.item_scaleFactor;

        Rigidbody rb = displayedSheet.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        Collider col = displayedSheet.GetComponent<Collider>();
        if (col != null) col.enabled = false;
        Item itemComp = displayedSheet.GetComponent<Item>();
        if (itemComp != null) Destroy(itemComp);

        inventory.inventory.RemoveAt(inventory.selectedIndex);
        inventory.inventoryUniqueIDs.RemoveAt(inventory.selectedIndex);
        inventory.currentInventorySize--;
        inventory.selectedIndex = Mathf.Clamp(inventory.selectedIndex, 0, Mathf.Max(0, inventory.currentInventorySize - 1));
        inventory.UpdateUI();

        isFilled = true;
        Debug.Log($"[PianoSlot] Partition posée sur l'emplacement {slotIndex}.");
        return true;
    }
}

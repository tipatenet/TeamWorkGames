using Microsoft.Unity.VisualStudio.Editor;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;
using static UnityEngine.GraphicsBuffer;

public class InventorySystem : MonoBehaviour
{
    public Interact interaction;
    public PlayerInputHandler keySystem;
    private Item_ScriptableObject itemPickUp;
    public float scroolValue = 0f;
    public int inventoryMaxSize = 10;
    public List<Item_ScriptableObject> inventory;
    public RectTransform item_Container;
    public int selectedIndex = 0;
    private float targetX;
    public float scrollSpeed;
    public int currentInventorySize = 0;
    private float cooldownTime = 1f;
    private bool canInteract = true;
    private UnityEngine.UI.Image currentIcon;
    private bool canDropItem = true;

    private void Start()
    {
        interaction = GetComponent<Interact>();
        keySystem = GetComponent<PlayerInputHandler>();
    }
    private void Update()
    {
        PickUpItem();
        DropItem();
        scroolValue = keySystem.ScrollInventory.y;
        ScroolInventory();

        if (keySystem.InteractPressed && canInteract)
        {
            StartCoroutine(InteractCooldown());
        }

        if (keySystem.DropItemPressed && canDropItem)
        {
            StartCoroutine(DropItemCooldown());
        }
    }

    //Fonction qui permet au joueur de ramasser un item
    private void PickUpItem()
    {
        RaycastHit hit = interaction.hitInteract;

        if (keySystem.InteractPressed && canInteract)
        {
            if (hit.collider != null && hit.collider.tag == "item") // Vérifie si l'objet toucher à le tag item
            {
                Item itemTouch = hit.collider.GetComponent<Item>();

                Item_ScriptableObject itemHit = itemTouch.info;

                if (!inventoryFull()) //Vérifie qu'il y a de la place dans l'inventaire
                {
                    if (currentInventorySize != 0)
                    {
                        selectedIndex = currentInventorySize;
                        ScroolInventory();
                    }

                    inventory.Add(itemHit);
                    currentInventorySize++;
                    UpdateUI();
                    Destroy(hit.collider.gameObject);

                }
            }
        }
    }

    private void DropItem()
    {
        if (keySystem.DropItemPressed && canDropItem)
        {
            if(currentInventorySize != 0)
            {
                print("Drop item");
                Item_ScriptableObject itemToDrop = inventory[selectedIndex];
                inventory.RemoveAt(selectedIndex);
                currentInventorySize--;
                selectedIndex--;
                ScroolInventory();
                UpdateUI();
                Vector3 dropPosition = interaction.cameraPosition;
                dropPosition.y += 20;
                itemToDrop.goItem.AddComponent<Rigidbody>();
                Instantiate(itemToDrop.goItem, dropPosition, Quaternion.identity);
            }
        }
    }

    private void UpdateUI()
    {
        for(int i = 0; i < currentInventorySize; i++)
        {
            currentIcon = item_Container.GetChild(i).GetComponent<UnityEngine.UI.Image>();
            currentIcon.sprite = inventory[i].icon;
            currentIcon.enabled = true;
        }
    }

    private bool inventoryFull()
    {
        if (inventory.Count == inventoryMaxSize)
        {
            return true;
        }
        return false;
    }

    private void ScroolInventory()
    {
        if (scroolValue > 0)
            selectedIndex--;
        else if (scroolValue < 0 && selectedIndex+1 < currentInventorySize)
            selectedIndex++;
        selectedIndex = Mathf.Clamp(selectedIndex, 0,inventoryMaxSize);

        float itemWidth = 100f + 10;
        targetX = -selectedIndex * itemWidth;
        Vector2 currentPos = item_Container.anchoredPosition;
        currentPos.x = Mathf.Lerp(currentPos.x, targetX, Time.deltaTime * scrollSpeed);
        item_Container.anchoredPosition = currentPos;

    }

    IEnumerator InteractCooldown()
    {
        canInteract = false;
        yield return new WaitForSeconds(cooldownTime);
        canInteract = true;
    }

    IEnumerator DropItemCooldown()
    {
        canDropItem = false;
        yield return new WaitForSeconds(cooldownTime);
        canDropItem = true;
    }
}

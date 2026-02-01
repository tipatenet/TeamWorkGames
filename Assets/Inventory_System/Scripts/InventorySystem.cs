using Microsoft.Unity.VisualStudio.Editor;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEditor.Progress;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
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
    private Transform cameraTransform;
    private float dropDistance = 1f;


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
                Item_ScriptableObject itemToDrop = inventory[selectedIndex];
                inventory.RemoveAt(selectedIndex);
                currentInventorySize--;
                selectedIndex--;
                ScroolInventory();
                UpdateUI();
                interaction.IsInteractive();
                Vector3 dropPosition;
                if (interaction.hitInteract.collider)
                {
                    dropPosition = Camera.main.transform.position + Camera.main.transform.forward * 0.45f;
                }
                else
                {
                    dropPosition = Camera.main.transform.position + Camera.main.transform.forward * dropDistance;
                }

                GameObject droppedObj = Instantiate(itemToDrop.goItem, dropPosition, Quaternion.identity);

                itemToDrop.goItem.GetComponent<Rigidbody>().constraints &= ~RigidbodyConstraints.FreezePositionY;
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
        if(currentInventorySize == 0)
        {
            item_Container.GetChild(0).GetComponent<UnityEngine.UI.Image>().enabled = false;
        }
        else
        {
            item_Container.GetChild(0).GetComponent<UnityEngine.UI.Image>().enabled = true;
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

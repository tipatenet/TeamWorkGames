using Microsoft.Unity.VisualStudio.Editor;
using System.Collections;
using System.Collections.Generic;
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

    private void Start()
    {
        interaction = GetComponent<Interact>();
        keySystem = GetComponent<PlayerInputHandler>();
    }
    private void Update()
    {
        PickUpItem();
        scroolValue = keySystem.ScrollInventory.y;
        ScroolInventory();

        if (keySystem.InteractPressed && canInteract)
        {
            StartCoroutine(InteractCooldown());
        }
    }

    //Fonction qui permet au joueur de ramasser un item
    public void PickUpItem()
    {

        RaycastHit hit = interaction.IsInteractive();

        if (keySystem.InteractPressed && canInteract)
        {
            if (hit.collider.tag == "item") // Vérifie si l'objet toucher à le tag item
            {
                Item itemTouch = hit.collider.GetComponent<Item>();

                Item_ScriptableObject itemHit = itemTouch.info;

                if (!inventoryFull()) //Vérifie qu'il y a de la place dans l'inventaire
                {
                    if (currentInventorySize != 0)
                    {
                        selectedIndex++;
                        ScroolInventory();
                    }

                    inventory.Add(itemHit);
                    currentInventorySize++;
                    currentIcon = item_Container.GetChild(selectedIndex).GetComponent<UnityEngine.UI.Image>();
                    currentIcon.sprite = itemHit.icon;
                    currentIcon.enabled = true;
                    Destroy(hit.collider.gameObject);

                }
            }
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
}

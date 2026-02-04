using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    //Variables Pulique :
    public int inventoryMaxSize = 10;
    public RectTransform item_Container;
    public float scrollSpeed = 20f;

    //Variables Privées :
    private float targetX;
    private float cooldownTime = 0.5f;
    private bool canInteract = true;
    private UnityEngine.UI.Image currentIcon;
    private bool canDropItem = true;
    private Transform cameraTransform;
    private float dropDistance = 1f;
    private float distanceHitWall = 1f;
    private Item_ScriptableObject itemPickUp;
    private float scroolValue = 0f;
    private Interact interaction;
    private PlayerInputHandler keySystem;

    //Pas touche :
    public int selectedIndex = 0;
    public int currentInventorySize = 0;
    public List<Item_ScriptableObject> inventory;


    private void Start()
    {
        //Récupère directement sur le Player les scripts : ATTENTION ils faut les ajoutées !!!
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

    //Fonction qui permet de drop les items
    private void DropItem()
    {
        if (keySystem.DropItemPressed && canDropItem)
        {
            if(currentInventorySize != 0)
            {
                Item_ScriptableObject itemToDrop = inventory[selectedIndex];
                inventory.RemoveAt(selectedIndex);
                currentInventorySize--;

                if (currentInventorySize <= 0)
                    selectedIndex = 0;
                else
                    selectedIndex = Mathf.Clamp(selectedIndex, 0, currentInventorySize - 1);

                UpdateUI();
                ScroolInventory();
                interaction.IsInteractive(false);
                Vector3 dropPosition;
                RaycastHit hitWall;

                Physics.Raycast(interaction.cameraPosition,interaction.cameraRotation, out hitWall, distanceHitWall);
                if (hitWall.collider)
                {
                    dropPosition = interaction.cameraPosition + interaction.cam.transform.forward * 0.45f;
                }
                else
                {
                    dropPosition = interaction.cameraPosition + interaction.cam.transform.forward * dropDistance;
                }
                GameObject droppedObj = Instantiate(itemToDrop.goItem, dropPosition, Quaternion.identity);

                Rigidbody rb = droppedObj.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.constraints &= ~RigidbodyConstraints.FreezePositionY;
            }
        }
    }

    //Fonction qui permet d'actualisée l'UI du canvas
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

    //Check pour savoir si l'inventaire est plein
    private bool inventoryFull()
    {
        if (inventory.Count == inventoryMaxSize)
        {
            return true;
        }
        return false;
    }


    //Permet de scroller dans l'inventaire
    private void ScroolInventory()
    {
        if (scroolValue > 0)
            selectedIndex--;
        else if (scroolValue < 0 && selectedIndex+1 < currentInventorySize)
            selectedIndex++;

        selectedIndex = Mathf.Clamp(selectedIndex, 0,currentInventorySize);


        float itemWidth = 100f + 10;
        targetX = -selectedIndex * itemWidth;
        Vector2 currentPos = item_Container.anchoredPosition;
        currentPos.x = Mathf.Lerp(currentPos.x, targetX, Time.deltaTime * scrollSpeed);
        item_Container.anchoredPosition = currentPos;
    }

    //Les coolDown pour les inputs :
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

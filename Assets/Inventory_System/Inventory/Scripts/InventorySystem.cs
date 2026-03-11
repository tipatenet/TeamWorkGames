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
    public float cooldownTime = 1f;
    public AudioSource source;
    public AudioClip DropSound;
    public AudioClip PickUpSound;
    [SerializeField] private HandAnimation handAnimation;

    //Variables Privées :
    private float targetX;
    private bool canPickOrDrop = true;
    private UnityEngine.UI.Image currentIcon;
    private Transform cameraTransform;
    private float dropDistance = 1f;
    private float distanceHitWall = 1f;
    private Item_ScriptableObject itemPickUp;
    private float scroolValue = 0f;
    private Interact interaction;
    private PlayerInputHandler keySystem;
    private bool canScroll = true;

    //Pas touche :
    public int selectedIndex = 0;
    public int currentInventorySize = 0;
    public List<Item_ScriptableObject> inventory;


    private void Start()
    {
        //Récupère directement sur le Player les scripts : ATTENTION ils faut les ajoutées !!!
        interaction = GetComponent<Interact>();
        keySystem = GetComponent<PlayerInputHandler>();
        source = this.gameObject.GetComponent<AudioSource>();
    }
    private void Update()
    {
        scroolValue = keySystem.ScrollInventory.y;
        ScroolInventory();

        if (keySystem.InteractPressed && canPickOrDrop)
        {
            canPickOrDrop = false; // bloque tout de suite
            PickUpItem();
            StartCoroutine(PickOrDropCooldown());
        }

        if (keySystem.DropItemPressed && canPickOrDrop)
        {
            canPickOrDrop = false; // bloque tout de suite
            DropItem();
            StartCoroutine(PickOrDropCooldown());
        }
    }

    //Fonction qui permet au joueur de ramasser un item
    private void PickUpItem()
    {
        RaycastHit hit = interaction.hitInteract;

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
                source.PlayOneShot(PickUpSound);
                //Joue les animations pour les mains
                handAnimation.PlayPickAnim();
                handAnimation.HoldAnimation();
            }
        }
    }

    //Fonction qui permet de drop les items
    private void DropItem()
    {
        if (currentInventorySize != 0)
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

            Physics.Raycast(interaction.cameraPosition, interaction.cameraRotation, out hitWall, distanceHitWall);
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
            source.PlayOneShot(DropSound);
            //Joue les animations pour les mains
            handAnimation.PlayDropAnim();
            handAnimation.HoldAnimation();
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
        if (canScroll)
        {
            if (scroolValue > 0)
            {
                selectedIndex--;
                selectedIndex = Mathf.Clamp(selectedIndex, 0, currentInventorySize);
                //Joue les animations pour les mains
                if (currentInventorySize > 1)
                {
                    handAnimation.PlayDropAnim();
                    handAnimation.HoldAnimation();
                }
            }
            else if (scroolValue < 0 && selectedIndex + 1 < currentInventorySize)
            {
                selectedIndex++;
                selectedIndex = Mathf.Clamp(selectedIndex, 0, currentInventorySize);
                //Joue les animations pour les mains
                if (currentInventorySize > 1)
                {
                    handAnimation.PlayDropAnim();
                    handAnimation.HoldAnimation();
                }
            }


            float itemWidth = 100f + 10;
            targetX = -selectedIndex * itemWidth;
            Vector2 currentPos = item_Container.anchoredPosition;
            currentPos.x = Mathf.Lerp(currentPos.x, targetX, Time.deltaTime * scrollSpeed);
            item_Container.anchoredPosition = currentPos;
        }
    }

    //Colldown pour pick up et drop item
    IEnumerator PickOrDropCooldown()
    {
        yield return new WaitForSeconds(cooldownTime);
        canPickOrDrop = true;
    }
}

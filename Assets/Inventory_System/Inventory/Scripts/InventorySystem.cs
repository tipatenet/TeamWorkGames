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
    public Text textNameZone;

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
    public List<string> inventoryUniqueIDs = new List<string>(); // Nouveau : trace l'instance d'origine


    private void Start()
    {
        //Récupère directement sur le Player les scripts : ATTENTION ils faut les ajoutées !!!
        interaction = GetComponent<Interact>();
        keySystem = GetComponent<PlayerInputHandler>();
        source = this.gameObject.GetComponent<AudioSource>();
    }
    public void Update()
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

        if (hit.collider != null && hit.collider.tag == "item")
        {
            Item itemTouch = hit.collider.GetComponent<Item>();
            Item_ScriptableObject itemHit = itemTouch.info;

            if (!inventoryFull())
            {
                if (currentInventorySize != 0)
                {
                    selectedIndex = currentInventorySize;
                    ScroolInventory();
                }

                inventory.Add(itemHit);
                inventoryUniqueIDs.Add(itemTouch.uniqueID); // Ajouté en parallèle
                currentInventorySize++;
                UpdateUI();

                GameManager.Instance.RegisterPickedUpItem(itemTouch.uniqueID);

                Destroy(hit.collider.gameObject);
                source.PlayOneShot(PickUpSound);
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
            string uniqueIDToDrop = inventoryUniqueIDs[selectedIndex];

            inventory.RemoveAt(selectedIndex);
            inventoryUniqueIDs.RemoveAt(selectedIndex); // Retiré en parallèle, même index
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

            // L'item original redevient "non ramassé" : il réapparaîtra à sa position d'origine au prochain chargement
            GameManager.Instance.UnregisterPickedUpItem(uniqueIDToDrop);

            source.PlayOneShot(DropSound);
            handAnimation.PlayDropAnim();
            handAnimation.HoldAnimation();
        }
    }

    //Fonction qui permet d'actualisée l'UI du canvas
    public void UpdateUI()
    {
        for(int i = 0; i < currentInventorySize; i++)
        {
            currentIcon = item_Container.GetChild(i).GetComponent<UnityEngine.UI.Image>();
            currentIcon.sprite = inventory[i].icon;
            currentIcon.enabled = true;
            textNameZone.text = "";
            textNameZone.text = inventory[i].item_Name;
        }
        if(currentInventorySize == 0)
        {
            item_Container.GetChild(0).GetComponent<UnityEngine.UI.Image>().enabled = false;
            textNameZone.text = "";
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
            int previousIndex = selectedIndex;

            if (scroolValue > 0)
            {
                selectedIndex--;
                selectedIndex = Mathf.Clamp(selectedIndex, 0, currentInventorySize - 1);
            }
            else if (scroolValue < 0)
            {
                selectedIndex++;
                selectedIndex = Mathf.Clamp(selectedIndex, 0, currentInventorySize - 1);
            }

            // Si l'item a changé alors jouer l'animation
            if (previousIndex != selectedIndex)
            {
                if (currentInventorySize > 1)
                {
                    handAnimation.PlayDropAnim();
                    handAnimation.HoldAnimation();
                    textNameZone.text = "";
                    textNameZone.text = inventory[selectedIndex].item_Name;
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

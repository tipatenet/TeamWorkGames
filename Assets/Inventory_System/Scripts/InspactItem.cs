using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Script qui permet d'inspecter l'object
public class InspactItem : MonoBehaviour
{
    //Variables Publique :
    public GameObject holdPosition;

    //Variables Privées :
    private bool canInspact = true;
    private float cooldownTime = 1f;
    private GameObject go;
    public bool isInspact = false;
    private float rotationSpeed = 10f;
    private Vector2 currentRotation;
    private InventorySystem inventory;
    private PlayerInputHandler keySystem;
    private Interact interact;

    private void Start()
    {
        //Permet de récupérer les scripts sur le Player automatiquement au lancement du jeu :
        inventory = GetComponent<InventorySystem>();
        keySystem = GetComponent<PlayerInputHandler>();
        interact = GetComponent<Interact>();
    }

    private void Update()
    {
        OpenInspactMenu();
        UpdatePosition();
    }

    //Permet d'instantier l'objet devant le joueur
    public void InstantiateItem()
    {
        go = Instantiate(inventory.inventory[inventory.selectedIndex].goItem, holdPosition.transform.position, Quaternion.identity);
        go.transform.SetParent(holdPosition.transform);
        go.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        go.GetComponent<Rigidbody>().useGravity = false;

    }

    //Permet de mettre à jour la postion de l'objet et la rotation
    private void UpdatePosition()
    {
        if (isInspact && go != null)
        {
            go.transform.position = holdPosition.transform.position;
            RotateItem();
        }
    }

    //Permet d'ouvrir le menu
    private void OpenInspactMenu()
    {
        if (inventory.currentInventorySize == 0)
            return;

        if (keySystem.InspactItem && canInspact)
        {
            StartCoroutine(InspactItemCooldown());

            if (inventory.currentInventorySize == 0)
                canInspact = false;

            if (!isInspact)
            {
                InstantiateItem();
                isInspact = true;
                interact.stopRaycast = true;
                keySystem.LockGameplayInputs(true);
            }
            else
            {
                Destroy(go);
                isInspact = false;
                interact.stopRaycast = false;
                keySystem.LockGameplayInputs(false);
            }
        }
    }

    //Permet de faire tourner l'item en main
    private void RotateItem()
    {
        Vector2 look = keySystem.RotateItemInspact;

        currentRotation.x += look.x * rotationSpeed * Time.deltaTime;
        currentRotation.y -= look.y * rotationSpeed * Time.deltaTime;

        go.transform.rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0f);
    }

    //CoolDown de la toiche inspecter
    IEnumerator InspactItemCooldown()
    {
        canInspact = false;
        yield return new WaitForSeconds(cooldownTime);
        canInspact = true;
    }
}

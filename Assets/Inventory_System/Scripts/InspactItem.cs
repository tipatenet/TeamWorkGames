using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InspactItem : MonoBehaviour
{
    public GameObject holdPosition;
    public InventorySystem inventory;
    public PlayerInputHandler keySystem;

    private bool canInspact = true;
    private float cooldownTime = 1f;
    private GameObject go;
    private bool isInspact = false;

    private void Start()
    {
        
    }

    private void Update()
    {
        OpenInspactMenu();
        if (keySystem.InspactItem && canInspact)
        {
            StartCoroutine(InspactItemCooldown());
        }
        UpdatePosition();

    }

    public void InstantiateItem()
    {
        go = Instantiate(inventory.inventory[inventory.selectedIndex].goItem, holdPosition.transform.position, Quaternion.identity);
        go.transform.SetParent(holdPosition.transform);
        go.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        go.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        go.GetComponent<Rigidbody>().useGravity = false;

    }

    private void UpdatePosition()
    {
        if (isInspact && go != null)
        {
            go.transform.position = holdPosition.transform.position;
        }
    }

    private void OpenInspactMenu()
    {
        if (keySystem.InspactItem && canInspact)
        {
            StartCoroutine(InspactItemCooldown());

            if (!isInspact)
            {
                InstantiateItem();
                isInspact = true;
            }
            else
            {
                Destroy(go);
                isInspact = false;
            }
        }
    }

    IEnumerator InspactItemCooldown()
    {
        canInspact = false;
        yield return new WaitForSeconds(cooldownTime);
        canInspact = true;
    }
}

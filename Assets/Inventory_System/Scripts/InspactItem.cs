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
    private float rotationSpeed = 10f;
    private Vector2 currentRotation;

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
            RotateItem();
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
                keySystem.LockGameplayInputs(true);
            }
            else
            {
                Destroy(go);
                isInspact = false;
                keySystem.LockGameplayInputs(false);
            }
        }
    }

    private void RotateItem()
    {
        Vector2 look = keySystem.RotateItemInspact;

        currentRotation.x += look.x * rotationSpeed * Time.deltaTime;
        currentRotation.y -= look.y * rotationSpeed * Time.deltaTime;

        go.transform.rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0f);
    }

    IEnumerator InspactItemCooldown()
    {
        canInspact = false;
        yield return new WaitForSeconds(cooldownTime);
        canInspact = true;
    }
}

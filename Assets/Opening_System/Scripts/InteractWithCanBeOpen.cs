using System.Collections;
using UnityEngine;

public class InteractWithCanBeOpen : MonoBehaviour
{
    public Interact interact;
    public PlayerInputHandler inputs;
    private bool canOpen = true;
    private float cooldownTime = 0.5f;
    private RaycastHit hit;

    private void Update()
    {
        Interact();
    }

    private void Interact()
    {
        if (inputs.InteractPressed && canOpen)
        {
            hit = interact.IsInteractive(false);

            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("canBeOpen"))
                {
                    Opening_System opening = hit.collider.GetComponent<Opening_System>();

                    if (opening != null)
                    {
                        opening.StartOpeningOrClose();
                        StartCoroutine(InteractCooldown());
                    }
                }
            }
        }
    }


    IEnumerator InteractCooldown()
    {
        canOpen = false;
        yield return new WaitForSeconds(cooldownTime);
        canOpen = true;
    }
}

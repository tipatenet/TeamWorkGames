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
            if (hit.transform.gameObject.tag == "canBeOpen")
            {
                hit.transform.gameObject.GetComponent<Opening_System>().StartOpeningOrClose();
                StartCoroutine(InteractCooldown());
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

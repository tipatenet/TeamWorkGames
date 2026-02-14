using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CadenasInteraction : MonoBehaviour
{
    public float interactRadius = 2f;
    public LayerMask player;
    public bool continueToInteract = true;
    public PlayerInputHandler keySystem;
    public Interact interact;
    public Camera lookCam;
    public Camera playerCam;

    private bool canInteract = true;
    private float cooldownTime = 0.5f;
    void Start()
    {

    }

    void Update()
    {
        InteractCadenas();
        if (keySystem.InteractPressed && canInteract)
            StartCoroutine(InteractCooldown());
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, interactRadius);
    }

    //Detecte si le joueur est dans la sphere
    bool detectPlayer(bool isActive)
    {
        if (isActive)
        {
            Collider[] hits = Physics.OverlapSphere(this.gameObject.transform.position, interactRadius, player);
            if (hits.Length > 0)
            {
                return true;
            }
        }
        return false;
    }

    void InteractCadenas()
    {
        if (detectPlayer(continueToInteract))
        {
            if (keySystem.InteractPressed && canInteract)
            {
                if (interact.IsInteractive(false).transform.gameObject.tag == "cadenas")
                {
                    print("cadenas hit");
                }
                else if (interact.IsInteractive(false).transform.gameObject.tag == "Code1")
                {
                    print("cadenas hit");
                }
                else if (interact.IsInteractive(false).transform.gameObject.tag == "Code2")
                {
                    print("cadenas hit");
                }
                else if (interact.IsInteractive(false).transform.gameObject.tag == "Code3")
                {
                    print("cadenas hit");
                }
                else if (interact.IsInteractive(false).transform.gameObject.tag == "Code4")
                {
                    print("cadenas hit");
                }
            }
        }
    }

    IEnumerator InteractCooldown()
    {
        canInteract = false;
        yield return new WaitForSeconds(cooldownTime);
        canInteract = true;
    }
}

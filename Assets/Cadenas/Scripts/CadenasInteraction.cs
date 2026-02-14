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
    private bool switchCam = false;
    private float transitionTime = 2f;
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
                    print("switch cam");
                    toogleSwitchCam(ref switchCam);
                    switchCamPos(ref switchCam);
                }
            }
        }
    }

    void toogleSwitchCam(ref bool switchCam)
    {
        if (!switchCam)
            switchCam = true;
        else
            switchCam = false;
    }

    void switchCamPos(ref bool switchCam)
    {
        if (switchCam)
        {
            playerCam.gameObject.SetActive(false);
            lookCam.gameObject.SetActive(true);
            camTransition();
        }
        else
        {
            playerCam.gameObject.SetActive(true);
            lookCam.gameObject.SetActive(false);
        }
    }

    void camTransition()
    {
        StartCoroutine(TransitionCoolDown());
        Vector3 targetPos = lookCam.gameObject.transform.position;
        Vector3 targetRotation = lookCam.transform.eulerAngles;
        lookCam.gameObject.transform.position = playerCam.gameObject.transform.position;
        lookCam.gameObject.transform.eulerAngles = playerCam.gameObject.transform.eulerAngles;
        iTween.MoveTo(lookCam.gameObject,targetPos, transitionTime);
        iTween.RotateTo(lookCam.gameObject, targetRotation, transitionTime*1.5f);
    }

    IEnumerator InteractCooldown()
    {
        canInteract = false;
        yield return new WaitForSeconds(cooldownTime);
        canInteract = true;
    }
    IEnumerator TransitionCoolDown()
    {
        canInteract = false;
        yield return new WaitForSeconds(transitionTime);
        canInteract = true;
    }
}

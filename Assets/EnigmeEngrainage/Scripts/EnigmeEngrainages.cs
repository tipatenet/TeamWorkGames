using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnigmeEngrainages : MonoBehaviour
{
    public PlayerInputHandler input;
    public Interact interactScript;
    public GameObject EngrainageExt;
    public GameObject EngrainageMid;
    public GameObject EngrainageCentre;
    public int codeNum1;
    public int codeNum2;
    public int codeNum3;
    public float SphereInteract = 3f;
    public LayerMask Player;
    bool canInteract = true;
    private float cooldownTime = 0.5f;
    private int currentNum1 = 0;
    private int currentNum2 = 0;
    private int currentNum3 = 0;

    void Start()
    {
        
    }

    void Update()
    {
        RotateEngrainage();

        if (input.InteractPressed && canInteract)
            StartCoroutine(InteractCooldown());
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, SphereInteract);
    }

    void RotateEngrainage()
    {
        if (PlayerCanInteract())
        {
            if (input.InteractPressed && canInteract)
            {
                if (interactScript.IsInteractive(false).transform.gameObject.tag == "ext")
                {
                    iTween.RotateAdd(EngrainageExt, new Vector3(0, 0, 60), 1f);
                }
                else if (interactScript.IsInteractive(false).transform.gameObject.tag == "mid")
                {
                    iTween.RotateAdd(EngrainageMid, new Vector3(0, 0, 90), 1f);
                }
                else if (interactScript.IsInteractive(false).transform.gameObject.tag == "centre")
                {
                    iTween.RotateAdd(EngrainageCentre, new Vector3(0, 0, 180), 1f);
                }
            }
        }
    }

    bool PlayerCanInteract()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, SphereInteract, Player);

        foreach (var hit in hits)
        {
            if (hit.gameObject.layer == 7)
            {
                return true;
            }
        }
        return false;
    }

    IEnumerator InteractCooldown()
    {
        canInteract = false;
        yield return new WaitForSeconds(cooldownTime);
        canInteract = true;
    }
}

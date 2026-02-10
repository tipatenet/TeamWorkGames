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
    public int currentNum1 = 0;
    public int currentNum2 = 0;
    public int currentNum3 = 0;

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
                    IncrementeExt(6, currentNum1);
                }
                else if (interactScript.IsInteractive(false).transform.gameObject.tag == "mid")
                {
                    iTween.RotateAdd(EngrainageMid, new Vector3(0, 0, 90), 1f);
                    IncrementeExt(4, currentNum2);
                }
                else if (interactScript.IsInteractive(false).transform.gameObject.tag == "centre")
                {
                    iTween.RotateAdd(EngrainageCentre, new Vector3(0, 0, 180), 1f);
                    IncrementeExt(2, currentNum3);
                }
            }
            VerifyCode();
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

    void IncrementeExt(int maxReset, int refIncrement)
    {
        if (refIncrement != maxReset)
            refIncrement++;
        else
            refIncrement = 0;
    }

    bool VerifyCode()
    {
        if(currentNum1 == codeNum1)
        {
            if(currentNum2 == codeNum2)
            {
                if(currentNum3 == codeNum3)
                {
                    print("Code bon");
                    return true;
                }
            }
        }
        return false;
    }
}

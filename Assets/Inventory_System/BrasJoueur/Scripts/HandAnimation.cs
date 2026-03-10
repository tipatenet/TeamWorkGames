using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using static UnityEditor.Progress;
using System.Collections;
using System.Collections.Generic;

public class HandAnimation : MonoBehaviour
{
    //Variables Visible dans l'inspector :
    [Header("References Settings")]
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private Transform holdItemPos;
    [SerializeField] private PlayerController player;

    //Variables Privées :
    private Animator anim;
    private bool objectHold = false;
    private bool isPlayingPickAnim;
    private float collDownAnim = 1.2f;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void PlayPickUpDropAnim()
    {
        if (isPlayingPickAnim) return;

        isPlayingPickAnim = true;

        anim.ResetTrigger("GrabbingItem");
        anim.SetTrigger("GrabbingItem");

        StartCoroutine(ResetPickAnim());
    }

    private void Update()
    {
        RuningAnimation();
    }

    public void HoldAnimation()
    {
        if (inventory.currentInventorySize >= 1)
        {
            RemoveItemHold();
            anim.SetBool("ItemInHand", true);
            anim.runtimeAnimatorController = inventory.inventory[inventory.selectedIndex].animatorOverride;
            GameObject go = Instantiate(inventory.inventory[inventory.selectedIndex].goItem);
            go.transform.SetParent(holdItemPos);
            go.transform.localPosition = Vector3.zero + inventory.inventory[inventory.selectedIndex].holdPositionOffset;
            go.transform.localRotation = Quaternion.identity;
            go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            go.GetComponent<Collider>().enabled = false;
            go.GetComponent<Rigidbody>().useGravity = false;
            objectHold = true;
        }
        else
        {
            RemoveItemHold();
            anim.SetBool("ItemInHand", false);
        }
    }

    private void RemoveItemHold()
    {
        if(objectHold)
        Destroy(holdItemPos.GetChild(0).gameObject);
        objectHold = false;
    }

    private void RuningAnimation()
    {
        anim.SetFloat("Speed", player.rb.linearVelocity.magnitude);
    }

    IEnumerator ResetPickAnim()
    {
        yield return new WaitForSeconds(collDownAnim);
        isPlayingPickAnim = false;
    }
}

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
    private bool isPlayingAnim;
    private float collDownAnim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        collDownAnim = inventory.cooldownTime;
}

    public void PlayPickAnim()
    {
        if (isPlayingAnim) return;

        isPlayingAnim = true;

        anim.ResetTrigger("PickUpItem");
        anim.SetTrigger("PickUpItem");

        StartCoroutine(ResetAnim());
    }
    public void PlayDropAnim()
    {
        if (isPlayingAnim) return;

        isPlayingAnim = true;

        anim.ResetTrigger("DropItem");
        anim.SetTrigger("DropItem");

        StartCoroutine(ResetAnim());
    }

    private void Update()
    {
        RunningAnimation();
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
            go.transform.localRotation = inventory.inventory[inventory.selectedIndex].holdRotation;
            go.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            go.GetComponent<Collider>().enabled = false;
            go.GetComponent<Rigidbody>().useGravity = false;
            objectHold = true;
        }
        else
        {
            if(inventory.currentInventorySize < 1)
            anim.SetBool("ItemInHand", false);

            anim.SetTrigger("DropItem");
            RemoveItemHold();
        }
    }

    private void RemoveItemHold()
    {
        if(objectHold)
        Destroy(holdItemPos.GetChild(0).gameObject);
        objectHold = false;
    }

    private void RunningAnimation()
    {
        anim.SetFloat("Speed", player.rb.linearVelocity.magnitude);
    }

    IEnumerator ResetAnim()
    {
        yield return new WaitForSeconds(collDownAnim);
        isPlayingAnim = false;
    }
}

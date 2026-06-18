using UnityEngine;
using System.Collections;

public class HandAnimation : MonoBehaviour
{
    //Variables Visible dans l'inspector :
    [Header("References Settings")]
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private Transform holdItemPos;
    [SerializeField] private PlayerController player;
    public float collDownAnim;

    //Variables Privées :
    private Animator anim;
    private bool objectHold = false;
    private bool isPlayingAnim;

    private void Awake()
    {
        anim = GetComponent<Animator>();

        //Pour que le colldown des animations soit le meme que celui de l'inventaire
        collDownAnim = inventory.cooldownTime;
    }

    //Permet de jouez l'animation pour récuperer l'item
    public void PlayPickAnim()
    {
        if (isPlayingAnim) return;

        isPlayingAnim = true;

        anim.ResetTrigger("PickUpItem");
        anim.SetTrigger("PickUpItem");

        StartCoroutine(ResetAnim());
    }

    //Permet de jouez l'animation pour jeter l'item
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

    //Permet de jouez l'animation pour tenir l'item
    public void HoldAnimation()
    {
        if (inventory.currentInventorySize >= 1)
        {
            RemoveItemHold();
            anim.SetBool("ItemInHand", true); //Permet d'activer l'animation
            anim.runtimeAnimatorController = inventory.inventory[inventory.selectedIndex].animatorOverride;
            GameObject go = Instantiate(inventory.inventory[inventory.selectedIndex].goItem);
            go.transform.SetParent(holdItemPos);
            go.transform.localPosition = Vector3.zero + inventory.inventory[inventory.selectedIndex].holdPositionOffset;
            go.transform.localRotation = inventory.inventory[inventory.selectedIndex].holdRotation;
            go.transform.localScale = go.transform.localScale * inventory.inventory[inventory.selectedIndex].item_scaleFactor;

            //Enleve l'ombre projeter de l'objet en main
            go.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            //Desactiver sont collider pour éviter q'il soit bloquer par un mur ou autres
            go.GetComponent<Collider>().enabled = false;

            //Desactive la gravité de l'objet
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

    //Permet de supprimer l'item en main
    private void RemoveItemHold()
    {
        if(objectHold)
        Destroy(holdItemPos.GetChild(0).gameObject);
        objectHold = false;
    }

    //Permet de changer la vitesse da l'animation en fonction de la vitesse du joueur
    private void RunningAnimation()
    {
        anim.SetFloat("Speed", player.rb.linearVelocity.magnitude);
    }

    public void InspactItem(bool inspact)
    {
        if (inspact)
        {
            anim.SetBool("ItemInHand", false);
        }
        else
        {
            anim.SetBool("ItemInHand", true);
        }
    }

    //Fonction colldown pour éviter de jouer plusieurs animations en meme temps
    IEnumerator ResetAnim()
    {
        yield return new WaitForSeconds(collDownAnim);
        isPlayingAnim = false;
    }
}

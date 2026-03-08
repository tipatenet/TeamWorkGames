using UnityEngine;

public class HandAnimation : MonoBehaviour
{
    private Animator anim;
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    public void PlayPickUpDropAnim()
    {
        anim.ResetTrigger("GrabbingItem");
        anim.SetTrigger("GrabbingItem");
    }
}

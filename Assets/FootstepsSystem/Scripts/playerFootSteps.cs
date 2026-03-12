using System.Collections;
using UnityEngine;

public class playerFootSteps : MonoBehaviour
{


    public AudioClip footStepSFX;

    public float footStepInterval;
    public float footStepVolume;

    private PlayerController playerController;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        StartCoroutine(PlayFootSteps());
    }

    IEnumerator PlayFootSteps()
    {
        while (true)
        {
            if (playerController.moveDirection.magnitude > 0.1f && playerController.Grounded)
            {
                AudioManager.instance.PlaySFX(footStepSFX, footStepVolume);
            }

            yield return new WaitForSeconds(footStepInterval);
        }
    }
}

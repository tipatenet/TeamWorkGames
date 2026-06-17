using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerFootSteps : MonoBehaviour
{
    public AudioClip footStepSFX;

    public float footStepInterval = 0.5f;
    [Range(0f, 1f)]
    public float footStepVolume = 1f;

    private PlayerController playerController;
    private AudioSource audioSource;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        audioSource = GetComponent<AudioSource>();

        StartCoroutine(PlayFootSteps());
    }

    IEnumerator PlayFootSteps()
    {
        while (true)
        {
            if (playerController.moveDirection.magnitude > 0.1f &&
                playerController.Grounded)
            {
                audioSource.PlayOneShot(footStepSFX, footStepVolume);
            }

            yield return new WaitForSeconds(footStepInterval);
        }
    }
}
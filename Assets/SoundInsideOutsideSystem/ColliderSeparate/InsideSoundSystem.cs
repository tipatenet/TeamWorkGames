using UnityEngine;
using UnityEngine.Audio;

public class InsideSoundSystem : MonoBehaviour
{
    public AudioMixerSnapshot outdoorSnapshot;
    public AudioMixerSnapshot indoorSnapshot;
    public float transitionTime = 0.5f;

    private void Start()
    {
        outdoorSnapshot.TransitionTo(transitionTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            indoorSnapshot.TransitionTo(transitionTime);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            outdoorSnapshot.TransitionTo(transitionTime);
    }
}

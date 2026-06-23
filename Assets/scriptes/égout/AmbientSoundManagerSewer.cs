using UnityEngine;

public class AmbientSoundManagerSewer : MonoBehaviour
{
    [Header("Ambiance continue (tunnel)")]
    public AudioSource ambianceSource;
    public AudioClip ambianceClip;

    [Header("Gouttes d'eau (fréquent)")]
    public AudioSource dripsSource;
    public AudioClip[] dripsClips;       // plusieurs sons de gouttes pour varier
    public float dripsMinInterval = 1.5f;
    public float dripsMaxInterval = 4f;

    [Header("Son de peur (rare)")]
    public AudioSource scareSource;
    public AudioClip[] scareClips;       // plusieurs sons de peur pour varier
    public float scareMinInterval = 25f;
    public float scareMaxInterval = 60f;

    void Start()
    {
        // Lance l'ambiance continue
        if (ambianceSource != null && ambianceClip != null)
        {
            ambianceSource.clip = ambianceClip;
            ambianceSource.loop = true;
            ambianceSource.Play();
        }

        // Lance les deux timers aléatoires
        ScheduleNextDrip();
        ScheduleNextScare();
    }

    // ───── GOUTTES ─────

    void ScheduleNextDrip()
    {
        float delay = Random.Range(dripsMinInterval, dripsMaxInterval);
        Invoke(nameof(PlayDrip), delay);
    }

    void PlayDrip()
    {
        if (dripsClips.Length > 0 && dripsSource != null)
        {
            AudioClip clip = dripsClips[Random.Range(0, dripsClips.Length)];
            dripsSource.PlayOneShot(clip);
        }
        ScheduleNextDrip(); // replanifie immédiatement
    }

    // ───── PEUR ─────

    void ScheduleNextScare()
    {
        float delay = Random.Range(scareMinInterval, scareMaxInterval);
        Invoke(nameof(PlayScare), delay);
    }

    void PlayScare()
    {
        if (scareClips.Length > 0 && scareSource != null)
        {
            AudioClip clip = scareClips[Random.Range(0, scareClips.Length)];
            scareSource.PlayOneShot(clip);
        }
        ScheduleNextScare(); // replanifie immédiatement
    }

    void OnDestroy()
    {
        CancelInvoke(); // nettoie les timers si l'objet est détruit
    }
}

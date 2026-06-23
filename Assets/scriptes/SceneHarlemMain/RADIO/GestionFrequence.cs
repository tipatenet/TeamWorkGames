using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class GestionFrequence : MonoBehaviour
{
    [Header("Référence vers l'affichage (TextMeshPro)")]
    public TextMeshProUGUI texteFrequence;

    [Header("Réglages de la fréquence")]
    public float frequenceActuelle = 87.5f;
    public float frequenceACherche = 102.0f;
    public float frequenceMin = 87.5f;
    public float frequenceMax = 108.0f;

    [Header("Tolérance de validation")]
    public float tolerance = 0.1f;

    [Header("Gestion du Temps (Validation)")]
    public float tempsRequis = 5f;
    private float timerFrequenceTrouvee = 0f;
    private bool surLaBonneFrequence = false;

    [Header("Gestion de l'Audio & Grésillements")]
    public AudioSource audioMusique;
    public AudioSource audioGresillement;
    public float distanceMaxAudio = 4.0f;

    [Header("Événement déclenché quand la fréquence est validée")]
    public UnityEvent OnFrequenceTrouvee;

    private bool dejaTrouvee = false;
    private bool radioActivementRegardee = false;

    void Start()
    {
        MettreAJourAffichage();

        // Diagnostic audio au démarrage
        Debug.Log($"[Radio] === DIAGNOSTIC AUDIO ===");
        Debug.Log($"[Radio] audioMusique: {(audioMusique == null ? "NULL !" : "OK")}");
        Debug.Log($"[Radio] audioGresillement: {(audioGresillement == null ? "NULL !" : "OK")}");

        if (audioMusique != null)
        {
            Debug.Log($"[Radio] Musique - clip: {(audioMusique.clip == null ? "NULL !" : audioMusique.clip.name)}, mute: {audioMusique.mute}, spatialBlend: {audioMusique.spatialBlend}, volume: {audioMusique.volume}");
            audioMusique.loop = true;
            audioMusique.mute = false;
            audioMusique.spatialBlend = 0f; // Force 2D pour garantir que la position ne bloque pas le son
            if (!audioMusique.isPlaying) audioMusique.Play();
            Debug.Log($"[Radio] Musique isPlaying après Play(): {audioMusique.isPlaying}");
        }

        if (audioGresillement != null)
        {
            Debug.Log($"[Radio] Grésil - clip: {(audioGresillement.clip == null ? "NULL !" : audioGresillement.clip.name)}, mute: {audioGresillement.mute}, spatialBlend: {audioGresillement.spatialBlend}, volume: {audioGresillement.volume}");
            audioGresillement.loop = true;
            audioGresillement.mute = false;
            audioGresillement.spatialBlend = 0f; // Force 2D
            if (!audioGresillement.isPlaying) audioGresillement.Play();
            Debug.Log($"[Radio] Grésil isPlaying après Play(): {audioGresillement.isPlaying}");
        }

        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        int activeListeners = 0;
        foreach (var l in listeners) if (l.enabled && l.gameObject.activeInHierarchy) activeListeners++;
        Debug.Log($"[Radio] AudioListeners actifs dans la scène: {activeListeners} (total trouvés: {listeners.Length})");

        GererAudioDynamique();
    }

    void Update()
    {
        if (dejaTrouvee) return;

        // Le décompte de 5s progresse UNIQUEMENT si on est calé ET qu'on regarde activement la radio
        if (surLaBonneFrequence && radioActivementRegardee)
        {
            timerFrequenceTrouvee += Time.deltaTime;
            if (timerFrequenceTrouvee >= tempsRequis)
            {
                ValiderFrequenceDefinitive();
            }
        }
    }

    // Cette fonction est appelée par RadioInteraction pour ouvrir/couper le son des caméras
    public void SetAudioActive(bool active)
    {
        Debug.Log($"[Radio] SetAudioActive({active}) appelé — radioActivementRegardee était: {radioActivementRegardee}");
        radioActivementRegardee = active;
        GererAudioDynamique();

        // Sécurité : Si on lâche la radio, on perd la progression du Timer de 5 secondes
        if (!active)
        {
            ResetTimer();
        }
    }

    public void ChangerFrequence(float valeurAjout)
    {
        if (dejaTrouvee) return;

        frequenceActuelle += valeurAjout;
        AppliquerChangement();
    }

    private void AppliquerChangement()
    {
        frequenceActuelle = Mathf.Round(frequenceActuelle * 10f) / 10f;
        frequenceActuelle = Mathf.Clamp(frequenceActuelle, frequenceMin, frequenceMax);

        MettreAJourAffichage();
        GererAudioDynamique();
        VerifierFrequenceImmobilite();
    }

    private void MettreAJourAffichage()
    {
        if (texteFrequence != null)
        {
            texteFrequence.text = frequenceActuelle.ToString("F1") + " MHz";
        }
    }

    private void GererAudioDynamique()
    {
        if (audioMusique == null || audioGresillement == null)
        {
            Debug.LogWarning("[Radio] GererAudioDynamique: audioMusique ou audioGresillement est NULL, impossible de jouer le son !");
            return;
        }

        // REGLE STRICTE : Si on n'est pas sur la LookCam (mode zoom inactif) et que l'énigme n'est pas finie, VOLUME = 0
        if (!radioActivementRegardee && !dejaTrouvee)
        {
            audioMusique.volume = 0f;
            audioGresillement.volume = 0f;
            return;
        }

        // Si on est sur la LookCam, on calcule le mixage (juxtaposition)
        float distance = Mathf.Abs(frequenceActuelle - frequenceACherche);
        float ratioProximite = Mathf.Clamp01(1f - (distance / distanceMaxAudio));

        float volMusique = Mathf.Lerp(0f, 1f, ratioProximite);
        float volGresil = Mathf.Lerp(1f, 0.04f, ratioProximite);

        audioMusique.volume = volMusique;
        audioGresillement.volume = volGresil;

        Debug.Log($"[Radio] GererAudio — radioActive:{radioActivementRegardee} | freq:{frequenceActuelle} | dist:{distance:F1} | ratio:{ratioProximite:F2} | volMusique:{volMusique:F2} | volGresil:{volGresil:F2} | musiqueIsPlaying:{audioMusique.isPlaying} | gresilIsPlaying:{audioGresillement.isPlaying}");
    }


    private void VerifierFrequenceImmobilite()
    {
        float distance = Mathf.Abs(frequenceActuelle - frequenceACherche);

        if (distance <= tolerance)
        {
            if (!surLaBonneFrequence)
            {
                surLaBonneFrequence = true;
                timerFrequenceTrouvee = 0f;
            }
        }
        else
        {
            ResetTimer();
        }
    }

    private void ResetTimer()
    {
        surLaBonneFrequence = false;
        timerFrequenceTrouvee = 0f;
    }

    private void ValiderFrequenceDefinitive()
    {
        dejaTrouvee = true;
        surLaBonneFrequence = false;

        // Une fois l'énigme résolue, la musique reste à fond même si on quitte la LookCam !
        if (audioMusique != null) audioMusique.volume = 1f;
        if (audioGresillement != null) audioGresillement.volume = 0f;

        OnFrequenceTrouvee?.Invoke();
        Debug.Log("Fréquence validée !");
    }
}
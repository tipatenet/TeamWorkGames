using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class GestionFrequence : MonoBehaviour
{
    [Header("Référence vers l'affichage (TextMeshPro)")]
    public TextMeshProUGUI texteFrequence;

    [Header("Réglages de l'frequency")]
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

        if (audioMusique != null)
        {
            audioMusique.loop = true;
            audioMusique.mute = false;
            audioMusique.spatialBlend = 1f; // <-- MODIFIE ICI (0f devient 1f pour passer en 3D)
            if (!audioMusique.isPlaying) audioMusique.Play();
        }

        if (audioGresillement != null)
        {
            audioGresillement.loop = true;
            audioGresillement.mute = false;
            audioGresillement.spatialBlend = 1f; // <-- MODIFIE ICI (0f devient 1f pour passer en 3D)
            if (!audioGresillement.isPlaying) audioGresillement.Play();
        }

        GererAudioDynamique();
    }

    void Update()
    {
        if (dejaTrouvee) return;

        if (surLaBonneFrequence && radioActivementRegardee)
        {
            timerFrequenceTrouvee += Time.deltaTime;
            if (timerFrequenceTrouvee >= tempsRequis)
            {
                ValiderFrequenceDefinitive();
            }
        }
    }

    public void SetAudioActive(bool active)
    {
        radioActivementRegardee = active;
        GererAudioDynamique();

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
        if (audioMusique == null || audioGresillement == null) return;

        // REGLE: Si on ne regarde pas la radio et que ce n'est pas résolu, pas de son
        if (!radioActivementRegardee && !dejaTrouvee)
        {
            audioMusique.volume = 0f;
            audioGresillement.volume = 0f;
            return;
        }

        // Si l'énigme est résolue, on laisse le comportement de ValiderFrequenceDefinitive gérer le volume fixe
        if (dejaTrouvee) return;

        // Calcul du mixage dynamique en mode recherche
        float distance = Mathf.Abs(frequenceActuelle - frequenceACherche);
        float ratioProximite = Mathf.Clamp01(1f - (distance / distanceMaxAudio));

        float volMusique = Mathf.Lerp(0f, 1f, ratioProximite);
        float volGresil = Mathf.Lerp(1f, 0.04f, ratioProximite);

        audioMusique.volume = volMusique;
        audioGresillement.volume = volGresil;
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

        // CORRECTION : Volume à 0.5f pour laisser le son 3D du tiroir s'entendre par-dessus
        if (audioMusique != null) audioMusique.volume = 0.5f;
        if (audioGresillement != null) audioGresillement.volume = 0f;

        OnFrequenceTrouvee?.Invoke();
        Debug.Log("Fréquence validée !");
    }
}
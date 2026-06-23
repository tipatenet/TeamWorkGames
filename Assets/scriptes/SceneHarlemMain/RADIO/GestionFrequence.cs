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
    [Tooltip("Marge d'erreur pour considérer qu'on est calé sur la bonne fréquence")]
    public float tolerance = 0.1f;

    [Header("Gestion du Temps (Validation)")]
    public float tempsRequis = 5f;
    private float timerFrequenceTrouvee = 0f;
    private bool surLaBonneFrequence = false;

    [Header("Gestion de l'Audio & Grésillements")]
    public AudioSource audioMusique;
    public AudioSource audioGresillement;
    [Tooltip("Distance de fréquence à partir de laquelle la musique commence à émerger des grésillements")]
    public float distanceMaxAudio = 4.0f;

    [Header("Événement de réussite")]
    public UnityEvent OnFrequenceTrouvee;

    private bool dejaTrouvee = false;
    private bool radioActivementRegardee = false;

    void Start()
    {
        MettreAJourAffichage();

        if (audioMusique != null) { audioMusique.loop = true; if (!audioMusique.isPlaying) audioMusique.Play(); }
        if (audioGresillement != null) { audioGresillement.loop = true; if (!audioGresillement.isPlaying) audioGresillement.Play(); }

        GererAudioDynamique();
    }

    void Update()
    {
        if (dejaTrouvee) return;

        // Validation uniquement si on est positionné sur la bonne fréquence ET qu'on regarde la radio
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

        // Arrondi propre pour éviter les bugs de virgules flottantes (ex: 98.30001)
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

        // Pas de son en dehors du mode Zoom
        if (!radioActivementRegardee && !dejaTrouvee)
        {
            audioMusique.volume = 0f;
            audioGresillement.volume = 0f;
            return;
        }

        float distance = Mathf.Abs(frequenceActuelle - frequenceACherche);
        float ratioProximite = Mathf.Clamp01(1f - (distance / distanceMaxAudio));

        // JUXTAPOSITION : Plus on est proche, plus la musique augmente et le grésillement diminue
        audioMusique.volume = Mathf.Lerp(0f, 1f, ratioProximite);
        audioGresillement.volume = Mathf.Lerp(1f, 0.05f, ratioProximite); // Laisse un léger fond de grésillement si souhaité, ou mets 0f
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

        if (audioMusique != null) audioMusique.volume = 1f;
        if (audioGresillement != null) audioGresillement.volume = 0f;

        OnFrequenceTrouvee?.Invoke();
        Debug.Log("Fréquence validée ! La musique reste active.");
    }
}
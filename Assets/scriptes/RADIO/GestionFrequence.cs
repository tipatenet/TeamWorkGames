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

        // On s'assure que les sons jouent en boucle à fond en arrière-plan
        if (audioMusique != null) { audioMusique.loop = true; audioMusique.mute = false; if (!audioMusique.isPlaying) audioMusique.Play(); }
        if (audioGresillement != null) { audioGresillement.loop = true; audioGresillement.mute = false; if (!audioGresillement.isPlaying) audioGresillement.Play(); }

        GererAudioDynamique();
    }

    void Update()
    {
        if (dejaTrouvee) return;

        if (surLaBonneFrequence)
        {
            timerFrequenceTrouvee += Time.deltaTime;
            if (timerFrequenceTrouvee >= tempsRequis)
            {
                ValiderFrequenceDefinitivon();
            }
        }
    }

    // Appelée par le script d'interaction
    public void SetAudioActive(bool active)
    {
        radioActivementRegardee = active;
        GererAudioDynamique();
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

        // Si le joueur ne regarde pas la radio, le volume est對 0 quoi qu'il arrive
        if (!radioActivementRegardee && !dejaTrouvee)
        {
            audioMusique.volume = 0f;
            audioGresillement.volume = 0f;
            return;
        }

        float distance = Mathf.Abs(frequenceActuelle - frequenceACherche);
        float ratioProximite = Mathf.Clamp01(1f - (distance / distanceMaxAudio));

        // Calcul classique du volume linéaire
        audioMusique.volume = Mathf.Lerp(0f, 1f, ratioProximite);
        audioGresillement.volume = Mathf.Lerp(1f, 0.04f, ratioProximite);
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
            if (surLaBonneFrequence)
            {
                surLaBonneFrequence = false;
                timerFrequenceTrouvee = 0f;
            }
        }
    }

    private void ValiderFrequenceDefinitivon()
    {
        dejaTrouvee = true;
        surLaBonneFrequence = false;

        if (audioMusique != null) audioMusique.volume = 1f;
        if (audioGresillement != null) audioGresillement.volume = 0f;

        OnFrequenceTrouvee?.Invoke();
    }
}
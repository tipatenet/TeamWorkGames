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

    void Start()
    {
        MettreAJourAffichage();

        // Au démarrage, on prépare les boucles mais on s'assure que le volume ou les sources sont gérées
        if (audioMusique != null) audioMusique.loop = true;
        if (audioGresillement != null) audioGresillement.loop = true;

        // Par défaut au spawn, on coupe les sources pour éviter tout bruit parasite
        SetAudioActive(false);
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

    // Fonction appelée par le script d'interaction pour allumer/éteindre la radio
    public void SetAudioActive(bool active)
    {
        if (audioMusique == null || audioGresillement == null) return;

        if (active)
        {
            // Si on passe sur la lookcam, on recalcule le bon volume et on lance la lecture
            GererAudioDynamique();
            if (!audioMusique.isPlaying) audioMusique.Play();
            if (!audioGresillement.isPlaying) audioGresillement.Play();
        }
        else
        {
            // Si on quitte la lookcam, on stoppe net les sons
            audioMusique.Stop();
            audioGresillement.Stop();
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

        float distance = Mathf.Abs(frequenceActuelle - frequenceACherche);
        float ratioProximite = Mathf.Clamp01(1f - (distance / distanceMaxAudio));

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
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
    [Tooltip("Distance de fréquence à partir de laquelle la musique commence à émerger")]
    public float distanceMaxAudio = 4.0f;

    [Header("Événement de réussite")]
    public UnityEvent OnFrequenceTrouvee;

    private bool dejaTrouvee = false;
    private bool radioActivementRegardee = false;

    void Start()
    {
        MettreAJourAffichage();

        // Sécurisation des AudioSources pour éviter les références nulles au démarrage
        if (audioMusique != null) { audioMusique.loop = true; if (!audioMusique.isPlaying) audioMusique.Play(); }
        if (audioGresillement != null) { audioGresillement.loop = true; if (!audioGresillement.isPlaying) audioGresillement.Play(); }

        GererAudioDynamique();
    }

    void Update()
    {
        if (dejaTrouvee) return;

        // Validation stricte : il faut être sur la bonne fréquence ET être en mode zoom
        if (surLaBonneFrequence && radioActivementRegardee)
        {
            timerFrequenceTrouvee += Time.deltaTime;
            if (timerFrequenceTrouvee >= tempsRequis)
            {
                ValiderFrequenceDefinitive();
            }
        }
    }

    // Appelé par RadioInteraction quand on entre/sort du mode Zoom
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

        // Arrondi mathématique d'une précision absolue pour éviter les dérives de float (ex: 101.99999)
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

        // Sécurité : Aucun son si on ne regarde pas la radio (sauf si l'énigme est résolue)
        if (!radioActivementRegardee && !dejaTrouvee)
        {
            audioMusique.volume = 0f;
            audioGresillement.volume = 0f;
            return;
        }

        float distance = Mathf.Abs(frequenceActuelle - frequenceACherche);
        float ratioProximite = Mathf.Clamp01(1f - (distance / distanceMaxAudio));

        // JUXTAPOSITION AUDIO : Interpolation linéaire croisée parfaite
        audioMusique.volume = Mathf.Lerp(0f, 1f, ratioProximite);
        audioGresillement.volume = Mathf.Lerp(1f, 0.03f, ratioProximite); // 0.03f laisse un infime souffle radio réaliste
    }

    private void VerifierFrequenceImmobilite()
    {
        float distance = Mathf.Abs(frequenceActuelle - frequenceACherche);

        if (distance <= tolerance)
        {
            if (!surLaBonneFrequence)
            {
                surLaBonneFrequence = true;
                timerFrequenceTrouvee = 0f; // Démarre le compteur à zéro dès qu'on se cale dessus
            }
        }
        else
        {
            ResetTimer(); // Réinitialisation immédiate si on bouge en dehors de la zone
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
        Debug.Log("<color=green><b>[RADIO]</b> Fréquence validée avec succès (5s d'immobilisation complétées) !</color>");
    }
}
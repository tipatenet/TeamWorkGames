using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class GestionFrequence : MonoBehaviour
{
    [Header("Référence vers l'affichage (TextMeshPro)")]
    [Tooltip("Glisse ici l'objet AffichageFrequence (celui qui a le composant TextMeshProUGUI)")]
    public TextMeshProUGUI texteFrequence;

    [Header("Réglages de la fréquence")]
    [Tooltip("Fréquence affichée au démarrage du jeu")]
    public float frequenceActuelle = 87.5f;

    [Tooltip("LA VALEUR A TROUVER : la fréquence que le joueur doit deviner")]
    public float frequenceACherche = 100.0f;

    [Tooltip("Pas d'incrémentation/décrémentation à chaque clic")]
    public float pas = 0.1f;

    [Tooltip("Bornes min/max du cadran (à adapter à ta radio)")]
    public float frequenceMin = 87.5f;
    public float frequenceMax = 108.0f;

    [Header("Tolérance de validation")]
    [Tooltip("Marge d'erreur acceptée (utile à cause des arrondis sur les float)")]
    public float tolerance = 0.01f;

    [Header("Événement déclenché quand la fréquence est trouvée")]
    public UnityEvent OnFrequenceTrouvee;

    private bool dejaTrouvee = false;

    void Start()
    {
        MettreAJourAffichage();
    }

    // À brancher sur le OnClick() du "Bouton Droite"
    public void AugmenterFrequence()
    {
        if (dejaTrouvee) return;

        frequenceActuelle += pas;
        AppliquerChangement();
    }

    // À brancher sur le OnClick() du "Bouton Gauche"
    public void DiminuerFrequence()
    {
        if (dejaTrouvee) return;

        frequenceActuelle -= pas;
        AppliquerChangement();
    }

    private void AppliquerChangement()
    {
        // Évite les erreurs d'arrondi des float (ex: 87.600000001)
        frequenceActuelle = Mathf.Round(frequenceActuelle * 10f) / 10f;
        frequenceActuelle = Mathf.Clamp(frequenceActuelle, frequenceMin, frequenceMax);

        MettreAJourAffichage();
        VerifierFrequence();
    }

    private void MettreAJourAffichage()
    {
        if (texteFrequence != null)
        {
            texteFrequence.text = frequenceActuelle.ToString("F1") + " MHz";
        }
    }

    private void VerifierFrequence()
    {
        if (Mathf.Abs(frequenceActuelle - frequenceACherche) <= tolerance)
        {
            dejaTrouvee = true;
            Debug.Log("Bonne fréquence trouvée : " + frequenceActuelle);
            OnFrequenceTrouvee?.Invoke();
        }
    }
}

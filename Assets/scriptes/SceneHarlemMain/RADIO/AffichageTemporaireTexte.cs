using UnityEngine;

public class AffichageTemporaireTexte : MonoBehaviour
{
    [SerializeField] private float tempsAffichage = 4f;

    // Cette fonction sera appelée par l'événement de la radio
    public void AfficherLeTexte()
    {
        // Active le texte
        gameObject.SetActive(true);

        // Demande à Unity de désactiver ce GameObject après X secondes
        Invoke(nameof(DesactiverTexte), tempsAffichage);
    }

    private void DesactiverTexte()
    {
        gameObject.SetActive(false);
    }
}
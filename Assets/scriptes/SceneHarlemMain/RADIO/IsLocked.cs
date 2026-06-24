using UnityEngine;

public class IsLocked : MonoBehaviour
{
    // Fonction magique pour l'événement Unity de la Radio
    [Header("Référence du Tiroir")]
    public Opening_System openingSystem;

    // Cette fonction sera appelée par l'événement de la radio
    public void DeclencherOuvertureTiroir()
    {
        if (openingSystem != null)
        {
            // 1. On déverrouille le tiroir
            openingSystem.isLocked = false;

            // 2. On force l'ouverture automatique
            openingSystem.StartOpeningOrClose();
        }
        else
        {
            Debug.LogWarning("[RadioTriggerAction] Aucun script Opening_System n'est assigné !");
        }
    }
}

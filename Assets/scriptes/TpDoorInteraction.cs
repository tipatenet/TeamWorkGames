using System.Collections;
using UnityEngine;

public class TpDoorInteraction : MonoBehaviour
{
    [Header("Réglages de la porte")]
    [Tooltip("Nom exact de la scène à charger (doit être dans les Build Settings).")]
    public string targetSceneName = "";

    [Tooltip("Rayon de détection du joueur autour de la porte.")]
    public float interactRadius = 2.5f;

    public LayerMask playerLayer;

    [Header("Références Player")]
    public PlayerInputHandler keySystem;
    public Interact interact;

    // Cooldown interne pour éviter un double-trigger
    private bool canInteract = true;

    void Update()
    {
        if (!canInteract) return;
        if (keySystem == null || !keySystem.InteractPressed) return;
        if (!IsPlayerInRange()) return;

        // Vérifie que le joueur regarde bien CETTE hitbox
        RaycastHit hit = interact.IsInteractive(false);
        if (hit.collider == null) return;

        // On accepte soit le collider exact, soit n'importe quel objet tagué "tpDoor"
        bool hitThisDoor = (hit.collider.gameObject == gameObject);
        bool hitTpDoorTag = hit.collider.CompareTag("tpDoor");

        if (!hitThisDoor && !hitTpDoorTag) return;

        StartCoroutine(DoTransition());
    }

    // -------------------------------------------------------------------------

    private IEnumerator DoTransition()
    {
        canInteract = false;

        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("[TpDoorInteraction] Aucune scène cible définie !");
            canInteract = true;
            yield break;
        }

        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogError("[TpDoorInteraction] SceneTransitionManager introuvable !");
            canInteract = true;
            yield break;
        }

        // savePlayerState = true → sauvegarde la position + l'inventaire
        //                          + enregistre targetSceneName comme scène actuelle
        SceneTransitionManager.Instance.GoToScene(targetSceneName, savePlayerState: true);

        // On ne remet pas canInteract à true : la scène va de toute façon être détruite
        yield break;
    }

    // -------------------------------------------------------------------------

    private bool IsPlayerInRange()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRadius, playerLayer);
        return hits.Length > 0;
    }

    // Affiche le rayon dans l'éditeur Unity
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.4f, 0.35f);
        Gizmos.DrawSphere(transform.position, interactRadius);
    }
}

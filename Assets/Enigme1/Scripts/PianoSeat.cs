using System.Collections;
using UnityEngine;

/// <summary>
/// Appuyer sur F près du piano pour s'asseoir / se lever.
/// Attacher sur props_148. Assigner seatCameraPoint dans l'Inspector.
/// </summary>
public class PianoSeat : MonoBehaviour
{
    [Header("Références")]
    public Transform seatCameraPoint;       // Empty GameObject positionné face au piano
    public MonoBehaviour playerController;  // Ton script de mouvement joueur (ex: CharacterController)
    public GameObject playerBody;           // Le GameObject du joueur (pour le cacher optionnellement)

    [Header("Paramètres")]
    public float interactionRange = 2.5f;
    public KeyCode sitKey = KeyCode.F;
    public float cameraTransitionSpeed = 5f;

    [Header("UI prompt (optionnel)")]
    public GameObject promptUI;             // Ex: un panel "Appuyer sur F pour s'asseoir"

    // --- Privé ---
    private Camera playerCamera;
    private bool isSeated = false;
    private Vector3 originalCamPos;
    private Quaternion originalCamRot;
    private Transform originalCamParent;
    private bool isTransitioning = false;

    void Start()
    {
        playerCamera = Camera.main;
    }

    void Update()
    {
        float dist = Vector3.Distance(playerCamera.transform.position, transform.position);
        bool inRange = dist <= interactionRange;

        // Afficher/masquer le prompt
        if (promptUI != null)
            promptUI.SetActive(inRange && !isSeated);

        // Appui sur F
        if (Input.GetKeyDown(sitKey) && !isTransitioning)
        {
            if (!isSeated && inRange)
                StartCoroutine(SitDown());
            else if (isSeated)
                StartCoroutine(StandUp());
        }
    }

    IEnumerator SitDown()
    {
        isTransitioning = true;

        // Sauvegarder la position/rotation originale de la caméra
        originalCamPos = playerCamera.transform.position;
        originalCamRot = playerCamera.transform.rotation;
        originalCamParent = playerCamera.transform.parent;

        // Désactiver le controller joueur
        if (playerController != null) playerController.enabled = false;

        // Détacher la caméra du joueur pour la déplacer librement
        playerCamera.transform.SetParent(null);

        // Transition fluide vers le point de vue piano
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * cameraTransitionSpeed;
            playerCamera.transform.position = Vector3.Lerp(originalCamPos, seatCameraPoint.position, t);
            playerCamera.transform.rotation = Quaternion.Slerp(originalCamRot, seatCameraPoint.rotation, t);
            yield return null;
        }

        playerCamera.transform.position = seatCameraPoint.position;
        playerCamera.transform.rotation = seatCameraPoint.rotation;

        // Attacher la caméra au point siège (si le piano bouge, la cam suit)
        playerCamera.transform.SetParent(seatCameraPoint);

        isSeated = true;
        isTransitioning = false;

        if (promptUI != null) promptUI.SetActive(false);

        Debug.Log("[PianoSeat] Assis au piano.");
    }

    IEnumerator StandUp()
    {
        isTransitioning = true;

        // Détacher la caméra du point siège
        playerCamera.transform.SetParent(null);

        // Transition retour vers position originale
        Vector3 fromPos = playerCamera.transform.position;
        Quaternion fromRot = playerCamera.transform.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * cameraTransitionSpeed;
            playerCamera.transform.position = Vector3.Lerp(fromPos, originalCamPos, t);
            playerCamera.transform.rotation = Quaternion.Slerp(fromRot, originalCamRot, t);
            yield return null;
        }

        // Réattacher la caméra au joueur
        playerCamera.transform.SetParent(originalCamParent);
        playerCamera.transform.localPosition = Vector3.zero; // Ajuste si besoin
        playerCamera.transform.localRotation = Quaternion.identity;

        // Réactiver le controller joueur
        if (playerController != null) playerController.enabled = true;

        isSeated = false;
        isTransitioning = false;

        Debug.Log("[PianoSeat] Debout.");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        if (seatCameraPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(seatCameraPoint.position, 0.1f);
            Gizmos.DrawLine(transform.position, seatCameraPoint.position);
        }
    }
}

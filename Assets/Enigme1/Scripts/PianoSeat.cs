using System.Collections;
using UnityEngine;

public class PianoSeat : MonoBehaviour
{
    [Header("Références")]
    public Transform seatCameraPoint;
    public MonoBehaviour playerController;
    public GameObject playerBody;
    public PlayerInputHandler playerInputHandler;
    public GameObject chairObject;

    [Header("UI à cacher au piano")]
    public GameObject inventoryUI;      // Glisser ton canvas inventaire ici
    public GameObject hotbarUI;         // Glisser ta hotbar ici (si séparée)
    public GameObject[] uisToHide;      // Autres UI à cacher (tableau générique)

    [Header("Paramètres")]
    public float interactionRange = 2.5f;
    public KeyCode sitKey = KeyCode.F;
    public float cameraTransitionSpeed = 5f;

    [Header("UI prompt")]
    public GameObject promptUI;

    // --- Privé ---
    private Camera playerCamera;
    private bool isSeated = false;
    private Vector3 originalCamPos;
    private Quaternion originalCamRot;
    private Transform originalCamParent;
    private Vector3 originalCamLocalPos;
    private Quaternion originalCamLocalRot;
    private bool isTransitioning = false;

    private PianoInteractable pianoInteractable;
    private Collider[] chairColliders;

    void Start()
    {
        playerCamera = Camera.main;
        pianoInteractable = GetComponent<PianoInteractable>();
        if (pianoInteractable == null)
            pianoInteractable = GetComponentInChildren<PianoInteractable>();

        GameObject target = chairObject != null ? chairObject : gameObject;
        var hoverable = target.GetComponent<Hoverable>();
        if (hoverable == null)
            hoverable = target.AddComponent<Hoverable>();
        hoverable.hoverColor = Color.yellow;
        hoverable.emissionIntensity = 1.5f;

        chairColliders = target.GetComponents<Collider>();
    }

    void Update()
    {
        float dist = Vector3.Distance(playerCamera.transform.position, transform.position);
        bool inRange = dist <= interactionRange;

        if (promptUI != null)
            promptUI.SetActive(inRange && !isSeated);

        if (!isTransitioning)
        {
            if (!isSeated && inRange && Input.GetKeyDown(sitKey))
                StartCoroutine(SitDown());
            else if (isSeated && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)))
                StartCoroutine(StandUp());
        }
    }

    void SetInventoryVisible(bool visible)
    {
        if (inventoryUI != null) inventoryUI.SetActive(visible);
        if (hotbarUI != null) hotbarUI.SetActive(visible);
        foreach (var ui in uisToHide)
            if (ui != null) ui.SetActive(visible);
    }

    IEnumerator SitDown()
    {
        isTransitioning = true;
        playerInputHandler.LockGameplayInputsForPiano(true);

        SetChairCollidersEnabled(false);

        originalCamPos = playerCamera.transform.position;
        originalCamRot = playerCamera.transform.rotation;
        originalCamParent = playerCamera.transform.parent;
        originalCamLocalPos = playerCamera.transform.localPosition;
        originalCamLocalRot = playerCamera.transform.localRotation;

        if (playerController != null) playerController.enabled = false;
        SetPlayerVisible(false);

        SetInventoryVisible(false);

        playerCamera.transform.SetParent(null);

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
        playerCamera.transform.SetParent(seatCameraPoint);

        // Activer le piano
        if (pianoInteractable != null)
            pianoInteractable.isActive = true;

        isSeated = true;
        isTransitioning = false;

        if (promptUI != null) promptUI.SetActive(false);
        Debug.Log("[PianoSeat] Assis. Pointez une touche et appuyez sur E.");
    }

    IEnumerator StandUp()
    {
        isTransitioning = true;
        playerInputHandler.LockGameplayInputsForPiano(false);

        // Désactiver le piano proprement
        if (pianoInteractable != null)
        {
            pianoInteractable.isActive = false;
            pianoInteractable.ClearHover();
            pianoInteractable.ResetSequence();
        }

        playerCamera.transform.SetParent(null);

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

        playerCamera.transform.SetParent(originalCamParent);
        playerCamera.transform.localPosition = originalCamLocalPos;
        playerCamera.transform.localRotation = originalCamLocalRot;

        SetPlayerVisible(true);
        if (playerController != null) playerController.enabled = true;

        SetInventoryVisible(true);

        SetChairCollidersEnabled(true);

        isSeated = false;
        isTransitioning = false;
        Debug.Log("[PianoSeat] Debout.");
    }

    void SetPlayerVisible(bool visible)
    {
        if (playerBody == null) return;
        foreach (var rend in playerBody.GetComponentsInChildren<Renderer>())
            rend.enabled = visible;
    }

    void SetChairCollidersEnabled(bool enabled)
    {
        if (chairColliders == null) return;
        foreach (var col in chairColliders)
            if (col != null) col.enabled = enabled;
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
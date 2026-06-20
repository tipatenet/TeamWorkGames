using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class RadioInteraction : MonoBehaviour
{
    [Header("Réglages de l'interaction")]
    public float interactRadius = 2f;
    public LayerMask playerLayer;
    public float transitionTime = 1f;
    public float maxDistanceInteract = 2f;
    public float cursorSpeed = 800f;

    [Header("Références Caméras & UI")]
    public PlayerInputHandler keySystem;
    public Interact interact;
    public Camera lookCam;     // Caméra fixe dédiée à la radio
    public Camera playerCam;   // Caméra principale du joueur
    public RectTransform cursorPoint; // Le point UI du curseur

    [Header("Référence Radio")]
    public GestionFrequence gestionFrequence; // Ton script de gestion de fréquence

    private bool canInteract = true;
    private bool cameraModeActive = false;
    private Vector2 cursorPosition;
    private BoxCollider boxRadio;

    void Start()
    {
        // Centre le curseur au démarrage
        cursorPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
    }

    void Update()
    {
        if (IsPlayerInRange())
        {
            HandleInteraction();
            ClickRadioButtons();
        }
    }

    private bool IsPlayerInRange()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRadius, playerLayer);
        return hits.Length > 0;
    }

    private void HandleInteraction()
    {
        if (!keySystem.InteractPressed || !canInteract)
            return;

        if (!cameraModeActive)
        {
            // Entrée en mode Zoom
            RaycastHit hit = interact.IsInteractive(false);
            if (hit.collider == null || !hit.collider.CompareTag("radio"))
                return;

            boxRadio = hit.collider.GetComponent<BoxCollider>();
            ToggleCameraMode();
            SwitchCameraPosition(cameraModeActive);
        }
        else
        {
            // Sortie du mode Zoom
            ToggleCameraMode();
            SwitchCameraPosition(cameraModeActive);
        }
    }

    private void ToggleCameraMode()
    {
        cameraModeActive = !cameraModeActive;

        if (cameraModeActive)
        {
            cursorPoint.gameObject.SetActive(true);
            if (boxRadio != null) boxRadio.enabled = false;
        }
        else
        {
            Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Mouse.current?.WarpCursorPosition(center);
            cursorPosition = center;
            cursorPoint.position = center;
            if (boxRadio != null) boxRadio.enabled = true;
        }
    }

    private void SwitchCameraPosition(bool active)
    {
        // Réutilise la fonction du cadenas pour bloquer le joueur mais garder le look/clic
        keySystem.LockGamePlayForCodeLock(active);

        lookCam.gameObject.SetActive(active);

        if (active)
            lookCam.depth = playerCam.depth + 1;
        else
            lookCam.depth = playerCam.depth - 1;

        playerCam.GetComponent<AudioListener>().enabled = !active;
        lookCam.GetComponent<AudioListener>().enabled = active;

        StartCoroutine(CameraTransition(active));
    }

    private IEnumerator CameraTransition(bool active)
    {
        canInteract = false;

        Vector3 targetPos = active ? lookCam.transform.position : playerCam.transform.position;
        Vector3 targetRot = active ? lookCam.transform.eulerAngles : playerCam.transform.eulerAngles;

        if (active)
        {
            lookCam.transform.position = playerCam.transform.position;
            lookCam.transform.eulerAngles = playerCam.transform.eulerAngles;
        }

        iTween.MoveTo(lookCam.gameObject, targetPos, transitionTime);
        iTween.RotateTo(lookCam.gameObject, targetRot, transitionTime * 1.5f);

        yield return new WaitForSeconds(transitionTime);
        canInteract = true;
    }

    private void ClickRadioButtons()
    {
        if (!cameraModeActive) return;

        UpdateCursor();

        // Envoie un Raycast depuis la caméra zoomée vers la position du curseur custom
        Ray ray = lookCam.ScreenPointToRay(cursorPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistanceInteract))
        {
            // Si on clique
            if (keySystem.ClickInteract && canInteract)
            {
                // Vérifie si l'objet cliqué possède un tag spécifique pour les boutons de la radio
                if (hit.transform.CompareTag("BoutonRadioDroite"))
                {
                    gestionFrequence.AugmenterFrequence();
                    StartCoroutine(InteractionCooldown());
                }
                else if (hit.transform.CompareTag("BoutonRadioGauche"))
                {
                    gestionFrequence.DiminuerFrequence();
                    StartCoroutine(InteractionCooldown());
                }
            }
        }
    }

    private void UpdateCursor()
    {
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            cursorPosition += mouseDelta;
        }

        cursorPosition += keySystem.LookInput * cursorSpeed * Time.deltaTime;

        cursorPosition.x = Mathf.Clamp(cursorPosition.x, 0, Screen.width);
        cursorPosition.y = Mathf.Clamp(cursorPosition.y, 0, Screen.height);

        if (cursorPoint != null)
            cursorPoint.position = cursorPosition;
    }

    private IEnumerator InteractionCooldown()
    {
        canInteract = false;
        yield return new WaitForSeconds(0.2f); // Cooldown rapide pour pouvoir cliquer à la suite confortablement
        canInteract = true;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, interactRadius);
    }
}
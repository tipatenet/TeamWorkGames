using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RadioInteraction : MonoBehaviour
{
    [Header("Réglages de l'interaction")]
    public float interactRadius = 2f;
    public LayerMask playerLayer;
    public float transitionTime = 1f;
    public float cursorSpeed = 800f;

    [Header("Références Caméras & UI")]
    public PlayerInputHandler keySystem;
    public Interact interact;
    public Camera lookCam;
    public Camera playerCam;
    public RectTransform cursorPoint;

    [Header("Référence Radio")]
    public GestionFrequence gestionFrequence;

    [Header("Canvas de la Radio")]
    public GraphicRaycaster graphicRaycaster;

    private bool canInteract = true;
    private bool cameraModeActive = false;
    private Vector2 cursorPosition;
    private BoxCollider boxRadio;
    private Coroutine transitionCoroutine;

    private float holdTimer = 0f;
    private float nextTickTimer = 0f;
    private bool isHoldingButton = false;
    private string currentHeldTag = "";
    private const float TIME_TO_HOLD = 0.4f;
    private const float TICK_RATE_HOLD = 0.1f;

    void Start()
    {
        cursorPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
        boxRadio = GetComponent<BoxCollider>();
    }

    void Update()
    {
        if (IsPlayerInRange())
        {
            HandleInteraction();

            if (cameraModeActive && canInteract)
            {
                UpdateCursor();
                ClickRadioButtons();
            }
        }
        else if (cameraModeActive)
        {
            ForceExitRadioMode();
        }
    }

    private bool IsPlayerInRange()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRadius, playerLayer);
        return hits.Length > 0;
    }

    private void HandleInteraction()
    {
        if (keySystem == null || !keySystem.InteractPressed || !canInteract)
            return;

        if (!cameraModeActive)
        {
            if (interact == null) return;
            RaycastHit hit = interact.IsInteractive(false);

            if (hit.collider == null || (hit.collider != boxRadio && !hit.collider.CompareTag("radio")))
                return;

            if (boxRadio == null) boxRadio = hit.collider.GetComponent<BoxCollider>();
            ToggleCameraMode(true);
        }
        else
        {
            ToggleCameraMode(false);
        }
    }

    private void ToggleCameraMode(bool active)
    {
        cameraModeActive = active;
        isHoldingButton = false;
        currentHeldTag = "";

        if (cameraModeActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            cursorPosition = center;
            if (cursorPoint != null)
            {
                cursorPoint.gameObject.SetActive(true);
                cursorPoint.position = center;
            }

            if (boxRadio != null) boxRadio.enabled = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (cursorPoint != null) cursorPoint.gameObject.SetActive(false);
            if (boxRadio != null) boxRadio.enabled = true;
        }

        SwitchCameraPosition(cameraModeActive);
    }

    private void SwitchCameraPosition(bool active)
    {
        if (keySystem != null) keySystem.LockGamePlayForCodeLock(active);

        if (lookCam != null && playerCam != null)
        {
            lookCam.gameObject.SetActive(active);
            lookCam.depth = active ? playerCam.depth + 1 : playerCam.depth - 1;

            // VÉRIFICATION : On s'assure qu'au moins un AudioListener capte le son
            AudioListener playerListener = playerCam.GetComponent<AudioListener>();
            AudioListener lookListener = lookCam.GetComponent<AudioListener>();

            if (playerListener != null) playerListener.enabled = !active;
            if (lookListener != null) lookListener.enabled = active;
            else if (playerListener != null) playerListener.enabled = true; // Sécurité si pas d'écouteur sur lookCam
        }

        if (gestionFrequence != null)
        {
            gestionFrequence.SetAudioActive(active);
        }

        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(CameraTransition(active));
    }

    private IEnumerator CameraTransition(bool active)
    {
        canInteract = false;

        if (lookCam != null && playerCam != null)
        {
            Vector3 startPos = lookCam.transform.position;
            Quaternion startRot = lookCam.transform.rotation;

            Vector3 targetPos = active ? lookCam.transform.position : playerCam.transform.position;
            Quaternion targetRot = active ? lookCam.transform.rotation : playerCam.transform.rotation;

            if (active)
            {
                startPos = playerCam.transform.position;
                startRot = playerCam.transform.rotation;
            }

            float elapsedTime = 0f;
            while (elapsedTime < transitionTime)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / transitionTime);
                float tSmoothed = Mathf.SmoothStep(0f, 1f, t);

                lookCam.transform.position = Vector3.Lerp(startPos, targetPos, tSmoothed);
                lookCam.transform.rotation = Quaternion.Lerp(startRot, targetRot, tSmoothed);
                yield return null;
            }

            lookCam.transform.position = targetPos;
            lookCam.transform.rotation = targetRot;
        }

        canInteract = true;
    }

    private void UpdateCursor()
    {
        if (Mouse.current != null)
        {
            cursorPosition = Mouse.current.position.ReadValue();
        }

        if (keySystem != null)
        {
            cursorPosition += keySystem.LookInput * cursorSpeed * Time.deltaTime;
        }

        cursorPosition.x = Mathf.Clamp(cursorPosition.x, 0, Screen.width);
        cursorPosition.y = Mathf.Clamp(cursorPosition.y, 0, Screen.height);

        if (cursorPoint != null)
            cursorPoint.position = cursorPosition;
    }

    private void ClickRadioButtons()
    {
        if (keySystem == null || graphicRaycaster == null) return;

        if (keySystem.ClickInteract && !isHoldingButton)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = cursorPosition };
            List<RaycastResult> results = new List<RaycastResult>();
            graphicRaycaster.Raycast(pointerData, results);

            foreach (RaycastResult result in results)
            {
                string tag = result.gameObject.tag;
                if (tag == "BoutonRadioDroite" || tag == "BoutonRadioGauche")
                {
                    isHoldingButton = true;
                    currentHeldTag = tag;
                    holdTimer = 0f;
                    AppliquerActionBouton(currentHeldTag, false);
                    break;
                }
            }
        }

        if (isHoldingButton && keySystem.ClickInteract)
        {
            holdTimer += Time.deltaTime;

            if (holdTimer >= TIME_TO_HOLD)
            {
                nextTickTimer += Time.deltaTime;
                if (nextTickTimer >= TICK_RATE_HOLD)
                {
                    nextTickTimer = 0f;
                    AppliquerActionBouton(currentHeldTag, true);
                }
            }
        }

        if (!keySystem.ClickInteract && isHoldingButton)
        {
            isHoldingButton = false;
            currentHeldTag = "";
            holdTimer = 0f;
            nextTickTimer = 0f;
        }
    }

    private void AppliquerActionBouton(string tag, bool isLongPress)
    {
        if (gestionFrequence == null) return;

        float pasAUtiliser = isLongPress ? 0.3f : 0.1f;

        if (tag == "BoutonRadioDroite")
        {
            gestionFrequence.ChangerFrequence(pasAUtiliser);
        }
        else if (tag == "BoutonRadioGauche")
        {
            gestionFrequence.ChangerFrequence(-pasAUtiliser);
        }
    }

    private void ForceExitRadioMode()
    {
        cameraModeActive = false;
        canInteract = true;
        isHoldingButton = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (cursorPoint != null) cursorPoint.gameObject.SetActive(false);
        if (boxRadio != null) boxRadio.enabled = true;
        if (keySystem != null) keySystem.LockGamePlayForCodeLock(false);
        if (lookCam != null) lookCam.gameObject.SetActive(false);

        if (gestionFrequence != null)
        {
            gestionFrequence.SetAudioActive(false);
        }

        if (playerCam != null)
        {
            AudioListener playerListener = playerCam.GetComponent<AudioListener>();
            if (playerListener != null) playerListener.enabled = true;
        }
    }
}
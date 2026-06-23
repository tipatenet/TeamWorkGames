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

    // Variables d'Appui Long & Clics unitaires
    private float holdTimer = 0f;
    private float nextTickTimer = 0f;
    private bool isHoldingButton = false;
    private bool dejaCliqueCeCoupCi = false;
    private string currentHeldTag = "";

    [Header("Paramètres Appui Long")]
    public float tempsAvantAppuiLong = 0.4f;
    public float vitesseDefilementLong = 0.08f;

    void Start()
    {
        boxRadio = GetComponent<BoxCollider>();
        if (boxRadio == null)
        {
            Debug.LogError($"[RadioInteraction] Il manque un BoxCollider sur l'objet {gameObject.name} pour détecter l'interaction initiale !");
        }
    }

    void Update()
    {
        if (IsPlayerInRange())
        {
            HandleInteraction();

            if (cameraModeActive)
            {
                // SÉCURITÉ ABSOLUE : Force le curseur à rester visible et déverrouillé à chaque frame
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                if (canInteract)
                {
                    UpdateCursor();
                    ClickRadioButtons();
                }
            }
        }
        else if (cameraModeActive)
        {
            ToggleCameraMode(false);
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
        dejaCliqueCeCoupCi = false;
        currentHeldTag = "";

        if (cameraModeActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (cursorPoint != null) cursorPoint.gameObject.SetActive(true);
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

            AudioListener playerListener = playerCam.GetComponent<AudioListener>();
            AudioListener lookListener = lookCam.GetComponent<AudioListener>();

            if (playerListener != null) playerListener.enabled = !active;
            if (lookListener != null) lookListener.enabled = active;
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
            Vector3 startPos = active ? playerCam.transform.position : lookCam.transform.position;
            Quaternion startRot = active ? playerCam.transform.rotation : lookCam.transform.rotation;

            Vector3 targetPos = active ? lookCam.transform.position : playerCam.transform.position;
            Quaternion targetRot = active ? lookCam.transform.rotation : playerCam.transform.rotation;

            if (active)
            {
                lookCam.transform.position = startPos;
                lookCam.transform.rotation = startRot;
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
    }

    private void ClickRadioButtons()
    {
        if (keySystem == null || graphicRaycaster == null) return;

        // 1. Détection du clic initial (Unitaire)
        if (keySystem.ClickInteract)
        {
            if (!dejaCliqueCeCoupCi && !isHoldingButton)
            {
                PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = cursorPosition };
                List<RaycastResult> results = new List<RaycastResult>();
                graphicRaycaster.Raycast(pointerData, results);

                foreach (RaycastResult result in results)
                {
                    string objTag = result.gameObject.tag;
                    if (objTag == "BoutonRadioDroite" || objTag == "BoutonRadioGauche")
                    {
                        isHoldingButton = true;
                        dejaCliqueCeCoupCi = true;
                        currentHeldTag = objTag;
                        holdTimer = 0f;
                        nextTickTimer = 0f;

                        AppliquerActionBouton(currentHeldTag, false);
                        break;
                    }
                }
            }
        }

        // 2. Gestion du maintien continu (Appui long)
        if (isHoldingButton && keySystem.ClickInteract)
        {
            holdTimer += Time.deltaTime;

            if (holdTimer >= tempsAvantAppuiLong)
            {
                nextTickTimer += Time.deltaTime;
                if (nextTickTimer >= vitesseDefilementLong)
                {
                    nextTickTimer = 0f;
                    AppliquerActionBouton(currentHeldTag, true);
                }
            }
        }

        // 3. Relâchement complet de la touche de clic
        if (!keySystem.ClickInteract)
        {
            isHoldingButton = false;
            dejaCliqueCeCoupCi = false;
            currentHeldTag = "";
            holdTimer = 0f;
            nextTickTimer = 0f;
        }
    }

    private void AppliquerActionBouton(string tag, bool isLongPress)
    {
        if (gestionFrequence == null) return;

        float pasAUtiliser = isLongPress ? 0.2f : 0.1f;

        if (tag == "BoutonRadioDroite")
        {
            gestionFrequence.ChangerFrequence(pasAUtiliser);
        }
        else if (tag == "BoutonRadioGauche")
        {
            gestionFrequence.ChangerFrequence(-pasAUtiliser);
        }
    }
}
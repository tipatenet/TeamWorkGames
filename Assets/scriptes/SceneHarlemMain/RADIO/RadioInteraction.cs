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

    // Variables pour mémoriser les coordonnées initiales de lookCam (caméra radio)
    private bool isInitialLookCamSaved = false;
    private Vector3 initialLookPos;
    private Quaternion initialLookRot;

    // Variables pour la gestion de l'appui long
    private float holdTimer = 0f;
    private float nextTickTimer = 0f;
    private bool isHoldingButton = false;
    private string currentHeldTag = "";

    [Header("Paramètres Appui Long")]
    public float tempsAvantAppuiLong = 0.4f; // Temps à attendre avant de défiler vite
    public float vitesseDefilementLong = 0.08f; // Intervalle entre chaque incrément rapide

    void Start()
    {
        boxRadio = GetComponent<BoxCollider>();
        SaveInitialLookCam();
    }

    private void SaveInitialLookCam()
    {
        if (lookCam != null && !isInitialLookCamSaved)
        {
            initialLookPos = lookCam.transform.position;
            initialLookRot = lookCam.transform.rotation;
            isInitialLookCamSaved = true;
        }
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

        SaveInitialLookCam();

        if (lookCam != null && playerCam != null)
        {
            // Lors du zoom avant (activation), on active immédiatement la caméra de zoom avec une priorité de profondeur supérieure
            if (active)
            {
                lookCam.gameObject.SetActive(true);
                lookCam.depth = playerCam.depth + 1;

                AudioListener playerListener = playerCam.GetComponent<AudioListener>();
                AudioListener lookListener = lookCam.GetComponent<AudioListener>();

                if (playerListener != null) playerListener.enabled = false;
                if (lookListener != null) lookListener.enabled = true;
            }
        }

        // Configuration dynamique de la caméra de rendu du Canvas de la radio et du culling mask
        if (graphicRaycaster != null && lookCam != null)
        {
            Canvas canvas = graphicRaycaster.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.worldCamera = active ? lookCam : playerCam;

                // On s'assure que lookCam inclut le layer du canvas de la radio dans son masque de rendu
                lookCam.cullingMask |= (1 << canvas.gameObject.layer);
                // On inclut aussi le layer UI par défaut (5) au cas où
                lookCam.cullingMask |= (1 << 5);
            }
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

            // Utilisation des coordonnées initiales sauvegardées (pour éviter la perte de position au zoom arrière)
            Vector3 targetPos = active ? initialLookPos : playerCam.transform.position;
            Quaternion targetRot = active ? initialLookRot : playerCam.transform.rotation;

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

            // Lors du zoom arrière (désactivation), on attend la fin de la transition avant de désactiver la caméra de zoom
            if (!active)
            {
                lookCam.gameObject.SetActive(false);
                lookCam.depth = playerCam.depth - 1;

                AudioListener playerListener = playerCam.GetComponent<AudioListener>();
                AudioListener lookListener = lookCam.GetComponent<AudioListener>();

                if (playerListener != null) playerListener.enabled = true;
                if (lookListener != null) lookListener.enabled = false;
            }
        }

        canInteract = true;
    }

    private void UpdateCursor()
    {
        if (Mouse.current != null)
        {
            cursorPosition = Mouse.current.position.ReadValue();
            if (cursorPoint != null)
            {
                cursorPoint.position = cursorPosition;
            }
        }
    }

    private void ClickRadioButtons()
    {
        if (keySystem == null || graphicRaycaster == null || EventSystem.current == null) return;

        // 1. Détection du premier clic (Instantone)
        if (keySystem.ClickInteract && !isHoldingButton)
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
                    currentHeldTag = objTag;
                    holdTimer = 0f;
                    nextTickTimer = 0f;

                    // Premier clic unitaire (pas de 0.1 MHz)
                    AppliquerActionBouton(currentHeldTag, false);
                    break;
                }
            }
        }

        // 2. Gestion du maintien enfoncé (Appui long)
        if (isHoldingButton && keySystem.ClickInteract)
        {
            holdTimer += Time.deltaTime;

            if (holdTimer >= tempsAvantAppuiLong)
            {
                nextTickTimer += Time.deltaTime;
                if (nextTickTimer >= vitesseDefilementLong)
                {
                    nextTickTimer = 0f;
                    // Mode rapide activé (pas de 0.2 MHz)
                    AppliquerActionBouton(currentHeldTag, true);
                }
            }
        }

        // 3. Relâchement du bouton
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

        // Si appui long, on avance de 0.2, sinon de 0.1 à chaque clic unitaire
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
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CadenasInteraction : MonoBehaviour
{
    [Header("Settings")]
    public float interactRadius = 2f;
    public LayerMask playerLayer;
    public float cooldownTime = 0.5f;
    public float transitionTime = 1f;
    public float maxDistanceInteract = 1f;
    public float cursorSpeed = 800f;

    /*Variable publique permetant au autres srcipts de s'avoir si le code est bon et
     donc de faire les actions après (ouverture coffre...)*/
    public bool codeValid = false;

    [Header("References")]
    public PlayerInputHandler keySystem;
    public Interact interact;
    public Camera lookCam;
    public Camera playerCam;
    public RectTransform cursorPoint;
    public AudioClip rotationCodeSound;
    public AudioClip openLock;
    public GameObject goCorps;
    public GameObject goArc;
    public GameObject[] codeObjects; // code1 à code4

    [Header("Code Values")]
    public int[] targetCodes = new int[4]; // code1 à code4

    private int[] currentCodes = new int[4];

    private bool canInteract = true;
    private bool cameraModeActive = false;
    private Vector2 cursorPosition;
    private BoxCollider boxCadenas;
    private AudioSource source;
    private bool isUnlocked = false;

    void Start()
    {
        cursorPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
        source = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (IsPlayerInRange())
        {
            HandleInteraction();
            RotateCodes();
            UnlockCadenasIfValid();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, interactRadius);
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

        RaycastHit hit = interact.IsInteractive(false);

        if (hit.collider == null)
            return;

        GameObject hitObj = hit.collider.gameObject;

        if (!hitObj.CompareTag("cadenas"))
            return;

        if (!cameraModeActive)
            boxCadenas = hitObj.GetComponent<BoxCollider>();

        ToggleCameraMode();
        SwitchCameraPosition(cameraModeActive);
    }


    private void ToggleCameraMode()
    {
        cameraModeActive = !cameraModeActive;

        if (cameraModeActive)
        {
            cursorPoint.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            if (boxCadenas != null) boxCadenas.enabled = false;
        }
        else
        {
            Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Mouse.current?.WarpCursorPosition(center);
            cursorPosition = center;
            cursorPoint.position = center;
            Cursor.lockState = CursorLockMode.Locked;
            if (boxCadenas != null) boxCadenas.enabled = true;
        }
    }

    private void SwitchCameraPosition(bool active)
    {
        keySystem.LockGamePlayForCodeLock(active);

        playerCam.gameObject.SetActive(!active);
        lookCam.gameObject.SetActive(active);

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

    private void RotateCodes()
    {
        if (!cameraModeActive) return;

        UpdateCursor();

        Ray ray = lookCam.ScreenPointToRay(cursorPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistanceInteract))
        {
            for (int i = 0; i < codeObjects.Length; i++)
            {
                if (hit.transform.gameObject == codeObjects[i] && keySystem.ClickInteract && canInteract)
                {
                    IncrementCode(i);
                    iTween.RotateAdd(codeObjects[i], new Vector3(0, 0, -36), cooldownTime);
                    source.PlayOneShot(rotationCodeSound);
                    StartCoroutine(InteractionCooldown());
                }
            }
        }
    }

    private void UpdateCursor()
    {
        // Souris
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

    private void IncrementCode(int index)
    {
        if (!canInteract) return;
        currentCodes[index] = (currentCodes[index] + 1) % 10;
    }

    public bool validCode()
    {
        for (int i = 0; i < targetCodes.Length; i++)
        {
            if (currentCodes[i] != targetCodes[i])
            {
                return false;
            }
        }
        return true;
    }

    private void UnlockCadenasIfValid()
    {
        if (isUnlocked) return;

        if (validCode() && canInteract)
        {
            isUnlocked = true;

            if (cameraModeActive)
            {
                ToggleCameraMode();
                SwitchCameraPosition(false);
            }

            foreach (var obj in codeObjects)
                obj.GetComponent<Collider>().enabled = false;

            goCorps.GetComponent<Collider>().enabled = false;
            goArc.GetComponent<Collider>().enabled = false;

            iTween.MoveTo(gameObject, transform.position + Vector3.up * 0.3f, 0.2f);
            goArc.transform.localRotation = Quaternion.Euler(-90f, 180f, 0f);

            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 0.005f;

            source.PlayOneShot(openLock);

            codeValid = true;
        }
    }


    private IEnumerator InteractionCooldown()
    {
        canInteract = false;
        yield return new WaitForSeconds(cooldownTime);
        canInteract = true;
    }
}

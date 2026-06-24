using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BookSystem : MonoBehaviour
{
    public PlayerInputHandler handler;
    public int nbPages = 0;
    public List<GameObject> pages = new List<GameObject>();
    public GameObject pagePos;
    public int selectedIndex = 0;
    public float cooldownTimeTransition = 0.2f;
    public float cooldownTimeTurn = 0.3f;
    public Camera playerCam;
    public Camera bookCam;
    bool canSwitch = true;
    bool canTurn = true;
    private bool cameraModeActive = false;
    public RectTransform cursorPoint;
    private Vector2 cursorPosition;
    public float cursorSpeed = 50f;
    public AudioSource audioSource;
    public List<AudioClip> pageSounds;

    // Zoom
    public float zoomSpeed = 5f;
    public float zoomMin = 20f;
    public float zoomMax = 80f;
    private float targetFOV;
    private float addY = 0.0001f;

    public static BookSystem Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        cameraModeActive = false;

        if (bookCam != null)
        {
            bookCam.gameObject.SetActive(false);
            bookCam.GetComponent<AudioListener>().enabled = false;
            targetFOV = bookCam.fieldOfView;
        }

        if (playerCam != null)
            playerCam.GetComponent<AudioListener>().enabled = true;

    }
    private void Start()
    {
        foreach (GameObject prefab in pages)
            AddPage(prefab);
    }

    void Update()
    {
        if (handler.BookInteraction && canSwitch)
            HandleInteraction();

        if (handler.ClickInteract && cameraModeActive && canSwitch && canTurn)
        {
            bool turned = TurnDirection();
            if (turned)
            {
                StartCoroutine(TurnCoolDown());
                RandomSound();
            }
        }

        if (cameraModeActive)
        {
            if (cursorPoint != null)
                UpdateCursor();

            UpdateZoom();
        }
    }

    bool TurnDirection()
    {
        Ray ray = bookCam.ScreenPointToRay(cursorPosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit))
            return false;

        if (hit.collider.CompareTag("boxR"))
        {
            if (selectedIndex < nbPages)
            {
                PageTurner pt = pages[selectedIndex].GetComponent<PageTurner>();
                pt.startRotation.x = 90f;
                pt.endRotation.x = -90f;
                pt.TurnPage();
                selectedIndex++;
                return true;
            }
        }
        else if (hit.collider.CompareTag("boxL"))
        {
            if (selectedIndex > 0)
            {
                selectedIndex--;
                PageTurner pt = pages[selectedIndex].GetComponent<PageTurner>();
                pt.startRotation.x = -90f;
                pt.endRotation.x = 90f;
                pt.TurnPage();
                return true;
            }
        }

        return false;
    }

    private void UpdateZoom()
    {
        float scroll = handler.ZoomInput.y;
        if (scroll != 0f)
            targetFOV = Mathf.Clamp(targetFOV - scroll * zoomSpeed, zoomMin, zoomMax);

        bookCam.fieldOfView = Mathf.Lerp(bookCam.fieldOfView, targetFOV, Time.deltaTime * 10f);
    }

    public void AddPage(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("pagePrefab n'est pas assigné !");
            return;
        }
        GameObject newPage = Instantiate(prefab, pagePos.transform.position, pagePos.transform.rotation);
        newPage.transform.localScale = new Vector3(0.91f, 0.65f, 0.65f);
        Vector3 temp = newPage.transform.localPosition;
        addY -= 0.0001f;
        temp.y += addY;
        newPage.transform.localPosition = temp;
        pages.Add(newPage);
        nbPages++;
    }

    private void RandomSound()
    {
        if (pageSounds.Count == 0) return;
        audioSource.PlayOneShot(pageSounds[Random.Range(0, pageSounds.Count)]);
    }

    private void HandleInteraction()
    {
        cameraModeActive = !cameraModeActive;
        ToggleCameraMode(cameraModeActive);
        SwitchCameraPosition(cameraModeActive);

        // Reset zoom quand on ferme le livre
        if (!cameraModeActive)
            targetFOV = bookCam.fieldOfView;
    }

    private void ToggleCameraMode(bool active)
    {

        cursorPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
        if (cursorPoint != null)
            cursorPoint.position = cursorPosition;
    }

    private void UpdateCursor()
    {
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            cursorPosition += mouseDelta;
        }

        cursorPosition += handler.LookInput * cursorSpeed * Time.deltaTime;
        cursorPosition.x = Mathf.Clamp(cursorPosition.x, 0, Screen.width);
        cursorPosition.y = Mathf.Clamp(cursorPosition.y, 0, Screen.height);

        if (cursorPoint != null)
            cursorPoint.position = cursorPosition;
    }

    private void SwitchCameraPosition(bool active)
    {
        handler.LockGamePlayForBook(active);

        bookCam.gameObject.SetActive(active);
        bookCam.depth = active ? playerCam.depth + 1 : playerCam.depth - 1;

        playerCam.GetComponent<AudioListener>().enabled = !active;
        bookCam.GetComponent<AudioListener>().enabled = active;

        StartCoroutine(CameraTransition());
    }

    private IEnumerator CameraTransition()
    {
        canSwitch = false;
        yield return new WaitForSeconds(cooldownTimeTransition);
        canSwitch = true;
    }

    private IEnumerator TurnCoolDown()
    {
        canTurn = false;
        yield return new WaitForSeconds(cooldownTimeTurn);
        canTurn = true;
    }
}
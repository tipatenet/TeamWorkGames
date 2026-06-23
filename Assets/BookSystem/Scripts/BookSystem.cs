using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BookSystem : MonoBehaviour
{
    public PlayerInputHandler handler;
    public int nbPages = 0;
    public List<GameObject> pages = new List<GameObject>();
    public GameObject pagePrefab;
    public GameObject pagePos;
    public int selectedIndex = 0;
    public float cooldownTimeTransition = 1f;
    public float cooldownTimeTurn = 0.3f;
    public Camera playerCam;
    public Camera bookCam;
    bool canSwitch = true;
    bool canTurn = true;
    float transitionTime = 1f;
    private bool cameraModeActive = false;
    public RectTransform cursorPoint;
    private Vector2 cursorPosition;
    public float cursorSpeed = 50f;
    public AudioSource audioSource;
    public List<AudioClip> pageSounds;

    void Awake()
    {
        cameraModeActive = false;

        if (bookCam != null)
        {
            bookCam.gameObject.SetActive(false);
            bookCam.GetComponent<AudioListener>().enabled = false;
        }

        if (playerCam != null)
            playerCam.GetComponent<AudioListener>().enabled = true;

        if (cursorPoint != null)
            cursorPoint.gameObject.SetActive(false);
    }

    void Update()
    {
        if (handler.InteractPressed)
            AddPage();

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

        if (cameraModeActive && cursorPoint != null)
            UpdateCursor();
    }

    bool TurnDirection()
    {
        Ray ray = bookCam.ScreenPointToRay(cursorPosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit))
            return false;

        if (hit.collider.CompareTag("boxR"))
        {
            if (selectedIndex < pages.Count - 1)
            {
                PageTurner pt = pages[selectedIndex].GetComponent<PageTurner>();
                selectedIndex++;
                pt.startRotation.x = 90f;
                pt.endRotation.x = -90f;
                pt.TurnPage();
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

    public void AddPage()
    {
        if (pagePrefab == null)
        {
            Debug.LogError("pagePrefab n'est pas assigné !");
            return;
        }
        GameObject newPage = Instantiate(pagePrefab, pagePos.transform.position, pagePos.transform.rotation);
        pages.Add(newPage);
        nbPages++;
    }

    private void RandomSound()
    {
        if (pageSounds.Count == 0) return;
        if (selectedIndex < 0 || selectedIndex >= pages.Count) return;
        if (pages[selectedIndex] == null) return;

        int intNb = Random.Range(0, pageSounds.Count);
        audioSource.PlayOneShot(pageSounds[intNb]);
    }

    private void HandleInteraction()
    {
        cameraModeActive = !cameraModeActive;
        ToggleCameraMode(cameraModeActive);
        SwitchCameraPosition(cameraModeActive);
    }

    private void ToggleCameraMode(bool active)
    {
        if (cursorPoint != null)
            cursorPoint.gameObject.SetActive(active);

        if (active)
            cursorPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
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
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
    private bool canTurn = true;
    public float cooldownTime = 1.5f;
    public Camera playerCam;
    public Camera bookCam;
    bool canSwitch = true;
    float transitionTime = 1f;
    private bool cameraModeActive = false;
    public RectTransform cursorPoint;
    private Vector2 cursorPosition;

    void Awake()
    {
        // Force l'état initial correct peu importe la config dans l'éditeur
        cameraModeActive = false;

        // Désactive la bookCam proprement
        if (bookCam != null)
        {
            bookCam.gameObject.SetActive(false);
            bookCam.GetComponent<AudioListener>().enabled = false;
        }

        // S'assure que la playerCam est bien la cam active avec son AudioListener
        if (playerCam != null)
        {
            playerCam.GetComponent<AudioListener>().enabled = true;
        }

        // Cache le curseur UI du livre
        if (cursorPoint != null)
            cursorPoint.gameObject.SetActive(false);
    }

    void Update()
    {
        if (handler.InteractPressed && !cameraModeActive)
        {
            AddPage();
        }

        if (handler.OpenCloseBook && canSwitch)
        {
            HandleInteraction();
        }

        if (handler.ClickInteract && cameraModeActive && canSwitch)
        {
            if (canTurn)
            {
                TurnDirection();
                StartCoroutine(TurnPageCooldown());
            }
        }

        // Curseur UI suit la souris seulement en mode livre
        if (cameraModeActive && cursorPoint != null)
        {
            cursorPosition = Mouse.current != null
                ? Mouse.current.position.ReadValue()
                : (Vector2)Input.mousePosition;

            cursorPoint.position = cursorPosition;
        }
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
        Debug.Log("Page ajoutée : " + pages.Count);
    }

    void TurnDirection()
    {
        Vector2 screenPos = cursorPoint != null ? cursorPoint.position : (Vector2)Input.mousePosition;
        Ray ray = bookCam.ScreenPointToRay(screenPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("boxL") && (selectedIndex - 1) >= 0)
            {
                PageTurner pt = pages[selectedIndex].GetComponent<PageTurner>();
                Vector3 start = pt.startRotation;
                Vector3 end = pt.endRotation;
                start.x = -90f;
                end.x = 90f;
                pt.startRotation = start;
                pt.endRotation = end;
                pt.TurnPage();
                selectedIndex--;
            }
            else if (hit.collider.CompareTag("boxR") && (selectedIndex + 1) < pages.Count)
            {
                selectedIndex++;
                PageTurner pt = pages[selectedIndex].GetComponent<PageTurner>();
                Vector3 start = pt.startRotation;
                Vector3 end = pt.endRotation;
                start.x = 90f;
                end.x = -90f;
                pt.startRotation = start;
                pt.endRotation = end;
                pt.TurnPage();
            }
        }
    }

    IEnumerator TurnPageCooldown()
    {
        canTurn = false;
        yield return new WaitForSeconds(cooldownTime);
        canTurn = true;
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

        if (!active)
        {
            Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Mouse.current?.WarpCursorPosition(center);
            cursorPosition = center;
            if (cursorPoint != null)
                cursorPoint.position = center;
        }
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
        yield return new WaitForSeconds(transitionTime);
        canSwitch = true;
    }
}
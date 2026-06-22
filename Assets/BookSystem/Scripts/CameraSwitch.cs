using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CameraSwitch : MonoBehaviour
{
    [Header("Caméras")]
    public Camera playerCamera;
    public Camera bookCamera;

    [Header("UI")]
    public GameObject UIPlayer;
    public RectTransform cursorPoint;
    public float cursorSpeed = 800f;

    [Header("Refs")]
    public PlayerInputHandler keySystem;

    [Header("Fondu")]
    public bool useFade = true;
    public float fadeDuration = 0.25f;

    public bool bookOpen = false;
    private bool canSwitch = true;
    private Vector2 cursorPosition;
    private CanvasGroup _fade;

    void Awake()
    {
        if (useFade) BuildFadeCanvas();
        cursorPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
        if (cursorPoint != null) cursorPoint.gameObject.SetActive(false);
    }

    void Update()
    {
        if (keySystem.OpenCloseBook && canSwitch)
            ToggleBook();

        if (bookOpen)
            UpdateCursor();
    }

    private void ToggleBook()
    {
        bookOpen = !bookOpen;

        keySystem.LockGamePlayForBook(bookOpen);

        if (UIPlayer != null) UIPlayer.SetActive(!bookOpen);

        if (bookOpen)
        {
            cursorPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
            if (cursorPoint != null) cursorPoint.gameObject.SetActive(true);
        }
        else
        {
            if (cursorPoint != null) cursorPoint.gameObject.SetActive(false);
        }

        if (useFade) StartCoroutine(SwitchFade());
        else
        {
            playerCamera.gameObject.SetActive(!bookOpen);
            bookCamera.gameObject.SetActive(bookOpen);
        }

        canSwitch = false;
        Invoke(nameof(ResetSwitch), 0.3f);
    }

    private void UpdateCursor()
    {
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            cursorPosition += mouseDelta;
        }

        cursorPosition.x = Mathf.Clamp(cursorPosition.x, 0, Screen.width);
        cursorPosition.y = Mathf.Clamp(cursorPosition.y, 0, Screen.height);

        if (cursorPoint != null)
            cursorPoint.position = cursorPosition;
    }

    private void ResetSwitch()
    {
        canSwitch = true;
    }

    IEnumerator SwitchFade()
    {
        if (_fade != null) _fade.gameObject.SetActive(true);
        float e = 0f;
        while (e < fadeDuration)
        {
            e += Time.unscaledDeltaTime;
            if (_fade != null) _fade.alpha = Mathf.Lerp(0f, 1f, e / fadeDuration);
            yield return null;
        }

        playerCamera.gameObject.SetActive(!bookOpen);
        bookCamera.gameObject.SetActive(bookOpen);

        e = 0f;
        while (e < fadeDuration)
        {
            e += Time.unscaledDeltaTime;
            if (_fade != null) _fade.alpha = Mathf.Lerp(1f, 0f, e / fadeDuration);
            yield return null;
        }
        if (_fade != null) _fade.gameObject.SetActive(false);
    }

    void BuildFadeCanvas()
    {
        var cgo = new GameObject("_FadeCanvas");
        cgo.transform.SetParent(transform);
        var canvas = cgo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        cgo.AddComponent<CanvasScaler>();
        cgo.AddComponent<GraphicRaycaster>();

        var igo = new GameObject("Img");
        igo.transform.SetParent(cgo.transform, false);
        var img = igo.AddComponent<Image>();
        img.color = Color.black;
        var rt = igo.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        _fade = cgo.AddComponent<CanvasGroup>();
        _fade.alpha = 0f;
        cgo.SetActive(false);
    }

    public bool IsBookMode => bookOpen;
    public bool IsPlayerMode => !bookOpen;
}
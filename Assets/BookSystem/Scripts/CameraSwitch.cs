using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CameraSwitch : MonoBehaviour
{
    [Header("Caméras")]
    public Camera playerCamera;
    public Camera bookCamera;

    [Header("UI")]
    public GameObject UIPlayer;

    [Header("Joueur")]
    public PlayerInputHandler handler;

    [Header("Fondu")]
    public float fadeDuration = 0.25f;

    private bool _bookOpen = false;
    private bool _busy = false;
    private CanvasGroup _fade;

    void Awake()
    {
        BuildFadeCanvas();
        SetCameras(false);
    }

    void Update()
    {
        if (handler.OpenCloseBook && !_busy)
            StartCoroutine(Switch());
    }

    IEnumerator Switch()
    {
        _busy = true;
        _bookOpen = !_bookOpen;

        // Fondu entrant
        _fade.gameObject.SetActive(true);
        float t = 0f;
        while (t < fadeDuration)
        {
            _fade.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            t += Time.deltaTime;
            yield return null;
        }

        SetCameras(_bookOpen);

        // Fondu sortant
        t = 0f;
        while (t < fadeDuration)
        {
            _fade.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            t += Time.deltaTime;
            yield return null;
        }

        _fade.alpha = 0f;
        _fade.gameObject.SetActive(false);
        _busy = false;
    }

    void SetCameras(bool bookOpen)
    {
        playerCamera.gameObject.SetActive(!bookOpen);
        bookCamera.gameObject.SetActive(bookOpen);
        UIPlayer.SetActive(!bookOpen);

        Cursor.visible = bookOpen;
        Cursor.lockState = bookOpen ? CursorLockMode.None : CursorLockMode.Locked;

        handler.LockGamePlayForBook(bookOpen);
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
}
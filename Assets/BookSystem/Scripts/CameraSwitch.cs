using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bascule entre la caméra joueur et la caméra livre (vue du dessus).
/// Touche configurable, fondu au noir optionnel, curseur géré automatiquement.
/// </summary>
public class CameraSwitch : MonoBehaviour
{
    [Header("Caméras")]
    public Camera playerCamera;
    public Camera bookCamera;

    [Header("Cursor")]
    public GameObject UIPlayer;

    [Header("Mouvement joueur à désactiver")]
    public PlayerInputHandler handler;

    [Header("Fondu")]
    public bool  useFade     = true;
    public float fadeDuration = 0.25f;

    // -----------------------------------------------------------
    public enum Mode { Player, Book }
    public Mode CurrentMode { get; private set; } = Mode.Player;

    private CanvasGroup _fade;
    private bool        _busy = false;

    // -----------------------------------------------------------
    void Awake()
    {
        if (useFade) BuildFadeCanvas();
        Apply(Mode.Player, instant: true);
    }

    void Update()
    {
        if (!_busy && handler.OpenCloseBook)
            Switch();
    }

    // -----------------------------------------------------------
    public void Switch()
    {
        if (_busy) return;
        Mode next = (CurrentMode == Mode.Player) ? Mode.Book : Mode.Player;

        if (useFade) StartCoroutine(SwitchFade(next));
        else         Apply(next, instant: true);
    }

    public void OpenBook()  { if (CurrentMode != Mode.Book)   Switch(); }
    public void CloseBook() { if (CurrentMode != Mode.Player) Switch(); }

    // -----------------------------------------------------------
    IEnumerator SwitchFade(Mode next)
    {
        _busy = true;
        yield return StartCoroutine(Fade(0f, 1f));
        Apply(next, instant: true);
        yield return StartCoroutine(Fade(1f, 0f));
        _busy = false;
    }

    IEnumerator Fade(float from, float to)
    {
        if (_fade == null) yield break;
        _fade.gameObject.SetActive(true);
        float e = 0f;
        while (e < fadeDuration)
        {
            e += Time.deltaTime;
            _fade.alpha = Mathf.Lerp(from, to, e / fadeDuration);
            yield return null;
        }
        _fade.alpha = to;
        if (to <= 0f) _fade.gameObject.SetActive(false);
    }

    // -----------------------------------------------------------
    void Apply(Mode mode, bool instant)
    {
        CurrentMode = mode;
        bool book = (mode == Mode.Book);

        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(!book);
            handler.LockGamePlayForBook(true);
            UIPlayer.SetActive(!book);
        }
        if (bookCamera != null)
        {
            bookCamera.gameObject.SetActive(book);
            handler.LockGamePlayForBook(false);
            UIPlayer.SetActive(!book);
        }

        Cursor.visible   = book;
        Cursor.lockState = book ? CursorLockMode.None : CursorLockMode.Locked;
    }

    // -----------------------------------------------------------
    void BuildFadeCanvas()
    {
        var cgo   = new GameObject("_FadeCanvas");
        cgo.transform.SetParent(transform);

        var canvas          = cgo.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        cgo.AddComponent<CanvasScaler>();
        cgo.AddComponent<GraphicRaycaster>();

        var igo   = new GameObject("Img");
        igo.transform.SetParent(cgo.transform, false);
        var img   = igo.AddComponent<Image>();
        img.color = Color.black;
        var rt    = igo.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        _fade       = cgo.AddComponent<CanvasGroup>();
        _fade.alpha = 0f;
        cgo.SetActive(false);
    }

    public bool IsBookMode   => CurrentMode == Mode.Book;
    public bool IsPlayerMode => CurrentMode == Mode.Player;
}

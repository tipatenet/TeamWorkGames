using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    public Image fadeImage; // Image plein écran, noire, dans un Canvas qui persiste
    public float fadeDuration = 0.5f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void GoToScene(string sceneName, bool savePlayerState = true)
    {
        StartCoroutine(TransitionRoutine(sceneName, savePlayerState));
    }

    private IEnumerator TransitionRoutine(string sceneName, bool savePlayerState)
    {
        yield return StartCoroutine(Fade(0f, 1f)); // fade to black

        if (savePlayerState && GameManager.Instance != null && GameManager.Instance.currentSlot >= 0)
        {
            GameManager.Instance.SaveCurrentGame(sceneName); // version modifiée, voir plus bas
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
            yield return null;

        yield return StartCoroutine(Fade(1f, 0f)); // fade from black
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float t = 0f;
        Color c = fadeImage.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t / fadeDuration);
            fadeImage.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(c.r, c.g, c.b, endAlpha);
    }

    public void FadeAndTeleport(System.Action onFadeComplete)
    {
        StartCoroutine(FadeTeleportRoutine(onFadeComplete));
    }

    private IEnumerator FadeTeleportRoutine(System.Action onFadeComplete)
    {
        yield return StartCoroutine(Fade(0f, 1f)); // fondu vers noir
        onFadeComplete?.Invoke();                   // téléporte pendant le noir
        yield return StartCoroutine(Fade(1f, 0f)); // fondu retour
    }
}
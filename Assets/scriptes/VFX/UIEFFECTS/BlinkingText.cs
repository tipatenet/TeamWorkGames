using System.Collections;
using TMPro;
using UnityEngine;

public class BlinkingText : MonoBehaviour
{
    public TMP_Text text;

    [SerializeField]
    private float fadeDuration = 0.8f;

    [SerializeField]
    private float minAlpha = 0f;

    [SerializeField]
    private float maxAlpha = 1f;

    private void Start()
    {
        StartCoroutine(Blink());
    }

    private IEnumerator Blink()
    {
        while (true)
        {
            yield return StartCoroutine(FadeTo(minAlpha, fadeDuration));
            yield return StartCoroutine(FadeTo(maxAlpha, fadeDuration));
        }
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = text.color.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            Color c = text.color;
            c.a = alpha;
            text.color = c;

            yield return null;
        }

        Color finalColor = text.color;
        finalColor.a = targetAlpha;
        text.color = finalColor;
    }
}
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class FlickeringLight : MonoBehaviour
{
    [Header("Réglages")]
    public float minIntensity = 0.1f;
    public float maxIntensity = 2f;

    public float minDelay = 0.02f;
    public float maxDelay = 0.2f;

    [Tooltip("Probabilité d'extinction complète")]
    [Range(0f, 1f)]
    public float offChance = 0.15f;

    private Light _light;

    private void Awake()
    {
        _light = GetComponent<Light>();
    }

    private void OnEnable()
    {
        StartCoroutine(FlickerRoutine());
    }

    private IEnumerator FlickerRoutine()
    {
        while (true)
        {
            if (Random.value < offChance)
            {
                _light.enabled = false;
            }
            else
            {
                _light.enabled = true;
                _light.intensity = Random.Range(minIntensity, maxIntensity);
            }

            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
        }
    }
}
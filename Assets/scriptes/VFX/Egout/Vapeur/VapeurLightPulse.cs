using UnityEngine;

public class VapeurLightPulse : MonoBehaviour
{
    private Light pointLight;
    public float minIntensity = 1.5f;
    public float maxIntensity = 2.5f;
    public float pulseSpeed = 8f;

    void Start()
    {
        pointLight = GetComponent<Light>();
    }

    void Update()
    {
        pointLight.intensity = Mathf.Lerp(minIntensity, maxIntensity,
            (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
    }
}
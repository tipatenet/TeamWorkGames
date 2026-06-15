using UnityEngine;

public class VolumetricFogSettings : MonoBehaviour
{
    [Header("Material utilisé par le Full Screen Pass")]
    [SerializeField] private Material fogMaterial;

    [Header("Fog")]
    [SerializeField] private Color fogColor = Color.white;
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private float stepSize = 1f;
    [SerializeField] private float densityMultiplier = 1f;
    [SerializeField] private float noiseOffset = 0f;

    [Header("Noise")]
    [SerializeField] private Texture3D fogNoise;
    [SerializeField] private float noiseTiling = 1f;
    [SerializeField] private float densityThreshold = 0.1f;

    [Header("Lighting")]
    [ColorUsage(true, true)]
    [SerializeField] private Color lightContribution = Color.white;

    [SerializeField] private float lightScattering = 0.2f;

    private void Awake()
    {
        ApplySettings();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
            ApplySettings();
    }
#endif

    public void ApplySettings()
    {
        if (fogMaterial == null)
            return;

        fogMaterial.SetColor("_Color", fogColor);
        fogMaterial.SetFloat("_MaxDistance", maxDistance);
        fogMaterial.SetFloat("_StepSize", stepSize);
        fogMaterial.SetFloat("_DensityMultiplier", densityMultiplier);
        fogMaterial.SetFloat("_NoiseOffset", noiseOffset);

        if (fogNoise != null)
            fogMaterial.SetTexture("_FogNoise", fogNoise);

        fogMaterial.SetFloat("_NoiseTiling", noiseTiling);
        fogMaterial.SetFloat("_DensityThreshold", densityThreshold);

        fogMaterial.SetColor("_LightContribution", lightContribution);
        fogMaterial.SetFloat("_LightScattering", lightScattering);
    }
}
using UnityEngine;

public class Hoverable : MonoBehaviour
{
    [Header("Hover Settings")]
    public GameObject overParticlePrefab;
    public float particleOffsetY = 0.5f;
    public Vector3 particleScale = Vector3.one;

    [Header("Renderer Settings")]
    public Renderer objectRenderer; // Optionnel (si null → prendra celui du collider)
    public Color hoverColor = Color.yellow;
    public float emissionIntensity = 2f;

    [HideInInspector] public GameObject currentParticle;

    // Événements pour extender : sons, UI...
    public virtual void OnHoverEnter() { }
    public virtual void OnHoverExit() { }
}

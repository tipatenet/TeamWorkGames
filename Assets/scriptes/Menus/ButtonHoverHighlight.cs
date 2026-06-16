using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Hoverable targetObject;

    private MaterialPropertyBlock mpb;

    void Awake()
    {
        mpb = new MaterialPropertyBlock();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetObject == null) return;

        Renderer rend = targetObject.objectRenderer != null
            ? targetObject.objectRenderer
            : targetObject.GetComponent<Renderer>();

        if (rend != null)
        {
            rend.sharedMaterial.EnableKeyword("_EMISSION");

            rend.GetPropertyBlock(mpb);
            mpb.SetColor("_EmissionColor",
                targetObject.hoverColor * targetObject.emissionIntensity);
            rend.SetPropertyBlock(mpb);
        }

        if (targetObject.overParticlePrefab != null &&
            targetObject.currentParticle == null)
        {
            GameObject particle = Instantiate(targetObject.overParticlePrefab);

            Vector3 pos = targetObject.transform.position;
            pos.y += targetObject.particleOffsetY;

            particle.transform.position = pos;
            particle.transform.localScale = targetObject.particleScale;
            particle.transform.SetParent(targetObject.transform);

            targetObject.currentParticle = particle;
        }

        targetObject.OnHoverEnter();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetObject == null) return;

        Renderer rend = targetObject.objectRenderer != null
            ? targetObject.objectRenderer
            : targetObject.GetComponent<Renderer>();

        if (rend != null)
        {
            rend.GetPropertyBlock(mpb);
            mpb.SetColor("_EmissionColor", Color.black);
            rend.SetPropertyBlock(mpb);
        }

        if (targetObject.currentParticle != null)
        {
            Destroy(targetObject.currentParticle);
            targetObject.currentParticle = null;
        }

        targetObject.OnHoverExit();
    }
}
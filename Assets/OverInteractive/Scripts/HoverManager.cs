using UnityEngine;

public class HoverManager : MonoBehaviour
{
    public Interact interact;
    public InspactItem inspact;

    private Hoverable lastHoverable;
    private MaterialPropertyBlock mpb;

    void Start()
    {
        mpb = new MaterialPropertyBlock();
    }

    void Update()
    {
        RaycastHit hit = interact.hitInteract;

        if (!inspact.isInspact)
        {
            if (hit.collider != null)
            {
                Hoverable hoverable = hit.collider.GetComponent<Hoverable>();

                if (hoverable != null)
                {
                    if (hoverable != lastHoverable)
                    {
                        ResetLast();

                        lastHoverable = hoverable;

                        // Renderer
                        Renderer rend = hoverable.objectRenderer != null
                            ? hoverable.objectRenderer
                            : hit.collider.GetComponent<Renderer>();

                        if (rend != null)
                        {
                            rend.sharedMaterial.EnableKeyword("_EMISSION");

                            rend.GetPropertyBlock(mpb);
                            mpb.SetColor("_EmissionColor",
                                hoverable.hoverColor * hoverable.emissionIntensity);
                            rend.SetPropertyBlock(mpb);
                        }

                        // Particule attachée à l'objet
                        if (hoverable.overParticlePrefab != null &&
                            hoverable.currentParticle == null)
                        {
                            GameObject particle =
                                Instantiate(hoverable.overParticlePrefab);

                            Vector3 pos = hoverable.transform.position;
                            pos.y += hoverable.particleOffsetY;

                            particle.transform.position = pos;
                            particle.transform.localScale =
                                hoverable.particleScale;

                            particle.transform.SetParent(hoverable.transform);

                            hoverable.currentParticle = particle;
                        }

                        hoverable.OnHoverEnter();
                    }
                }
                else
                {
                    ResetLast();
                }
            }
            else
            {
                ResetLast();
            }
        }
    }

    void ResetLast()
    {
        if (lastHoverable != null)
        {
            Renderer rend = lastHoverable.objectRenderer != null
                ? lastHoverable.objectRenderer
                : lastHoverable.GetComponent<Renderer>();

            if (rend != null)
            {
                rend.GetPropertyBlock(mpb);
                mpb.SetColor("_EmissionColor", Color.black);
                rend.SetPropertyBlock(mpb);
            }

            if (lastHoverable.currentParticle != null)
            {
                Destroy(lastHoverable.currentParticle);
                lastHoverable.currentParticle = null;
            }

            lastHoverable.OnHoverExit();
            lastHoverable = null;
        }
    }
}

using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class Interact : MonoBehaviour //Sera placer sur le joueur
{
    public Transform cameTransfrom;
    public float maxDistanceInteract = 3f;
    public Material materialOver;
    public RaycastHit hitInteract;

    Renderer lastRenderer = null;
    GameObject particle = null;
    Vector3 particlePositon;
    Vector3 particleScale;
    public Vector3 cameraRotation;
    public Vector3 cameraPosition;
    private Vector3 itemPositon;
    private Item overParticle;


    private void Start()
    {
        cameTransfrom = Camera.main.transform;
        itemPositon = transform.position;
    }
    private void Update()
    {
        cameraPosition = cameTransfrom.position;
        cameraRotation = cameTransfrom.forward;
        hitInteract = IsInteractive();
        OverInteractive();
    }

    //Fonction pour les interactions en globalité
    public RaycastHit IsInteractive()
    {
        Debug.DrawLine(cameraPosition, cameraPosition + cameraRotation * maxDistanceInteract, Color.red);
        RaycastHit hit;
        Physics.Raycast(cameraPosition, cameraRotation, out hit, maxDistanceInteract);
        return hit;
    }

    //Fonction qui permet de faire le over de l'objet

    public void OverInteractive()
    {
        if (hitInteract.collider != null && hitInteract.collider.CompareTag("item"))
        {
            Renderer rend = hitInteract.collider.GetComponent<Renderer>();

            if (rend != lastRenderer)
            {
                ResetLast();

                lastRenderer = rend;

                lastRenderer.material.EnableKeyword("_EMISSION");
                lastRenderer.material.SetColor("_EmissionColor", Color.yellow * 2f);

                overParticle = hitInteract.collider.GetComponent<Item>();

                if (particle == null)
                {
                    particle = Instantiate(overParticle.info.overParticle);

                    Vector3 pos = hitInteract.collider.transform.position;
                    pos.y += overParticle.info.particlePositionY;

                    particle.transform.position = pos;
                    particle.transform.localScale = overParticle.info.particleScale;
                }
            }
        }
        else
        {
            ResetLast();
        }
    }

    void ResetLast()
    {
        if (lastRenderer != null)
        {
            lastRenderer.material.DisableKeyword("_EMISSION");
            lastRenderer = null;
        }

        if (particle != null)
        {
            Destroy(particle);
            particle = null;
        }
    }



}

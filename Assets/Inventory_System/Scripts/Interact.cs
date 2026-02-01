using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class Interact : MonoBehaviour //Sera placer sur le joueur
{
    public Transform cameTransfrom;
    public float maxDistanceInteract = 3f;
    public Material materialOver;

    Renderer lastRenderer = null;
    Material lastMaterial = null;
    GameObject particle = null;
    Vector3 particlePositon;
    Vector3 particleScale;
    private Vector3 cameraRotation;
    private Vector3 cameraPosition;
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
        RaycastHit hit;

        if (Physics.Raycast(cameraPosition, cameraRotation, out hit, maxDistanceInteract) && hit.collider.CompareTag("item"))
        {
            Renderer rend = hit.collider.GetComponent<Renderer>();

            if (rend != lastRenderer)
            {
                if (lastRenderer != null)
                {
                    lastRenderer.material = lastMaterial;
                    Destroy(particle);
                }
                lastRenderer = rend;
                lastMaterial = rend.material;
                rend.material = materialOver;
                overParticle = hit.collider.GetComponent<Item>();
                particle = Instantiate(overParticle.info.overParticle);
                particlePositon = hit.collider.gameObject.transform.position;
                particlePositon.y = overParticle.info.particlePositionY;
                particleScale = overParticle.info.particleScale;
                particle.transform.position = particlePositon;
                particle.transform.localScale = particleScale;
            }
        }
        else
        {
            if (lastRenderer != null)
            {
                lastRenderer.material = lastMaterial;
                lastRenderer = null;
                Destroy(particle);
            }
        }
    }

}

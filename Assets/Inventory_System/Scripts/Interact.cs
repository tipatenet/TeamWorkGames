using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class Interact : MonoBehaviour //Sera placer sur l'item
{
    public Transform cameTransfrom;
    public float maxDistanceInteract = 3f;
    public float sphereDetectionRadius = 3f;
    public Item_ScriptableObject itemVide;
    public LayerMask playerLay;
    private Vector3 cameraRotation;
    private Vector3 cameraPosition;
    private Vector3 itemPositon;
    private Item itemTouch;

    //Variable pour over
    Renderer lastRenderer = null;
    Material lastMaterial = null;
    GameObject particle = null;
    public Material materialOver;
    public Item overParticle;
    Vector3 particlePositon;
    Vector3 particleScale;


    private void Start()
    {
        cameTransfrom = Camera.main.transform;
        itemPositon = transform.position;
    }
    private void Update()
    {
        cameraPosition = cameTransfrom.position;
        cameraRotation = cameTransfrom.forward;
        OverItem();
    }

    //Fonction qui permet au personnage d'interagir avec les items
    public Item_ScriptableObject InteractionTrace()
    {
        Debug.DrawLine(cameraPosition, cameraPosition + cameraRotation * maxDistanceInteract, Color.red);
        RaycastHit hit;
        Item_ScriptableObject itemHit = itemVide;
        if (Physics.Raycast(cameraPosition, cameraRotation, out hit, maxDistanceInteract))
        {
            if(hit.collider.tag == "item") // Vérifie si l'objet toucher à le tag item
            {
                itemTouch = hit.collider.GetComponent<Item>();
                itemHit = itemTouch.info;
                Destroy(hit.collider.gameObject);
                return itemHit;
            }
        }
        return itemHit;
    }

    //Fonction qui permet de faire le over de l'objet
    public void OverItem()
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

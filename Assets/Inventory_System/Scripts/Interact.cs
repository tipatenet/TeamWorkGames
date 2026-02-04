using UnityEngine;


/*Cette class permet de gerer toute les interaction avec les jeux 
 * (Raycast lancer en continue pour voir se que le joueur regarde)*/

public class Interact : MonoBehaviour //Sera placer sur le joueur
{
    //Variables Publique :
    public Camera cam;
    public float maxDistanceInteract = 3f;
    public RaycastHit hitInteract;
    public bool stopRaycast = false;

    //Variables Privées :
    private Renderer lastRenderer = null;
    private GameObject particle = null;
    private Item overParticle;
    private Transform cameTransfrom;
    private MaterialPropertyBlock mpb;

    //Pas Touche :
    public Vector3 cameraRotation;
    public Vector3 cameraPosition;


    private void Start()
    {
        cameTransfrom = cam.transform;
        mpb = new MaterialPropertyBlock();
    }
    private void Update()
    {
        cameraPosition = cameTransfrom.position;
        cameraRotation = cameTransfrom.forward;
        hitInteract = IsInteractive(false);
        if (!stopRaycast)
            OverInteractive();
    }

    //Fonction pour les interactions en globalité
    public RaycastHit IsInteractive(bool stopRaycast)
    {
        RaycastHit hit = default;
        if (!stopRaycast)
        {
            Debug.DrawLine(cameraPosition, cameraPosition + cameraRotation * maxDistanceInteract, Color.red);
            Physics.Raycast(cameraPosition, cameraRotation, out hit, maxDistanceInteract);
        }
        return hit;
    }

    //Fonction qui permet de faire le over de l'objet
    public void OverInteractive()
    {
        //Marche que sur les gameobjects ayant un tag item
        /*SINON pour les over qui sont pas des items il faudra créer un autre script
         * mais c'est rapide c'est casiment la meme chose*/
        //ATTENTION !! ne change que un material donc s'il y à un item qui en a plusieurs bah sa marche pas
        if (hitInteract.collider != null && (hitInteract.collider.CompareTag("item")))
        {
            Renderer rend = hitInteract.collider.GetComponent<Renderer>();

            if (rend != lastRenderer)
            {
                ResetLast();

                lastRenderer = rend;

                lastRenderer.sharedMaterial.EnableKeyword("_EMISSION");
                lastRenderer.GetPropertyBlock(mpb);

                //Parametre de couleur du hover :
                mpb.SetColor("_EmissionColor", Color.yellow * 2f);

                lastRenderer.SetPropertyBlock(mpb);

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

    //Fonction reset pour les particules et Material
    void ResetLast()
    {
        if (lastRenderer != null)
        {
            lastRenderer.SetPropertyBlock(null);
            lastRenderer = null;
        }

        if (particle != null)
        {
            Destroy(particle);
            particle = null;
        }
    }
}

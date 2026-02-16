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
}

using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;

public class Interact : MonoBehaviour //Sera placer sur l'item
{
    public Transform cameTransfrom;
    public float maxDistanceInteract = 3f;
    public float sphereDetectionRadius;
    public LayerMask playerLay;
    private Vector3 cameraRotation;
    private Vector3 cameraPosition;
    private Vector3 itemPositon;

    private void Start()
    {
        cameTransfrom = Camera.main.transform;
        itemPositon = transform.position;
    }
    private void Update()
    {
        cameraPosition = cameTransfrom.position;
        cameraRotation = cameTransfrom.forward;
        DetectPlayer();
    }

    //Fonction qui permet au personnage d'interagir avec les items
    void InteractionTrace()
    {
        Debug.DrawLine(cameraPosition, cameraPosition + cameraRotation * maxDistanceInteract, Color.red);
        RaycastHit hit;
        if(Physics.Raycast(cameraPosition, cameraRotation, out hit, maxDistanceInteract))
        {
            if(hit.collider.tag == "item") // Vérifie si l'objet toucher à le tag item
            {
                print("C'est un item");
            }
        }
    }

    //Fonction qui permet de dessiner la sphere de détection
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(itemPositon, sphereDetectionRadius);
    }

    void DetectPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(itemPositon, sphereDetectionRadius, playerLay);
        if(hits.Length > 0) //Vérifie si le player est dans la sphere
        {
            InteractionTrace();
        }
    }
}

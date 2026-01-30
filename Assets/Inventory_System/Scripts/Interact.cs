using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;

public class Interact : MonoBehaviour
{
    public Transform cameTransfrom;
    public float maxDistanceInteract = 3f;
    private Vector3 cameraRotation;
    private Vector3 cameraPosition;

    private void Start()
    {
        cameTransfrom = Camera.main.transform;
    }
    private void Update()
    {
        cameraPosition = cameTransfrom.position;
        cameraRotation = cameTransfrom.forward;
        InteractionTrace();
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
}

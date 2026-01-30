using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;

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
}

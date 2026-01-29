using UnityEngine;


/*Ce Scripte Permet a la camera de suivre le joueur sans etre ratacher au gameObject du jouer (Sinon sa peut etre pas mal bugger)*/
public class CameraPos : MonoBehaviour
{
    public Transform cameraPosition;

    private void Update()
    {
        transform.position = cameraPosition.position;
    }
}


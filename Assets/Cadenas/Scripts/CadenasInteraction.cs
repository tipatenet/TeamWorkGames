using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CadenasInteraction : MonoBehaviour
{
    //Variables Publique :
    public float interactRadius = 2f;
    public LayerMask player;
    public bool continueToInteract = true;
    public PlayerInputHandler keySystem;
    public Interact interact;
    public Camera lookCam;
    public Camera playerCam;
    public RectTransform cursorPoint;
    public int code1 = 0;
    public int code2 = 0;
    public int code3 = 0;
    public int code4 = 0;
    public bool CodeValid = false;
    public AudioClip rotationCodeSound;

    //Variables Privée :
    private bool canInteract = true;
    private float cooldownTime = 0.5f;
    private bool switchCam = false;
    private float transitionTime = 1f;
    private Vector3 resetPos;
    private Vector3 resetRot;
    private Vector3 cameraRotation;
    private Vector3 cameraPosition;
    private float maxDistanceInteract = 1f;
    private BoxCollider boxCadenas;
    private Vector2 cursorPosition;
    private float cursorSpeed = 800f;
    public int currentCode1 = 0;
    private int currentCode2 = 0;
    private int currentCode3 = 0;
    private int currentCode4 = 0;
    private AudioSource source;

    void Start()
    {
        cursorPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
        source = this.gameObject.GetComponent<AudioSource>();
    }

    void Update()
    {
        InteractCadenas();
        if (keySystem.InteractPressed && canInteract)
            StartCoroutine(InteractCooldown());
    }

    //Permet de dessiner la sphere d'interaction
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, interactRadius);
    }

    //Detecte si le joueur est dans la sphere
    bool detectPlayer(bool isActive)
    {
        if (isActive)
        {
            Collider[] hits = Physics.OverlapSphere(this.gameObject.transform.position, interactRadius, player);
            if (hits.Length > 0)
            {
                return true;
            }
        }
        return false;
    }

    //Fonction qui gère l'interaction avec le cadenas
    void InteractCadenas()
    {
        if (detectPlayer(continueToInteract))
        {
            if (keySystem.InteractPressed && canInteract)
            {
                if (interact.IsInteractive(false).transform.gameObject.tag == "cadenas")
                {
                    if(!switchCam)
                    boxCadenas = interact.IsInteractive(false).transform.gameObject.GetComponent<BoxCollider>();

                    toogleSwitchCam(ref switchCam);
                    switchCamPos(switchCam);
                }
            }
            RotateCodes();
        }
        CodeValid = validCode();
    }

    //Fonction qui permet de switch de cam et aussi la desactivation des inputs
    void toogleSwitchCam(ref bool switchCam)
    {
        if (!switchCam)
        {
            switchCam = true;
            cursorPoint.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            boxCadenas.enabled = false;
        }
        else
        {
            switchCam = false;
            Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Mouse.current.WarpCursorPosition(center);
            cursorPosition = center;
            cursorPoint.position = center;
            Cursor.lockState = CursorLockMode.Locked;
            boxCadenas.enabled = true;
        }
    }

    //Fonction qui permet d'activer et de désactiver les cameras
    void switchCamPos(bool switchCam)
    {
        if (switchCam)
        {
            keySystem.LockGamePlayForCodeLock(true);
            playerCam.gameObject.SetActive(false);
            lookCam.gameObject.SetActive(true);
            camTransition(switchCam);
        }
        else
        {
            keySystem.LockGamePlayForCodeLock(false);
            camTransition(switchCam);
            playerCam.gameObject.SetActive(true);
            lookCam.gameObject.SetActive(false);
        }
    }

    //Fonction qui permet de faire les transitions entre les cameras
    void camTransition(bool switchCam)
    {
        if (switchCam)
        {
            StartCoroutine(TransitionCoolDown());
            Vector3 targetPos = lookCam.gameObject.transform.position;
            Vector3 targetRotation = lookCam.transform.eulerAngles;
            lookCam.gameObject.transform.position = playerCam.gameObject.transform.position;
            lookCam.gameObject.transform.eulerAngles = playerCam.gameObject.transform.eulerAngles;
            iTween.MoveTo(lookCam.gameObject, targetPos, transitionTime);
            iTween.RotateTo(lookCam.gameObject, targetRotation, transitionTime * 1.5f);
        }
        else
        {
            StartCoroutine(TransitionCoolDown());
            Vector3 targetPos = playerCam.gameObject.transform.position;
            Vector3 targetRotation = playerCam.transform.eulerAngles;
            iTween.MoveTo(lookCam.gameObject, targetPos, transitionTime);
            iTween.RotateTo(lookCam.gameObject, targetRotation, transitionTime * 1.5f);
        }
    }

    void RotateCodes()
    {
        if (!switchCam) return; // seulement en mode cadenas

        UpdateCursor();

        Ray ray = lookCam.ScreenPointToRay(cursorPosition);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * maxDistanceInteract, Color.green);
        if (Physics.Raycast(ray, out hit, maxDistanceInteract))
        {
            if (hit.transform.gameObject.tag == "Code1") 
            {
                if(keySystem.ClickInteract && canInteract)
                {
                    IncrementCode(ref currentCode1);
                    iTween.RotateAdd(hit.transform.gameObject, new Vector3(0, 0, -36), cooldownTime);
                    source.PlayOneShot(rotationCodeSound);
                    StartCoroutine(InteractCooldown());
                }
            }
            if (hit.transform.gameObject.tag == "Code2") 
            {
                if (keySystem.ClickInteract && canInteract)
                {
                    IncrementCode(ref currentCode2);
                    iTween.RotateAdd(hit.transform.gameObject, new Vector3(0, 0, -36), cooldownTime);
                    source.PlayOneShot(rotationCodeSound);
                    StartCoroutine(InteractCooldown());
                }
            }
            if (hit.transform.gameObject.tag == "Code3") 
            {
                if (keySystem.ClickInteract && canInteract)
                {
                    IncrementCode(ref currentCode3);
                    iTween.RotateAdd(hit.transform.gameObject, new Vector3(0, 0, -36), cooldownTime);
                    source.PlayOneShot(rotationCodeSound);
                    StartCoroutine(InteractCooldown());
                }
            }
            if (hit.transform.gameObject.tag == "Code4") 
            {
                if (keySystem.ClickInteract && canInteract)
                {
                    IncrementCode(ref currentCode4);
                    iTween.RotateAdd(hit.transform.gameObject, new Vector3(0, 0, -36), cooldownTime);
                    source.PlayOneShot(rotationCodeSound);
                    StartCoroutine(InteractCooldown());
                }
            }
        }
    }

    void UpdateCursor()
    {
        // Si souris utilisée
        if (Mouse.current != null && Mouse.current.delta.ReadValue() != Vector2.zero)
        {
            cursorPosition = Mouse.current.position.ReadValue();
        }
        else
        {
            // Sinon manette (stick droit)
            Vector2 stickInput = keySystem.LookInput;
            cursorPosition += stickInput * cursorSpeed * Time.deltaTime;
        }

        // Limite à l'écran
        cursorPosition.x = Mathf.Clamp(cursorPosition.x, 0, Screen.width);
        cursorPosition.y = Mathf.Clamp(cursorPosition.y, 0, Screen.height);

        // 👇 AJOUT IMPORTANT : déplacer le point UI
        if (cursorPoint != null)
            cursorPoint.position = cursorPosition;
    }

    void IncrementCode(ref int code)
    {
        if (canInteract)
        {
            if ((code + 1) > 9)
                code = 0;
            else
                code++;
        }
    }

    public bool validCode()
    {
        if(currentCode1 == code1)
        {
            if (currentCode2 == code2)
            {
                if (currentCode3 == code3)
                {
                    if (currentCode4 == code4)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }




    IEnumerator InteractCooldown()
    {
        canInteract = false;
        yield return new WaitForSeconds(cooldownTime);
        canInteract = true;
    }
    IEnumerator TransitionCoolDown()
    {
        canInteract = false;
        yield return new WaitForSeconds(transitionTime);
        canInteract = true;
    }
}

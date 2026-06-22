using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnigmeTuyaux : MonoBehaviour
{
    public PlayerInputHandler input;
    public Interact interactScript;

    [Header("Tuyaux (GameObjects à tourner)")]
    public GameObject tuyau1;
    public GameObject tuyau2;
    public GameObject tuyau3;

    [Header("Solution (0 = 0°, 1 = 45°, 2 = 90°, 3 = 135°, 4 = 180°, 5 = 225°, 6 = 270°, 7 = 315°)")]
    public int solutionTuyau1 = 1;
    public int solutionTuyau2 = 2;
    public int solutionTuyau3 = 3;

    [Header("Interaction")]
    public float SphereInteract = 3f;
    public LayerMask Player;

    [Header("Sons")]
    public AudioSource source;
    public AudioClip rotationSound;
    public AudioClip successSound;

    [Header("Objets reliés")]
    public GameObject vapeurBlockage;  // la vapeur qui bloque le passage

    private bool canInteract = true;
    private float cooldownTime = 0.6f;

    private int currentPos1 = 0;
    private int currentPos2 = 0;
    private int currentPos3 = 0;

    private bool enigmeResolue = false;

    void Start()
    {
        source = GetComponent<AudioSource>();

        // Active la vapeur au départ
        if (vapeurBlockage != null)
            vapeurBlockage.SetActive(true);
    }

    void Update()
    {
        if (input.InteractPressed)
        {
            Debug.Log("Touche Interact détectée");
        }

        if (enigmeResolue) return;

        if (input.InteractPressed && canInteract && PlayerCanInteract())
        {
            Debug.Log("Conditions OK");
            StartCoroutine(TryRotateTuyau());
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.4f);
        Gizmos.DrawSphere(transform.position, SphereInteract);
    }

    IEnumerator TryRotateTuyau()
    {
        Debug.Log("Coroutine lancée");

        canInteract = false;

        RaycastHit hit = interactScript.IsInteractive(false);

        Debug.Log(hit.collider != null ? hit.collider.name : "Aucun collider touché");
        Debug.Log($"[TUYAU] Objet touché : {(hit.collider != null ? hit.collider.gameObject.name : "RIEN")}");
        if (hit.collider != null)
        {
            GameObject target = hit.transform.gameObject;
            bool touched = false;

            if (target.CompareTag("tuyau1"))
            {
                RotateTuyau(tuyau1, ref currentPos1);
                touched = true;
            }
            else if (target.CompareTag("tuyau2"))
            {
                RotateTuyau(tuyau2, ref currentPos2);
                touched = true;
            }
            else if (target.CompareTag("tuyau3"))
            {
                RotateTuyau(tuyau3, ref currentPos3);
                touched = true;
            }

            if (touched)
            {
                source.PlayOneShot(rotationSound);

                if (VerifyCode())
                    OnEnigmeResolue();
            }
        }

        yield return new WaitForSeconds(cooldownTime);
        canInteract = true;
    }

    void RotateTuyau(GameObject tuyau, ref int currentPos)
    {
        currentPos = (currentPos + 1) % 8;

        StartCoroutine(SmoothRotate(tuyau, new Vector3(0, 0, 45), 0.2f));
        if (VerifyCode()) OnEnigmeResolue();
    }

    IEnumerator SmoothRotate(GameObject obj, Vector3 rotation, float duration)
    {
        Quaternion startRot = obj.transform.rotation;
        Quaternion endRot = obj.transform.rotation * Quaternion.Euler(rotation);

        float time = 0f;

        while (time < duration)
        {
            obj.transform.rotation = Quaternion.Slerp(startRot, endRot, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        obj.transform.rotation = endRot;
    }

    bool PlayerCanInteract()
    {
        return true;
    }
    bool VerifyCode()
    {
        return currentPos1 == solutionTuyau1 &&
               currentPos2 == solutionTuyau2 &&
               currentPos3 == solutionTuyau3;
    }

    void OnEnigmeResolue()
    {
        if (enigmeResolue) return;
        enigmeResolue = true;
        source.PlayOneShot(successSound);

        // Désactive la vapeur
        if (vapeurBlockage != null)
            vapeurBlockage.SetActive(false);
    }
}
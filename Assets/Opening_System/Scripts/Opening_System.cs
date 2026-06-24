using System.Collections;
using UnityEngine;

public class Opening_System : MonoBehaviour
{
    public enum typeOfOpening
    {
        Translation,
        Rotation
    }

    [Header("Settings")]
    public float timeToOpen = 1f;
    public float timeToClose = 1f;
    public GameObject objectHaveToMove;
    public typeOfOpening typeOfMouvement;
    public Vector3 amountOpen;
    public AudioSource source;
    public AudioClip openSound;
    public AudioClip closeSound;
    public bool isLocked = false;

    [Header("Collider")]
    public bool disableColliderWhenOpen = false;

    [Header("L'object à t'il un code")]
    public CadenasInteraction cadenasInteraction = null;

    [Header("States")]
    private bool isMoving = false;
    private bool isOpen = false;

    private Collider[] itemColliders;

    private void Start()
    {
        itemColliders = GetComponents<Collider>();
    }

    // Fonction qui permet de lancer l'ouverture
    public void StartOpeningOrClose()
    {
        if (canBeOpened())
        {
            if (!isOpen && !isLocked)
            {
                if (!isMoving)
                {
                    ModeSelected(amountOpen, timeToOpen);

                    if (source != null && openSound != null)
                        source.PlayOneShot(openSound);

                    StartCoroutine(coolDownMoving());
                    isOpen = true;

                    if (disableColliderWhenOpen)
                    {
                        foreach (Collider col in itemColliders)
                        {
                            col.enabled = false;
                        }
                    }
                }
            }
            else
            {
                Close();
            }
        }
    }

    public void Close()
    {
        if (canBeOpened())
        {
            if (isOpen)
            {
                if (!isMoving)
                {
                    ModeSelected(-amountOpen, timeToClose);

                    if (source != null && closeSound != null)
                        source.PlayOneShot(closeSound);

                    StartCoroutine(coolDownMoving());
                    isOpen = false;

                    if (disableColliderWhenOpen)
                    {
                        foreach (Collider col in itemColliders)
                        {
                            col.enabled = true;
                        }
                    }
                }
            }
        }
    }

    // Fonction qui fait le mouvement en fonction du mode de mouvement
    public void ModeSelected(Vector3 amount, float time)
    {
        if (typeOfMouvement == typeOfOpening.Rotation)
        {
            iTween.RotateAdd(objectHaveToMove, amount, time);
        }
        else if (typeOfMouvement == typeOfOpening.Translation)
        {
            iTween.MoveBy(objectHaveToMove, amount, time);
        }
    }

    private bool canBeOpened()
    {
        if (cadenasInteraction != null)
        {
            return cadenasInteraction.codeValid;
        }

        return true;
    }

    /* CoolDown pour eviter que la fonction soit appelée
       alors que l'animation n'est pas finie */
    IEnumerator coolDownMoving()
    {
        isMoving = true;
        yield return new WaitForSeconds(Mathf.Max(timeToOpen, timeToClose));
        isMoving = false;
    }
}
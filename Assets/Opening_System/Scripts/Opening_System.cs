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

    [Header("L'object à t'il un code")]
    public CadenasInteraction cadenasInteraction = null;

    [Header("States")]

    private bool isMoving = false;
    private bool isOpen = false;

    //Fonction qui permet de lancer l'ouverture
    public void StartOpeningOrClose()
    {
        if (canBeOpened())
        {
            if (!isOpen)
            {
                if (!isMoving)
                {
                    ModeSelected(amountOpen, timeToOpen);
                    source.PlayOneShot(openSound);
                    StartCoroutine(coolDownMoving());
                    isOpen = true;
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
                    source.PlayOneShot(closeSound);
                    StartCoroutine(coolDownMoving());
                    isOpen = false;
                }
            }
        }
    }

    //Fonction qui fait le mouvement en fonction du mode de mouvement
    public void ModeSelected(Vector3 amount, float time)
    {
        if (typeOfMouvement == typeOfOpening.Rotation)
        {
            iTween.RotateAdd(objectHaveToMove, amount, time);
        }
        else if(typeOfMouvement == typeOfOpening.Translation)
        {
            iTween.MoveBy(objectHaveToMove, amount, time);
        }
    }

    private bool canBeOpened()
    {
        if (cadenasInteraction != null)
        {
            if (cadenasInteraction.codeValid)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    /*CoolDown pour eviter que la fonction soit appelée
      alors que l'animation n'est pas finis*/
    IEnumerator coolDownMoving()
    {
        isMoving = true;
        yield return new WaitForSeconds(timeToOpen);
        isMoving = false;
    }
}

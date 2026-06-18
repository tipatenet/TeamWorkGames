using UnityEngine;

public class ValidTaskBox : MonoBehaviour
{
    public TextTuto textLink;
    void ValidTaskLink()
    {
        textLink.TaskValid = true;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            textLink.TaskValid = true;
        }
    }
}

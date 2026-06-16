using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextTuto : MonoBehaviour
{
    public bool TaskValid = false;
    [SerializeField] private string text;
    private TextMeshPro textRef;
    void Start()
    {
        textRef = GetComponent<TextMeshPro>();
        textRef.text = text;
    }

    void LateUpdate()
    {
        textRef.transform.forward = Camera.main.transform.forward;
        ValidTask();
    }

    private void ValidTask()
    {
        if(TaskValid)
            textRef.text = "";
    }
}

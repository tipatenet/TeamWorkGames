using UnityEngine;
using UnityEngine.UIElements;

public class bookManagerUI : MonoBehaviour
{
    public bool HaveBook = true;
    public bool inBook = false;

    public GameObject bookUIIn;
    public GameObject bookUIOpenClose;

    private void Update()
    {
        if (inBook)
        {
            bookUIIn.SetActive(true);
            bookUIOpenClose.SetActive(true);

        }
        else
        {
            bookUIIn.SetActive(false);
            bookUIOpenClose.SetActive(true);
        }

        if (HaveBook == false)
        {
            bookUIOpenClose.SetActive(false);
        }
        else
        {
            bookUIOpenClose.SetActive(true);
        }
    }
}

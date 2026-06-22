using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookSystem : MonoBehaviour
{
    public PlayerInputHandler handler;

    public int nbPages = 0;
    public List<GameObject> pages = new List<GameObject>();

    public GameObject pagePrefab;
    public GameObject pagePos;
    public int selectedIndex = 0;
    private bool canTurn = true;
    public float cooldownTime = 1.5f;

    void Update()
    {
        if (handler.InteractPressed)
        {
            AddPage();
        }

        if (handler.JumpPressed)
        {
            if (canTurn)
            {
                TurnPage();
                StartCoroutine(turnPageCooldown());
            }
        }
    }

    public void AddPage()
    {
        if (pagePrefab == null)
        {
            Debug.LogError("pagePrefab n'est pas assigné !");
            return;
        }

        GameObject newPage = Instantiate(pagePrefab,pagePos.transform.position,pagePos.transform.rotation);

        pages.Add(newPage);
        nbPages++;

        Debug.Log("Page ajoutée : " + pages.Count);
    }

    void TurnPage()
    {
        if (pages.Count == 0)
            return;

        if (selectedIndex >= pages.Count)
            return;

        pages[selectedIndex].GetComponent<PageTurner>().TurnPage();

        selectedIndex++;
    }

    void TurnDirection()
    {
        
    }

    IEnumerator turnPageCooldown()
    {
        canTurn = false;
        yield return new WaitForSeconds(cooldownTime);
        canTurn = true;
    }
}
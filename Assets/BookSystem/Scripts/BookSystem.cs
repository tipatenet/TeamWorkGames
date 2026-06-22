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
    public Camera cam;

    void Update()
    {
        if (handler.InteractPressed)
        {
            AddPage();
        }
        if (handler.ClickInteract)
        {
            if (canTurn)
            {
                TurnDirection();
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
        GameObject newPage = Instantiate(pagePrefab, pagePos.transform.position, pagePos.transform.rotation);
        pages.Add(newPage);
        nbPages++;
        Debug.Log("Page ajoutée : " + pages.Count);
    }

    void TurnDirection()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Tourner à gauche : on accède à la page AVANT de décrémenter
            if (hit.collider.tag == "boxL" && (selectedIndex - 1) >= 0)
            {
                PageTurner pt = pages[selectedIndex].GetComponent<PageTurner>();
                Vector3 start = pt.startRotation;
                Vector3 end = pt.endRotation;
                start.x = -90f;
                end.x = 90f;
                pt.startRotation = start;
                pt.endRotation = end;
                pt.TurnPage();
                selectedIndex--; // décrément APRÈS
            }
            // Tourner à droite : condition stricte < et non <=
            else if (hit.collider.tag == "boxR" && (selectedIndex + 1) < pages.Count)
            {
                selectedIndex++;
                PageTurner pt = pages[selectedIndex].GetComponent<PageTurner>();
                Vector3 start = pt.startRotation;
                Vector3 end = pt.endRotation;
                start.x = 90f;
                end.x = -90f;
                pt.startRotation = start;
                pt.endRotation = end;
                pt.TurnPage();
            }
        }
    }

    IEnumerator turnPageCooldown()
    {
        canTurn = false;
        yield return new WaitForSeconds(cooldownTime);
        canTurn = true;
    }
}
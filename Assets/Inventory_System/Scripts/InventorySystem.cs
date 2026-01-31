using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;
using static UnityEngine.GraphicsBuffer;

public class InventorySystem : MonoBehaviour
{
    public Interact interaction;
    public PlayerInputHandler keySystem;
    private Item_ScriptableObject itemPickUp;
    public float scroolValue = 0f;
    public int inventorySize = 10;
    public List<Item_ScriptableObject> inventory = new List<Item_ScriptableObject>(10);
    public RectTransform item_Container;
    public int selectedIndex = 0;
    private float targetX;
    public float scrollSpeed;

    private void Start()
    {
        interaction = GetComponent<Interact>();
        keySystem = GetComponent<PlayerInputHandler>();
    }
    private void Update()
    {
        PickUpItem();
        scroolValue = Input.mouseScrollDelta.y;
        ScroolInventory();
    }

    private void PickUpItem()
    {
        if (keySystem.InteractPressed)
        {
            itemPickUp = interaction.InteractionTrace();
            print(itemPickUp.item_Name);
        }
    }

    private void ScroolInventory()
    {
        if (scroolValue > 0)
            selectedIndex--;
        else if (scroolValue < 0 && selectedIndex != 9)
            selectedIndex++;
        selectedIndex = Mathf.Clamp(selectedIndex, 0,inventorySize);

        float itemWidth = 100f + 10;
        targetX = -selectedIndex * itemWidth;
        Vector2 currentPos = item_Container.anchoredPosition;
        currentPos.x = Mathf.Lerp(currentPos.x, targetX, Time.deltaTime * scrollSpeed);
        item_Container.anchoredPosition = currentPos;

    }
}

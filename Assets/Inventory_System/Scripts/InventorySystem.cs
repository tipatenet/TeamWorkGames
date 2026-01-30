using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public Interact interaction;
    public PlayerInputHandler keySystem;
    private Item_ScriptableObject itemPickUp;

    private void Start()
    {
        interaction = GetComponent<Interact>();
        keySystem = GetComponent<PlayerInputHandler>();
    }
    private void Update()
    {
        PickUpItem();
    }

    private void PickUpItem()
    {
        if (keySystem.InteractPressed)
        {
            itemPickUp = interaction.InteractionTrace();
            print(itemPickUp.name);
        }
    }
}

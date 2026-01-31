using UnityEngine;

[CreateAssetMenu(fileName = "Item_ScriptableObject", menuName = "Item/Item_ScriptableObject")]
public class Item_ScriptableObject : ScriptableObject
{
    public string item_Name;
    public GameObject item_Object;
    public GameObject overParticle;
    public float particlePositionY;
    public Vector3 particleScale;
    public Sprite icon;
}

using UnityEngine;

[CreateAssetMenu(fileName = "Item_ScriptableObject", menuName = "Item/Item_ScriptableObject")]

//Variables de l'item
//Possible des rajouter des variables (par exemple pour des clés ou quoi...) ME DEMANDER AVANT POUR EVITER LES BUGS
//Si oui attention à changer pour chacun des items !!!
public class Item_ScriptableObject : ScriptableObject
{
    public string item_Name;
    public Sprite icon;
    public GameObject goItem;
}

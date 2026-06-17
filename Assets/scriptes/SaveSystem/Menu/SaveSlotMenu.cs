using UnityEngine;
using TMPro;

public class SaveSlotMenu : MonoBehaviour
{
    [System.Serializable]
    public class SlotUI
    {
        public TextMeshProUGUI slotLabel;
    }

    public SlotUI[] slots; // Taille 3, à assigner dans l'inspecteur
    public string newGameStartingScene = "Level1"; // Nom de ta première scène de jeu

    void Start()
    {
        RefreshAllSlots();
    }

    void RefreshAllSlots()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            RefreshSlot(i);
        }
    }

    void RefreshSlot(int slotIndex)
    {
        SlotUI ui = slots[slotIndex];

        bool exists = SaveManager.SaveExists(slotIndex);

        if (exists)
        {
            SaveData data = SaveManager.Load(slotIndex);
            ui.slotLabel.text = $"Sauvegarde {slotIndex + 1}\nScène: {data.sceneName}";
        }
        else
        {
            ui.slotLabel.text = $"Emplacement {slotIndex + 1}\n(Vide)";
        }
    }

    public void OnSlotClicked(int slotIndex)
    {
        if (SaveManager.SaveExists(slotIndex))
        {
            GameManager.Instance.LoadGame(slotIndex);
        }
        else
        {
            GameManager.Instance.StartNewGame(slotIndex, newGameStartingScene);
        }
    }

    public void OnDeleteClicked(int slotIndex)
    {
        SaveManager.DeleteSave(slotIndex);
        RefreshSlot(slotIndex);
    }
}
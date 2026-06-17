using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotMenu : MonoBehaviour
{
    [System.Serializable]
    public class SlotUI
    {
        public Button slotButton;
        public TextMeshProUGUI slotLabel;
        public Button deleteButton;
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
            int slotIndex = i; // capture locale pour le lambda
            RefreshSlot(slotIndex);
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
            ui.deleteButton.gameObject.SetActive(true);
        }
        else
        {
            ui.slotLabel.text = $"Emplacement {slotIndex + 1}\n(Vide)";
            ui.deleteButton.gameObject.SetActive(false);
        }

        ui.slotButton.onClick.RemoveAllListeners();
        ui.slotButton.onClick.AddListener(() => OnSlotClicked(slotIndex));

        ui.deleteButton.onClick.RemoveAllListeners();
        ui.deleteButton.onClick.AddListener(() => OnDeleteClicked(slotIndex));
    }

    void OnSlotClicked(int slotIndex)
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

    void OnDeleteClicked(int slotIndex)
    {
        SaveManager.DeleteSave(slotIndex);
        RefreshSlot(slotIndex);
    }
}
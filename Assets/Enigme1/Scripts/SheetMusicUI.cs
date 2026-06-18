using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Affiche la partition de l'énigme sous forme d'UI.
/// Attacher sur un Canvas/Panel dédié à la partition.
/// Se connecte au PianoInteractable pour surligner les notes.
/// </summary>
public class SheetMusicUI : MonoBehaviour
{
    [Header("Références")]
    public PianoInteractable piano;
    public GameObject sheetPanel;           // Le panel UI de la partition
    public Transform notesContainer;        // Layout horizontal des notes
    public GameObject notePrefab;           // Prefab d'une case de note (Image + Text)

    [Header("Séquence de la partition")]
    [Tooltip("Liste dans le même ordre que PianoInteractable.targetSequence")]
    public List<string> sheetNotes = new List<string> { "Do", "Mi", "Sol", "Mi", "Do" };

    [Header("Style")]
    public Color defaultNoteColor = Color.white;
    public Color activeNoteColor = new Color(1f, 0.85f, 0.2f);  // Jaune
    public Color successNoteColor = new Color(0.3f, 0.9f, 0.4f); // Vert
    public Color failNoteColor = new Color(0.9f, 0.3f, 0.3f);    // Rouge
    public KeyCode toggleSheetKey = KeyCode.Tab;

    [Header("Hint")]
    public TextMeshProUGUI hintText;
    public string defaultHint = "Jouez les notes dans l'ordre indiqué sur la partition.";

    private List<Image> noteImages = new List<Image>();
    private int currentHighlight = -1;
    private bool isVisible = false;

    void Start()
    {
        BuildSheet();

        if (sheetPanel != null)
            sheetPanel.SetActive(false);

        // Connecter les events du piano
        if (piano != null)
        {
            piano.onSequenceSuccess.AddListener(OnSuccess);
            piano.onSequenceFail.AddListener(OnFail);
        }

        if (hintText != null)
            hintText.text = defaultHint;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleSheetKey))
            ToggleSheet();
    }

    // Construire les cases de notes dynamiquement
    void BuildSheet()
    {
        if (notesContainer == null || notePrefab == null) return;

        // Nettoyer d'abord
        foreach (Transform child in notesContainer)
            Destroy(child.gameObject);
        noteImages.Clear();

        foreach (string note in sheetNotes)
        {
            GameObject obj = Instantiate(notePrefab, notesContainer);

            // Chercher l'Image et le Text dans le prefab
            Image img = obj.GetComponent<Image>();
            if (img == null) img = obj.GetComponentInChildren<Image>();

            TextMeshProUGUI txt = obj.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = note;

            if (img != null)
            {
                img.color = defaultNoteColor;
                noteImages.Add(img);
            }

            obj.SetActive(true);
        }
    }

    // Afficher / masquer la partition
    public void ToggleSheet()
    {
        isVisible = !isVisible;
        if (sheetPanel != null)
            sheetPanel.SetActive(isVisible);
    }

    public void ShowSheet() { isVisible = true; if (sheetPanel != null) sheetPanel.SetActive(true); }
    public void HideSheet() { isVisible = false; if (sheetPanel != null) sheetPanel.SetActive(false); }

    // Surligner une note spécifique (index 0-based)
    public void HighlightNote(int index)
    {
        ResetAllNotes();
        if (index >= 0 && index < noteImages.Count)
        {
            noteImages[index].color = activeNoteColor;
            currentHighlight = index;
        }
    }

    public void ResetAllNotes()
    {
        foreach (var img in noteImages)
            img.color = defaultNoteColor;
        currentHighlight = -1;
    }

    void OnSuccess()
    {
        StartCoroutine(FlashAllNotes(successNoteColor, "Bravo ! Porte déverrouillée."));
    }

    void OnFail()
    {
        StartCoroutine(FlashAllNotes(failNoteColor, "Mauvaise séquence. Réessayez."));
    }

    IEnumerator FlashAllNotes(Color color, string message)
    {
        if (hintText != null) hintText.text = message;

        foreach (var img in noteImages)
            img.color = color;

        yield return new WaitForSeconds(1.2f);

        ResetAllNotes();

        if (hintText != null) hintText.text = defaultHint;
    }
}

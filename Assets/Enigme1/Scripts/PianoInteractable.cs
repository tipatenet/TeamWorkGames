using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Attacher ce script sur le GameObject du piano (props_148).
/// Le joueur pointe une touche avec la souris et clique pour jouer la note.
/// Le survol (hover) met en surbrillance la touche pointée.
/// </summary>
public class PianoInteractable : MonoBehaviour
{
    [System.Serializable]
    public class PianoKey
    {
        public string noteName;
        public KeyCode keyBinding;      // Conservé pour compatibilité, non utilisé en jeu
        public AudioClip audioClip;
        public GameObject keyObject;
        public Color highlightColor = Color.yellow;
    }

    [Header("Touches du piano")]
    public List<PianoKey> pianoKeys = new List<PianoKey>();

    [Header("Paramètres audio")]
    [Range(0f, 1f)]
    public float volume = 0.8f;

    [Header("Paramètres d'interaction")]
    public float interactionRange = 3f;
    public LayerMask pianoLayer;        // Laisser vide = tous les layers

    [Header("Enigme - Partition")]
    public List<string> targetSequence = new List<string>();
    public float sequenceTimeLimit = 10f;
    public UnityEvent onSequenceSuccess;
    public UnityEvent onSequenceFail;

    // --- Privé ---
    private AudioSource audioSource;
    private Camera playerCamera;
    private Dictionary<string, PianoKey> keyMap = new Dictionary<string, PianoKey>();
    private Dictionary<GameObject, PianoKey> objectMap = new Dictionary<GameObject, PianoKey>();

    // Suivi de la séquence
    private List<string> playerSequence = new List<string>();
    private bool sequenceActive = false;
    private float sequenceTimer = 0f;

    // Animation des touches
    private Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>();

    // Hover
    private PianoKey hoveredKey = null;
    private Color hoveredOriginalColor = Color.white;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.maxDistance = 10f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;

        playerCamera = Camera.main;

        foreach (var key in pianoKeys)
        {
            if (!string.IsNullOrEmpty(key.noteName))
                keyMap[key.noteName] = key;

            if (key.keyObject != null)
            {
                objectMap[key.keyObject] = key;
                originalPositions[key.keyObject] = key.keyObject.transform.localPosition;
            }
        }
    }

    void Update()
    {
        HandleHoverAndClick();
        HandleSequenceTimer();
    }

    // --- Hover + clic gauche sur une touche du piano ---
    void HandleHoverAndClick()
    {
        if (playerCamera == null) return;

        PianoKey detectedKey = null;

        if (IsPlayerInRange())
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Si pianoLayer vaut 0 (rien coché), on raycaste sans filtre de layer
            bool didHit = (pianoLayer.value != 0)
                ? Physics.Raycast(ray, out hit, interactionRange, pianoLayer)
                : Physics.Raycast(ray, out hit, interactionRange);

            if (didHit && objectMap.TryGetValue(hit.collider.gameObject, out PianoKey key))
                detectedKey = key;
        }

        // --- Mise à jour du hover ---
        if (detectedKey != hoveredKey)
        {
            // Retirer highlight de l'ancienne touche
            if (hoveredKey != null && hoveredKey.keyObject != null)
            {
                var r = hoveredKey.keyObject.GetComponent<Renderer>();
                if (r != null) r.material.color = hoveredOriginalColor;
            }

            hoveredKey = detectedKey;

            // Appliquer highlight sur la nouvelle touche
            if (hoveredKey != null && hoveredKey.keyObject != null)
            {
                var r = hoveredKey.keyObject.GetComponent<Renderer>();
                if (r != null)
                {
                    hoveredOriginalColor = r.material.color;
                    r.material.color = hoveredKey.highlightColor;
                }
            }
        }

        // --- Clic gauche = jouer la note ---
        if (Input.GetMouseButtonDown(0) && hoveredKey != null)
        {
            PlayNote(hoveredKey);
        }
    }

    // --- Jouer une note ---
    public void PlayNote(PianoKey key)
    {
        if (key == null || key.audioClip == null) return;

        audioSource.PlayOneShot(key.audioClip, volume);

        if (key.keyObject != null)
            StartCoroutine(AnimateKeyPress(key.keyObject));

        RecordNoteForSequence(key.noteName);

        Debug.Log($"[Piano] Note jouée : {key.noteName}");
    }

    public void PlayNoteByName(string noteName)
    {
        if (keyMap.TryGetValue(noteName, out PianoKey key))
            PlayNote(key);
    }

    // --- Gestion de la séquence ---
    void RecordNoteForSequence(string noteName)
    {
        if (targetSequence == null || targetSequence.Count == 0) return;

        if (!sequenceActive)
        {
            sequenceActive = true;
            playerSequence.Clear();
            sequenceTimer = sequenceTimeLimit;
        }

        playerSequence.Add(noteName);

        int idx = playerSequence.Count - 1;
        if (idx < targetSequence.Count && playerSequence[idx] != targetSequence[idx])
        {
            Debug.Log("[Piano] Mauvaise note ! Séquence réinitialisée.");
            FailSequence();
            return;
        }

        if (playerSequence.Count == targetSequence.Count)
        {
            Debug.Log("[Piano] Séquence correcte !");
            SuccessSequence();
        }
    }

    void HandleSequenceTimer()
    {
        if (!sequenceActive) return;

        sequenceTimer -= Time.deltaTime;
        if (sequenceTimer <= 0f)
        {
            Debug.Log("[Piano] Temps écoulé !");
            FailSequence();
        }
    }

    void SuccessSequence()
    {
        sequenceActive = false;
        playerSequence.Clear();
        onSequenceSuccess?.Invoke();
    }

    void FailSequence()
    {
        sequenceActive = false;
        playerSequence.Clear();
        onSequenceFail?.Invoke();
    }

    public void ResetSequence()
    {
        sequenceActive = false;
        playerSequence.Clear();
        sequenceTimer = 0f;
    }

    public void HighlightNoteOnSheet(string noteName, bool highlight)
    {
        if (!keyMap.TryGetValue(noteName, out PianoKey key)) return;
        if (key.keyObject == null) return;

        var renderer = key.keyObject.GetComponent<Renderer>();
        if (renderer == null) return;

        renderer.material.color = highlight ? key.highlightColor : Color.white;
    }

    IEnumerator AnimateKeyPress(GameObject keyObj)
    {
        if (!originalPositions.TryGetValue(keyObj, out Vector3 original)) yield break;

        float pressDepth = 0.015f;
        float duration = 0.08f;
        Vector3 pressed = original + Vector3.down * pressDepth;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            keyObj.transform.localPosition = Vector3.Lerp(original, pressed, t);
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / (duration * 1.5f);
            keyObj.transform.localPosition = Vector3.Lerp(pressed, original, t);
            yield return null;
        }

        keyObj.transform.localPosition = original;
    }

    bool IsPlayerInRange()
    {
        if (playerCamera == null) return false;
        return Vector3.Distance(playerCamera.transform.position, transform.position) <= interactionRange;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}

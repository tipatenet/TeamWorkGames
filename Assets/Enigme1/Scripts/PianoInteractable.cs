using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class PianoInteractable : MonoBehaviour
{
    [System.Serializable]
    public class PianoKey
    {
        public string noteName;
        public KeyCode keyBinding; // Optionnel : si tu veux aussi jouer au clavier
        public AudioClip audioClip;
        public GameObject keyObject;
        public Color highlightColor = Color.yellow;
    }

    [Header("Touches du piano")]
    public List<PianoKey> pianoKeys = new List<PianoKey>();

    [Header("Auto-mapping")]
    [Tooltip("Cocher pour mapper automatiquement toutes les touches enfants au Start")]
    public bool autoMapKeys = true;
    public Transform keysParent;
    public AudioClip[] noteClips;

    [Header("Paramètres audio")]
    [Range(0f, 1f)]
    public float volume = 0.8f;

    [Header("Paramètres d'interaction")]
    public float interactionRange = 3f;
    public KeyCode playKey = KeyCode.F;
    public LayerMask pianoLayer;

    [Header("Enigme - Partition")]
    public List<string> targetSequence = new List<string> { "Do", "Do", "Do" };
    public float sequenceTimeLimit = 10f;
    public UnityEvent onSequenceSuccess;
    public UnityEvent onSequenceFail;

    [Header("Récompense - Coffre")]
    public GameObject chestPrefab;
    public Transform chestSpawnPoint;
    [Tooltip("Durée de l'animation de pop du coffre")]
    public float chestPopDuration = 0.5f;

    // --- Privé ---
    private AudioSource audioSource;
    private Camera playerCamera;
    private Dictionary<string, PianoKey> keyMap = new Dictionary<string, PianoKey>();
    private Dictionary<GameObject, PianoKey> objectMap = new Dictionary<GameObject, PianoKey>();

    private List<string> playerSequence = new List<string>();
    private bool sequenceActive = false;
    private float sequenceTimer = 0f;

    private Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, Hoverable> hoverableMap = new Dictionary<GameObject, Hoverable>();
    private MaterialPropertyBlock mpb;
    private PianoKey hoveredKey = null;
    private bool puzzleSolved = false;
    private GameObject noteLabel;

    [Header("État du Piano")]
    public bool isActive = false;
    public GameObject interactPromptUI;

    [Header("Emplacements partitions")]
    public PianoSheetSlot[] sheetSlots;
    public LayerMask slotLayer;

    [Header("Label note")]
    [Tooltip("Hauteur au-dessus de la touche pour le label")]
    public float labelHeight = 0.05f;
    public float labelSize = 0.3f;

    void Start()
    {
        mpb = new MaterialPropertyBlock();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.maxDistance = 10f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;

        playerCamera = Camera.main;

        if (autoMapKeys)
            AutoMapKeys();

        CreateNoteLabel();

        foreach (var key in pianoKeys)
        {
            if (!string.IsNullOrEmpty(key.noteName))
                keyMap[key.noteName] = key;

            if (key.keyObject != null)
            {
                objectMap[key.keyObject] = key;
                originalPositions[key.keyObject] = key.keyObject.transform.localPosition;

                var hoverable = key.keyObject.GetComponent<Hoverable>();
                if (hoverable == null)
                    hoverable = key.keyObject.AddComponent<Hoverable>();
                hoverable.hoverColor = key.highlightColor;
                hoverable.emissionIntensity = 2f;
                hoverableMap[key.keyObject] = hoverable;

                var r = key.keyObject.GetComponent<Renderer>();
                if (r != null && r.sharedMaterial != null)
                {
                    r.sharedMaterial.EnableKeyword("_EMISSION");
                }
            }
        }
    }

    void Update()
    {
        if (!isActive) return;

        HandleHover();
        HandleSlotInteraction();

        if (Input.GetKeyDown(playKey) && hoveredKey != null)
        {
            PlayNote(hoveredKey);
        }

        if (sequenceActive)
        {
            sequenceTimer -= Time.deltaTime;
            if (sequenceTimer <= 0f)
            {
                Debug.Log("[Piano] Temps écoulé !");
                FailSequence();
            }
        }
    }

    void HandleSlotInteraction()
    {
        if (sheetSlots == null || sheetSlots.Length == 0) return;
        if (!Input.GetKeyDown(playKey)) return;
        if (hoveredKey != null) return;

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange))
        {
            PianoSheetSlot slot = hit.collider.GetComponent<PianoSheetSlot>();
            if (slot != null && !slot.isFilled)
            {
                slot.TryPlaceSheet();
            }
        }
    }

    void CreateNoteLabel()
    {
        noteLabel = new GameObject("NoteLabel");
        noteLabel.transform.SetParent(transform);
        var tmp = noteLabel.AddComponent<TextMeshPro>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = labelSize;
        tmp.color = Color.white;
        tmp.text = "";
        tmp.rectTransform.sizeDelta = new Vector2(1f, 0.5f);
        noteLabel.SetActive(false);
    }

    void AutoMapKeys()
    {
        string[] noteNames = { "La", "Si", "Do", "Re", "Mi", "Fa", "Sol" };

        Transform parent = keysParent != null ? keysParent : transform;
        List<Transform> keys = new List<Transform>();

        foreach (Transform child in parent)
        {
            if (child.name.Contains("key"))
                keys.Add(child);
        }

        keys.Sort((a, b) => b.position.x.CompareTo(a.position.x));

        pianoKeys.Clear();
        for (int i = 0; i < keys.Count; i++)
        {
            int noteIndex = i % noteNames.Length;
            var pk = new PianoKey
            {
                noteName = noteNames[noteIndex],
                keyObject = keys[i].gameObject,
                audioClip = (noteClips != null && noteClips.Length > noteIndex) ? noteClips[noteIndex] : null,
                highlightColor = Color.yellow
            };
            pianoKeys.Add(pk);
        }

        Debug.Log($"[Piano] Auto-mapping : {keys.Count} touches mappées.");
    }

    int ExtractKeyNumber(string name)
    {
        string digits = "";
        for (int i = name.Length - 1; i >= 0; i--)
        {
            if (char.IsDigit(name[i]))
                digits = name[i] + digits;
            else
                break;
        }
        return digits.Length > 0 ? int.Parse(digits) : 0;
    }

    void HandleHover()
    {
        if (playerCamera == null) return;

        PianoKey detectedKey = null;

        // Raycast depuis le centre exact de l'écran (Réticule)
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hit;

        bool didHit = (pianoLayer.value != 0)
            ? Physics.Raycast(ray, out hit, interactionRange, pianoLayer)
            : Physics.Raycast(ray, out hit, interactionRange);

        if (didHit && hit.collider != null)
        {
            if (objectMap.TryGetValue(hit.collider.gameObject, out PianoKey key))
            {
                detectedKey = key;
            }
        }

        if (detectedKey != hoveredKey)
        {
            if (hoveredKey != null && hoveredKey.keyObject != null)
            {
                if (hoverableMap.TryGetValue(hoveredKey.keyObject, out Hoverable prevHov))
                    prevHov.OnHoverExit();
                SetKeyEmission(hoveredKey.keyObject, Color.black);
            }

            hoveredKey = detectedKey;

            if (hoveredKey != null && hoveredKey.keyObject != null)
            {
                if (hoverableMap.TryGetValue(hoveredKey.keyObject, out Hoverable newHov))
                    newHov.OnHoverEnter();
                SetKeyEmission(hoveredKey.keyObject, hoveredKey.highlightColor * 2f);
            }

            if (interactPromptUI != null)
                interactPromptUI.SetActive(hoveredKey != null);

            if (noteLabel != null)
            {
                if (hoveredKey != null && hoveredKey.keyObject != null)
                {
                    noteLabel.SetActive(true);
                    noteLabel.transform.position = hoveredKey.keyObject.transform.position + Vector3.up * labelHeight;
                    noteLabel.transform.rotation = playerCamera.transform.rotation;
                    noteLabel.GetComponent<TextMeshPro>().text = hoveredKey.noteName;
                }
                else
                {
                    noteLabel.SetActive(false);
                }
            }
        }
    }

    // Méthode sécurisée URP utilisant MaterialPropertyBlock pour la couleur d'émission
    void SetKeyEmission(GameObject keyObj, Color color)
    {
        Renderer rend = keyObj.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.GetPropertyBlock(mpb);
            mpb.SetColor("_EmissionColor", color);
            rend.SetPropertyBlock(mpb);
        }
    }

    public void ClearHover()
    {
        if (hoveredKey != null && hoveredKey.keyObject != null)
        {
            SetKeyEmission(hoveredKey.keyObject, Color.black);
        }
        hoveredKey = null;
        if (interactPromptUI != null)
            interactPromptUI.SetActive(false);
        if (noteLabel != null)
            noteLabel.SetActive(false);
    }

    public void PlayNote(PianoKey key)
    {
        if (key == null || key.audioClip == null) return;

        audioSource.PlayOneShot(key.audioClip, volume);

        if (key.keyObject != null)
            StartCoroutine(AnimateKeyPress(key.keyObject));

        RecordNoteForSequence(key.noteName);
        Debug.Log($"[Piano] ♪ {key.noteName}");
    }

    public void PlayNoteByName(string noteName)
    {
        if (keyMap.TryGetValue(noteName, out PianoKey key))
            PlayNote(key);
    }

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

        // Si la note jouée ne correspond pas à la partition cible
        if (playerSequence[idx] != targetSequence[idx])
        {
            Debug.Log("[Piano] ✗ Mauvaise note ! Séquence réinitialisée.");
            FailSequence();
            return;
        }

        // Si toute la séquence est réussie
        if (playerSequence.Count == targetSequence.Count)
        {
            Debug.Log("[Piano] ✓ Séquence correcte ! Énigme résolue.");
            SuccessSequence();
        }
    }

    void SuccessSequence()
    {
        if (puzzleSolved) return;
        puzzleSolved = true;

        sequenceActive = false;
        playerSequence.Clear();
        onSequenceSuccess?.Invoke();

        SpawnChest();
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

    void SpawnChest()
    {
        if (chestPrefab == null)
        {
            Debug.LogWarning("[Piano] Pas de prefab coffre assigné !");
            return;
        }

        Vector3 spawnPos = chestSpawnPoint != null
            ? chestSpawnPoint.position
            : transform.position + transform.right * 2f;

        Quaternion spawnRot = chestSpawnPoint != null
            ? chestSpawnPoint.rotation
            : Quaternion.identity;

        GameObject chest = Instantiate(chestPrefab, spawnPos, spawnRot);
        Debug.Log("[Piano] Coffre apparu !");
        StartCoroutine(AnimateChestPop(chest));
    }

    IEnumerator AnimateChestPop(GameObject chest)
    {
        Vector3 targetScale = chest.transform.localScale;
        chest.transform.localScale = Vector3.zero;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / chestPopDuration;
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            chest.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, eased);
            yield return null;
        }
        chest.transform.localScale = targetScale;
    }

    IEnumerator AnimateKeyPress(GameObject keyObj)
    {
        if (!originalPositions.TryGetValue(keyObj, out Vector3 original)) yield break;

        float pressDepth = 0.01f; // Enfoncement de 1cm
        float duration = 0.07f;
        Vector3 pressed = original + Vector3.down * pressDepth;

        // Enfoncement
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            keyObj.transform.localPosition = Vector3.Lerp(original, pressed, t);
            yield return null;
        }

        // Retour à la position initiale
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / (duration * 1.5f);
            keyObj.transform.localPosition = Vector3.Lerp(pressed, original, t);
            yield return null;
        }
        keyObj.transform.localPosition = original;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
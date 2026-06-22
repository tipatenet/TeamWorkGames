using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    [Header("Paramètres audio")]
    [Range(0f, 1f)]
    public float volume = 0.8f;

    [Header("Paramètres d'interaction")]
    public float interactionRange = 3f;
    public KeyCode playKey = KeyCode.E;
    public LayerMask pianoLayer;

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

    private List<string> playerSequence = new List<string>();
    private bool sequenceActive = false;
    private float sequenceTimer = 0f;

    private Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>();
    private MaterialPropertyBlock mpb;
    private PianoKey hoveredKey = null;

    [Header("État du Piano")]
    public bool isActive = false; // /!\ Doit être mis à TRUE (via ton script de chaise/interaction) pour que le piano s'active

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

        foreach (var key in pianoKeys)
        {
            if (!string.IsNullOrEmpty(key.noteName))
                keyMap[key.noteName] = key;

            if (key.keyObject != null)
            {
                objectMap[key.keyObject] = key;
                originalPositions[key.keyObject] = key.keyObject.transform.localPosition;

                // Pré-activer le mot-clé d'émission sur le matériau partagé pour éviter les bugs
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
        // Si le joueur n'est pas assis/actif sur le piano, on ne fait rien
        if (!isActive) return;

        HandleHover();

        // Si on appuie sur la touche d'interaction (E) et qu'on regarde une touche valide
        if (Input.GetKeyDown(playKey) && hoveredKey != null)
        {
            PlayNote(hoveredKey);
        }

        // Gestion du timer de l'énigme
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

        // Gestion du changement d'état (Hover Enter / Hover Exit)
        if (detectedKey != hoveredKey)
        {
            // 1. Quitter l'ancienne touche (Reset visuel)
            if (hoveredKey != null && hoveredKey.keyObject != null)
            {
                hoveredKey.keyObject.GetComponent<Hoverable>().OnHoverExit();

                SetKeyEmission(hoveredKey.keyObject, Color.black);
            }

            hoveredKey = detectedKey;

            // 2. Entrer sur la nouvelle touche (Allumage visuel)
            if (hoveredKey != null && hoveredKey.keyObject != null)
            {
                hoveredKey.keyObject.GetComponent<Hoverable>().OnHoverEnter();
                SetKeyEmission(hoveredKey.keyObject, hoveredKey.highlightColor * 2f); // Intensité de 2f
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
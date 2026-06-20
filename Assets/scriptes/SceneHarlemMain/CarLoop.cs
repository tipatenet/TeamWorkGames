using UnityEngine;

public class CarLoop : MonoBehaviour
{
    [Header("Route")]
    public Transform[] waypointsLaneA;
    public Transform[] waypointsLaneB;

    [Header("Départ")]
    public bool startOnLaneA = true;
    public int startWaypointIndex = 0;

    [Header("Paramètres")]
    public float speed = 10f;
    public float waypointThreshold = 0.5f;

    [Header("Détection obstacle")]
    public float detectionDistance = 15f;
    public float minDistance = 3f; // Espace minimum entre les voitures
    public LayerMask obstacleLayer;
    public LayerMask carLayer;

    [Header("Audio")]
    [Tooltip("Glisse le composant AudioSource de la voiture ici")]
    public AudioSource stopAudioSource;

    private Transform[] currentLane;
    private int currentWaypointIndex;
    private bool onLaneA;
    private bool isStopped = false;
    private bool wasStoppedLastFrame = false; // Permet de savoir si le son joue déjà

    void Start()
    {
        onLaneA = startOnLaneA;
        currentLane = onLaneA ? waypointsLaneA : waypointsLaneB;
        currentWaypointIndex = Mathf.Clamp(startWaypointIndex, 0, currentLane.Length - 1);
        transform.position = currentLane[currentWaypointIndex].position;

        // Sécurité : si la case est vide dans l'inspecteur, on cherche l'AudioSource sur l'objet
        if (stopAudioSource == null)
        {
            stopAudioSource = GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        MoveTowardsWaypoint();
        HandleAudio();

        // Le rayon de debug change de couleur si la voiture détecte un obstacle et freine/s'arrête
        Debug.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * detectionDistance,
            isStopped ? Color.red : Color.green);
    }

    void MoveTowardsWaypoint()
    {
        if (currentLane == null || currentLane.Length == 0) return;

        Transform target = currentLane[currentWaypointIndex];
        Vector3 direction = (target.position - transform.position).normalized;

        float currentSpeed = speed;
        isStopped = false;

        // Détection large devant (légèrement surélevée pour éviter le sol)
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            1.5f, // Rayon de la sphère de détection
            transform.forward,
            detectionDistance,
            obstacleLayer | carLayer
        );

        float closestDistance = Mathf.Infinity;

        foreach (RaycastHit hit in hits)
        {
            // Ignore son propre collider et ses propres enfants
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
                continue;

            // Utilisation de la distance d'impact du point de contact (plus précis)
            if (hit.distance < closestDistance)
                closestDistance = hit.distance;
        }

        if (closestDistance < Mathf.Infinity)
        {
            if (closestDistance <= minDistance)
            {
                // Trop proche → arrêt total
                currentSpeed = 0f;
                isStopped = true;
            }
            else
            {
                // Ralentissement progressif entre detectionDistance et minDistance
                float ratio = Mathf.Clamp01((closestDistance - minDistance) / (detectionDistance - minDistance));
                currentSpeed = speed * ratio;
            }
        }

        // Déplacement
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            currentSpeed * Time.deltaTime
        );

        // Rotation fluide uniquement sur l'axe Y (évite que la voiture ne plonge en avant/arrière)
        if (direction != Vector3.zero)
        {
            direction.y = 0;
            if (direction != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(direction);
        }

        // Changement de waypoint
        if (Vector3.Distance(transform.position, target.position) < waypointThreshold)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= currentLane.Length)
                SwitchLane();
        }
    }

    // Gestion du son d'arrêt
    void HandleAudio()
    {
        if (stopAudioSource == null) return;

        // Si la voiture vient de s'arrêter à cette frame
        if (isStopped && !wasStoppedLastFrame)
        {
            if (!stopAudioSource.isPlaying)
            {
                stopAudioSource.Play();
            }
        }
        // Si la voiture vient de redémarrer à cette frame
        else if (!isStopped && wasStoppedLastFrame)
        {
            if (stopAudioSource.isPlaying)
            {
                stopAudioSource.Stop();
            }
        }

        // Sauvegarde de l'état pour la frame suivante
        wasStoppedLastFrame = isStopped;
    }

    void SwitchLane()
    {
        onLaneA = !onLaneA;
        currentLane = onLaneA ? waypointsLaneA : waypointsLaneB;
        currentWaypointIndex = 0;
    }
}
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
    public float detectionDistance = 5f;
    public LayerMask obstacleLayer;
    public LayerMask carLayer;

    private Transform[] currentLane;
    private int currentWaypointIndex;
    private bool onLaneA;
    private bool isStopped = false;

    void Start()
    {
        onLaneA = startOnLaneA;
        currentLane = onLaneA ? waypointsLaneA : waypointsLaneB;
        currentWaypointIndex = Mathf.Clamp(startWaypointIndex, 0, currentLane.Length - 1);
        transform.position = currentLane[currentWaypointIndex].position;
    }

    void Update()
    {
        CheckForObstacle();

        if (!isStopped)
            MoveTowardsWaypoint();
    }

    void CheckForObstacle()
    {
        LayerMask combined = obstacleLayer | carLayer;

        RaycastHit[] hits = Physics.SphereCastAll(
            transform.position + transform.forward * 2f,
            1.5f,
            transform.forward,
            detectionDistance,
            combined
        );

        isStopped = false;
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject == gameObject) continue;
            isStopped = true;
            break;
        }

        Debug.DrawRay(transform.position, transform.forward * detectionDistance,
            isStopped ? Color.red : Color.green);
    }

    void MoveTowardsWaypoint()
    {
        if (currentLane == null || currentLane.Length == 0) return;

        Transform target = currentLane[currentWaypointIndex];
        Vector3 direction = (target.position - transform.position).normalized;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);

        if (Vector3.Distance(transform.position, target.position) < waypointThreshold)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= currentLane.Length)
                SwitchLane();
        }
    }

    void SwitchLane()
    {
        onLaneA = !onLaneA;
        currentLane = onLaneA ? waypointsLaneA : waypointsLaneB;
        currentWaypointIndex = 0;
        transform.position = currentLane[0].position;
    }
}
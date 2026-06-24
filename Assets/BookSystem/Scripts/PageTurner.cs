using UnityEngine;
[RequireComponent(typeof(MeshFilter))]
public class PageTurner : MonoBehaviour
{
    [Header("Animation")]
    public float turnDuration = 1.2f;
    public float maxCurve = 0.6f;

    [Header("Rotation")]
    public Vector3 startRotation = Vector3.zero;
    public Vector3 endRotation = new Vector3(-180f, 0f, 0f);

    [Header("Fold Pivot (0 = left/bottom, 1 = right/top)")]
    [Range(0f, 1f)] public float foldOriginX = 1f;
    [Range(0f, 1f)] public float foldOriginY = 0f;

    private Mesh mesh;
    private Vector3[] originalVertices;
    private Vector3[] vertices;
    private bool turning;
    private float timer;
    private float minX, maxX, width;
    private float minY, maxY, height;

    // Rotation réelle depuis laquelle on interpole
    // (peut être mid-animation si on interrompt)
    private Quaternion actualStartRotation;

    public enum State { NotTurn, Turning }
    public State currentState = State.NotTurn;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        originalVertices = mesh.vertices;
        vertices = new Vector3[originalVertices.Length];

        if (originalVertices.Length == 0)
        {
            Debug.LogError("Mesh vide !");
            return;
        }

        minX = maxX = originalVertices[0].x;
        minY = maxY = originalVertices[0].y;

        for (int i = 1; i < originalVertices.Length; i++)
        {
            minX = Mathf.Min(minX, originalVertices[i].x);
            maxX = Mathf.Max(maxX, originalVertices[i].x);
            minY = Mathf.Min(minY, originalVertices[i].y);
            maxY = Mathf.Max(maxY, originalVertices[i].y);
        }

        width = maxX - minX;
        height = maxY - minY;

        transform.localRotation = Quaternion.Euler(startRotation);
    }

    void Update()
    {
        if (currentState != State.Turning) return;

        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / turnDuration);
        float smooth = Mathf.SmoothStep(0f, 1f, progress);

        // Slerp depuis la rotation capturée au moment du clic
        // (pas startRotation fixe — peut être mid-animation)
        transform.localRotation = Quaternion.Slerp(
            actualStartRotation,
            Quaternion.Euler(endRotation),
            smooth
        );

        float curveAmount = Mathf.Sin(progress * Mathf.PI) * maxCurve;
        BendPage(curveAmount, progress);

        if (progress >= 1f)
            FinishTurn();
    }

    public void TurnPage()
    {
        // Capture la rotation ACTUELLE comme point de départ
        // Si la page était en train de tourner dans l'autre sens,
        // on repart proprement depuis là où elle en est
        actualStartRotation = transform.localRotation;

        timer = 0f;
        currentState = State.Turning;
    }

    void FinishTurn()
    {
        currentState = State.NotTurn;
        transform.localRotation = Quaternion.Euler(endRotation);
        BendPage(0f, 1f);
    }

    void BendPage(float curveAmount, float progress)
    {
        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 v = originalVertices[i];
            float tx = (v.x - minX) / width;
            float ty = (v.y - minY) / height;
            float dx = tx - foldOriginX;
            float dy = ty - foldOriginY;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            float edgeWeight = Mathf.Clamp01(dist);
            float bend = Mathf.Sin(dist * Mathf.PI) * edgeWeight * curveAmount;
            float roll = Mathf.Sin(progress * Mathf.PI) *
                         edgeWeight * edgeWeight *
                         maxCurve * 0.5f;

            Vector3 newVertex = v;
            newVertex.y += bend;
            newVertex.x -= roll;
            vertices[i] = newVertex;
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}
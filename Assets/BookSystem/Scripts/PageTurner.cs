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
    [Range(0f, 1f)]
    public float foldOriginX = 1f;   // 0 = gauche (reliure), 1 = droite
    [Range(0f, 1f)]
    public float foldOriginY = 0f;   // 0 = bas, 1 = haut

    private Mesh mesh;
    private Vector3[] originalVertices;
    private Vector3[] vertices;

    private bool turning;
    private float timer;

    private float minX, maxX, width;
    private float minY, maxY, height;

    public enum states { 
        turn,
        notTurn
    };

    public states currentState = states.notTurn;

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

        // bounds X
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
        if (!turning) return;

        timer += Time.deltaTime;

        float progress = Mathf.Clamp01(timer / turnDuration);
        float smooth = Mathf.SmoothStep(0f, 1f, progress);

        transform.localRotation = Quaternion.Euler(
            Vector3.Lerp(startRotation, endRotation, smooth)
        );

        float curveAmount = Mathf.Sin(progress * Mathf.PI) * maxCurve;

        BendPage(curveAmount, progress);

        if (progress >= 1f)
        {
            turning = false;
            transform.localRotation = Quaternion.Euler(endRotation);
            BendPage(0f, 1f);
        }
    }

    public void TurnPage()
    {
        timer = 0f;
        turning = true;
    }

    void BendPage(float curveAmount, float progress)
    {
        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 v = originalVertices[i];

            // normalisation 0–1
            float tx = (v.x - minX) / width;
            float ty = (v.y - minY) / height;

            // distance au point de pli configurable
            float dx = tx - foldOriginX;
            float dy = ty - foldOriginY;

            float dist = Mathf.Sqrt(dx * dx + dy * dy);

            // plus loin du pivot = plus de pli
            float edgeWeight = Mathf.Clamp01(dist);

            // courbure principale type "papier"
            float bend = Mathf.Sin(dist * Mathf.PI) * edgeWeight * curveAmount;

            // roulage pendant rotation
            float roll = Mathf.Sin(progress * Mathf.PI) *
                         edgeWeight * edgeWeight *
                         maxCurve * 0.5f;

            Vector3 newVertex = v;

            // courbure verticale
            newVertex.y += bend;

            // déplacement latéral effet page qui se retourne
            newVertex.x -= roll;

            vertices[i] = newVertex;
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}
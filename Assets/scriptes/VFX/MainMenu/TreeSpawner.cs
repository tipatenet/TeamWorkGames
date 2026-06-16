using System.Collections.Generic;
using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] treePrefabs;
    [SerializeField] private int treeCount = 30;
    [SerializeField] private float roadWidth = 6f;
    [SerializeField] private float spreadDistance = 100f;
    [SerializeField] private float minOffsetFromRoad = 3f;
    [SerializeField] private float maxOffsetFromRoad = 15f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float minDistanceBetweenTrees = 4f;
    [SerializeField] private int maxSpawnAttempts = 20;

    private List<Transform> trees = new List<Transform>();
    private List<Vector3> spawnPositions = new List<Vector3>();

    private void Start()
    {
        for (int i = 0; i < treeCount; i++)
        {
            SpawnTree(Random.Range(-spreadDistance / 2f, spreadDistance / 2f));
        }
    }

    private void SpawnTree(float zPos)
    {
        Vector3 pos = Vector3.zero;
        bool foundValidPosition = false;

        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            float side = Random.value > 0.5f ? 1f : -1f;
            float xOffset = side * (roadWidth + Random.Range(minOffsetFromRoad, maxOffsetFromRoad));

            // A chaque tentative, on varie aussi legerement le zPos pour eviter les alignements parfaits
            float candidateZ = zPos + Random.Range(-2f, 2f);

            Vector3 candidatePos = new Vector3(xOffset, 0f, candidateZ);

            if (IsPositionValid(candidatePos))
            {
                pos = candidatePos;
                foundValidPosition = true;
                break;
            }
        }

        // Si aucune position valide trouvee apres plusieurs tentatives, on annule ce spawn
        if (!foundValidPosition) return;

        GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
        Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        GameObject tree = Instantiate(prefab, pos, rot, transform);
        trees.Add(tree.transform);
        spawnPositions.Add(pos);
    }

    private bool IsPositionValid(Vector3 candidatePos)
    {
        foreach (var existingPos in spawnPositions)
        {
            if (Vector3.Distance(candidatePos, existingPos) < minDistanceBetweenTrees)
            {
                return false;
            }
        }
        return true;
    }

    private void Update()
    {
        for (int i = 0; i < trees.Count; i++)
        {
            Transform tree = trees[i];
            tree.position += Vector3.back * moveSpeed * Time.deltaTime;

            if (tree.position.z < -spreadDistance / 2f)
            {
                Vector3 newPos = new Vector3(
                    tree.position.x,
                    tree.position.y,
                    spreadDistance / 2f
                );

                tree.position = newPos;
                spawnPositions[i] = newPos;
            }
            else
            {
                spawnPositions[i] = tree.position;
            }
        }
    }
}
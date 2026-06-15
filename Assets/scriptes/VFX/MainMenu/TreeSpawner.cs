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

    private List<Transform> trees = new List<Transform>();

    private void Start()
    {
        for (int i = 0; i < treeCount; i++)
        {
            SpawnTree(Random.Range(-spreadDistance / 2f, spreadDistance / 2f));
        }
    }

    private void SpawnTree(float zPos)
    {
        GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];

        // Choisit un cote (gauche ou droite)
        float side = Random.value > 0.5f ? 1f : -1f;
        float xOffset = side * (roadWidth + Random.Range(minOffsetFromRoad, maxOffsetFromRoad));

        Vector3 pos = new Vector3(xOffset, 0f, zPos);
        Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        GameObject tree = Instantiate(prefab, pos, rot, transform);
        trees.Add(tree.transform);
    }

    private void Update()
    {
        foreach (var tree in trees)
        {
            tree.position += Vector3.back * moveSpeed * Time.deltaTime;

            if (tree.position.z < -spreadDistance / 2f)
            {
                tree.position = new Vector3(
                    tree.position.x,
                    tree.position.y,
                    spreadDistance / 2f
                );
            }
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

public class LampPostSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] lampPostPrefabs;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spreadDistance = 100f;
    [SerializeField] private float moveSpeed = 5f;

    [SerializeField] private float minSpawnInterval = 4f;
    [SerializeField] private float maxSpawnInterval = 10f;

    private List<Transform> lampPosts = new List<Transform>();
    private List<int> availableSpawnIndices = new List<int>();
    private float nextSpawnTime;

    private void Start()
    {
        ResetAvailableIndices();
        ScheduleNextSpawn();
    }

    private void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnLampPost();
            ScheduleNextSpawn();
        }

        for (int i = lampPosts.Count - 1; i >= 0; i--)
        {
            Transform lamp = lampPosts[i];
            lamp.position += Vector3.back * moveSpeed * Time.deltaTime;

            // Detruit le lampadaire une fois sorti de la zone visible
            if (lamp.position.z < -spreadDistance / 2f)
            {
                Destroy(lamp.gameObject);
                lampPosts.RemoveAt(i);
            }
        }
    }

    private void ScheduleNextSpawn()
    {
        nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    private void ResetAvailableIndices()
    {
        availableSpawnIndices.Clear();
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            availableSpawnIndices.Add(i);
        }
    }

    private void SpawnLampPost()
    {
        if (lampPostPrefabs.Length == 0 || spawnPoints.Length == 0) return;

        // Si tous les points ont ete utilises, on recommence le cycle
        if (availableSpawnIndices.Count == 0)
        {
            ResetAvailableIndices();
        }

        int randomIndex = Random.Range(0, availableSpawnIndices.Count);
        int spawnPointIndex = availableSpawnIndices[randomIndex];
        availableSpawnIndices.RemoveAt(randomIndex);

        Transform spawnPoint = spawnPoints[spawnPointIndex];
        GameObject prefab = lampPostPrefabs[Random.Range(0, lampPostPrefabs.Length)];

        // Le point de spawn defile avec la route, donc on spawn devant a une position fixe en X/Y,
        // mais decalee en Z pour apparaitre au bout de la zone visible
        Vector3 pos = new Vector3(
            spawnPoint.position.x,
            spawnPoint.position.y,
            spreadDistance / 2f
        );

        GameObject lamp = Instantiate(prefab, pos, spawnPoint.rotation, transform);
        lampPosts.Add(lamp.transform);
    }
}
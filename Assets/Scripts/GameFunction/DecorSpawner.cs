using UnityEngine;
using System.Collections.Generic;

public class DecorSpawner : MonoBehaviour
{
    [Header("Decor Prefabs")]
    public GameObject[] decorPrefabs;

    [Header("Decor Parent")]
    public GameObject decorsParent;

    [Header("Decor Area")]
    public float startX;
    public float endX;
    public float startY;
    public float endY;

    [Header("Decor Spawn Amount")]
    public int decorAmount = 50;

    [Header("Spawn Protection Areas")]
    public List<SpawnProtection> spawnProtectionAreas = new List<SpawnProtection>();

    [System.Serializable]
    public class SpawnProtection
    {
        public float startX;
        public float endX;
        public float startY;
        public float endY;
    }

    void Start()
    {
        SpawnDecors();
    }

    void SpawnDecors()
    {
        if (decorPrefabs.Length == 0)
        {
            Debug.LogWarning("No decor prefabs have been added!");
            return;
        }

        for (int i = 0; i < decorAmount; i++)
        {
            Vector2 spawnPosition = GetRandomSpawnPosition();

            // Pick a random decor prefab
            GameObject randomDecor =
                decorPrefabs[Random.Range(0, decorPrefabs.Length)];

            // Spawn the decor
            GameObject decor = Instantiate(
                randomDecor,
                spawnPosition,
                Quaternion.identity,
                decorsParent.transform
            );
        }
    }

    Vector2 GetRandomSpawnPosition()
    {
        Vector2 position;

        // Keep trying until we find a valid position
        do
        {
            float randomX = Random.Range(startX, endX);
            float randomY = Random.Range(startY, endY);

            position = new Vector2(randomX, randomY);

        } while (IsProtected(position));

        return position;
    }

    bool IsProtected(Vector2 position)
    {
        foreach (SpawnProtection area in spawnProtectionAreas)
        {
            if (position.x >= area.startX &&
                position.x <= area.endX &&
                position.y >= area.startY &&
                position.y <= area.endY)
            {
                return true;
            }
        }

        return false;
    }
}
using UnityEngine;
using System.Collections.Generic;

public class TreeSpawner : MonoBehaviour
{
    [Header("Tree Prefabs")]
    public GameObject[] treePrefabs;

    [Header("Trees Parent")]
    public GameObject treesParent;

    [Header("Spawn Area")]
    public float startX;
    public float endX;
    public float startY;
    public float endY;

    [Header("Tree Spawn Amount")]
    public int treeAmount = 50;

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
        SpawnTrees();
    }

    void SpawnTrees()
    {
        if (treePrefabs.Length == 0)
        {
            Debug.LogWarning("No tree prefabs have been added!");
            return;
        }

        for (int i = 0; i < treeAmount; i++)
        {
            Vector2 spawnPosition = GetRandomSpawnPosition();

            // Pick a random tree prefab
            GameObject randomTree =
                treePrefabs[Random.Range(0, treePrefabs.Length)];

            // Spawn the tree
            GameObject tree = Instantiate(
                randomTree,
                spawnPosition,
                Quaternion.identity,
                treesParent.transform
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
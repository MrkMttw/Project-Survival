using UnityEngine;
using System.Collections.Generic;

public class StoneSpawner : MonoBehaviour
{
    [Header("Stone Prefabs")]
    public GameObject[] stonePrefabs;

    [Header("Stone Parent")]
    public GameObject stonesParent;

    [Header("Spawn Area")]
    public float startX;
    public float endX;
    public float startY;
    public float endY;

    [Header("Stone Spawn Amount")]
    public int stoneAmount = 50;

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
        SpawnStones();
    }

    void SpawnStones()
    {
        if (stonePrefabs.Length == 0)
        {
            Debug.LogWarning("No stone prefabs have been added!");
            return;
        }

        for (int i = 0; i < stoneAmount; i++)
        {
            Vector2 spawnPosition = GetRandomSpawnPosition();

            // Pick a random stone prefab
            GameObject randomStone =
                stonePrefabs[Random.Range(0, stonePrefabs.Length)];

            // Spawn the stone
            GameObject stone = Instantiate(
                randomStone,
                spawnPosition,
                Quaternion.identity,
                stonesParent.transform
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
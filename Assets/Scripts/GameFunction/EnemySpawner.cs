using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnData
    {
        [Header("Enemy")]
        public GameObject enemyPrefab;

        [Header("Spawn Settings")]
        [Range(0f, 100f)]
        public float spawnPercentage = 50f;

        [Header("Day Requirement")]
        public int spawnFromDay = 1;

        [Header("Time")]
        [Range(0f, 24f)]
        public float spawnStartTime = 18f;

        [Range(0f, 24f)]
        public float spawnEndTime = 6f;
    }

    [Header("Game Clock")]
    public GameClock gameClock;

    [Header("Enemy Presets")]
    public EnemySpawnData[] enemies;

    [Header("Enemy Parent")]
    public Transform enemyParent;

    [Header("Random Spawn Area")]
    public float startX = -20f;
    public float endX = 20f;
    public float startY = -20f;
    public float endY = 20f;

    [Header("Spawn Settings")]
    public float spawnInterval = 5f;
    public int maxEnemies = 10;

    private float spawnTimer;

    private void Update()
    {
        if (gameClock == null)
            return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            TrySpawnEnemy();
        }
    }

    private void TrySpawnEnemy()
    {
        if (!CanSpawn())
            return;

        EnemySpawnData selectedEnemy = ChooseEnemy();

        if (selectedEnemy == null)
            return;

        SpawnEnemy(selectedEnemy);
    }

    private bool CanSpawn()
    {
        if (enemyParent == null)
            return false;

        if (enemyParent.childCount >= maxEnemies)
            return false;

        return true;
    }

    private EnemySpawnData ChooseEnemy()
    {
        if (enemies == null || enemies.Length == 0)
            return null;

        float totalPercentage = 0f;

        foreach (EnemySpawnData enemy in enemies)
        {
            if (enemy.enemyPrefab == null)
                continue;

            if (!CanEnemySpawn(enemy))
                continue;

            totalPercentage += enemy.spawnPercentage;
        }

        if (totalPercentage <= 0f)
            return null;

        float randomValue = Random.Range(0f, totalPercentage);

        foreach (EnemySpawnData enemy in enemies)
        {
            if (enemy.enemyPrefab == null)
                continue;

            if (!CanEnemySpawn(enemy))
                continue;

            randomValue -= enemy.spawnPercentage;

            if (randomValue <= 0f)
                return enemy;
        }

        return null;
    }

    private bool CanEnemySpawn(EnemySpawnData enemy)
    {
        // Check day
        int currentDay = gameClock.GetDay();

        if (currentDay < enemy.spawnFromDay)
            return false;

        // Check time
        return IsWithinSpawnTime(
            enemy.spawnStartTime,
            enemy.spawnEndTime
        );
    }

    private bool IsWithinSpawnTime(float startTime, float endTime)
    {
        float currentTime =
            gameClock.GetHour() +
            (gameClock.GetMinute() / 60f);

        // Normal range
        // Example: 08:00 -> 17:00
        if (startTime < endTime)
        {
            return currentTime >= startTime &&
                   currentTime < endTime;
        }

        // Crosses midnight
        // Example: 18:00 -> 06:00
        return currentTime >= startTime ||
               currentTime < endTime;
    }

    private void SpawnEnemy(EnemySpawnData enemy)
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();

        GameObject spawnedEnemy = Instantiate(
            enemy.enemyPrefab,
            spawnPosition,
            Quaternion.identity,
            enemyParent
        );

        spawnedEnemy.name = enemy.enemyPrefab.name;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        float randomX = Random.Range(startX, endX);
        float randomY = Random.Range(startY, endY);

        return new Vector3(randomX, randomY, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 center = new Vector3(
            (startX + endX) / 2f,
            (startY + endY) / 2f,
            0f
        );

        Vector3 size = new Vector3(
            Mathf.Abs(endX - startX),
            Mathf.Abs(endY - startY),
            0f
        );

        Gizmos.DrawWireCube(center, size);
    }
}
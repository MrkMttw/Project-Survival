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

    [Header("Player Spawn Area")]
    [Tooltip("Maximum distance from the player where enemies can spawn.")]
    public float spawnRadius = 15f;

    [Tooltip("Enemies cannot spawn within this distance of the player.")]
    public float spawnProtectionRadius = 5f;

    [Header("Spawn Settings")]
    public float spawnInterval = 5f;
    public int maxEnemies = 10;

    private Transform player;
    private float spawnTimer;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError(
                "EnemySpawner: Player with tag 'Player' not found!"
            );
        }
    }

    private void Update()
    {
        if (gameClock == null || player == null)
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
        int currentDay = gameClock.GetDay();

        if (currentDay < enemy.spawnFromDay)
            return false;

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

        // Normal time range
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
        Vector2 randomDirection =
            Random.insideUnitCircle.normalized;

        float randomDistance = Random.Range(
            spawnProtectionRadius,
            spawnRadius
        );

        Vector3 spawnPosition =
            player.position +
            new Vector3(
                randomDirection.x,
                randomDirection.y,
                0f
            ) * randomDistance;

        return spawnPosition;
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null)
            return;

        // Spawn radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            player.position,
            spawnRadius
        );

        // Protection radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(
            player.position,
            spawnProtectionRadius
        );
    }
}
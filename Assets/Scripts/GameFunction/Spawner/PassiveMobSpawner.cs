
using UnityEngine;

public class PassiveMobSpawner : MonoBehaviour
{
    [System.Serializable]
    public class PassiveMobSpawnData
    {
        [Header("Animal")]
        public GameObject mobPrefab;

        [Header("Spawn Chance")]
        [Range(0f, 100f)]
        public float spawnPercentage = 50f;
    }

    [Header("Animal Presets")]
    public PassiveMobSpawnData[] passiveMobs;

    [Header("Mob Parent")]
    public Transform mobParent;

    [Header("Player Spawn Area")]
    [Tooltip("Maximum distance from the player where animals can spawn.")]
    public float spawnRadius = 15f;

    [Tooltip("Animals cannot spawn within this distance of the player.")]
    public float spawnProtectionRadius = 5f;

    [Header("Spawn Settings")]
    public float spawnInterval = 5f;
    public int maxMobs = 10;

    private Transform player;
    private float spawnTimer;

    private void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError(
                "PassiveMobSpawner: Player with tag 'Player' not found!",
                this
            );
        }
    }

    private void Update()
    {
        if (player == null || mobParent == null)
            return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            TrySpawnMob();
        }
    }

    private void TrySpawnMob()
    {
        if (mobParent.childCount >= maxMobs)
            return;

        PassiveMobSpawnData selectedMob = ChooseMob();

        if (selectedMob == null)
            return;

        SpawnMob(selectedMob);
    }

    private PassiveMobSpawnData ChooseMob()
    {
        if (passiveMobs == null || passiveMobs.Length == 0)
            return null;

        float totalPercentage = 0f;

        foreach (PassiveMobSpawnData mob in passiveMobs)
        {
            if (mob.mobPrefab == null || mob.spawnPercentage <= 0f)
                continue;

            totalPercentage += mob.spawnPercentage;
        }

        if (totalPercentage <= 0f)
            return null;

        float randomValue = Random.Range(0f, totalPercentage);

        foreach (PassiveMobSpawnData mob in passiveMobs)
        {
            if (mob.mobPrefab == null || mob.spawnPercentage <= 0f)
                continue;

            randomValue -= mob.spawnPercentage;

            if (randomValue <= 0f)
                return mob;
        }

        return null;
    }

    private void SpawnMob(PassiveMobSpawnData mob)
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();

        GameObject spawnedMob = Instantiate(
            mob.mobPrefab,
            spawnPosition,
            Quaternion.identity,
            mobParent
        );

        spawnedMob.name = mob.mobPrefab.name;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector2 randomDirection =
            Random.insideUnitCircle.normalized;

        float randomDistance = Random.Range(
            spawnProtectionRadius,
            spawnRadius
        );

        return player.position + new Vector3(
            randomDirection.x,
            randomDirection.y,
            0f
        ) * randomDistance;
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, spawnRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(
            player.position,
            spawnProtectionRadius
        );
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobSpawnerController : MonoBehaviour
{
    [System.Serializable]
    public class HostileSpawnData
    {
        public GameObject enemyPrefab;

        [Range(0, 100)]
        public int spawnPercentage = 50;

        public int spawnFromDay = 1;

        [Range(0, 23)]
        public int spawnStartTime = 18;

        [Range(0, 23)]
        public int spawnEndTime = 6;
    }

    [System.Serializable]
    public class PassiveSpawnData
    {
        public GameObject mobPrefab;

        [Range(0, 100)]
        public int spawnPercentage = 50;
    }

    [Header("Global Population Limits")]
    public int maxHostileMobs = 15;
    public int maxPassiveMobs = 15;

    [Header("Hostile Spawn Settings")]
    public HostileSpawnData[] hostileSpawnData;
    public float hostileSpawnInterval = 5f;
    public float hostileSpawnRadius = 15f;
    public float hostileProtectionRadius = 5f;

    [Header("Passive Spawn Settings")]
    public PassiveSpawnData[] passiveSpawnData;
    public float passiveSpawnInterval = 5f;
    public float passiveSpawnRadius = 15f;
    public float passiveProtectionRadius = 5f;

    [Header("Boss Spawn Settings")]
    public GameObject[] bossPrefabs;

    [Min(1)]
    public int bossSpawnIntervalDays = 15;

    [Range(0, 23)]
    public int bossSpawnHour = 22;

    public float bossSpawnRadius = 20f;
    public float bossProtectionRadius = 10f;

    [Tooltip("How often the boss spawn condition is checked.")]
    public float bossSpawnCheckInterval = 1f;

    [Header("Boss Spawn Tracking")]
    [SerializeField] private int bossDayCounter = 1;
    [SerializeField] private int nextBossPrefabIndex = 0;

    private int lastTrackedDay;
    private bool bossTrackingInitialized;
    private bool bossSpawnPending;
    private float bossSpawnCheckTimer;

    [Header("References")]
    public Transform overWorld;
    public GameClock gameClock;

    [Header("Mob Organization")]
    [HideInInspector]
    public Transform activeMobsParent;

    [Header("Chunk-Based Despawning")]
    public float despawnCheckInterval = 1f;

    private float despawnCheckTimer;

    [Header("Population Tracking")]
    private HashSet<MobIdentity> hostileMobs =
        new HashSet<MobIdentity>();

    private HashSet<MobIdentity> passiveMobs =
        new HashSet<MobIdentity>();

    private HashSet<ChunkMobSpawner> activeChunks =
        new HashSet<ChunkMobSpawner>();

    private WorldGenerator worldGenerator;

    private Coroutine hostileSpawnRoutine;
    private Coroutine passiveSpawnRoutine;

    // INITIALIZATION

    private void Awake()
    {
        if (gameClock == null)
        {
            gameClock = FindFirstObjectByType<GameClock>();
        }

        worldGenerator = FindFirstObjectByType<WorldGenerator>();

        if (worldGenerator != null)
        {
            if (overWorld == null)
                overWorld = worldGenerator.overWorld;
        }

        CreateActiveMobsParent();

        if (gameClock == null)
        {
            Debug.LogError(
                "MobSpawnerController: GameClock not found."
            );
        }

        if (worldGenerator == null)
        {
            Debug.LogError(
                "MobSpawnerController: WorldGenerator not found."
            );
        }
    }

    // GLOBAL SPAWNING

    private void Start()
    {
        hostileSpawnRoutine =
            StartCoroutine(HostileSpawnRoutine());

        passiveSpawnRoutine =
            StartCoroutine(PassiveSpawnRoutine());
    }

    private IEnumerator HostileSpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(
                Mathf.Max(0.1f, hostileSpawnInterval)
            );

            if (!CanSpawnMob(MobType.Hostile))
                continue;

            if (!TryChooseHostilePrefab(out GameObject prefab))
                continue;

            if (!TryGetLoadedSpawnPosition(
                hostileSpawnRadius,
                hostileProtectionRadius,
                out Vector3 position,
                out Vector2Int originChunk,
                true))
                continue;

            SpawnMob(prefab, MobType.Hostile, position, originChunk);
        }
    }

    private IEnumerator PassiveSpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(
                Mathf.Max(0.1f, passiveSpawnInterval)
            );

            if (!CanSpawnMob(MobType.Passive))
                continue;

            if (!TryChoosePassivePrefab(out GameObject prefab))
                continue;

            if (!TryGetLoadedSpawnPosition(
                passiveSpawnRadius,
                passiveProtectionRadius,
                out Vector3 position,
                out Vector2Int originChunk))
                continue;

            SpawnMob(prefab, MobType.Passive, position, originChunk);
        }
    }

    // BOSS SPAWNING

    private void InitializeBossTracking()
    {
        if (gameClock == null)
            return;

        lastTrackedDay = gameClock.GetDay();

        // The starting day counts as the first day.
        bossDayCounter = 1;

        bossTrackingInitialized = true;
    }

    private void UpdateBossDayCounter()
    {
        if (gameClock == null)
            return;

        if (!bossTrackingInitialized)
        {
            InitializeBossTracking();
            return;
        }

        int currentDay = gameClock.GetDay();

        if (currentDay > lastTrackedDay)
        {
            int daysPassed = currentDay - lastTrackedDay;

            bossDayCounter += daysPassed;

            lastTrackedDay = currentDay;

            Debug.Log(
                "Boss Day Counter: " + bossDayCounter +
                "/" + bossSpawnIntervalDays
            );
        }

        // Boss becomes scheduled even if multiple days were skipped.
        if (bossDayCounter >= Mathf.Max(1, bossSpawnIntervalDays))
        {
            bossSpawnPending = true;
        }
    }

    private void CheckBossSpawning()
    {
        UpdateBossDayCounter();

        if (!bossSpawnPending || gameClock == null)
            return;

        bossSpawnCheckTimer += Time.deltaTime;

        if (bossSpawnCheckTimer < Mathf.Max(0.1f, bossSpawnCheckInterval))
            return;

        bossSpawnCheckTimer = 0f;

        // Wait until the configured game hour.
        if (gameClock.GetHour() < bossSpawnHour)
            return;

        TrySpawnBoss();
    }

    private void TrySpawnBoss()
    {
        if (bossPrefabs == null || bossPrefabs.Length == 0)
            return;

        if (activeMobsParent == null)
            return;

        if (!TryGetLoadedSpawnPosition(
            bossSpawnRadius,
            bossProtectionRadius,
            out Vector3 position,
            out Vector2Int originChunk))
        {
            return;
        }

        if (!TryGetNextBossPrefab(
            out GameObject selectedBoss,
            out int selectedIndex))
        {
            return;
        }

        // SPAWN BOSS
        GameObject spawnedBoss = Instantiate(
            selectedBoss,
            position,
            Quaternion.identity
        );

        spawnedBoss.transform.SetParent(
            activeMobsParent,
            true
        );

        // Automatically mark the spawned boss.
        StructureObjectIdentity bossIdentity =
            spawnedBoss.GetComponentInChildren<StructureObjectIdentity>(true);

        if (bossIdentity != null)
        {
            bossIdentity.isBoss = true;
        }
        else
        {
            Debug.LogWarning(
                "Boss prefab has no StructureObjectIdentity: " +
                spawnedBoss.name
            );
        }

        // ADVANCE BOSS PREFAB INDEX
        nextBossPrefabIndex =
            (selectedIndex + 1) % bossPrefabs.Length;

        // RESET BOSS DAY COUNTER
        bossDayCounter = 0;
        bossSpawnPending = false;

        Debug.Log(
            "BOSS SPAWNED: " + spawnedBoss.name +
            " | Day: " + gameClock.GetDay() +
            " | Time: " + gameClock.GetHour() + ":00"
        );
    }

    private bool TryGetNextBossPrefab(
        out GameObject selectedBoss,
        out int selectedIndex)
    {
        selectedBoss = null;
        selectedIndex = -1;

        if (bossPrefabs == null || bossPrefabs.Length == 0)
            return false;

        for (int i = 0; i < bossPrefabs.Length; i++)
        {
            int index =
                (nextBossPrefabIndex + i) % bossPrefabs.Length;

            if (bossPrefabs[index] == null)
                continue;

            selectedBoss = bossPrefabs[index];
            selectedIndex = index;

            return true;
        }

        return false;
    }

    // LOADED CHUNK SPAWN POSITION

    private bool TryGetLoadedSpawnPosition(
        float spawnRadius,
        float protectionRadius,
        out Vector3 spawnPosition,
        out Vector2Int originChunk,
        bool checkHostileLightProtection = false)
    {
        spawnPosition = Vector3.zero;
        originChunk = Vector2Int.zero;

        if (worldGenerator == null ||
            worldGenerator.player == null ||
            activeChunks.Count == 0)
        {
            return false;
        }

        List<ChunkMobSpawner> loadedChunkList =
            new List<ChunkMobSpawner>(activeChunks);

        float currentChunkSize = worldGenerator.chunkSize;

        if (currentChunkSize <= 0f)
            return false;

        Vector2 playerPosition =
            worldGenerator.player.position;

        for (int i = 0; i < 50; i++)
        {
            ChunkMobSpawner chunk = loadedChunkList[
                Random.Range(0, loadedChunkList.Count)
            ];

            if (chunk == null)
                continue;

            float minX =
                chunk.chunkCoordinate.x * currentChunkSize;

            float minY =
                chunk.chunkCoordinate.y * currentChunkSize;

            Vector2 candidate = new Vector2(
                Random.Range(minX, minX + currentChunkSize),
                Random.Range(minY, minY + currentChunkSize)
            );

            float distance =
                Vector2.Distance(candidate, playerPosition);

            if (distance < protectionRadius)
                continue;

            if (checkHostileLightProtection &&
                WorldObjectLightController.Instance != null)
            {
                if (WorldObjectLightController.Instance
                    .IsPositionProtectedFromHostileSpawning(candidate))
                {
                    continue;
                }
            }

            spawnPosition = new Vector3(
                candidate.x,
                candidate.y,
                worldGenerator.player.position.z
            );

            originChunk = chunk.chunkCoordinate;

            return true;
        }

        return false;
    }

    private void CreateActiveMobsParent()
    {
        if (overWorld == null)
        {
            Debug.LogError(
                "MobSpawnerController: OverWorld reference is missing."
            );

            return;
        }

        Transform existingParent =
            overWorld.Find("ActiveMobs");

        if (existingParent != null)
        {
            activeMobsParent = existingParent;
            return;
        }

        GameObject activeMobsObject =
            new GameObject("ActiveMobs");

        activeMobsObject.transform.SetParent(
            overWorld,
            false
        );

        activeMobsParent = activeMobsObject.transform;
    }

    // CHUNK REGISTRATION

    public void RegisterChunk(ChunkMobSpawner chunk)
    {
        if (chunk == null)
            return;

        activeChunks.Add(chunk);
    }

    public void UnregisterChunk(ChunkMobSpawner chunk)
    {
        if (chunk == null)
            return;

        activeChunks.Remove(chunk);
    }

    // MOB REGISTRATION

    public bool RegisterMob(MobIdentity mob)
    {
        if (mob == null)
            return false;

        if (activeMobsParent == null)
            return false;

        StructureObjectIdentity structureIdentity =
            mob.GetComponentInChildren<StructureObjectIdentity>(true);

        // Bosses are managed separately.
        if (structureIdentity != null && structureIdentity.isBoss)
            return false;

        HashSet<MobIdentity> targetCollection =
            GetMobCollection(mob.mobType);

        if (targetCollection == null)
            return false;

        CleanNullMobs(targetCollection);

        if (targetCollection.Contains(mob))
            return true;

        if (!CanSpawnMob(mob.mobType))
            return false;

        mob.transform.SetParent(
            activeMobsParent,
            true
        );

        targetCollection.Add(mob);

        return true;
    }

    public void UnregisterMob(MobIdentity mob)
    {
        if (mob == null)
            return;

        hostileMobs.Remove(mob);
        passiveMobs.Remove(mob);
    }

    // GLOBAL MOB CREATION

    private void SpawnMob(
        GameObject prefab,
        MobType mobType,
        Vector3 position,
        Vector2Int originChunk)
    {
        if (prefab == null)
            return;

        GameObject spawnedMob = Instantiate(
            prefab,
            position,
            Quaternion.identity
        );

        MobIdentity identity =
            spawnedMob.GetComponent<MobIdentity>();

        if (identity == null)
        {
            identity = spawnedMob.AddComponent<MobIdentity>();
        }

        identity.mobType = mobType;
        identity.originalChunkCoordinate = originChunk;

        if (!RegisterMob(identity))
        {
            Destroy(spawnedMob);
        }
    }

    // HOSTILE PREFAB SELECTION

    public bool TryChooseHostilePrefab(out GameObject selectedPrefab)
    {
        selectedPrefab = null;

        if (gameClock == null || hostileSpawnData == null)
            return false;

        int currentDay = gameClock.GetDay();
        int currentHour = gameClock.GetHour();

        int totalWeight = 0;

        foreach (HostileSpawnData data in hostileSpawnData)
        {
            if (!CanEnemySpawn(data, currentDay, currentHour))
                continue;

            totalWeight += data.spawnPercentage;
        }

        if (totalWeight <= 0)
            return false;

        int randomValue = Random.Range(0, totalWeight);

        foreach (HostileSpawnData data in hostileSpawnData)
        {
            if (!CanEnemySpawn(data, currentDay, currentHour))
                continue;

            randomValue -= data.spawnPercentage;

            if (randomValue < 0)
            {
                selectedPrefab = data.enemyPrefab;
                return true;
            }
        }

        return false;
    }

    private bool CanEnemySpawn(
        HostileSpawnData data,
        int currentDay,
        int currentHour)
    {
        if (data == null || data.enemyPrefab == null)
            return false;

        if (data.spawnPercentage <= 0)
            return false;

        if (currentDay < data.spawnFromDay)
            return false;

        StructureObjectIdentity structureIdentity =
            data.enemyPrefab.GetComponentInChildren<StructureObjectIdentity>(true);

        if (structureIdentity != null && structureIdentity.isBoss)
            return false;

        return IsWithinSpawnTime(
            currentHour,
            data.spawnStartTime,
            data.spawnEndTime
        );
    }

    private bool IsWithinSpawnTime(
        int currentHour,
        int startTime,
        int endTime)
    {
        if (startTime == endTime)
            return true;

        if (startTime < endTime)
        {
            return currentHour >= startTime &&
                   currentHour < endTime;
        }

        return currentHour >= startTime ||
               currentHour < endTime;
    }

    // PASSIVE PREFAB SELECTION

    public bool TryChoosePassivePrefab(out GameObject selectedPrefab)
    {
        selectedPrefab = null;

        if (passiveSpawnData == null)
            return false;

        int totalWeight = 0;

        foreach (PassiveSpawnData data in passiveSpawnData)
        {
            if (!CanPassiveMobSpawn(data))
                continue;

            totalWeight += data.spawnPercentage;
        }

        if (totalWeight <= 0)
            return false;

        int randomValue = Random.Range(0, totalWeight);

        foreach (PassiveSpawnData data in passiveSpawnData)
        {
            if (!CanPassiveMobSpawn(data))
                continue;

            randomValue -= data.spawnPercentage;

            if (randomValue < 0)
            {
                selectedPrefab = data.mobPrefab;
                return true;
            }
        }

        return false;
    }

    private bool CanPassiveMobSpawn(PassiveSpawnData data)
    {
        if (data == null || data.mobPrefab == null)
            return false;

        if (data.spawnPercentage <= 0)
            return false;

        return true;
    }

    // POPULATION LIMITS

    public bool CanSpawnMob(MobType mobType)
    {
        if (mobType == MobType.Hostile)
        {
            CleanNullMobs(hostileMobs);

            return hostileMobs.Count < maxHostileMobs;
        }

        if (mobType == MobType.Passive)
        {
            CleanNullMobs(passiveMobs);

            return passiveMobs.Count < maxPassiveMobs;
        }

        return false;
    }

    public int GetMobCount(MobType mobType)
    {
        HashSet<MobIdentity> collection =
            GetMobCollection(mobType);

        if (collection == null)
            return 0;

        CleanNullMobs(collection);

        return collection.Count;
    }

    // CHUNK-BASED DESPAWNING

    private void Update()
    {
        // BOSS SPAWNING
        CheckBossSpawning();

        // CHUNK DESPAWNING
        despawnCheckTimer += Time.deltaTime;

        if (despawnCheckTimer < Mathf.Max(0.1f, despawnCheckInterval))
            return;

        despawnCheckTimer = 0f;

        CheckChunkDespawning(hostileMobs);
        CheckChunkDespawning(passiveMobs);
    }

    private void CheckChunkDespawning(
        HashSet<MobIdentity> collection)
    {
        CleanNullMobs(collection);

        activeChunks.RemoveWhere(chunk => chunk == null);

        List<MobIdentity> mobsToDespawn =
            new List<MobIdentity>();

        foreach (MobIdentity mob in collection)
        {
            if (mob == null)
                continue;

            StructureObjectIdentity identity =
                mob.GetComponentInChildren<StructureObjectIdentity>(true);

            // NEVER DESPAWN BOSSES.
            if (identity != null && identity.isBoss)
                continue;

            Vector2 mobPosition = mob.transform.position;

            bool insideLoadedChunk = false;

            foreach (ChunkMobSpawner chunk in activeChunks)
            {
                if (chunk == null)
                    continue;

                float currentChunkSize = worldGenerator.chunkSize;

                float minX = chunk.chunkCoordinate.x * currentChunkSize;
                float minY = chunk.chunkCoordinate.y * currentChunkSize;

                float maxX = minX + currentChunkSize;
                float maxY = minY + currentChunkSize;

                if (
                    mobPosition.x >= minX &&
                    mobPosition.x < maxX &&
                    mobPosition.y >= minY &&
                    mobPosition.y < maxY
                )
                {
                    insideLoadedChunk = true;
                    break;
                }
            }

            if (!insideLoadedChunk)
                mobsToDespawn.Add(mob);
        }

        foreach (MobIdentity mob in mobsToDespawn)
        {
            UnregisterMob(mob);

            Destroy(mob.gameObject);

            Debug.Log(
                "Mob despawned outside loaded chunks: " +
                mob.name
            );
        }
    }

    // INTERNAL HELPERS

    private HashSet<MobIdentity> GetMobCollection(MobType mobType)
    {
        if (mobType == MobType.Hostile)
            return hostileMobs;

        if (mobType == MobType.Passive)
            return passiveMobs;

        return null;
    }

    private void CleanNullMobs(HashSet<MobIdentity> collection)
    {
        collection.RemoveWhere(mob => mob == null);
    }
}
using UnityEngine;

[CreateAssetMenu(fileName = "WorldSpawnData", menuName = "World/World Spawn Data")]
public class WorldSpawnData : ScriptableObject
{
    [Header("Prefabs")]
    public GameObject[] prefabs;

    [Header("Generation")]
    public int minSpawnCount = 5;
    public int maxSpawnCount = 10;
    public float minSpacing = 2f;
    public bool ignoreSpacingValidation = false;
}
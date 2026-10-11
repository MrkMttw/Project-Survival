using UnityEngine;

[CreateAssetMenu(
    fileName = "StructureSpawnData",
    menuName = "World/Structure Spawn Data"
)]
public class StructureSpawnData : ScriptableObject
{
    [Header("Structure")]
    public GameObject prefab;

    [Header("Generation")]
    [Min(0)]
    public int maxWorldCount = 1;

    [Min(0f)]
    public float minPlayerDistance = 30f;

    [Min(0f)]
    public float clearRadius = 6f;
}
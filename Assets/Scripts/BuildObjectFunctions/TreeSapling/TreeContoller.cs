using UnityEngine;

public class TreeController : MonoBehaviour
{
    [Header("Tree Settings")]
    public GameObject[] treePrefabs;
    public Transform treeParent;

    [Header("Growth Time (Real Seconds)")]
    public float minimumGrowthTime = 60f;
    public float maximumGrowthTime = 180f;

    public GameObject GetRandomTreePrefab()
    {
        if (treePrefabs == null || treePrefabs.Length == 0)
        {
            return null;
        }

        // Select randomly from the assigned prefabs.
        return treePrefabs[Random.Range(0, treePrefabs.Length)];
    }
}
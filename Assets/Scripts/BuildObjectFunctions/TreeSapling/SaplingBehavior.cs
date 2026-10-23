using UnityEngine;

public class SaplingBehavior : MonoBehaviour
{
    [Header("References")]
    private WorldGenerator worldGenerator;
    private TreeController treeController;

    private BuildingObject building;
    private bool isGrowing;
    private bool isReplacing;

    private void Start()
    {
        building = GetComponentInParent<BuildingObject>();

        if (building == null)
        {
            building = GetComponentInChildren<BuildingObject>();
        }

        if (building == null)
        {
            Debug.LogError(
                "SaplingBehavior: BuildingObject is missing.",
                this
            );

            enabled = false;
            return;
        }

        if (worldGenerator == null)
        {
            worldGenerator =
                FindFirstObjectByType<WorldGenerator>();
        }

        if (treeController == null)
        {
            treeController =
                FindFirstObjectByType<TreeController>();
        }

        if (worldGenerator == null || treeController == null)
        {
            Debug.LogError(
                "SaplingBehavior: WorldGenerator or TreeController is missing.",
                this
            );

            enabled = false;
            return;
        }

        if (treeController.treePrefabs == null ||
            treeController.treePrefabs.Length == 0 ||
            treeController.GetRandomTreePrefab() == null)
        {
            Debug.LogError(
                "SaplingBehavior: No valid tree prefabs are assigned.",
                this
            );

            enabled = false;
            return;
        }

        if (!worldGenerator.TryGetBuildingSaveData(
            building,
            out BuildingSaveData data))
        {
            Debug.LogError(
                "SaplingBehavior: Building save data was not found.",
                this
            );

            enabled = false;
            return;
        }

        if (data.hasGrown)
        {
            // Already grown. Do not start another growth timer.
            enabled = false;
            return;
        }

        // Choose a tree type only if one hasn't been selected before.
        GameObject selectedTreePrefab = data.grownPrefab;

        if (selectedTreePrefab == null)
        {
            selectedTreePrefab =
                treeController.GetRandomTreePrefab();
        }

        worldGenerator.StartSaplingGrowth(
            building,
            treeController.minimumGrowthTime,
            treeController.maximumGrowthTime,
            selectedTreePrefab
        );

        isGrowing = true;
    }

    private void Update()
    {
        if (!isGrowing || isReplacing)
        {
            return;
        }

        if (building == null)
        {
            return;
        }

        if (!worldGenerator.TryGetBuildingSaveData(
            building,
            out BuildingSaveData data))
        {
            return;
        }

        if (data.hasGrown)
        {
            isGrowing = false;
            return;
        }

        if (data.growthDeadline <= 0f)
        {
            return;
        }

        if (Time.realtimeSinceStartup < data.growthDeadline)
        {
            return;
        }

        GrowIntoTree(data);
    }

    private void GrowIntoTree(BuildingSaveData data)
    {
        if (isReplacing || data.hasGrown)
        {
            return;
        }

        GameObject treePrefab = data.grownPrefab;

        if (treePrefab == null)
        {
            treePrefab = treeController.GetRandomTreePrefab();
        }

        if (treePrefab == null)
        {
            return;
        }

        isReplacing = true;
        isGrowing = false;

        Vector3 treePosition = transform.position;
        Quaternion treeRotation = transform.rotation;

        Vector2Int chunkCoordinate = data.chunkCoordinate;

        Transform chunkParent =
            worldGenerator.GetBuildingChunkParent(chunkCoordinate);

        // Save the grown state before destroying the sapling.
        worldGenerator.MarkSaplingGrown(
            building,
            treePrefab
        );

        GameObject grownTree = Instantiate(
            treePrefab,
            treePosition,
            treeRotation,
            chunkParent
        );

        BuildingObject treeBuilding =
            grownTree.GetComponentInChildren<BuildingObject>();

        if (treeBuilding == null)
        {
            treeBuilding =
                grownTree.AddComponent<BuildingObject>();
        }

        // Preserve the identity of the original planted sapling.
        treeBuilding.itemID = building.itemID;
        treeBuilding.buildingID = building.buildingID;

        Destroy(gameObject);
    }
}
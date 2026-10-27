using UnityEngine;

public class ItemsGeneratorController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public WorldGenerator worldGenerator;

    [Header("Supported Generator Buildings")]
    public GameObject[] generatorBuildingPrefabs;

    private void Awake()
    {
        if (worldGenerator == null)
        {
            worldGenerator = FindFirstObjectByType<WorldGenerator>();
        }
    }

    public bool TryInteract()
    {
        if (player == null || worldGenerator == null ||
            worldGenerator.itemDictionary == null)
        {
            return false;
        }

        ItemsGeneratorBehavior[] generators =
            FindObjectsByType<ItemsGeneratorBehavior>(
                FindObjectsSortMode.None);

        foreach (ItemsGeneratorBehavior generator in generators)
        {
            if (generator == null)
                continue;

            BuildingObject building =
                generator.GetComponent<BuildingObject>();

            if (building == null)
                continue;

            BoxCollider2D boxCollider =
                generator.GetComponent<BoxCollider2D>();

            if (boxCollider == null ||
                !boxCollider.OverlapPoint(player.position))
            {
                continue;
            }

            GameObject itemPrefab =
                worldGenerator.itemDictionary.GetItemPrefab(
                    building.itemID);

            if (itemPrefab == null)
                continue;

            Item item = itemPrefab.GetComponent<Item>();

            if (item == null || item.buildingPrefab == null)
                continue;

            bool supported = false;

            if (generatorBuildingPrefabs != null)
            {
                foreach (GameObject prefab in generatorBuildingPrefabs)
                {
                    if (prefab == item.buildingPrefab)
                    {
                        supported = true;
                        break;
                    }
                }
            }

            if (!supported)
                continue;

            generator.ToggleActive();
            return true;
        }

        return false;
    }
}
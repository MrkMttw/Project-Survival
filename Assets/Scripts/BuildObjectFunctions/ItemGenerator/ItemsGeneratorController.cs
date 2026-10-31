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

    private void Update()
    {
        UpdateGeneratorPrompt();
    }

    // FIND NEARBY GENERATOR
    private ItemsGeneratorBehavior FindNearbyGenerator()
    {
        if (player == null || worldGenerator == null ||
            worldGenerator.itemDictionary == null)
        {
            return null;
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

            if (generatorBuildingPrefabs == null)
                continue;

            foreach (GameObject prefab in generatorBuildingPrefabs)
            {
                if (prefab == item.buildingPrefab)
                {
                    return generator;
                }
            }
        }

        return null;
    }

    // UPDATE GENERATOR PROMPT
    private void UpdateGeneratorPrompt()
    {
        if (PromptController.Instance == null)
            return;

        if (FindNearbyGenerator() != null)
        {
            PromptController.Instance.RequestPrompt(
                PromptType.E, this);
        }
        else
        {
            PromptController.Instance.ReleasePrompt(
                PromptType.E, this);
        }
    }

    // INTERACTION
    public bool TryInteract()
    {
        ItemsGeneratorBehavior generator = FindNearbyGenerator();

        if (generator == null)
            return false;

        generator.ToggleActive();
        return true;
    }

    private void OnDisable()
    {
        if (PromptController.Instance != null)
        {
            PromptController.Instance.ReleasePrompt(
                PromptType.E, this);
        }
    }
}
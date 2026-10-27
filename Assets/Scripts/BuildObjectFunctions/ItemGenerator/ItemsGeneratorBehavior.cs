using UnityEngine;

public class ItemsGeneratorBehavior : MonoBehaviour
{
    [Header("Items")]
    public Item[] possibleItems;

    [Header("Quantity")]
    [Min(1)]
    public int minimumQuantity = 1;

    [Min(1)]
    public int maximumQuantity = 3;

    [Header("Generation Timer")]
    [Min(0.1f)]
    public float generationDelay = 60f;

    [Header("State")]
    public bool isActive;

    [Header("Vibration")]
    public float vibrationAmount = 0.03f;
    public float vibrationSpeed = 25f;

    private Vector3 originalLocalPosition;
    private BuildingObject building;
    private WorldGenerator worldGenerator;
    private float remainingTime;

    private void Start()
    {
        originalLocalPosition = transform.localPosition;

        building = GetComponent<BuildingObject>();
        worldGenerator = FindFirstObjectByType<WorldGenerator>();

        remainingTime = Mathf.Max(0.1f, generationDelay);

        if (building == null || worldGenerator == null)
            return;

        if (worldGenerator.TryGetBuildingSaveData(
            building, out BuildingSaveData data))
        {
            isActive = data.generatorIsActive;

            if (data.generatorRemainingTime > 0f)
            {
                remainingTime = data.generatorRemainingTime;
            }
        }
    }

    private void Update()
    {
        if (!isActive || building == null || worldGenerator == null)
            return;

        remainingTime -= Time.unscaledDeltaTime;

        if (remainingTime > 0f)
        {
            SaveGeneratorState();
            return;
        }

        GenerateItem();

        isActive = false;
        remainingTime = Mathf.Max(0.1f, generationDelay);

        SaveGeneratorState();
    }

    public void ToggleActive()
    {
        if (isActive)
            return;

        isActive = true;
        remainingTime = Mathf.Max(0.1f, generationDelay);

        SaveGeneratorState();
    }

    private void GenerateItem()
    {
        if (possibleItems == null || possibleItems.Length == 0)
            return;

        Item selectedItem = possibleItems[
            Random.Range(0, possibleItems.Length)];

        if (selectedItem == null)
            return;

        int minQuantity = Mathf.Max(1, minimumQuantity);
        int maxQuantity = Mathf.Max(minQuantity, maximumQuantity);

        int quantity = Random.Range(minQuantity, maxQuantity + 1);

        GameObject spawnedItem = selectedItem.CloneItem(quantity);

        if (spawnedItem != null)
        {
            spawnedItem.transform.position = transform.position;
        }
    }

    private void SaveGeneratorState()
    {
        if (worldGenerator == null || building == null)
            return;

        if (worldGenerator.TryGetBuildingSaveData(
            building, out BuildingSaveData data))
        {
            data.generatorIsActive = isActive;
            data.generatorRemainingTime = Mathf.Max(0f, remainingTime);
        }
    }

    private void LateUpdate()
    {
        if (isActive)
        {
            float offsetX = Random.Range(-1f, 1f) * vibrationAmount;
            float offsetY = Random.Range(-1f, 1f) * vibrationAmount;

            transform.localPosition = originalLocalPosition
                + new Vector3(offsetX, offsetY, 0f);
        }
        else
        {
            transform.localPosition = originalLocalPosition;
        }
    }
}
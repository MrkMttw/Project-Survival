using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlacementController : MonoBehaviour
{
    private GameObject ghostObject;

    [Header("Grid")]
    public bool useGrid = true;
    public float gridSize = 1f;

    [Header("Grid Prompt")]
    public TMP_Text gridPrompt;

    [Header("Ghost")]
    [Range(0f, 1f)]
    public float ghostAlpha = 0.5f;

    [Header("Placement")]
    public LayerMask blockingLayers;

    [Header("Building Parent")]
    public Transform buildingParent;

    [Header("Hotbar")]
    public HotbarController hotbarController;

    public event Action<bool> RelocationFinished;

    public Color validColor = Color.green;
    public Color invalidColor = Color.red;

    private Item currentItem;
    private GameObject currentBuildingPrefab;
    private int currentDirectionalPresetIndex = -1;

    private BuildingObject relocatingBuilding;
    private bool isRelocating;
    private bool canPlace;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        UpdateRotationPrompt();

        if (ghostObject == null)
            return;

        if (Keyboard.current == null)
            return;

        // ROTATE DIRECTIONAL BUILDING
        if (Keyboard.current.rKey.wasPressedThisFrame &&
            currentItem != null &&
            currentItem.useDirectionalPrefabs)
        {
            CycleDirectionalPreset();
        }

        // TOGGLE GRID WITH TAB
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            useGrid = !useGrid;
            UpdateGridPrompt();
        }

        FollowMouse();
        CheckPlacement();

        if (Mouse.current.leftButton.wasPressedThisFrame &&
            canPlace)
        {
            if (isRelocating)
                PlaceRelocatedBuilding();
            else
                PlaceBuilding();
        }
    }
        
    private BuildingPreset GetCurrentPreset()
    {
        if (currentItem == null ||
            !currentItem.useDirectionalPrefabs)
            return null;

        if (currentDirectionalPresetIndex < 0 ||
            currentDirectionalPresetIndex >=
            currentItem.buildingPresets.Count)
            return null;

        return currentItem.buildingPresets[
            currentDirectionalPresetIndex
        ];
    }

    private void CycleDirectionalPreset()
    {
        if (currentItem.buildingPresets == null ||
            currentItem.buildingPresets.Count == 0)
            return;

        currentDirectionalPresetIndex++;

        if (currentDirectionalPresetIndex >=
            currentItem.buildingPresets.Count)
        {
            currentDirectionalPresetIndex = 0;
        }

        Vector3 position =
            ghostObject.transform.position;

        Quaternion rotation =
            ghostObject.transform.rotation;

        Destroy(ghostObject);

        ApplyDirectionalPreset(position, rotation);
    }

    // UPDATE ROTATION PROMPT
    private void UpdateRotationPrompt()
    {
        if (PromptController.Instance == null)
            return;

        if (ghostObject != null &&
            currentItem != null &&
            currentItem.useDirectionalPrefabs)
        {
            PromptController.Instance.RequestPrompt(PromptType.R, this);
        }
        else
        {
            PromptController.Instance.ReleasePrompt(PromptType.R, this);
        }
    }

    private void ApplyDirectionalPreset(
        Vector3 position,
        Quaternion rotation)
    {
        BuildingPreset preset = GetCurrentPreset();

        if (preset == null ||
            preset.ghostPrefab == null ||
            preset.buildingPrefab == null)
        {
            Debug.LogError(
                "Invalid directional preset for: " +
                currentItem.name
            );

            return;
        }

        currentBuildingPrefab =
            preset.buildingPrefab;

        ghostObject = Instantiate(
            preset.ghostPrefab,
            position,
            rotation
        );

        ghostObject.name =
            currentItem.name +
            "_" +
            preset.direction +
            "_Ghost";

        SetGhostCollision(false);
        SetGhostTransparency();
    }

    public void StartPlacement(Item item)
    {
        if (item == null)
            return;

        if (!item.isBuildable)
        {
            CancelPlacement();
            return;
        }

        if (ghostObject != null)
            Destroy(ghostObject);

        currentItem = item;
        currentDirectionalPresetIndex = -1;

        if (item.useDirectionalPrefabs)
        {
            if (item.buildingPresets == null ||
                item.buildingPresets.Count == 0)
            {
                Debug.LogError(
                    "No directional presets configured for: " +
                    item.name
                );

                CancelPlacement();
                return;
            }

            currentDirectionalPresetIndex = 0;

            ApplyDirectionalPreset(
                Vector3.zero,
                Quaternion.identity
            );
        }
        else
        {
            if (item.ghostPrefab == null ||
                item.buildingPrefab == null)
            {
                Debug.LogWarning(
                    "Building or ghost prefab missing for: " +
                    item.name
                );

                CancelPlacement();
                return;
            }

            currentBuildingPrefab =
                item.buildingPrefab;

            ghostObject = Instantiate(
                item.ghostPrefab
            );

            ghostObject.name =
                item.name + "_Ghost";

            SetGhostCollision(false);
            SetGhostTransparency();
        }

        if (ghostObject == null)
        {
            CancelPlacement();
            return;
        }

        ShowGridPrompt();
    }

    public bool StartRelocation(BuildingObject building)
    {
        if (building == null || isRelocating)
            return false;

        CancelPlacement();

        ItemDictionary itemDictionary =
            FindObjectOfType<ItemDictionary>();

        if (itemDictionary == null)
        {
            Debug.LogError("ItemDictionary not found.");
            return false;
        }

        GameObject itemPrefab =
            itemDictionary.GetItemPrefab(building.itemID);

        if (itemPrefab == null)
        {
            Debug.LogError(
                "No item prefab found for Item ID: " +
                building.itemID
            );

            return false;
        }

        Item item = itemPrefab.GetComponent<Item>();

        if (item == null)
            return false;

        currentItem = item;
        currentDirectionalPresetIndex =
            building.directionalPresetIndex;

        isRelocating = true;
        relocatingBuilding = building;

        Vector3 position = building.transform.position;
        Quaternion rotation = building.transform.rotation;

        if (item.useDirectionalPrefabs)
        {
            if (item.buildingPresets == null ||
                item.buildingPresets.Count == 0)
            {
                CancelPlacement();
                Debug.LogError("No directional presets configured for: " + item.name);
                return false;
            }

            if (currentDirectionalPresetIndex < 0 ||
                currentDirectionalPresetIndex >= item.buildingPresets.Count)
            {
                currentDirectionalPresetIndex = 0;
            }

            ApplyDirectionalPreset(position, rotation);
        }
        else
        {
            if (item.ghostPrefab == null)
            {
                CancelPlacement();
                return false;
            }

            currentBuildingPrefab =
                item.buildingPrefab;

            ghostObject = Instantiate(
                item.ghostPrefab,
                position,
                rotation
            );

            SetGhostCollision(false);
            SetGhostTransparency();
        }

        if (ghostObject == null)
        {
            CancelPlacement();
            return false;
        }

        ghostObject.name =
            building.gameObject.name + "_Ghost";

        ShowGridPrompt();
        return true;
    }

    public void CancelPlacement()
    {
        if (isRelocating)
        {
            RelocationFinished?.Invoke(false);
        }

        if (ghostObject != null)
        {
            Destroy(ghostObject);
        }

        HideGridPrompt();

        ghostObject = null;
        currentBuildingPrefab = null;
        currentItem = null;
        currentDirectionalPresetIndex = -1;
        relocatingBuilding = null;
        isRelocating = false;
        canPlace = false;
    }

    private void FollowMouse()
    {
        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(
                Mouse.current.position.ReadValue()
            );

        mouseWorldPosition.z = 0f;

        if (useGrid)
        {
            mouseWorldPosition.x =
                Mathf.Round(
                    mouseWorldPosition.x / gridSize
                ) * gridSize;

            mouseWorldPosition.y =
                Mathf.Round(
                    mouseWorldPosition.y / gridSize
                ) * gridSize;
        }

        ghostObject.transform.position =
            mouseWorldPosition;
    }

    private void SetGhostCollision(bool enabled)
    {
        CapsuleCollider2D[] colliders =
            ghostObject.GetComponentsInChildren<CapsuleCollider2D>();

        foreach (CapsuleCollider2D collider in colliders)
        {
            collider.enabled = true;
            collider.isTrigger = !enabled;
        }
    }

    private void SetGhostTransparency()
    {
        SpriteRenderer[] renderers =
            ghostObject
            .GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer renderer in renderers)
        {
            Color color = renderer.color;

            color.a = ghostAlpha;

            renderer.color = color;
        }
    }

    private void CheckPlacement()
    {
        CapsuleCollider2D ghostCollider =
            ghostObject.GetComponent<CapsuleCollider2D>();

        if (ghostCollider == null)
        {
            canPlace = true;

            SetGhostColor(validColor);

            return;
        }

        ContactFilter2D contactFilter =
            new ContactFilter2D();

        contactFilter.SetLayerMask(blockingLayers);
        contactFilter.useTriggers = false;

        Collider2D[] results =
            new Collider2D[20];

        int hitCount =
            ghostCollider.Overlap(
                contactFilter,
                results
            );

        bool blocked = false;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = results[i];

            if (hit == null)
                continue;

            if (hit.transform.root == ghostObject.transform.root)
                continue;

            if (hit is not CapsuleCollider2D)
                continue;

            blocked = true;
            break;
        }

        canPlace = !blocked;

        SetGhostColor(
            canPlace
                ? validColor
                : invalidColor
        );
    }

    private void PlaceBuilding()
    {
        WorldGenerator worldGenerator =
            FindObjectOfType<WorldGenerator>();

        Transform buildingChunkParent =
            buildingParent;

        if (worldGenerator != null)
        {
            Vector2Int chunkCoordinate =
                worldGenerator.GetChunkCoordinate(
                    ghostObject.transform.position
                );

            buildingChunkParent =
                worldGenerator.GetBuildingChunkParent(
                    chunkCoordinate
                );
        }

        GameObject placedBuilding = Instantiate(
            currentBuildingPrefab,
            ghostObject.transform.position,
            ghostObject.transform.rotation,
            buildingChunkParent
        );

        BuildingObject buildingObject =
            placedBuilding
            .GetComponentInChildren<BuildingObject>();

        if (buildingObject == null)
        {
            Debug.LogError(
                "BuildingObject not found on: " +
                placedBuilding.name
            );

            Destroy(placedBuilding);

            return;
        }

        buildingObject.itemID =
            currentItem.ID;
        
        buildingObject.buildingID =
            Guid.NewGuid().ToString();

        buildingObject.directionalPresetIndex =
            currentDirectionalPresetIndex;

        if (worldGenerator != null)
        {
            worldGenerator.RegisterBuilding(buildingObject);

            FindFirstObjectByType<RadarController>()
                ?.RefreshCoordinateUI();
        }

        TentController tentController =
            FindObjectOfType<TentController>();

        if (tentController != null)
        {
            tentController.DebugTentCount();
        }

        SpriteRenderer[] renderers =
            placedBuilding
            .GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.color = Color.white;
        }

        // Consume the building item
        if (hotbarController != null)
        {
            hotbarController.ConsumeSelectedItem(1);
        }
        else
        {
            Debug.LogWarning(
                "PlacementController: " +
                "HotbarController is not assigned."
            );

            currentItem.RemoveFromStack(1);
        }

        HideGridPrompt();

        Debug.Log(
            "Building placed: " +
            placedBuilding.name +
            " | Item ID: " +
            buildingObject.itemID
        );

        Destroy(ghostObject);

        ghostObject = null;
        currentBuildingPrefab = null;
        currentDirectionalPresetIndex = -1;
        canPlace = false;

        // RESTART PLACEMENT IF ITEMS REMAIN
        if (currentItem != null && currentItem.quantity > 0)
        {
            StartPlacement(currentItem);
        }
        else
        {
            currentItem = null;
        }
    }
        
    private void PlaceRelocatedBuilding()
    {
        GameObject targetBuilding =
            relocatingBuilding.gameObject;

        bool changePrefab =
            currentItem.useDirectionalPrefabs &&
            currentDirectionalPresetIndex !=
            relocatingBuilding.directionalPresetIndex;

        if (changePrefab)
        {
            GameObject replacement = Instantiate(
                currentBuildingPrefab,
                ghostObject.transform.position,
                ghostObject.transform.rotation,
                relocatingBuilding.transform.parent
            );

            BuildingObject replacementData =
                replacement.GetComponentInChildren<BuildingObject>();

            if (replacementData == null)
            {
                replacementData =
                    replacement.AddComponent<BuildingObject>();
            }

            replacementData.itemID =
                relocatingBuilding.itemID;

            replacementData.buildingID =
                relocatingBuilding.buildingID;

            replacementData.directionalPresetIndex =
                currentDirectionalPresetIndex;

            targetBuilding = replacement;

            Destroy(relocatingBuilding.gameObject);
        }
        else
        {
            targetBuilding.transform.position =
                ghostObject.transform.position;

            targetBuilding.transform.rotation =
                ghostObject.transform.rotation;

            relocatingBuilding.directionalPresetIndex =
                currentDirectionalPresetIndex;
        }

        SpriteRenderer[] renderers =
            targetBuilding.GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.color = Color.white;
        }

        WorldGenerator worldGenerator =
            FindObjectOfType<WorldGenerator>();

        BuildingObject buildingObject =
            targetBuilding.GetComponentInChildren<BuildingObject>();

        if (worldGenerator != null && buildingObject != null)
        {
            Vector2Int chunkCoordinate =
                worldGenerator.GetChunkCoordinate(
                    targetBuilding.transform.position
                );

            Transform buildingChunkParent =
                worldGenerator.GetBuildingChunkParent(
                    chunkCoordinate
                );

            targetBuilding.transform.SetParent(
                buildingChunkParent,
                true
            );

            worldGenerator.UpdateBuilding(buildingObject);

            FindFirstObjectByType<RadarController>()
                ?.RefreshCoordinateUI();
        }

        Destroy(ghostObject);

        HideGridPrompt();

        Debug.Log(
            "Building relocated: " +
            targetBuilding.name
        );

        RelocationFinished?.Invoke(true);

        ghostObject = null;
        currentBuildingPrefab = null;
        currentItem = null;
        currentDirectionalPresetIndex = -1;
        relocatingBuilding = null;
        isRelocating = false;
        canPlace = false;
    }

    private void SetGhostColor(Color color)
    {
        SpriteRenderer[] renderers =
            ghostObject
            .GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer renderer in renderers)
        {
            Color newColor = color;

            newColor.a = ghostAlpha;

            renderer.color = newColor;
        }
    }

    private void ShowGridPrompt()
    {
        if (gridPrompt == null)
            return;

        gridPrompt.gameObject.SetActive(true);

        UpdateGridPrompt();
    }

    private void HideGridPrompt()
    {
        if (gridPrompt == null)
            return;

        gridPrompt.gameObject.SetActive(false);
    }

    private void UpdateGridPrompt()
    {
        if (gridPrompt == null)
            return;

        gridPrompt.text =
            "Press Tab to Toggle Grid  " +
            "Grid: " +
            (useGrid ? "ON" : "OFF");
    }

    private void OnDisable()
    {
        if (PromptController.Instance != null)
            PromptController.Instance.ReleasePrompt(PromptType.R, this);
    }
}
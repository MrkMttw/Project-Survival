using UnityEngine;
using UnityEngine.InputSystem;

public class PlacementController : MonoBehaviour
{
    private GameObject ghostObject;

    [Header("Grid")]
    public bool useGrid = true;
    public float gridSize = 1f;

    [Header("Ghost")]
    [Range(0f, 1f)]
    public float ghostAlpha = 0.5f;

    [Header("Placement")]
    public LayerMask blockingLayers;

    [Header("Building Parent")]
    public Transform buildingParent;

    [Header("Hotbar")]
    public HotbarController hotbarController;

    public Color validColor = Color.green;
    public Color invalidColor = Color.red;

    private Item currentItem;
    private GameObject currentBuildingPrefab;

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
        if (ghostObject == null)
            return;

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

    public void StartPlacement(Item item)
    {
        if (item == null)
            return;

        if (!item.isBuildable)
        {
            CancelPlacement();
            return;
        }

        if (item.buildingPrefab == null)
        {
            Debug.LogWarning(
                "Cannot place " +
                item.name +
                ": buildingPrefab is missing."
            );

            return;
        }

        if (ghostObject != null)
            Destroy(ghostObject);

        currentItem = item;
        currentBuildingPrefab = item.buildingPrefab;

        ghostObject =
            Instantiate(currentBuildingPrefab);

        ghostObject.name =
            item.name + "_Ghost";

        SetGhostTransparency();

        Debug.Log(
            "Ghost created: " +
            ghostObject.name
        );
    }

    public void StartRelocation(BuildingObject building)
    {
        if (building == null)
            return;

        CancelPlacement();

        relocatingBuilding = building;
        isRelocating = true;

        ghostObject = building.gameObject;

        SetGhostTransparency();

        Debug.Log(
            "Relocation started: " +
            building.gameObject.name
        );
    }

    public void CancelPlacement()
    {
        if (ghostObject != null &&
            !isRelocating)
        {
            Destroy(ghostObject);
        }

        ghostObject = null;
        currentBuildingPrefab = null;
        currentItem = null;
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
        Collider2D ghostCollider =
            ghostObject.GetComponent<Collider2D>();

        if (ghostCollider == null)
        {
            canPlace = true;

            SetGhostColor(validColor);

            return;
        }

        Collider2D[] hits =
            Physics2D.OverlapBoxAll(
                ghostCollider.bounds.center,
                ghostCollider.bounds.size,
                0f,
                blockingLayers
            );

        bool blocked = false;

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == ghostObject)
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
        GameObject placedBuilding =
            Instantiate(
                currentBuildingPrefab,
                ghostObject.transform.position,
                ghostObject.transform.rotation,
                buildingParent
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

        Debug.Log(
            "Building placed: " +
            placedBuilding.name +
            " | Item ID: " +
            buildingObject.itemID
        );

        Destroy(ghostObject);

        ghostObject = null;
        currentBuildingPrefab = null;
        currentItem = null;
        canPlace = false;
    }

    private void PlaceRelocatedBuilding()
    {
        SpriteRenderer[] renderers =
            relocatingBuilding
            .GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.color = Color.white;
        }

        Debug.Log(
            "Building relocated: " +
            relocatingBuilding.gameObject.name
        );

        ghostObject = null;
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
}
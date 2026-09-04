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

    private GameObject currentBuildingPrefab;
    public Color validColor = Color.green;
    public Color invalidColor = Color.red;

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

        if (Mouse.current.leftButton.wasPressedThisFrame && canPlace)
        {
            PlaceBuilding();
        }
    }

    public void StartPlacement(Item item)
    {
        Debug.Log("START PLACEMENT: " + item.name);

        if (item == null)
            return;

        Debug.Log("START PLACEMENT: " + item.name);
        
        if (!item.isBuildable)
        {
            Debug.Log("ITEM IS NOT BUILDABLE");
            return;
        }

        if (item.buildingPrefab == null)
        {
            Debug.Log("BUILDING PREFAB IS NULL");
            return;
        }

        if (ghostObject != null)
            Destroy(ghostObject);

        currentBuildingPrefab = item.buildingPrefab;

        ghostObject = Instantiate(currentBuildingPrefab);

        ghostObject.name = item.name + "_Ghost";

        SetGhostTransparency();

        Debug.Log("GHOST CREATED: " + ghostObject.name);
    }

    private void FollowMouse()
    {
        Vector3 mouseScreenPosition =
            Mouse.current.position.ReadValue();

        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        mouseWorldPosition.z = 0f;

        if (useGrid)
        {
            mouseWorldPosition.x =
                Mathf.Round(mouseWorldPosition.x / gridSize) * gridSize;

            mouseWorldPosition.y =
                Mathf.Round(mouseWorldPosition.y / gridSize) * gridSize;
        }

        ghostObject.transform.position =
            mouseWorldPosition;
    }
    
    private void SetGhostTransparency()
    {
        SpriteRenderer[] renderers =
            ghostObject.GetComponentsInChildren<SpriteRenderer>();

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

        Collider2D[] hits = Physics2D.OverlapBoxAll(
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

        if (canPlace)
            SetGhostColor(validColor);
        else
            SetGhostColor(invalidColor);
    }

    private void PlaceBuilding()
    {
        GameObject placedBuilding = Instantiate(
            currentBuildingPrefab,
            ghostObject.transform.position,
            ghostObject.transform.rotation
        );

        SpriteRenderer[] renderers =
            placedBuilding.GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.color = Color.white;
        }

        Destroy(ghostObject);

        ghostObject = null;
        currentBuildingPrefab = null;
    }

    private void SetGhostColor(Color color)
    {
        SpriteRenderer[] renderers =
            ghostObject.GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer renderer in renderers)
        {
            Color newColor = color;
            newColor.a = ghostAlpha;

            renderer.color = newColor;
        }
    }
}
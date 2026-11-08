using UnityEngine;

public class ContainerController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject containerPanel;

    [Header("Container Slots")]
    public Transform slotParent;
    public GameObject slotPrefab;

    [Header("Hotbar Resize")]
    public RectTransform hotbarRect;
    public Vector3 containerOpenHotbarScale =
        new Vector3(0.7f, 0.7f, 1f);

    public bool IsOpen => isOpen;

    private Vector3 originalHotbarScale;
    private Vector2 originalHotbarPosition;

    private ContainerBehavior currentContainer;
    private Slot[] currentSlots;

    private bool isOpen;

    private void Start()
    {
        if (containerPanel != null)
        {
            containerPanel.SetActive(false);
        }

        if (slotParent == null && containerPanel != null)
        {
            slotParent = containerPanel.transform;
        }

        if (hotbarRect != null)
        {
            originalHotbarScale = hotbarRect.localScale;
            originalHotbarPosition = hotbarRect.anchoredPosition;
        }
    }

    private void Update()
    {
        UpdateContainerPrompt();
    }

    // UPDATE CONTAINER PROMPT
    private void UpdateContainerPrompt()
    {
        if (PromptController.Instance == null)
            return;

        if (isOpen || player == null)
        {
            PromptController.Instance.ReleasePrompt(
                PromptType.E, this);

            return;
        }

        ContainerBehavior nearbyContainer =
            FindNearbyContainer();

        if (nearbyContainer != null)
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

    // FIND CONTAINER USING HITBOX
    private ContainerBehavior FindNearbyContainer()
    {
        if (player == null)
            return null;

        ContainerBehavior[] containers =
            FindObjectsByType<ContainerBehavior>(
                FindObjectsSortMode.None);

        foreach (ContainerBehavior container in containers)
        {
            if (container == null)
                continue;

            if (container.IsPlayerInRange(player.position))
            {
                return container;
            }
        }

        return null;
    }

    // TRY INTERACT
    public bool TryInteract()
    {
        if (isOpen)
        {
            CloseContainer();
            return true;
        }

        if (player == null ||
            containerPanel == null ||
            slotPrefab == null ||
            slotParent == null)
        {
            return false;
        }

        ContainerBehavior container =
            FindNearbyContainer();

        if (container == null)
            return false;

        if (!GenerateSlots(container.Capacity))
            return false;

        currentContainer = container;
        isOpen = true;

        if (PromptController.Instance != null)
        {
            PromptController.Instance.ReleasePrompt(
                PromptType.E, this);
        }

        // RESIZE AND CENTER HOTBAR
        if (hotbarRect != null)
        {
            hotbarRect.localScale =
                containerOpenHotbarScale;

            hotbarRect.anchoredPosition =
                new Vector2(0f, originalHotbarPosition.y);
        }

        currentContainer.LoadSlots(currentSlots);

        containerPanel.SetActive(true);

        return true;
    }

    // GENERATE CONTAINER SLOTS
    private bool GenerateSlots(int capacity)
    {
        if (capacity <= 0)
        {
            Debug.LogError(
                "ContainerController: Invalid container capacity.");

            return false;
        }

        for (int i = slotParent.childCount - 1; i >= 0; i--)
        {
            Destroy(slotParent.GetChild(i).gameObject);
        }

        currentSlots = new Slot[capacity];

        for (int i = 0; i < capacity; i++)
        {
            GameObject slotObject = Instantiate(
                slotPrefab,
                slotParent);

            Slot slot = slotObject.GetComponentInChildren<Slot>(
                true);

            if (slot == null)
            {
                Debug.LogError(
                    "ContainerController: Slot prefab is missing " +
                    "the Slot component.");

                Destroy(slotObject);
                currentSlots = null;

                return false;
            }

            currentSlots[i] = slot;
        }

        return true;
    }

    // CLOSE CONTAINER
    private void CloseContainer()
    {
        if (currentContainer != null &&
            currentSlots != null)
        {
            currentContainer.SyncSlots(currentSlots);
        }

        isOpen = false;
        currentContainer = null;
        currentSlots = null;

        // RESTORE HOTBAR
        if (hotbarRect != null)
        {
            hotbarRect.localScale = originalHotbarScale;
            hotbarRect.anchoredPosition = originalHotbarPosition;
        }

        if (containerPanel != null)
        {
            containerPanel.SetActive(false);
        }
    }

    // REFRESH CONTAINER SLOTS
    public void RefreshSlots(ContainerBehavior container)
    {
        if (!isOpen ||
            currentContainer == null ||
            currentContainer != container)
        {
            return;
        }

        currentContainer.LoadSlots(currentSlots);
    }

    // NOTIFY SLOT CHANGES
    public void NotifySlotsChanged()
    {
        if (!isOpen ||
            currentContainer == null ||
            currentSlots == null)
        {
            return;
        }

        currentContainer.SyncSlots(currentSlots);
    }

    // CLOSE FOR OTHER MENUS
    public void CloseForMenu()
    {
        if (isOpen)
        {
            CloseContainer();
        }
    }

    // CLEANUP
    private void OnDisable()
    {
        if (isOpen)
        {
            CloseContainer();
        }

        if (PromptController.Instance != null)
        {
            PromptController.Instance.ReleasePrompt(
                PromptType.E, this);
        }
    }
}
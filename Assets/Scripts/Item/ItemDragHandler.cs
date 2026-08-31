using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerClickHandler
{
    private Transform originalParent;
    private CanvasGroup canvasGroup;
    private InventoryController inventoryController;

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        inventoryController = InventoryController.instance;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;

        transform.SetParent(transform.root);

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        Slot dropSlot = eventData.pointerEnter?.GetComponent<Slot>();

        if (dropSlot == null)
        {
            GameObject dropItem = eventData.pointerEnter;

            if (dropItem != null)
            {
                dropSlot = dropItem.GetComponentInParent<Slot>();
            }
        }

        Slot originalSlot = originalParent.GetComponent<Slot>();

        if (originalSlot == null)
        {
            ResetToOriginalPosition();
            return;
        }

        // ==========================================
        // DROPPED BACK ON ORIGINAL SLOT
        // ==========================================

        if (dropSlot == originalSlot)
        {
            ResetToOriginalPosition();
            return;
        }

        // ==========================================
        // DROPPED ON ANOTHER SLOT
        // ==========================================

        if (dropSlot != null)
        {
            HandleSlotDrop(dropSlot, originalSlot);
            return;
        }

        // ==========================================
        // DROPPED OUTSIDE INVENTORY
        // ==========================================

        if (!IsWithinInventory(eventData.position))
        {
            DropItem(originalSlot);
        }
        else
        {
            ResetToOriginalPosition();
        }
    }

    private void HandleSlotDrop(Slot dropSlot, Slot originalSlot)
    {
        // ==========================================
        // EMPTY SLOT
        // ==========================================

        if (dropSlot.currentItem == null)
        {
            originalSlot.currentItem = null;

            transform.SetParent(dropSlot.transform);

            dropSlot.currentItem = gameObject;

            ResetToSlotPosition();

            return;
        }

        // ==========================================
        // SLOT HAS AN ITEM
        // ==========================================

        Item draggedItem = GetComponent<Item>();
        Item targetItem = dropSlot.currentItem.GetComponent<Item>();

        // ==========================================
        // TRY STACKING
        // ==========================================

        if (CanStack(draggedItem, targetItem))
        {
            int amountToMove = Mathf.Min(
                draggedItem.quantity,
                targetItem.GetRemainingStackSpace()
            );

            // Target stack is already full
            if (amountToMove <= 0)
            {
                ResetToOriginalPosition();
                return;
            }

            // Add only what can fit
            int amountAdded = targetItem.AddToStack(amountToMove);

            draggedItem.RemoveFromStack(amountAdded);

            // ==========================================
            // ENTIRE DRAGGED STACK WAS MOVED
            // ==========================================

            if (draggedItem.quantity <= 0)
            {
                originalSlot.currentItem = null;

                Destroy(gameObject);
            }
            else
            {
                // ==========================================
                // ONLY PART OF THE STACK WAS MOVED
                // ==========================================

                transform.SetParent(originalSlot.transform);

                ResetToSlotPosition();
            }

            return;
        }

        // ==========================================
        // NOT STACKABLE → SWAP
        // ==========================================

        GameObject targetObject = dropSlot.currentItem;

        targetObject.transform.SetParent(
            originalSlot.transform
        );

        originalSlot.currentItem = targetObject;

        targetObject
            .GetComponent<RectTransform>()
            .anchoredPosition = Vector2.zero;

        transform.SetParent(dropSlot.transform);

        dropSlot.currentItem = gameObject;

        ResetToSlotPosition();
    }

    private bool CanStack(Item draggedItem, Item targetItem)
    {
        if (draggedItem == null || targetItem == null)
            return false;

        return
            draggedItem.ID == targetItem.ID &&
            draggedItem.stackable &&
            targetItem.stackable;
    }

    // ==========================================
    // RIGHT CLICK → SPLIT STACK
    // ==========================================

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            SplitStack();
        }
    }

    private void SplitStack()
    {
        Item item = GetComponent<Item>();

        // Non-stackable items cannot split
        if (item == null ||
            !item.stackable ||
            item.quantity <= 1)
        {
            return;
        }

        int splitAmount = item.quantity / 2;

        // Find empty slot BEFORE removing quantity
        Slot emptySlot = FindEmptyInventorySlot();

        if (emptySlot == null)
        {
            Debug.Log("No empty inventory slot available for split.");
            return;
        }

        // Remove half from original
        item.RemoveFromStack(splitAmount);

        // Create new stack
        GameObject newItem = item.CloneItem(splitAmount);

        if (newItem == null)
        {
            // Restore original if clone failed
            item.AddToStack(splitAmount);
            return;
        }

        newItem.transform.SetParent(emptySlot.transform);

        newItem.GetComponent<RectTransform>()
            .anchoredPosition = Vector2.zero;

        emptySlot.currentItem = newItem;
    }

    private Slot FindEmptyInventorySlot()
    {
        if (inventoryController == null)
            return null;

        foreach (Transform slotTransform
                 in inventoryController.inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot != null &&
                slot.currentItem == null)
            {
                return slot;
            }
        }

        return null;
    }

    // CHECK IF MOUSE IS INSIDE INVENTORY

    private bool IsWithinInventory(Vector2 mousePosition)
    {
        if (originalParent == null)
            return false;

        Transform inventoryParent = originalParent.parent;

        if (inventoryParent == null)
            return false;

        RectTransform inventoryRect =
            inventoryParent.GetComponent<RectTransform>();

        if (inventoryRect == null)
            return false;

        return RectTransformUtility.RectangleContainsScreenPoint(
            inventoryRect,
            mousePosition
        );
    }

    // DROP ITEM INTO WORLD

    private void DropItem(Slot originalSlot)
    {
        Item item = GetComponent<Item>();

        if (item == null)
            return;

        // DROP ONE ITEM FROM A STACK

        if (item.quantity > 1)
        {
            item.RemoveFromStack(1);

            CreateWorldItem(1);

            transform.SetParent(originalSlot.transform);

            ResetToSlotPosition();

            return;
        }

        // DROP THE ENTIRE ITEM

        originalSlot.currentItem = null;

        CreateWorldItem(1);

        Destroy(gameObject);
    }

    private void CreateWorldItem(int amount)
    {
        Transform playerTransform =
            GameObject.FindGameObjectWithTag("Player")?.transform;

        if (playerTransform == null)
        {
            Debug.LogError("Missing Player tag.");
            return;
        }

        Vector2 dropPosition =
            (Vector2)playerTransform.position;

        GameObject dropItem = Instantiate(
            gameObject,
            dropPosition,
            Quaternion.identity
        );

        Item droppedItem =
            dropItem.GetComponent<Item>();

        if (droppedItem != null)
        {
            droppedItem.quantity = amount;
            droppedItem.UpdateQuantityDisplay();
        }
    }

    // RESET POSITION

    private void ResetToOriginalPosition()
    {
        transform.SetParent(originalParent);

        ResetToSlotPosition();
    }

    private void ResetToSlotPosition()
    {
        RectTransform rectTransform =
            GetComponent<RectTransform>();

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}
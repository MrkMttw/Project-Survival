
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

        Slot dropSlot = null;

        if (eventData.pointerEnter != null)
        {
            dropSlot =
                eventData.pointerEnter.GetComponentInParent<Slot>();
        }

        Slot originalSlot = originalParent.GetComponent<Slot>();

        if (originalSlot == null)
        {
            ResetToOriginalPosition();
            return;
        }

        // Dropped back onto the original slot.
        if (dropSlot == originalSlot)
        {
            ResetToOriginalPosition();
            return;
        }

        // Dropped onto another slot.
        if (dropSlot != null)
        {
            HandleSlotDrop(dropSlot, originalSlot);
            return;
        }

        // Dropped outside the inventory.
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
        Item draggedItem = GetComponent<Item>();

        if (draggedItem == null)
        {
            ResetToOriginalPosition();
            return;
        }

        FurnaceSlot sourceFurnaceSlot =
            originalSlot.GetComponentInParent<FurnaceSlot>();

        FurnaceSlot targetFurnaceSlot =
            dropSlot.GetComponentInParent<FurnaceSlot>();

        // Furnace output cannot be a destination.
        if (targetFurnaceSlot != null &&
            targetFurnaceSlot.slotType == FurnaceSlot.SlotType.Output)
        {
            ResetToOriginalPosition();
            return;
        }

        // Output items cannot be moved into another furnace slot.
        if (sourceFurnaceSlot != null &&
            sourceFurnaceSlot.slotType == FurnaceSlot.SlotType.Output &&
            targetFurnaceSlot != null)
        {
            ResetToOriginalPosition();
            return;
        }

        // Validate items entering furnace slots.
        if (targetFurnaceSlot != null &&
            !targetFurnaceSlot.CanAccept(gameObject))
        {
            ResetToOriginalPosition();
            return;
        }

        // EMPTY DESTINATION
        if (dropSlot.currentItem == null)
        {
            originalSlot.currentItem = null;

            transform.SetParent(dropSlot.transform);
            dropSlot.currentItem = gameObject;

            ResetToSlotPosition();
            NotifyFurnaceSlotsChanged();
            return;
        }

        // OCCUPIED DESTINATION
        Item targetItem =
            dropSlot.currentItem.GetComponent<Item>();

        if (targetItem == null)
        {
            ResetToOriginalPosition();
            return;
        }

        // TRY STACKING
        if (CanStack(draggedItem, targetItem))
        {
            int amountToMove = Mathf.Min(
                draggedItem.quantity,
                targetItem.GetRemainingStackSpace()
            );

            if (amountToMove <= 0)
            {
                ResetToOriginalPosition();
                return;
            }

            int amountAdded = targetItem.AddToStack(amountToMove);
            draggedItem.RemoveFromStack(amountAdded);

            if (draggedItem.quantity <= 0)
            {
                originalSlot.currentItem = null;
                Destroy(gameObject);
            }
            else
            {
                transform.SetParent(originalSlot.transform);
                ResetToSlotPosition();
            }

            NotifyFurnaceSlotsChanged();
            return;
        }

        // Validate the item moving into the source furnace slot.
        if (sourceFurnaceSlot != null &&
            !sourceFurnaceSlot.CanAccept(dropSlot.currentItem))
        {
            ResetToOriginalPosition();
            return;
        }

        // SWAP
        GameObject targetObject = dropSlot.currentItem;

        targetObject.transform.SetParent(originalSlot.transform);
        originalSlot.currentItem = targetObject;

        RectTransform targetRect =
            targetObject.GetComponent<RectTransform>();

        if (targetRect != null)
        {
            targetRect.anchoredPosition = Vector2.zero;
        }

        transform.SetParent(dropSlot.transform);
        dropSlot.currentItem = gameObject;

        ResetToSlotPosition();
        NotifyFurnaceSlotsChanged();
    }

    private bool CanStack(Item draggedItem, Item targetItem)
    {
        if (draggedItem == null || targetItem == null)
            return false;

        return draggedItem.ID == targetItem.ID &&
               draggedItem.stackable &&
               targetItem.stackable;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // LEFT CLICK: Select the hotbar slot.
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Slot slot = GetComponentInParent<Slot>();

            if (slot != null)
            {
                HotbarController hotbar =
                    slot.GetComponentInParent<HotbarController>();

                if (hotbar != null)
                {
                    int slotIndex = slot.transform.GetSiblingIndex();
                    hotbar.SelectSlot(slotIndex);
                }
            }
        }

        // RIGHT CLICK: Split stack.
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            SplitStack();
        }
    }

    private void SplitStack()
    {
        Item item = GetComponent<Item>();

        if (item == null ||
            !item.stackable ||
            item.quantity <= 1)
        {
            return;
        }

        int splitAmount = item.quantity / 2;
        Slot emptySlot = FindEmptyInventorySlot();

        if (emptySlot == null)
        {
            Debug.Log("No empty inventory slot available for split.");
            return;
        }

        item.RemoveFromStack(splitAmount);

        GameObject newItem = item.CloneItem(splitAmount);

        if (newItem == null)
        {
            item.AddToStack(splitAmount);
            return;
        }

        newItem.transform.SetParent(emptySlot.transform);

        RectTransform rect = newItem.GetComponent<RectTransform>();

        if (rect != null)
        {
            rect.anchoredPosition = Vector2.zero;
        }

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

            if (slot != null && slot.currentItem == null)
                return slot;
        }

        return null;
    }

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

    private void DropItem(Slot originalSlot)
    {
        Item item = GetComponent<Item>();

        if (item == null)
            return;

        int amountToDrop = item.quantity;

        CreateWorldItem(amountToDrop);

        originalSlot.currentItem = null;
        NotifyFurnaceSlotsChanged();
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

        Vector2 dropPosition = playerTransform.position;

        GameObject dropItem = Instantiate(
            gameObject,
            dropPosition,
            Quaternion.identity
        );

        dropItem.transform.localScale = Vector3.one;
        
        Item droppedItem = dropItem.GetComponent<Item>();

        if (droppedItem != null)
        {
            droppedItem.quantity = amount;
            droppedItem.UpdateQuantityDisplay();
        }
    }

    private void NotifyFurnaceSlotsChanged()
    {
        FurnaceController controller =
            FindFirstObjectByType<FurnaceController>();

        if (controller != null)
        {
            controller.NotifySlotsChanged();
        }
    }

    private void ResetToOriginalPosition()
    {
        transform.SetParent(originalParent);
        ResetToSlotPosition();
    }

    private void ResetToSlotPosition()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}
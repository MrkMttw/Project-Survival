using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour,
    IPointerDownHandler,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerEnterHandler
{

    [SerializeField] private float swipeThreshold = 80f;
    private Vector2 dragStartPosition;  
    private bool passedSwipeThreshold;
    
    private Vector3 originalLocalScale;
    public static ItemDragHandler ActiveHandler { get; private set; }

    private Slot sourceSlot;
    private RectTransform sourceContainerRect;
    private Canvas rootCanvas;
    private CanvasGroup canvasGroup;
    private InventoryController inventoryController;

    private readonly List<Slot> visitedSlots = new List<Slot>();

    private bool isSelected;
    private bool isDragging;

    private void Awake()
    {
        originalLocalScale = transform.localScale;

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
        inventoryController = InventoryController.instance;
    }

    // PICK UP ITEM
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        // PLACE SELECTED ITEM ON OCCUPIED SLOT
        if (ActiveHandler != null && ActiveHandler != this)
        {
            Slot targetSlot = GetComponentInParent<Slot>();

            if (targetSlot != null)
                ActiveHandler.TryPlaceOnSlot(targetSlot);
        }
    }

    private void PickUpItem()
    {
        sourceSlot = GetComponentInParent<Slot>();

        if (sourceSlot == null)
            return;

        if (sourceSlot.currentItem != gameObject)
            return;

        rootCanvas = GetComponentInParent<Canvas>();

        if (rootCanvas == null)
        {
            Debug.LogError("ItemDragHandler requires a Canvas.");
            return;
        }

        rootCanvas = rootCanvas.rootCanvas;

        sourceContainerRect =
            sourceSlot.transform.parent.GetComponent<RectTransform>();

        sourceSlot.currentItem = null;

        transform.SetParent(rootCanvas.transform, true);

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.85f;

        isSelected = true;
        ActiveHandler = this;
    }

    // DRAGGING

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isSelected)
            PickUpItem();

        if (!isSelected)
            return;

        isDragging = true;
        dragStartPosition = eventData.position;
        passedSwipeThreshold = false;

        visitedSlots.Clear();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isSelected)
            return;

        transform.position = eventData.position;

        if (Vector2.Distance(dragStartPosition, eventData.position)
            >= swipeThreshold)
        {
            passedSwipeThreshold = true;
        }

        if (!passedSwipeThreshold)
            return;

        if (eventData.pointerEnter != null)
        {
            Slot slot =
                eventData.pointerEnter.GetComponentInParent<Slot>();

            if (slot != null)
                RecordSwipeSlot(slot);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ActiveHandler == null || ActiveHandler == this)
            return;

        Slot slot = GetComponentInParent<Slot>();

        if (slot != null)
            ActiveHandler.RecordSwipeSlot(slot);
    }

    public void RecordSwipeSlot(Slot slot)
    {
        if (!isSelected ||
            !isDragging ||
            !passedSwipeThreshold ||
            slot == null)
            return;

        if (slot == sourceSlot)
            return;

        if (!visitedSlots.Contains(slot))
            visitedSlots.Add(slot);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isSelected || !isDragging)
            return;

        isDragging = false;

        Slot finalSlot = null;

        if (eventData.pointerEnter != null)
        {
            finalSlot =
                eventData.pointerEnter.GetComponentInParent<Slot>();
        }

        // DROP INTO ANOTHER CONTAINER.
        if (finalSlot != null &&
            finalSlot != sourceSlot &&
            finalSlot.transform.parent != sourceSlot.transform.parent)
        {
            HandleSlotDrop(finalSlot);
            return;
        }

        // DISTRIBUTE ONLY AFTER A DELIBERATE SWIPE.
        if (passedSwipeThreshold && visitedSlots.Count > 1)
        {
            DistributeStack();
            return;
        }

        // NORMAL SINGLE-SLOT DROP.
        if (finalSlot != null && finalSlot != sourceSlot)
        {
            HandleSlotDrop(finalSlot);
            return;
        }

        // DROP OUTSIDE THE SOURCE CONTAINER.
        if (!IsWithinSourceContainer(eventData.position))
        {
            DropItem();
        }
        else
        {
            ReturnToSource();
        }
    }

    private void RecordFinalSlot(Slot slot)
    {
        if (slot == null || slot == sourceSlot)
            return;

        if (!visitedSlots.Contains(slot))
            visitedSlots.Add(slot);
    }

    // PLACE ITEM

    public void TryPlaceOnSlot(Slot targetSlot)
    {
        if (!isSelected || isDragging || targetSlot == null)
            return;

        HandleSlotDrop(targetSlot);
    }

    private void HandleSlotDrop(Slot dropSlot)
    {
        Item draggedItem = GetComponent<Item>();

        if (draggedItem == null || sourceSlot == null)
        {
            ReturnToSource();
            return;
        }

        if (dropSlot == sourceSlot)
        {
            ReturnToSource();
            return;
        }

        FurnaceSlot sourceFurnaceSlot =
            sourceSlot.GetComponentInParent<FurnaceSlot>();

        FurnaceSlot targetFurnaceSlot =
            dropSlot.GetComponentInParent<FurnaceSlot>();

        // OUTPUT CANNOT BE A DESTINATION.
        if (targetFurnaceSlot != null &&
            targetFurnaceSlot.slotType == FurnaceSlot.SlotType.Output)
        {
            ReturnToSource();
            return;
        }

        // OUTPUT ITEMS CANNOT ENTER OTHER FURNACE SLOTS.
        if (sourceFurnaceSlot != null &&
            sourceFurnaceSlot.slotType == FurnaceSlot.SlotType.Output &&
            targetFurnaceSlot != null)
        {
            ReturnToSource();
            return;
        }

        // VALIDATE FURNACE DESTINATION.
        if (targetFurnaceSlot != null &&
            !targetFurnaceSlot.CanAccept(gameObject))
        {
            ReturnToSource();
            return;
        }

        // EMPTY DESTINATION.
        if (dropSlot.currentItem == null)
        {
            dropSlot.currentItem = gameObject;

            transform.SetParent(dropSlot.transform);

            FinishPlacement();
            NotifyFurnaceSlotsChanged();
            return;
        }

        GameObject targetObject = dropSlot.currentItem;

        Item targetItem = targetObject.GetComponent<Item>();

        if (targetItem == null)
        {
            ReturnToSource();
            return;
        }

        // STACKING.
        if (CanStack(draggedItem, targetItem))
        {
            int amountToMove = Mathf.Min(
                draggedItem.quantity,
                targetItem.GetRemainingStackSpace()
            );

            if (amountToMove <= 0)
            {
                ReturnToSource();
                return;
            }

            int amountAdded = targetItem.AddToStack(amountToMove);

            draggedItem.quantity -= amountAdded;
            draggedItem.UpdateQuantityDisplay();

            if (draggedItem.quantity <= 0)
            {
                FinishSelection();
                Destroy(gameObject);
            }
            else
            {
                ReturnToSource();
            }

            NotifyFurnaceSlotsChanged();
            return;
        }

        // VALIDATE ITEM ENTERING SOURCE FURNACE SLOT.
        if (sourceFurnaceSlot != null &&
            !sourceFurnaceSlot.CanAccept(targetObject))
        {
            ReturnToSource();
            return;
        }

        // SWAP.
        targetObject.transform.SetParent(sourceSlot.transform);

        sourceSlot.currentItem = targetObject;

        RectTransform targetRect =
            targetObject.GetComponent<RectTransform>();

        if (targetRect != null)
            targetRect.anchoredPosition = Vector2.zero;

        transform.SetParent(dropSlot.transform);

        dropSlot.currentItem = gameObject;

        FinishPlacement();
        NotifyFurnaceSlotsChanged();
    }

    private bool CanStack(Item first, Item second)
    {
        return first != null &&
               second != null &&
               first.ID == second.ID &&
               first.stackable &&
               second.stackable;
    }

    // EQUAL DISTRIBUTION

    private void DistributeStack()
    {
        Item draggedItem = GetComponent<Item>();

        if (draggedItem == null || sourceSlot == null)
        {
            ReturnToSource();
            return;
        }

        List<Slot> validSlots = new List<Slot>();

        foreach (Slot slot in visitedSlots)
        {
            if (slot == null || slot == sourceSlot)
                continue;

            if (CanReceiveDistribution(slot, draggedItem))
                validSlots.Add(slot);
        }

        if (validSlots.Count == 0)
        {
            ReturnToSource();
            return;
        }

        int totalQuantity = draggedItem.quantity;

        int baseAmount = totalQuantity / validSlots.Count;
        int remainder = totalQuantity % validSlots.Count;

        int totalMoved = 0;

        for (int i = 0; i < validSlots.Count; i++)
        {
            int allocation = baseAmount;

            if (i < remainder)
                allocation++;

            if (allocation <= 0)
                continue;

            Slot targetSlot = validSlots[i];

            int moved = TransferAmount(targetSlot, allocation);

            totalMoved += moved;
        }

        draggedItem.quantity = totalQuantity - totalMoved;
        draggedItem.UpdateQuantityDisplay();

        if (draggedItem.quantity <= 0)
        {
            FinishSelection();
            Destroy(gameObject);
        }
        else
        {
            ReturnToSource();
        }

        NotifyFurnaceSlotsChanged();
    }

    private bool CanReceiveDistribution(Slot targetSlot, Item draggedItem)
    {
        FurnaceSlot sourceFurnaceSlot =
            sourceSlot.GetComponentInParent<FurnaceSlot>();

        FurnaceSlot targetFurnaceSlot =
            targetSlot.GetComponentInParent<FurnaceSlot>();

        // OUTPUT ITEMS CANNOT ENTER FURNACE SLOTS.
        if (sourceFurnaceSlot != null &&
            sourceFurnaceSlot.slotType == FurnaceSlot.SlotType.Output &&
            targetFurnaceSlot != null)
            return false;

        // FURNACE OUTPUT IS NEVER A DESTINATION.
        if (targetFurnaceSlot != null &&
            targetFurnaceSlot.slotType == FurnaceSlot.SlotType.Output)
            return false;

        // FURNACE VALIDATION.
        if (targetFurnaceSlot != null &&
            !targetFurnaceSlot.CanAccept(gameObject))
            return false;

        // EMPTY SLOT.
        if (targetSlot.currentItem == null)
            return true;

        // OCCUPIED SLOT MUST BE STACK-COMPATIBLE.
        Item targetItem =
            targetSlot.currentItem.GetComponent<Item>();

        return CanStack(draggedItem, targetItem) &&
               targetItem.GetRemainingStackSpace() > 0;
    }

    private int TransferAmount(Slot targetSlot, int amount)
    {
        Item draggedItem = GetComponent<Item>();

        if (draggedItem == null || amount <= 0)
            return 0;

        // STACK INTO EXISTING ITEM.
        if (targetSlot.currentItem != null)
        {
            Item targetItem =
                targetSlot.currentItem.GetComponent<Item>();

            if (!CanStack(draggedItem, targetItem))
                return 0;

            return targetItem.AddToStack(amount);
        }

        // CREATE A NEW STACK.
        int amountToCreate = draggedItem.stackable
            ? Mathf.Min(amount, draggedItem.maxStackSize)
            : 1;

        GameObject newItem =
            draggedItem.CloneItem(amountToCreate);

        if (newItem == null)
            return 0;

        PlaceCloneInSlot(newItem, targetSlot);

        return amountToCreate;
    }

    private void PlaceCloneInSlot(GameObject newItem, Slot slot)
    {
        newItem.transform.SetParent(slot.transform, false);
        newItem.transform.localScale = originalLocalScale;

        RectTransform rect = newItem.GetComponent<RectTransform>();

        if (rect != null)
            rect.anchoredPosition = Vector2.zero;

        CanvasGroup cg = newItem.GetComponent<CanvasGroup>();

        if (cg != null)
        {
            cg.alpha = 1f;
            cg.blocksRaycasts = true;
        }

        slot.currentItem = newItem;
    }

    // RETURN ITEM

    private void ReturnToSource()
    {
        if (sourceSlot == null)
        {
            Debug.LogError("Cannot return item: source slot is missing.");
            return;
        }

        if (sourceSlot.currentItem != null)
        {
            Debug.LogError("Cannot return item: source slot is occupied.");
            return;
        }

        sourceSlot.currentItem = gameObject;

        transform.SetParent(sourceSlot.transform);

        FinishPlacement();
    }

    private void FinishPlacement()
    {
        RectTransform rect = GetComponent<RectTransform>();

        if (rect != null)
            rect.anchoredPosition = Vector2.zero;

        transform.localScale = originalLocalScale;

        Slot currentSlot = GetComponentInParent<Slot>();

        if (currentSlot != null)
            sourceSlot = currentSlot;

        FinishSelection();
    }

    private void FinishSelection()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        isSelected = false;
        isDragging = false;

        visitedSlots.Clear();

        if (ActiveHandler == this)
            ActiveHandler = null;
    }

    // WORLD DROPPING

    private bool IsWithinSourceContainer(Vector2 screenPosition)
    {
        if (sourceContainerRect == null)
            return true;

        return RectTransformUtility.RectangleContainsScreenPoint(
            sourceContainerRect,
            screenPosition,
            null
        );
    }

    private void DropItem()
    {
        Item item = GetComponent<Item>();

        if (item == null)
        {
            ReturnToSource();
            return;
        }

        Transform player =
            GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogError("Missing Player tag.");
            ReturnToSource();
            return;
        }

        int amount = item.quantity;

        GameObject droppedItem = Instantiate(
            gameObject,
            player.position,
            Quaternion.identity
        );

        droppedItem.transform.SetParent(null);
        droppedItem.transform.localScale = Vector3.one;

        Item droppedItemComponent =
            droppedItem.GetComponent<Item>();

        if (droppedItemComponent != null)
        {
            droppedItemComponent.quantity = amount;
            droppedItemComponent.UpdateQuantityDisplay();
        }

        FinishSelection();
        Destroy(gameObject);

        NotifyFurnaceSlotsChanged();
    }

    // RIGHT-CLICK SPLITTING

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (!isSelected)
                SplitStack();

            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Slot slot = GetComponentInParent<Slot>();

            if (slot == null)
                return;

            HotbarController hotbar =
                slot.GetComponentInParent<HotbarController>();

            if (hotbar != null)
            {
                int slotIndex = slot.transform.GetSiblingIndex();
                hotbar.SelectSlot(slotIndex);
            }
        }
    }

    private void SplitStack()
    {
        Item item = GetComponent<Item>();

        if (item == null ||
            !item.stackable ||
            item.quantity <= 1)
            return;

        Slot emptySlot = FindEmptyInventorySlot();

        if (emptySlot == null)
        {
            Debug.Log("No empty inventory slot available for split.");
            return;
        }

        int splitAmount = item.quantity / 2;

        item.quantity -= splitAmount;
        item.UpdateQuantityDisplay();

        GameObject newItem = item.CloneItem(splitAmount);

        if (newItem == null)
        {
            item.quantity += splitAmount;
            item.UpdateQuantityDisplay();
            return;
        }

        newItem.transform.SetParent(emptySlot.transform);

        RectTransform rect =
            newItem.GetComponent<RectTransform>();

        if (rect != null)
            rect.anchoredPosition = Vector2.zero;

        emptySlot.currentItem = newItem;

        NotifyFurnaceSlotsChanged();
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

    // FURNACE NOTIFICATION

    private void NotifyFurnaceSlotsChanged()
    {
        FurnaceController controller =
            FindFirstObjectByType<FurnaceController>();

        if (controller != null)
            controller.NotifySlotsChanged();
    }
}
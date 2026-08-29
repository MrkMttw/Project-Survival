using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    Transform originalParent;
    CanvasGroup canvasGroup;
    private InventoryController inventoryController;

    void Start()
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

        // FIX: If dropped back onto the same slot, return to its original position
        if (dropSlot == originalSlot)
        {
            transform.SetParent(originalParent);
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            return;
        }

        if (dropSlot != null)
        {
            if (dropSlot.currentItem != null)
            {
                Item draggedItem = GetComponent<Item>();
                Item targetItem = dropSlot.currentItem.GetComponent<Item>();

                // Check if target slot has same item type to stack them together
                if (draggedItem != null && targetItem != null && draggedItem.ID == targetItem.ID)
                {
                    targetItem.AddToStack(draggedItem.quantity);
                    originalSlot.currentItem = null;
                    Destroy(gameObject);
                }
                else
                {
                    // Swap items
                    dropSlot.currentItem.transform.SetParent(originalSlot.transform);
                    originalSlot.currentItem = dropSlot.currentItem;
                    dropSlot.currentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                    transform.SetParent(dropSlot.transform);
                    dropSlot.currentItem = gameObject;
                    GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                }
            }
            else
            {
                // Place in empty slot
                originalSlot.currentItem = null;
                transform.SetParent(dropSlot.transform);
                dropSlot.currentItem = gameObject;
                GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
        }
        else
        {
            if (!IsWithinInventory(eventData.position))
            {
                DropItem(originalSlot);
            }
            else
            {
                // Snap back to original slot
                transform.SetParent(originalParent);
                GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
        }
    }

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

        if (item == null || item.quantity <= 1)
            return;

        int splitAmount = item.quantity / 2;

        item.RemoveFromStack(splitAmount);

        GameObject newItem = item.CloneItem(splitAmount);

        if (inventoryController == null || newItem == null)
            return;

        foreach (Transform slotTransform in inventoryController.inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot != null && slot.currentItem == null)
            {
                slot.currentItem = newItem;
                newItem.transform.SetParent(slot.transform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                return;
            }
        }

        // If no empty slot found, return the split items back to original stack
        item.AddToStack(splitAmount);
        Destroy(newItem);
    }

    private bool IsWithinInventory(Vector2 mousePosition)
    {
        RectTransform inventoryRect = originalParent.parent.GetComponent<RectTransform>();

        return RectTransformUtility.RectangleContainsScreenPoint(
            inventoryRect,
            mousePosition
        );
    }

    private void DropItem(Slot originalSlot)
    {
        Item item = GetComponent<Item>();
        int quantity = item != null ? item.quantity : 1;

        if (quantity > 1)
        {
            item.RemoveFromStack(1);

            transform.SetParent(originalParent);
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
        else
        {
            originalSlot.currentItem = null;
        }

        Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (playerTransform == null)
        {
            Debug.LogError("Missing player tag");
            return;
        }

        Vector2 dropPosition = (Vector2)playerTransform.position;

        GameObject dropItem = Instantiate(gameObject, dropPosition, Quaternion.identity);

        Item droppedItemComponent = dropItem.GetComponent<Item>();

        if (droppedItemComponent != null)
        {
            droppedItemComponent.quantity = 1;
        }

        if (quantity <= 1)
        {
            Destroy(gameObject);
        }
    }
}
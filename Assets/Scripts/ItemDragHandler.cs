using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles drag-and-drop functionality for inventory items.
/// Implements Unity's drag event interfaces to allow items to be moved between slots.
/// </summary>
public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    /// <summary>
    /// The original parent transform of the item before dragging began.
    /// Used to return the item to its original slot if dropped on an invalid location.
    /// </summary>
    Transform originalParent;

    /// <summary>
    /// CanvasGroup component used to control transparency and raycast blocking during drag.
    /// </summary>
    CanvasGroup canvasGroup;

    /// <summary>
    /// Initializes the drag handler by caching the CanvasGroup component.
    /// Called before the first frame update.
    /// </summary>
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Called when dragging begins. Sets up the item for dragging by moving it to the root canvas
    /// and making it semi-transparent.
    /// </summary>
    /// <param name="eventData">Data about the pointer event that triggered the drag.</param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    /// <summary>
    /// Called while dragging. Updates the item's position to follow the mouse cursor.
    /// </summary>
    /// <param name="eventData">Data about the pointer event during dragging.</param>
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    /// <summary>
    /// Called when dragging ends. Handles dropping the item into a slot or returning it to its original position.
    /// Supports item swapping between slots.
    /// </summary>
    /// <param name="eventData">Data about the pointer event that ended the drag.</param>
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        Slot dropSlot = eventData.pointerEnter?.GetComponent<Slot>();
        if(dropSlot == null)
        {
            GameObject dropItem = eventData.pointerEnter;
            if (dropItem != null)
            {
                dropSlot = dropItem.GetComponentInParent<Slot>();
            }
        }
        Slot originalSlot = originalParent.GetComponent<Slot>();

        if(dropSlot != null)
        {
            if (dropSlot.currentItem != null)
            {
                dropSlot.currentItem.transform.SetParent(originalSlot.transform);
                originalSlot.currentItem = dropSlot.currentItem;
                dropSlot.currentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
            else
            {
                originalSlot.currentItem = null;
            }

            transform.SetParent(dropSlot.transform);
            dropSlot.currentItem = gameObject;
        }
        else
        {
            transform.SetParent(originalParent);
        }

        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }
}
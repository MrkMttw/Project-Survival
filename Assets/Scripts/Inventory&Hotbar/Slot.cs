using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour,
    IPointerDownHandler,
    IPointerEnterHandler
{
    public GameObject currentItem;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (ItemDragHandler.ActiveHandler != null &&
            currentItem == null)
        {
            ItemDragHandler.ActiveHandler.TryPlaceOnSlot(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ItemDragHandler.ActiveHandler != null)
        {
            ItemDragHandler.ActiveHandler.RecordSwipeSlot(this);
        }
    }
}
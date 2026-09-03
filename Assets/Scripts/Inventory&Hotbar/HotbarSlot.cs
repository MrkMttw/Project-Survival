using UnityEngine;
using UnityEngine.EventSystems;

public class HotbarSlotClick : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        HotbarController hotbar = FindFirstObjectByType<HotbarController>();

        if (hotbar != null)
        {
            hotbar.SelectSlot(transform.GetSiblingIndex());
        }
    }
}
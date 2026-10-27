using UnityEngine;

public class FurnaceSlot : MonoBehaviour
{
    public enum SlotType
    {
        Input,
        Fuel,
        Output
    }

    public SlotType slotType;

    public bool CanAccept(GameObject itemObject)
    {
        if (itemObject == null)
            return false;

        Item item = itemObject.GetComponent<Item>();

        if (item == null)
            return false;

        switch (slotType)
        {
            case SlotType.Input:
                return item.isSmeltable;

            case SlotType.Fuel:
                return item.isFuel;

            case SlotType.Output:
                return false;

            default:
                return false;
        }
    }
}
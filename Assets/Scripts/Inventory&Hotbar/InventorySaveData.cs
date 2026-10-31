using System;

[Serializable]
public class InventorySaveData
{
    public int itemID;
    public int slotIndex;
    public int quantity = 1;

    public int currentDurability;
    public bool hasSavedDurability;
}
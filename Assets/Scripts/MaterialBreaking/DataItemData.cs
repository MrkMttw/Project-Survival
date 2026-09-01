using UnityEngine;

[System.Serializable]
public class DropItemData
{
    public Item item;

    [Min(1)]
    public int minAmount = 1;

    [Min(1)]
    public int maxAmount = 1;
}
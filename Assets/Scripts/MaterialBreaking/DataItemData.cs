
using UnityEngine;

[System.Serializable]
public class DropItemData
{
    public Item item;

    [Range(0f, 100f)]
    public float dropChance = 100f;

    [Min(0)]
    public int minAmount = 1;

    [Min(0)]
    public int maxAmount = 1;
}
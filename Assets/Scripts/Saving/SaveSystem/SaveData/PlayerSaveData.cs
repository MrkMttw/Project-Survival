using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerSaveData
{
    public string worldID;
    public int saveVersion = 1;

    // PLAYER POSITION
    public Vector3 playerPosition;

    // RESPAWN POINT
    public bool hasRespawnPoint;
    public Vector3 respawnPosition;

    // PLAYER STATS
    public float currentHealth;
    public float currentHunger;
    public float currentSaturation;

    // INVENTORY
    public List<ItemSlotSaveData> inventoryItems =
        new List<ItemSlotSaveData>();

    // HOTBAR
    public List<ItemSlotSaveData> hotbarItems =
        new List<ItemSlotSaveData>();

    public int selectedHotbarSlot = -1;
}

[Serializable]
public class ItemSlotSaveData
{
    public int slotIndex;
    public int itemID;
    public int quantity;
    public int durability;
}
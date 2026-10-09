
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "New Mob Drop Preset",
    menuName = "Mob Drop/Mob Drop Preset"
)]
public class MobDropData : ScriptableObject
{
    [System.Serializable]
    public class DropEntry
    {
        [Header("Drop Item")]
        public GameObject itemPrefab;

        [Header("Drop Quantity")]
        [Min(1)]
        public int minAmount = 1;

        [Min(1)]
        public int maxAmount = 1;

        [Header("Drop Chance")]
        [Range(0f, 100f)]
        public float dropChance = 100f;
    }

    [Header("Drop Entries")]
    public List<DropEntry> drops = new List<DropEntry>();
}
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

[CreateAssetMenu(
    fileName = "New Tool Break Preset",
    menuName = "Game/Tool Break Preset"
)]
public class ToolBreakPreset : ScriptableObject
{
    [Header("Tool")]
    public Item requiredTool;

    [Header("Materials This Tool Can Break")]
    public string[] breakableMaterials;

    [Header("Drops")]
    public DropItemData[] drops;

    [Header("Damage")]
    public int damage = 1;

    [Header("Breaking Speed")]
    public float breakCooldown = 0.3f;
}
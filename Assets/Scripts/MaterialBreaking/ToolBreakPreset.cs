using UnityEngine;

[CreateAssetMenu(
    fileName = "New Tool Break Preset",
    menuName = "Game/Tool Break Preset"
)]
public class ToolBreakPreset : ScriptableObject
{
    [Header("Tool")]
    public bool requiresTool = true;
    public Item requiredTool;

    [Header("Materials This Tool Can Break")]
    public string[] breakableMaterials;

    [Header("Drops")]
    public DropItemData[] drops;

    [Header("Damage")]
    [Min(1)]
    public int damage = 1;

    [Header("Breaking Speed")]
    [Min(0)]
    public float breakCooldown = 0.3f;
}
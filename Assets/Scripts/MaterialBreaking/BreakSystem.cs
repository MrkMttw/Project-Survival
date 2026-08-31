using UnityEngine;
using UnityEngine.InputSystem;

public class BreakSystem : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Mining")]
    public float breakRange = 2.5f;

    [Header("Tool Presets")]
    public ToolBreakPreset[] toolPresets;

    [Header("Detection")]
    public LayerMask breakableLayer;

    private float nextBreakTime;

    private PlayerHeldItem playerHeldItem;

    private void Start()
    {
        playerHeldItem = GetComponent<PlayerHeldItem>();

        if (playerHeldItem == null)
        {
            Debug.LogError(
                "BreakSystem: PlayerHeldItem was not found on Player!"
            );
        }
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryBreak();
        }

    }

    private void TryBreak()
    {
        if (playerHeldItem == null)
            return;

        Item equippedItem = playerHeldItem.GetHeldItem();

        if (equippedItem == null)
        {
            Debug.Log("No item equipped.");
            return;
        }

        Vector2 mousePosition =
            Camera.main.ScreenToWorldPoint(
                Mouse.current.position.ReadValue()
            );

        Collider2D hit =
            Physics2D.OverlapPoint(
                mousePosition,
                breakableLayer
            );

        if (hit == null)
        {
            Debug.Log("Nothing breakable clicked.");
            return;
        }

        BreakableMaterial material =
            hit.GetComponent<BreakableMaterial>();

        if (material == null)
        {
            Debug.Log("Clicked object is not breakable.");
            return;
        }

        float distance =
            Vector2.Distance(
                player.position,
                hit.transform.position
            );

        if (distance > breakRange)
        {
            Debug.Log("Material is too far away.");
            return;
        }

        ToolBreakPreset preset =
    FindPreset(
        equippedItem,
        material.materialType
    );

    if (preset == null)
    {
        Debug.Log(
            equippedItem.name +
            " cannot break " +
            material.materialType
        );

        return;
    }

    if (Time.time < nextBreakTime)
    {
        return;
    }

    nextBreakTime =
        Time.time + preset.breakCooldown;

        // Damage material
        bool broken = material.TakeDamage(preset.damage);

        // If material reaches zero HP
        if (broken)
        {
            DropItem(preset, material.transform.position);
        }
    }

    private void DropItem(
        ToolBreakPreset preset,
        Vector3 position
    )
    {
        if (preset.drops == null || preset.drops.Length == 0)
        {
            Debug.LogWarning(
                "ToolBreakPreset has no Drop Items assigned!"
            );

            return;
        }

        foreach (DropItemData drop in preset.drops)
        {
            if (drop.item == null)
            {
                Debug.LogWarning(
                    "A Drop Item is missing in ToolBreakPreset!"
                );

                continue;
            }

            int amount = Random.Range(
                drop.minAmount,
                drop.maxAmount + 1
            );

            GameObject droppedItem =
                drop.item.CloneItem(amount);

            if (droppedItem == null)
            {
                Debug.LogWarning(
                    "CloneItem returned null for " +
                    drop.item.name
                );

                continue;
            }

            droppedItem.transform.position = position;

            Debug.Log(
                "Dropped " +
                amount +
                "x " +
                drop.item.name
            );
        }
    }

    private ToolBreakPreset FindPreset(
        Item tool,
        string materialType
    )
    {
        foreach (ToolBreakPreset preset in toolPresets)
        {
            if (preset.requiredTool == null)
                continue;

            if (preset.requiredTool.ID != tool.ID)
                continue;

            foreach (string material in preset.breakableMaterials)
            {
                if (material == materialType)
                {
                    return preset;
                }
            }
        }

        return null;
    }
}
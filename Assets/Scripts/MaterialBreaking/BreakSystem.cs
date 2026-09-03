using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

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

    public GameObject hotbarPanel;

    private void Start()
    {
        playerHeldItem = GetComponent<PlayerHeldItem>();

        if (playerHeldItem == null)
        {
            Debug.LogError(
                "BreakSystem: PlayerHeldItem was not found on Player!"
            );
        }

        if (player == null)
        {
            Debug.LogError(
                "BreakSystem: Player Transform is not assigned!"
            );
        }
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (IsPointerOverHotbar())
                return;

            TryBreak();
        }
    }

    private bool IsPointerOverHotbar()
    {
        if (hotbarPanel == null)
            return false;

        PointerEventData pointerData =
            new PointerEventData(EventSystem.current);

        pointerData.position =
            Mouse.current.position.ReadValue();

        var results = new System.Collections.Generic.List<RaycastResult>();

        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.transform.IsChildOf(
                hotbarPanel.transform))
            {
                return true;
            }
        }

        return false;
    }

    private void TryBreak()
    {
        if (playerHeldItem == null)
            return;

        if (player == null)
            return;

        // Get currently equipped item.
        // This can legitimately be null because the player
        // may be using their bare hands.
        Item equippedItem = playerHeldItem.GetHeldItem();

        if (equippedItem == null)
        {
            Debug.Log("Breaking with bare hands.");
        }
        else
        {
            Debug.Log(
                "Breaking with equipped item: " +
                equippedItem.name
            );
        }

        // Check cooldown before doing anything else.
        if (Time.time < nextBreakTime)
        {
            return;
        }

        // Make sure camera exists.
        if (Camera.main == null)
        {
            Debug.LogError(
                "BreakSystem: No Main Camera found!"
            );

            return;
        }

        // Get mouse position in world space.
        Vector2 mousePosition =
            Camera.main.ScreenToWorldPoint(
                Mouse.current.position.ReadValue()
            );

        // Find breakable object under mouse.
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

        // Get BreakableMaterial from clicked object.
        BreakableMaterial material =
            hit.GetComponent<BreakableMaterial>();

        if (material == null)
        {
            Debug.Log(
                "Clicked object does not have BreakableMaterial."
            );

            return;
        }

        // Check distance.
        float distance =
            Vector2.Distance(
                player.position,
                hit.transform.position
            );

        if (distance > breakRange)
        {
            Debug.Log(
                "Material is too far away. Distance: " +
                distance.ToString("F2")
            );

            return;
        }

        // Find a preset that allows this item/material combination.
        ToolBreakPreset preset =
            FindPreset(
                equippedItem,
                material.materialType
            );

        if (preset == null)
        {
            if (equippedItem == null)
            {
                Debug.Log(
                    "Bare hands cannot break " +
                    material.materialType
                );
            }
            else
            {
                Debug.Log(
                    equippedItem.name +
                    " cannot break " +
                    material.materialType
                );
            }

            return;
        }

        // Apply cooldown.
        nextBreakTime =
            Time.time + preset.breakCooldown;

        Debug.Log(
            "Breaking " +
            material.gameObject.name +
            " using preset: " +
            preset.name
        );

        // Deal damage.
        bool broken =
            material.TakeDamage(preset.damage);

        // Drop items only when completely broken.
        if (broken)
        {
            DropItem(
                preset,
                material.transform.position
            );
        }
    }

    private void DropItem(
        ToolBreakPreset preset,
        Vector3 position
    )
    {
        if (preset.drops == null ||
            preset.drops.Length == 0)
        {
            Debug.LogWarning(
                "ToolBreakPreset '" +
                preset.name +
                "' has no Drop Items assigned!"
            );

            return;
        }

        foreach (DropItemData drop in preset.drops)
        {
            if (drop == null)
                continue;

            if (drop.item == null)
            {
                Debug.LogWarning(
                    "A Drop Item is missing in ToolBreakPreset '" +
                    preset.name +
                    "'!"
                );

                continue;
            }

            int minAmount =
                Mathf.Max(1, drop.minAmount);

            int maxAmount =
                Mathf.Max(minAmount, drop.maxAmount);

            int amount =
                Random.Range(
                    minAmount,
                    maxAmount + 1
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
        if (toolPresets == null ||
            toolPresets.Length == 0)
        {
            Debug.LogWarning(
                "BreakSystem: No ToolBreakPresets assigned!"
            );

            return null;
        }

        foreach (ToolBreakPreset preset in toolPresets)
        {
            if (preset == null)
                continue;

            if (preset.breakableMaterials == null)
                continue;

            // Check if this preset supports the material.
            bool materialMatches = false;

            foreach (string material in preset.breakableMaterials)
            {
                if (string.IsNullOrEmpty(material))
                    continue;

                if (material == materialType)
                {
                    materialMatches = true;
                    break;
                }
            }

            if (!materialMatches)
                continue;

            // ==========================================
            // BARE HANDS
            // ==========================================

            if (!preset.requiresTool)
            {
                // This preset does not require a tool,
                // so it can be used with bare hands.
                return preset;
            }

            // ==========================================
            // TOOL REQUIRED
            // ==========================================

            // A tool is required, but player has empty hands.
            if (tool == null)
                continue;

            // Preset says a tool is required but no
            // required tool has been assigned.
            if (preset.requiredTool == null)
            {
                Debug.LogWarning(
                    "Preset '" +
                    preset.name +
                    "' requires a tool but has no Required Tool assigned!"
                );

                continue;
            }

            // Check if equipped tool matches required tool.
            if (preset.requiredTool.ID == tool.ID)
            {
                return preset;
            }
        }

        return null;
    }
}
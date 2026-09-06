using UnityEngine;

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

    [Header("Held Item")]
    public PlayerHeldItem playerHeldItem;

    // Current object inside the player's break hitbox
    private Collider2D currentBreakable;

    private void Start()
    {

        if (player == null)
        {
            Debug.LogError(
                "BreakSystem: Player Transform is not assigned!"
            );
        }
    }

    // Called by BreakHitbox
    public void SetTarget(Collider2D target)
    {
        currentBreakable = target;
    }

    // Called by BreakHitbox
    public void RemoveTarget(Collider2D target)
    {
        if (currentBreakable == target)
        {
            currentBreakable = null;
        }
    }

    // Called by WeaponController
    public void TryBreak()
    {
        if (playerHeldItem == null)
            return;

        if (player == null)
            return;

        // Get currently equipped item.
        // Null means bare hands.
        Item equippedItem =
            playerHeldItem.GetHeldItem();

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

        // Check cooldown.
        if (Time.time < nextBreakTime)
            return;

        // No breakable inside player's hitbox.
        if (currentBreakable == null)
        {
            Debug.Log(
                "No breakable object aligned with player."
            );

            return;
        }

        // Make sure the target is on the breakable layer.
        if (((1 << currentBreakable.gameObject.layer) &
            breakableLayer) == 0)
        {
            return;
        }

        // Get BreakableMaterial.
        BreakableMaterial material =
            currentBreakable.GetComponentInParent<BreakableMaterial>();

        if (material == null)
        {
            Debug.Log(
                "Target does not have BreakableMaterial."
            );

            return;
        }

        // Check distance.
        float distance =
            Vector2.Distance(
                player.position,
                currentBreakable.transform.position
            );

        if (distance > breakRange)
        {
            Debug.Log(
                "Material is too far away. Distance: " +
                distance.ToString("F2")
            );

            return;
        }

        // Find preset.
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

        // Shake the material when hit.
        HitShake hitShake =
            material.GetComponent<HitShake>();

        if (hitShake != null)
        {
            hitShake.Shake();
        }

        // Deal damage.
        bool broken =
            material.TakeDamage(preset.damage);

        // Drop items only when completely broken.
        if (broken)
        {
            DropItem(
                material,
                material.transform.position
            );

            currentBreakable = null;
        }
    }

    private void DropItem(
        BreakableMaterial material,
        Vector3 position
    )
    {
        if (material.drops == null ||
            material.drops.Length == 0)
        {
            Debug.LogWarning(
                "BreakableMaterial '" +
                material.name +
                "' has no Drop Items assigned!"
            );

            return;
        }

        foreach (DropItemData drop in material.drops)
        {
            if (drop == null)
                continue;

            if (drop.item == null)
            {
                Debug.LogWarning(
                    "A Drop Item is missing in BreakableMaterial '" +
                    material.name +
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

            // BARE HANDS
            if (!preset.requiresTool)
            {
                return preset;
            }

            // TOOL REQUIRED
            if (tool == null)
                continue;

            if (preset.requiredTool == null)
            {
                Debug.LogWarning(
                    "Preset '" +
                    preset.name +
                    "' requires a tool but has no Required Tool assigned!"
                );

                continue;
            }

            if (preset.requiredTool.ID == tool.ID)
            {
                return preset;
            }
        }

        return null;
    }
}
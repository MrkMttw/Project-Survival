using System.Collections;
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
    public LayerMask[] breakableLayers;

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

    // CHECK IF LAYER IS BREAKABLE
    private bool IsBreakableLayer(GameObject target)
    {
        foreach (LayerMask layer in breakableLayers)
        {
            if (((1 << target.layer) & layer) != 0)
                return true;
        }

        return false;
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

    public bool CanBreakCurrentTarget()
    {
        if (playerHeldItem == null || player == null)
            return false;

        if (currentBreakable == null)
            return false;

        // CHECK BREAKABLE LAYER
        if (!IsBreakableLayer(currentBreakable.gameObject))
            return false;

        // GET MATERIAL
        BreakableMaterial material =
            currentBreakable.GetComponentInParent<BreakableMaterial>();

        if (material == null)
            return false;

        // CHECK DISTANCE
        Vector2 closestPoint =
            currentBreakable.ClosestPoint(player.position);

        float distance =
            Vector2.Distance(player.position, closestPoint);

        if (distance > breakRange)
            return false;

        // CHECK TOOL PRESET
        Item equippedItem = playerHeldItem.GetHeldItem();

        ToolBreakPreset preset =
            FindPreset(equippedItem, material.materialType);

        return preset != null;
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
        if (!IsBreakableLayer(currentBreakable.gameObject))
            return;

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
        Vector2 closestPoint =
            currentBreakable.ClosestPoint(player.position);

        float distance =
            Vector2.Distance(
                player.position,
                closestPoint
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

        // PLAY TOOL SWING
        playerHeldItem.PlaySwing();

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
        
        // CONSUME TOOL DURABILITY
        if (equippedItem != null && preset.damage > 0)
        {
            equippedItem.UseDurability();
        }

        // HANDLE BROKEN OBJECT
        if (broken)
        {
            currentBreakable = null;

            StartCoroutine(FinishBreaking(material));
        }
    }
        
    private IEnumerator FinishBreaking(BreakableMaterial material)
    {
        // CHECK FOR TREE ANIMATION
        TreeFallAnimation treeFall =
            material.GetComponentInParent<TreeFallAnimation>();

        // PLAY TREE FALL ANIMATION
        if (treeFall != null)
        {
            yield return treeFall.PlayFall(player.position);
        }

        // MARK STRUCTURE AS DESTROYED
        StructureObjectIdentity structureIdentity =
            material.GetComponentInParent<StructureObjectIdentity>();

        if (structureIdentity != null)
        {
            StructureSpawner structureSpawner =
                FindFirstObjectByType<StructureSpawner>();

            if (structureSpawner != null)
            {
                structureSpawner.MarkStructureObjectDestroyed(
                    structureIdentity
                );
            }
        }

        // MARK WORLD OBJECT AS DESTROYED
        WorldObjectIdentity identity =
            material.GetComponentInParent<WorldObjectIdentity>();

        if (identity != null)
        {
            WorldGenerator worldGenerator =
                FindFirstObjectByType<WorldGenerator>();

            if (worldGenerator != null)
            {
                worldGenerator.MarkObjectDestroyed(identity);
            }
        }

        // DROP LOOT
        Vector3 dropPosition = material.transform.position;

        if (treeFall != null)
        {
            dropPosition = treeFall.transform.position;
        }

        DropItem(material, dropPosition);

        // DESTROY SHADOW
        if (treeFall != null && treeFall.shadowElement != null)
        {
            Destroy(treeFall.shadowElement);
        }

        // DESTROY OBJECT
        if (treeFall != null)
        {
            Destroy(treeFall.gameObject);
        }
        else
        {
            Destroy(material.gameObject);
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
            return;
        }

        foreach (DropItemData drop in material.drops)
        {
            if (drop == null || drop.item == null)
                continue;

            // ROLL DROP CHANCE
            float chance = Mathf.Clamp(
                drop.dropChance, 0f, 100f
            );

            if (chance <= 0f)
                continue;

            if (chance < 100f &&
                Random.Range(0f, 100f) >= chance)
            {
                continue;
            }

            // CALCULATE AMOUNT
            int minAmount = Mathf.Max(0, drop.minAmount);
            int maxAmount = Mathf.Max(minAmount, drop.maxAmount);

            int amount = Random.Range(
                minAmount, maxAmount + 1
            );

            if (amount <= 0)
                continue;

            // CLONE ITEM
            GameObject droppedItem = drop.item.CloneItem(amount);

            if (droppedItem == null)
                continue;

            droppedItem.transform.position = position;

            // ADD DROP ANIMATION
            LootDropAnimation dropAnimation =
                droppedItem.GetComponent<LootDropAnimation>();

            if (dropAnimation == null)
            {
                dropAnimation =
                    droppedItem.AddComponent<LootDropAnimation>();
            }

            dropAnimation.Play();

            Debug.Log(
                "Dropped " + amount + "x " + drop.item.name
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
            if (tool == null)
            {
                if (!preset.requiresTool)
                    return preset;

                continue;
            }

            // EQUIPPED TOOL
            if (!preset.requiresTool)
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
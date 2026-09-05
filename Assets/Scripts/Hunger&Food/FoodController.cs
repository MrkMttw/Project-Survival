using UnityEngine;
using UnityEngine.InputSystem;

public class FoodController : MonoBehaviour
{
    [Header("Player")]
    public PlayerHeldItem playerHeldItem;

    [Header("Hunger")]
    public HungerController hungerController;

    [Header("Food Presets")]
    public FoodPreset[] foodPresets;

    private bool isEating = false;

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        // Press E to eat
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryEat();
        }
    }

    private void TryEat()
    {
        // Already eating
        if (isEating)
            return;

        // Make sure PlayerHeldItem exists
        if (playerHeldItem == null)
        {
            Debug.LogWarning("FoodController: Player Held Item is not assigned.");
            return;
        }

        // Get currently held item
        Item heldItem = playerHeldItem.GetHeldItem();

        if (heldItem == null)
        {
            Debug.Log("FoodController: No item is currently held.");
            return;
        }

        // Find matching food preset
        FoodPreset food = GetFoodPreset(heldItem);

        if (food == null)
        {
            Debug.Log("FoodController: " + heldItem.name + " is not food.");
            return;
        }

        // Make sure hunger controller exists
        if (hungerController == null)
        {
            Debug.LogWarning("FoodController: Hunger Controller is not assigned.");
            return;
        }

        // Start eating
        StartCoroutine(EatFood(heldItem, food));
    }

    private FoodPreset GetFoodPreset(Item item)
    {
        if (foodPresets == null)
            return null;

        foreach (FoodPreset food in foodPresets)
        {
            if (food == null || food.itemPrefab == null)
                continue;

            Item foodItem = food.itemPrefab.GetComponent<Item>();

            if (foodItem == null)
                continue;

            // Match using Item ID
            if (foodItem.ID == item.ID)
            {
                return food;
            }
        }

        return null;
    }

    private System.Collections.IEnumerator EatFood(
        Item item,
        FoodPreset food)
    {
        isEating = true;

        Debug.Log(
            "Eating " + item.name +
            "... (" + food.eatingDelay + " seconds)"
        );

        // Wait for eating delay
        yield return new WaitForSeconds(food.eatingDelay);

        // Make sure the same item is still being held
        if (playerHeldItem.GetHeldItem() != item)
        {
            Debug.Log("FoodController: Food was unequipped before eating finished.");

            isEating = false;
            yield break;
        }

        // Restore hunger
        hungerController.RestoreHunger(food.hungerRestore);

        // Remove ONE food
        item.RemoveFromStack(1);

        // If the item still exists, update the held item
        if (item != null)
        {
            playerHeldItem.SetHeldItem(item);
        }
        else
        {
            playerHeldItem.ClearHeldItem();
        }

        Debug.Log(
            "Finished eating! +" +
            food.hungerRestore +
            " hunger."
        );

        isEating = false;
    }
}
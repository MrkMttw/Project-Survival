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

    [Header("Food UI")]
    public GameObject eatPrompt;

    private bool isEating = false;

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        UpdateEatPrompt();
    }

    private void UpdateEatPrompt()
    {
        if (eatPrompt == null || playerHeldItem == null)
            return;

        Item heldItem = playerHeldItem.GetHeldItem();

        // Nothing held = hide prompt
        if (heldItem == null)
        {
            eatPrompt.SetActive(false);
            return;
        }

        // Check if held item is food
        FoodPreset food = GetFoodPreset(heldItem);

        if (food != null)
        {
            eatPrompt.SetActive(true);
        }
        else
        {
            eatPrompt.SetActive(false);
        }
    }
    
    public bool TryEatHeldItem()
    {
        if (isEating || playerHeldItem == null)
            return false;

        Item heldItem = playerHeldItem.GetHeldItem();

        if (heldItem == null)
            return false;

        FoodPreset food = GetFoodPreset(heldItem);

        if (food == null || hungerController == null)
            return false;

        StartCoroutine(EatFood(heldItem, food));
        return true;
    }

    private FoodPreset GetFoodPreset(Item item)
    {
        if (foodPresets == null)
            return null;

        foreach (FoodPreset food in foodPresets)
        {
            if (food == null || food.itemPrefab == null)
                continue;

            Item foodItem =
                food.itemPrefab.GetComponent<Item>();

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
            "Eating " +
            item.name +
            "... (" +
            food.eatingDelay +
            " seconds)"
        );

        // Hide eat prompt while eating
        if (eatPrompt != null)
        {
            eatPrompt.SetActive(false);
        }

        // Wait for eating delay
        yield return new WaitForSeconds(food.eatingDelay);

        // Make sure the same item is still being held
        if (playerHeldItem.GetHeldItem() != item)
        {
            Debug.Log(
                "FoodController: Food was unequipped before eating finished."
            );

            isEating = false;
            yield break;
        }

        // RESTORE HUNGER

        hungerController.RestoreHunger(
            food.hungerRestore
        );

        // RESTORE HEALTH

        hungerController.RestoreHealth(
            food.hpRestore
        );

        // Remove ONE food
        item.RemoveFromStack(1);

        // If there is still food left, keep holding it
        if (item != null && item.quantity > 0)
        {
            playerHeldItem.SetHeldItem(item);
        }
        else
        {
            // No food left, clear the held item visual
            playerHeldItem.ClearHeldItem();
        }

        Debug.Log(
            "Finished eating! +" +
            food.hungerRestore +
            " hunger, +" +
            food.hpRestore +
            " HP."
        );

        isEating = false;
    }
}
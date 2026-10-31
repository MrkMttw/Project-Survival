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

    // CHECK IF HOLDING FOOD
    public bool CanEatHeldItem()
    {
        if (isEating || playerHeldItem == null || hungerController == null)
            return false;

        Item heldItem = playerHeldItem.GetHeldItem();

        if (heldItem == null)
            return false;

        return GetFoodPreset(heldItem) != null;
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

            Item foodItem = food.itemPrefab.GetComponent<Item>();

            if (foodItem == null)
                continue;

            if (foodItem.ID == item.ID)
                return food;
        }

        return null;
    }

    private System.Collections.IEnumerator EatFood(
        Item item,
        FoodPreset food)
    {
        isEating = true;

        yield return new WaitForSeconds(food.eatingDelay);

        if (playerHeldItem.GetHeldItem() != item)
        {
            isEating = false;
            yield break;
        }

        // RESTORE HUNGER
        hungerController.RestoreHunger(food.hungerRestore);

        // RESTORE HEALTH
        hungerController.RestoreHealth(food.hpRestore);

        // REMOVE ONE FOOD
        item.RemoveFromStack(1);

        // UPDATE HELD ITEM
        if (item != null && item.quantity > 0)
        {
            playerHeldItem.SetHeldItem(item);
        }
        else
        {
            playerHeldItem.ClearHeldItem();
        }

        isEating = false;
    }
}
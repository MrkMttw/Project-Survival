using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FurnaceController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject furnacePanel;

    [Header("Furnace Slots")]
    public Slot inputSlot;
    public Slot fuelSlot;
    public Slot outputSlot;

    [Header("Progress UI")]
    public Image fuelFill;
    public Image arrowFill;

    [Header("Hotbar Resize")]
    public RectTransform hotbarRect;
    public Vector3 furnaceOpenHotbarScale = new Vector3(0.7f, 0.7f, 1f);

    [Header("Furnace Recipes")]
    public List<CraftingRecipe> furnaceRecipes;

    public bool IsOpen => isOpen;
    
    private Vector3 originalHotbarScale;

    private FurnaceBehavior currentFurnace;
    private bool isOpen;

    private void Start()
    {
        if (furnacePanel != null)
        {
            furnacePanel.SetActive(false);
        }

        if (hotbarRect != null)
        {
            originalHotbarScale = hotbarRect.localScale;
        }
    }

    private void Update()
    {
        if (!isOpen || currentFurnace == null)
            return;

        if (fuelFill != null)
        {
            fuelFill.fillAmount = currentFurnace.GetFuelProgress();
        }

        if (arrowFill != null)
        {
            arrowFill.fillAmount = currentFurnace.GetSmeltProgress();
        }
    }

    public bool TryInteract()
    {
        // Pressing E again closes the currently open furnace.
        if (isOpen)
        {
            CloseFurnace();
            return true;
        }

        if (player == null || furnacePanel == null)
        {
            return false;
        }

        FurnaceBehavior[] furnaces =
            FindObjectsByType<FurnaceBehavior>(FindObjectsSortMode.None);

        foreach (FurnaceBehavior furnace in furnaces)
        {
            if (furnace == null ||
                !furnace.IsPlayerInRange(player.position))
            {
                continue;
            }

            currentFurnace = furnace;
            isOpen = true;

            if (hotbarRect != null)
            {
                hotbarRect.localScale = furnaceOpenHotbarScale;
            }

            currentFurnace.LoadSlots(inputSlot, fuelSlot, outputSlot);
            furnacePanel.SetActive(true);
            return true;
        }

        return false;
    }

    private void CloseFurnace()
    {
        isOpen = false;
        currentFurnace = null;

        if (hotbarRect != null)
        {
            hotbarRect.localScale = originalHotbarScale;
        }

        if (furnacePanel != null)
        {
            furnacePanel.SetActive(false);
        }
    }
    
    public void StartCooking()
    {
        if (!isOpen || currentFurnace == null)
        {
            return;
        }

        currentFurnace.SetCooking(true);
    }

    public void RefreshSlots(FurnaceBehavior furnace)
    {
        if (!isOpen ||
            currentFurnace == null ||
            currentFurnace != furnace)
        {
            return;
        }

        currentFurnace.LoadSlots(inputSlot, fuelSlot, outputSlot);
    }

    public void NotifySlotsChanged()
    {
        if (!isOpen || currentFurnace == null)
            return;

        currentFurnace.SyncSlots(inputSlot, fuelSlot, outputSlot);
    }

    public void CloseForMenu()
    {
        if (isOpen)
        {
            CloseFurnace();
        }
    }
}
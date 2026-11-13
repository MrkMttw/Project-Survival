using UnityEngine;
using UnityEngine.InputSystem;

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;
    public GameObject[] pages;
    public TabController tabController;
    public GameObject Hotbar;

    public CraftingController craftingController;
    public FurnaceController furnaceController;
    public ContainerController containerController;

    private RectTransform panelRect;
    private Vector2 originalPosition;
    private bool wasCraftingPageActive;

    void Start()
    {
        panelRect = Hotbar.GetComponent<RectTransform>();

        // SAVE THE HOTBAR'S STARTING POSITION
        originalPosition = panelRect.anchoredPosition;

        // MENU STARTS CLOSED
        menuCanvas.SetActive(false);
    }

    void Update()
    {
        UpdateMenuPrompt();

        // OPEN INVENTORY USING B
        if (Keyboard.current != null &&
            Keyboard.current.bKey.wasPressedThisFrame)
        {
            OpenInventoryPage();
        }

        // HOTBAR POSITION
        if (containerController != null &&
            containerController.IsOpen)
        {
            // CONTAINER CONTROLLER HANDLES THE POSITION
            Hotbar.SetActive(true);
        }
        else if (!menuCanvas.activeSelf)
        {
            Hotbar.SetActive(true);
            panelRect.anchoredPosition = originalPosition;
        }
        else if (pages[1].activeSelf)
        {
            Hotbar.SetActive(true);
            panelRect.anchoredPosition = new Vector2(0, -315);
        }
        else if (pages[2].activeSelf)
        {
            Hotbar.SetActive(true);
            panelRect.anchoredPosition = new Vector2(0, -490);
        }
        else
        {
            Hotbar.SetActive(false);
        }

        bool isCraftingPageActive =
            menuCanvas.activeSelf && pages[2].activeSelf;

        if (isCraftingPageActive && !wasCraftingPageActive)
        {
            if (craftingController != null)
            {
                craftingController.RefreshRecipes();
            }
        }

        wasCraftingPageActive = isCraftingPageActive;
    }

    // OPEN INVENTORY PAGE
    public void OpenInventoryPage()
    {
        // CLOSE FURNACE
        if (furnaceController != null &&
            furnaceController.IsOpen)
        {
            furnaceController.CloseForMenu();
        }

        // CLOSE CONTAINER
        if (containerController != null &&
            containerController.IsOpen)
        {
            containerController.CloseForMenu();
        }

        // CLOSE CRAFTING
        if (craftingController != null &&
            craftingController.IsOpen)
        {
            craftingController.CloseForMenu();
            craftingController.ResetCraftingPanel();
        }

        menuCanvas.SetActive(true);

        // SELECT INVENTORY TAB
        if (tabController != null)
        {
            tabController.ActiveTab(1);
        }
    }

    // MOBILE INVENTORY BUTTON
    public void OnInventoryButtonPressed()
    {
        OpenInventoryPage();
    }

    // EXIT INVENTORY TAB
    public void ExitTab()
    {
        menuCanvas.SetActive(false);
    }

    // UPDATE MENU PROMPT
    private void UpdateMenuPrompt()
    {
        if (PromptController.Instance == null)
            return;

        if (!menuCanvas.activeSelf)
            PromptController.Instance.RequestPrompt(
                PromptType.B, this);
        else
            PromptController.Instance.ReleasePrompt(
                PromptType.B, this);
    }

    private void OnDisable()
    {
        if (PromptController.Instance != null)
            PromptController.Instance.ReleasePrompt(
                PromptType.B, this);
    }

    // OPEN CRAFTING PAGE
    public void OpenCraftingPage()
    {
        menuCanvas.SetActive(true);

        foreach (GameObject page in pages)
        {
            page.SetActive(false);
        }

        pages[2].SetActive(true);

        if (craftingController != null)
        {
            craftingController.ResetCraftingPanel();
            craftingController.RefreshRecipes();
        }
    }
}
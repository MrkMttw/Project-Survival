using UnityEngine;
using UnityEngine.InputSystem;

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;
    public GameObject[] pages;
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

        if (Keyboard.current != null &&
            Keyboard.current.bKey.wasPressedThisFrame)
        {
            // FURNACE OPEN
            if (furnaceController != null &&
                furnaceController.IsOpen)
            {
                furnaceController.CloseForMenu();
                menuCanvas.SetActive(true);
            }
            // CONTAINER OPEN
            else if (containerController != null &&
                     containerController.IsOpen)
            {
                containerController.CloseForMenu();
                OpenInventoryPage();
            }
            // CRAFTING PAGE OPEN
            else if (menuCanvas.activeSelf &&
                     pages[2].activeSelf)
            {
                if (craftingController != null)
                {
                    craftingController.CloseForMenu();
                    craftingController.ResetCraftingPanel();
                }

                menuCanvas.SetActive(false);
            }
            // CRAFTING CONTROLLER OPEN
            else if (craftingController != null &&
                     craftingController.IsOpen)
            {
                craftingController.CloseForMenu();
                menuCanvas.SetActive(false);
            }
            else
            {
                menuCanvas.SetActive(!menuCanvas.activeSelf);
            }
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
    private void OpenInventoryPage()
    {
        menuCanvas.SetActive(true);

        foreach (GameObject page in pages)
        {
            page.SetActive(false);
        }

        // PAGE 1 = INVENTORY
        pages[1].SetActive(true);
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
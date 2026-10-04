using UnityEngine;
using UnityEngine.InputSystem;

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;
    public GameObject[] pages;
    public GameObject Hotbar;
    public CraftingController craftingController;

    private RectTransform panelRect;
    private Vector2 originalPosition;
    private bool wasCraftingPageActive;

    void Start()
    {
        panelRect = Hotbar.GetComponent<RectTransform>();

        // Save the Hotbar's starting position
        originalPosition = panelRect.anchoredPosition;

        // Menu starts closed
        menuCanvas.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            menuCanvas.SetActive(!menuCanvas.activeSelf);
        }

        if (!menuCanvas.activeSelf)
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
}
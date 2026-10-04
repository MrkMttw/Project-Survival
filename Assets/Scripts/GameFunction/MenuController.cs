using UnityEngine;
using UnityEngine.InputSystem;

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;
    public GameObject[] pages;
    public GameObject Hotbar;
    
    private RectTransform panelRect;
    private Vector2 originalPosition;

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
            // Menu closed → restore Hotbar
            Hotbar.SetActive(true);
            panelRect.anchoredPosition = originalPosition;
        }
        else if (pages[1].activeSelf)
        {
            // Inventory tab
            Hotbar.SetActive(true);
            panelRect.anchoredPosition = new Vector2(0, -315);
        }
        else if (pages[2].activeSelf)
        {
            // Crafting tab
            Hotbar.SetActive(true);
            panelRect.anchoredPosition = new Vector2(0, -490);
        }
        else
        {
            // Other tabs → hide Hotbar
            Hotbar.SetActive(false);
        }
    }
}
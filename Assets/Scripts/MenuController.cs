using UnityEngine;

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
        if (Input.GetKeyDown(KeyCode.B))
        {
            menuCanvas.SetActive(!menuCanvas.activeSelf);
        }

        if (menuCanvas.activeSelf && pages[1].activeSelf)
        {
            // Menu open → move Hotbar
            Hotbar.SetActive(true);
            panelRect.anchoredPosition = new Vector2(0, -315);
        }
        else if (menuCanvas.activeSelf)
        {
            // if not InventoryPage is active
            Hotbar.SetActive(false);
        }
        else
        {
            // Menu closed → restore Hotbar
            panelRect.anchoredPosition = originalPosition;
        }
    }
}
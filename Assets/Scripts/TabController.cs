using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls tab-based UI navigation.
/// Manages switching between different pages/content panels by activating or deactivating them.
/// </summary>
public class TabController : MonoBehaviour
{
    /// <summary>
    /// The main menu canvas that contains the tabbed interface.
    /// Used to show or hide the menu when the Settings button is pressed.
    /// </summary>
    [SerializeField] GameObject menuCanvas;

    /// <summary>
    /// Array of Image components representing the tab buttons.
    /// Used to visually indicate which tab is currently active.
    /// </summary>
    public Image[] tabImages;

    /// <summary>
    /// Array of GameObjects representing the content pages for each tab.
    /// Only the selected page is active at a time.
    /// </summary>
    public GameObject[] pages;

    /// <summary>
    /// Initializes the tab controller by activating the second tab.
    /// Called once before the first Update call.
    /// </summary>
    void Start()
    {
        ActiveTab(1);
    }

    /// <summary>
    /// Activates the specified tab and deactivates all other tabs.
    /// Also updates the visual appearance of the tab buttons to indicate
    /// which tab is currently active.
    /// </summary>
    /// <param name="tabNo">The zero-based index of the tab to activate.</param>
    public void ActiveTab(int tabNo)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
            tabImages[i].color = Color.gray;
        }

        pages[tabNo].SetActive(true);
        tabImages[tabNo].color = Color.white;
    }

    /// <summary>
    /// Toggles the visibility of the menu canvas and activates the Settings tab.
    /// If the menu canvas is currently active, it will be hidden.
    /// If it is inactive, it will be shown.
    /// </summary>
    public void SettingsButton()
    {
        menuCanvas.SetActive(!menuCanvas.activeSelf);
        ActiveTab(3);
    }
}
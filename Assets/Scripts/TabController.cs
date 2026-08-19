using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls tab-based UI navigation.
/// Manages switching between different pages/content panels by activating/deactivating them.
/// </summary>
public class TabController : MonoBehaviour
{
    /// <summary>
    /// Array of Image components representing the tab buttons.
    /// Used to visually indicate which tab is active.
    /// </summary>
    public Image[] tabImages;

    /// <summary>
    /// Array of GameObjects representing the content pages for each tab.
    /// Only one page is active at a time.
    /// </summary>
    public GameObject[] pages;

    /// <summary>
    /// Initializes the tab controller by activating the first tab.
    /// Called once before the first Update call.
    /// </summary>
    void Start()
    {
        ActivateTab(0);
    }

    /// <summary>
    /// Activates the specified tab and deactivates all others.
    /// Updates the visual state of tab buttons to show which is active.
    /// </summary>
    /// <param name="tabNo">The index of the tab to activate.</param>
    public void ActivateTab(int tabNo)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
            tabImages[i].color = Color.gray;
        }
        pages[tabNo].SetActive(true);
        tabImages[tabNo].color = Color.white;
    }
}
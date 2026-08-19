using UnityEngine;

/// <summary>
/// Controls the visibility of the game menu.
/// Toggles the menu canvas on and off when the Tab key is pressed.
/// </summary>
public class MenuController : MonoBehaviour
{
    /// <summary>
    /// The canvas GameObject that contains the menu UI elements.
    /// This is toggled on and off based on user input.
    /// </summary>
    public GameObject menuCanvas;

    /// <summary>
    /// Initializes the menu by hiding it at startup.
    /// Called once before the first Update call.
    /// </summary>
    void Start()
    {
        menuCanvas.SetActive(false);
    }

    /// <summary>
    /// Checks for input to toggle the menu visibility each frame.
    /// The menu is toggled when the Tab key is pressed.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            menuCanvas.SetActive(!menuCanvas.activeSelf);
        }
    }
}

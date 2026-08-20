using Unity.Cinemachine;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// Handles saving and loading the player's game data.
/// Stores the player's position and the current map boundary in a JSON file.
/// </summary>
public class SaveController : MonoBehaviour
{
    /// <summary>
    /// The file path where the game's save data is stored.
    /// </summary>
    private string saveLocation;

    /// <summary>
    /// Initializes the save location and loads previously saved game data.
    /// Called once before the first Update call.
    /// </summary>
    void Start()
    {
        //Define save location
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");

        LoadGame();
    }

    /// <summary>
    /// Saves the player's current position and the active map boundary
    /// to a JSON save file.
    /// </summary>
    public void SaveGame()
    {
        SaveData saveData = new SaveData
        {
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,
            mapBoundary = FindObjectOfType<CinemachineConfiner2D>().BoundingShape2D.gameObject.name
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
    }

    public void SaveAndExit()
    {
        SaveGame();
        SceneManager.LoadScene("MainMenuScene");
    }

    /// <summary>
    /// Loads the saved game data if a save file exists.
    /// Restores the player's position and map boundary from the save file.
    /// If no save file exists, a new save file is created using the current game state.
    /// </summary>
    public void LoadGame()
    {
        if (File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));
            GameObject.FindGameObjectWithTag("Player").transform.position = saveData.playerPosition;

            FindObjectOfType<CinemachineConfiner2D>().BoundingShape2D = GameObject.Find(saveData.mapBoundary).GetComponent<PolygonCollider2D>();
        }
        else
        {
            SaveGame();
        }
    }
}
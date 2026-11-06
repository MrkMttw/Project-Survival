using TMPro;
using UnityEngine;

public class CreateWorldController : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField worldNameField;
    public TMP_InputField seedField;

    [Header("References")]
    public WorldSelectionController worldSelectionController;
    public StartMenuController startMenuController;

    public void OnClickCreate()
    {
        // GET WORLD NAME
        string worldName = worldNameField.text.Trim();

        if (string.IsNullOrEmpty(worldName))
        {
            Debug.LogWarning("PLEASE ENTER A WORLD NAME.");
            return;
        }

        // GENERATE OR PARSE WORLD SEED
        int worldSeed;

        if (string.IsNullOrWhiteSpace(seedField.text))
        {
            worldSeed = Random.Range(0, int.MaxValue);
        }
        else if (!int.TryParse(seedField.text, out worldSeed))
        {
            Debug.LogWarning("INVALID WORLD SEED.");
            return;
        }

        // CREATE WORLD
        WorldSaveManager.CreateWorld(worldName, worldSeed);

        Debug.Log("WORLD CREATED: " + worldName);

        // REFRESH WORLD LIST
        worldSelectionController.RefreshWorldList();

        // RETURN TO WORLD SELECTION
        startMenuController.OnClickCreateWorldBack();
    }
}